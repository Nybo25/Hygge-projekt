using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Bruges til LINQ og databaseoperationer
using ZealandLokaleBooking.Data; // Namespace til ApplicationDbContext, som bruges til databasekommunikation
using ZealandLokaleBooking.Models; // Namespace til Room og Booking modeller
using System.Linq; // Giver adgang til LINQ-metoder til databaseforespørgsler
using System; // Indeholder grundlæggende typer og funktioner som DateTime

namespace ZealandLokaleBooking.Controllers
{
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context; // Databasekontekst for at interagere med databasen

        // Constructor til at initialisere RoomController med databasekonteksten
        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Room/AvailableRooms
        [HttpGet]
        public IActionResult AvailableRooms()
        {
            // Finder alle ledige lokaler
            var availableRooms = _context.Rooms
                .Where(r => !_context.Bookings
                    .Any(b => b.RoomId == r.RoomId && b.EndTime > DateTime.UtcNow)) // Tjekker, om der ikke er overlappende aktive bookinger
                .ToList(); // Konverterer resultatet til en liste

            // Returnerer en visning med de ledige lokaler
            return View(availableRooms);
        }

        // GET: Room/BookRoom
        [HttpGet]
        public IActionResult BookRoom(int roomId)
        {
            // Finder det ønskede lokale baseret på dets ID
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == roomId);

            if (room == null)
            {
                // Returnerer en fejlmeddelelse, hvis lokalet ikke findes
                return NotFound("Lokalet findes ikke.");
            }

            // Returnerer en visning med oplysninger om lokalet
            return View(room);
        }

        // POST: Room/BookRoom
        [HttpPost]
        public IActionResult BookRoom(int roomId, DateTime startTime, DateTime endTime)
        {
            // Validerer, at sluttidspunktet er senere end starttidspunktet
            if (endTime <= startTime)
            {
                ViewBag.ErrorMessage = "Sluttidspunkt skal være efter starttidspunkt.";
                var room = _context.Rooms.FirstOrDefault(r => r.RoomId == roomId);
                return View(room);
            }

            // Kontrollerer, om der er overlap med eksisterende bookinger for det samme lokale
            var isBooked = _context.Bookings.Any(b =>
                b.RoomId == roomId &&
                ((startTime < b.EndTime && startTime >= b.StartTime) || // Starttidspunkt overlapper med en eksisterende booking
                 (endTime > b.StartTime && endTime <= b.EndTime)));    // Sluttidspunkt overlapper med en eksisterende booking

            if (isBooked)
            {
                // Returnerer en fejlmeddelelse, hvis lokalet allerede er booket i det ønskede tidsrum
                ViewBag.ErrorMessage = "Lokalet er allerede booket i dette tidsrum.";
                var room = _context.Rooms.FirstOrDefault(r => r.RoomId == roomId);
                return View(room);
            }

            // Henter brugerens ID fra claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                // Returnerer en fejlmeddelelse, hvis brugeren ikke er logget ind
                return Unauthorized("Brugeren er ikke logget ind.");
            }

            // Opretter en ny booking
            var booking = new Booking
            {
                RoomId = roomId,                      // ID på det lokale, der bookes
                StartTime = startTime,                // Starttidspunkt for bookingen
                EndTime = endTime,                    // Sluttidspunkt for bookingen
                UserId = int.Parse(userIdClaim.Value) // Bruger-ID fra claims (skal være en valid integer)
            };

            // Tilføjer den nye booking til databasen
            _context.Bookings.Add(booking);
            _context.SaveChanges(); // Gemmer ændringer i databasen

            // Omdirigerer til oversigten over ledige lokaler
            return RedirectToAction("AvailableRooms");
        }
    }
}
