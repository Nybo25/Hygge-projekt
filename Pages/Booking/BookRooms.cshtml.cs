using Microsoft.AspNetCore.Mvc.RazorPages;
using ZealandLokaleBooking.Data; // Namespace til databasekonteksten
using ZealandLokaleBooking.Models; // Namespace til modellerne
using Microsoft.AspNetCore.Mvc; // Til brug af IActionResult og TempData
using Microsoft.EntityFrameworkCore; // For Include() og ThenInclude()
using System.Linq; // For LINQ-metoder som FirstOrDefault()
using System.Collections.Generic; // For List<T>
using System; // For DateTime og TimeSpan

namespace Zealand_Lokale_Booking.Pages.Booking
{
    public class BookRoomsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // Konstruktor til at injicere databasekonteksten
        public BookRoomsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Liste over lokaler, der bliver brugt i View
        public List<Room> Rooms { get; set; }

        // Håndterer GET-forespørgsler og indlæser alle lokaler
        public void OnGet()
        {
            // Hent lokaler med deres aktive bookinger og relaterede brugere
            Rooms = _context.Rooms
                .Include(r => r.Bookings.Where(b => b.IsActive && b.Status == "Active")) // Filtrer aktive bookinger
                .ThenInclude(b => b.User) // Inkluder brugeroplysninger for hver booking
                .ToList(); // Konverter resultatet til en liste
        }

        // Håndterer POST-forespørgsler til booking af et lokale
        public IActionResult OnPostBookRoom(int roomId, DateTime bookingDate, TimeSpan startTime, int intervalMinutes)
        {
            // Hent lokalet og dets aktive bookinger
            var room = _context.Rooms
                .Include(r => r.Bookings.Where(b => b.IsActive && b.Status == "Active"))
                .FirstOrDefault(r => r.RoomId == roomId);

            // Kontrollér, om lokalet findes
            if (room == null)
            {
                TempData["ErrorMessage"] = "Lokalet findes ikke.";
                return RedirectToPage();
            }

            // Find den loggede bruger via deres email
            var userEmail = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Brugeren kunne ikke findes.";
                return RedirectToPage();
            }

            // Restriktion: Kun lærere kan booke auditoriet
            if (room.RoomType == "Auditorium" && user.RoleId != 2) // Antager, at RoleId = 2 repræsenterer lærerrollen
            {
                TempData["ErrorMessage"] = "Kun lærere kan booke auditoriet.";
                return RedirectToPage();
            }

            // Beregn start- og sluttidspunkt for bookingen
            var startDateTime = bookingDate.Add(startTime);
            var endDateTime = startDateTime.AddMinutes(intervalMinutes);

            // Restriktion: Klasselokaler kan kun have 2 aktive bookinger på én dag
            if (room.RoomType == "Klasselokale")
            {
                var sameDateBookings = room.Bookings
                    .Where(b => b.StartTime.Date == bookingDate.Date) // Filtrer bookinger efter dato
                    .ToList();

                if (sameDateBookings.Count >= 2)
                {
                    TempData["ErrorMessage"] = "Klasselokalet har allerede to aktive bookinger på denne dato.";
                    return RedirectToPage();
                }

                // Kontrollér overlap mellem eksisterende bookinger og den nye booking
                var userOverlap = sameDateBookings
                    .Any(b => b.StartTime < endDateTime && b.EndTime > startDateTime && b.UserId == user.UserId);

                if (userOverlap)
                {
                    TempData["ErrorMessage"] = "Du har allerede booket dette lokale i dette tidsrum.";
                    return RedirectToPage();
                }
            }

            // Opret og gem en ny booking
            var booking = new ZealandLokaleBooking.Models.Booking
            {
                RoomId = room.RoomId,
                UserId = user.UserId,
                StartTime = startDateTime,
                EndTime = endDateTime,
                IsDeleted = false,
                IsActive = true,
                Status = "Active"
            };

            _context.Bookings.Add(booking);

            // Opdater lokalet afhængigt af dets type
            if (room.RoomType == "Klasselokale")
            {
                room.IsBooked = room.Bookings.Any(b => b.IsActive); // Markér som booket, hvis der er aktive bookinger
            }
            else
            {
                room.IsBooked = true; // Auditorier bliver altid markeret som booket
                room.BookedByUserId = user.UserId; // Gem bruger-ID for auditoriebookinger
            }

            _context.SaveChanges();

            // Informér brugeren om den succesfulde booking
            TempData["SuccessMessage"] = $"Lokalet '{room.RoomName}' blev booket fra {startDateTime:HH:mm} til {endDateTime:HH:mm} den {bookingDate:dd-MM-yyyy}.";
            return RedirectToPage();
        }
    }
}























































