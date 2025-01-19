using Microsoft.AspNetCore.Mvc.RazorPages;
using ZealandLokaleBooking.Data; // Namespace for databasekonteksten
using ZealandLokaleBooking.Models; // Namespace for modellerne
using Microsoft.EntityFrameworkCore; // For Include og ThenInclude metoder
using Microsoft.AspNetCore.Mvc; // For IActionResult og TempData
using System.Collections.Generic; // For List<T>
using System.Linq; // For LINQ-metoder
using System.Threading.Tasks; // For async/await
using System; // For DateTime

namespace Zealand_Lokale_Booking.Pages.Booking
{
    public class ManageRoomsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // Konstruktor til at injicere databasekonteksten
        public ManageRoomsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Liste over lokaler, der vises i View
        public List<Room> Rooms { get; set; }

        // Filtreringsparameter til at sortere lokaler
        public string Filter { get; set; }

        // Håndterer GET-forespørgsler og henter filtrerede lokaler
        public void OnGet(string filter = "all")
        {
            Filter = filter;

            // Start en forespørgsel, der inkluderer lokaler, deres bookinger og brugere
            var query = _context.Rooms
                .Include(r => r.Bookings.Where(b => b.IsActive && b.Status == "Active")) // Kun aktive bookinger
                .ThenInclude(b => b.User) // Inkluder brugeroplysninger for bookinger
                .AsQueryable();

            // Tilføj filtrering baseret på parameteren
            if (filter == "booked")
            {
                query = query.Where(r => r.Bookings.Any(b => b.IsActive && b.Status == "Active")); // Kun bookede lokaler
            }
            else if (filter == "available")
            {
                query = query.Where(r => !r.Bookings.Any(b => b.IsActive && b.Status == "Active")); // Kun ledige lokaler
            }

            Rooms = query.ToList(); // Udfør forespørgslen og konverter til en liste
        }

        // Håndterer POST-forespørgsler for at annullere en booking
        public async Task<IActionResult> OnPostCancelBookingAsync(int roomId)
        {
            // Find lokalet og dets tilknyttede bookinger
            var room = _context.Rooms
                .Include(r => r.Bookings)
                .FirstOrDefault(r => r.RoomId == roomId);

            // Kontrollér, om lokalet findes og har aktive bookinger
            if (room == null || !room.Bookings.Any(b => b.IsActive && b.Status == "Active"))
            {
                TempData["ErrorMessage"] = "Lokalet har ingen aktiv booking.";
                return RedirectToPage(new { filter = Filter });
            }

            // Find den første aktive booking og markér den som annulleret
            var booking = room.Bookings.FirstOrDefault(b => b.IsActive && b.Status == "Active");
            if (booking != null)
            {
                booking.Status = "Cancelled";
                booking.IsActive = false;
            }

            // Hvis der ikke er flere aktive bookinger, opdateres lokalet som ledigt
            if (!room.Bookings.Any(b => b.IsActive && b.Status == "Active"))
            {
                room.IsBooked = false;
                room.BookedByUserId = null;
            }

            // Opret en notifikation til brugeren, hvis lokalet var booket
            var bookedUserId = room.BookedByUserId;
            if (bookedUserId.HasValue) // Kun hvis der er en bruger tilknyttet
            {
                var notification = new Notification
                {
                    UserId = bookedUserId.Value,
                    Message = $"Din booking for lokalet \"{room.RoomName}\" blev annulleret.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }

            // Gem ændringerne i databasen
            await _context.SaveChangesAsync();

            // Giv feedback til brugeren om handlingen
            TempData["SuccessMessage"] =
                $"Bookingen for lokalet \"{room.RoomName}\" blev annulleret, og brugeren fik en notifikation.";
            return RedirectToPage(new { filter = Filter });
        }
    }
}









