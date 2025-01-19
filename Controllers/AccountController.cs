using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ZealandLokaleBooking.Data; // Namespace til ApplicationDbContext, der bruges til databasekommunikation
using ZealandLokaleBooking.Models; // Namespace til User-modellen, som repræsenterer brugere i systemet
using System.Linq; // Giver LINQ-metoder til databaseforespørgsler
using Microsoft.EntityFrameworkCore; // Giver funktioner til at inkludere relaterede data i databaseforespørgsler
using System; // Indeholder grundlæggende typer og funktioner
using System.Threading.Tasks; // Gør det muligt at arbejde med asynkrone operationer
using System.Collections.Generic; // Giver adgang til samlinger som List<T>

namespace ZealandLokaleBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context; // Felt til databasekontekst, der bruges til at interagere med databasen

        // Constructor til at initialisere AccountController med databasekontekst
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Returnerer Login.cshtml fra Views/Account, som viser loginformularen
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Finder en bruger i databasen baseret på email og adgangskode
            var user = _context.Users.Include(u => u.Role) // Inkluderer brugerens rolle i forespørgslen
                .FirstOrDefault(u => u.Email == email && u.Password == password);

            // Debug-log til at spore loginforsøg
            Console.WriteLine($"Login forsøg: Email = {email}, Rolle = {user?.Role?.RoleName}");

            if (user != null)
            {
                // Opretter claims, som bruges til at identificere brugeren i systemet
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email), // Email som brugerens navn
                    new Claim("role", user.Role.RoleName)  // Brugerens rolle
                };

                // Opretter en identitet og principal baseret på claims
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Logger brugeren ind med cookie-baseret autentificering
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Omdirigerer brugeren til den relevante dashboard-side baseret på deres rolle
                if (user.Role.RoleName == "Student")
                {
                    Console.WriteLine("Elev logget ind, omdirigerer til StudentDashboard.");
                    return RedirectToPage("/StudentDashboard");
                }
                else if (user.Role.RoleName == "Teacher")
                {
                    Console.WriteLine("Lærer logget ind, omdirigerer til TeacherDashboard.");
                    return RedirectToPage("/TeacherDashboard");
                }
            }

            // Returnerer en fejlmeddelelse, hvis login mislykkes
            Console.WriteLine("Login mislykkedes. Ugyldig email eller adgangskode.");
            ViewBag.ErrorMessage = "Forkert brugernavn eller adgangskode. Prøv igen.";
            return View();
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Logger brugeren ud ved at fjerne autentificeringscookien
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Omdirigerer brugeren til startsiden
            return RedirectToPage("/Index");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Returnerer Register.cshtml fra Views/Account, som viser registreringsformularen
            return View();
        }

        [HttpPost]
        public IActionResult Register(string firstName, string lastName, string email, string password, string phoneNumber)
        {
            // Bestemmer rolle-ID baseret på emaildomænet
            int roleId = email.Contains("edu.zealand.dk") ? 1 : 2; // 1 = Student, 2 = Teacher

            // Opretter en ny bruger baseret på inputdata
            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                PhoneNumber = phoneNumber,
                RoleId = roleId
            };

            // Tilføjer den nye bruger til databasen og gemmer ændringerne
            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Viser en succesmeddelelse og omdirigerer til startsiden
            TempData["SuccessMessage"] = "Din konto er blevet oprettet. Du kan nu logge ind.";
            return RedirectToPage("/Index");
        }
    }
}
