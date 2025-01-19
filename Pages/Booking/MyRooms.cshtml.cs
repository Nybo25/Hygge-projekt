using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ZealandLokaleBooking.Data; // Namespace for databasekonteksten
using ZealandLokaleBooking.Models; // Namespace for modellerne
using Microsoft.EntityFrameworkCore; // For Include og LINQ-metoder
using System.Linq; // For LINQ-metoder
using System.Collections.Generic; // For List<T>
using System; // For DateTime

namespace Zealand_Lokale_Booking.Pages.Booking
{
    public class MyRoomsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // Konstruktor til at injicere databasekonteksten
        public MyRoomsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Liste over aktive bookinger for den nuværende bruger
        public List<ZealandLokaleBooking.Models.Booking> ActiveBookings { get; set; }

        // Håndterer GET-forespørgsler og henter den nuværende brugers aktive bookinger
        public void OnGet()
        {
            var userEmail = User.Identity.Name;

            // Find den nuværende bruger baseret på deres email
            var currentUserId = _context.Users
                .FirstOrDefault(u => u.Email == userEmail)?.UserId;

            // Hvis brugeren er fundet, hentes deres aktive bookinger
            if (currentUserId.HasValue)
            {
                ActiveBookings = _context.Bookings
                    .Where(b => b.UserId == currentUserId.Value && b.IsActive) // Filtrer kun aktive bookinger
                    .Include(b => b.Room) // Inkluder lokaleoplysninger for hver booking
                    .ToList();
            }
            else
            {
                ActiveBookings = new List<ZealandLokaleBooking.Models.Booking>(); // Hvis brugeren ikke findes, returneres en tom liste
            }
        }

        // Håndterer POST-forespørgsler for at slette en booking
        public IActionResult OnPostDeleteRoom(int roomId)
        {
            // Find booking for det angivne lokale og sørg for, at det er aktivt
            var booking = _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefault(b => b.RoomId == roomId && b.IsActive);

            if (booking != null)
            {
                // Markér booking som annulleret
                booking.IsActive = false;
                booking.Status = "Cancelled";
                booking.CancelledDate = DateTime.Now;

                // Opdater lokalet for at markere det som ledigt
                var room = booking.Room;
                room.IsBooked = false;
                room.BookedByUserId = null;

                _context.SaveChanges(); // Gem ændringerne i databasen

                // Tilføj succesmeddelelse, der vises til brugeren
                TempData["SuccessMessage"] = $"Bookingen for {room.RoomName} blev annulleret.";
            }
            else
            {
                // Hvis booking ikke findes eller allerede er annulleret, vis fejlinformation
                TempData["ErrorMessage"] = "Der opstod en fejl under annulleringen.";
            }

            // Omdirigér tilbage til samme side for at vise opdateringer
            return RedirectToPage();
        }
    }
}


