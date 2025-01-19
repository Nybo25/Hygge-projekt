using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // Giver adgang til funktioner som Include() for relationer
using ZealandLokaleBooking.Data; // Namespace for databasekonteksten
using ZealandLokaleBooking.Models; // Namespace for bruger- og andre relaterede modeller
using System; // Bruges til debugging (Console.WriteLine)
using System.Linq; // Giver adgang til LINQ-funktioner som FirstOrDefault() og ToList()

namespace Zealand_Lokale_Booking.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // Konstruktor til at injicere databasekonteksten
        public ProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ejendom til at holde den aktuelle bruger, der er logget ind
        public User CurrentUser { get; set; } // Omdøbt for bedre klarhed

        // Kører, når siden hentes via GET-forespørgsel
        public void OnGet()
        {
            // Kontrollér, om brugeren er logget ind
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Debugging: Udskriv brugerens email (fra Identity.Name)
                Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");

                var email = User.Identity.Name; // Hent email fra den aktuelle bruger
                CurrentUser = _context.Users
                    .Include(u => u.Role) // Inkluder relateret rolle i forespørgslen
                    .FirstOrDefault(u => u.Email == email); // Find brugeren med matchende email

                if (CurrentUser == null)
                {
                    // Hvis brugeren ikke findes i databasen
                    Console.WriteLine("Brugeren kunne ikke findes i databasen.");
                }
                else
                {
                    // Debugging: Udskriv information om den fundne bruger
                    Console.WriteLine($"Bruger fundet: {CurrentUser.FirstName} {CurrentUser.LastName}, {CurrentUser.Email}");
                }
            }
            else
            {
                // Hvis brugeren ikke er logget ind
                Console.WriteLine("Bruger er ikke logget ind.");
            }
        }
    }
}

