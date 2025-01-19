using Microsoft.AspNetCore.Mvc;
using ZealandLokaleBooking.Data; // Namespace til ApplicationDbContext, bruges til databasekommunikation
using ZealandLokaleBooking.Models; // Namespace til Room, User og Booking-modeller
using System; // Indeholder grundlæggende typer og funktioner som DateTime
using System.Linq; // Giver LINQ-metoder til databaseforespørgsler

namespace ZealandLokaleBooking.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context; // Felt til databasekontekst, bruges til at interagere med databasen

        // Constructor til at initialisere BookingController med databasekontekst
        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Booking/BookRoom
        [HttpPost]
        public IActionResult BookRoom(int roomId, DateTime startTime, DateTime endTime)
        {
            // Henter den aktuelt loggede brugers email fra User.Identity
            var userEmail = User.Identity.Name;

            // Finder brugeren i databasen baseret på email
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail); 

            if (user == null)
            {
                // Returnerer en fejl, hvis brugeren ikke findes
                return NotFound("User not found.");
            }

            // Finder lokalet baseret på det angivne roomId
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == roomId);

            if (room == null)
            {
                // Returnerer en fejl, hvis lokalet ikke findes
                return NotFound("Room not found.");
            }

            if (room.IsBooked)
            {
                // Hvis lokalet allerede er booket, vises en fejlmeddelelse
                TempData["ErrorMessage"] = "Lokalet er allerede booket.";
                return RedirectToPage("/Booking/BookRooms");
            }

            // Tjekker hvor mange aktive bookinger brugeren har
            var activeBookingsCount = _context.Bookings
                .Where(b => b.UserId == user.UserId && b.IsActive && !b.IsDeleted)
                .Count();

            if (activeBookingsCount >= 3)
            {
                // Hvis brugeren har 3 aktive bookinger, vises en fejlmeddelelse
                TempData["ErrorMessage"] = "Du kan kun have 3 aktive bookinger ad gangen.";
                return RedirectToPage("/Booking/BookRooms");
            }

            // Opretter en ny booking
            var booking = new Booking
            {
                RoomId = room.RoomId,
                UserId = user.UserId,
                StartTime = startTime,
                EndTime = endTime,
                IsDeleted = false,
                IsActive = true
            };

            // Tilføjer booking til databasen og markerer lokalet som booket
            _context.Bookings.Add(booking);
            room.IsBooked = true;  // Markér lokalet som booket
            _context.SaveChanges(); // Gem ændringer i databasen

            // Omdirigerer brugeren til StudentDashboard efter booking
            return RedirectToPage("/StudentDashboard");
        }

        // GET: Booking/ConfirmDelete/5
        public IActionResult ConfirmDelete(int id)
        {
            // Finder en booking baseret på dens ID
            var booking = _context.Bookings.Where(b => b.Id == id).FirstOrDefault();
            if (booking == null)
            {
                // Returnerer en fejl, hvis bookingen ikke findes
                return NotFound();
            }

            // Returnerer en visning med booking-detaljer til bekræftelse af sletning
            return View(booking);
        }

        // POST: Booking/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken] // Sikrer, at anmodningen kommer fra en valideret kilde
        public IActionResult DeleteConfirmed(int id)
        {
            // Finder bookingen, der skal slettes, baseret på dens ID
            var booking = _context.Bookings.Where(b => b.Id == id).FirstOrDefault();
            if (booking == null)
            {
                // Returnerer en fejl, hvis bookingen ikke findes
                return NotFound();
            }

            // Finder det tilhørende lokale og markerer det som ledigt
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == booking.RoomId);
            if (room != null)
            {
                room.IsBooked = false; // Markér lokalet som ledigt
                room.BookedByUserId = null; // Fjern referencen til den bookede bruger
                _context.SaveChanges();
            }

            // Fjerner booking-posten fra databasen
            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            // Omdirigerer brugeren til StudentDashboard efter sletning
            return RedirectToAction("Index", "StudentDashboard");
        }

        // GET: Booking/FilterByDate
        [HttpGet("filter-by-date")]
        public IActionResult FilterBookingsByDate(DateTime date)
        {
            // Finder bookinger for en specifik dato og returnerer relevant information
            var bookings = _context.Bookings
                                   .Where(b => b.StartTime.Date == date.Date)
                                   .Select(b => new
                                   {
                                       b.Id,
                                       b.Room.RoomName, // Navnet på det tilknyttede lokale
                                       b.StartTime,
                                       b.EndTime
                                   })
                                   .ToList();

            // Returnerer resultatet som JSON
            return Ok(bookings);
        }
    }
}


