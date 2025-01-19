using Microsoft.AspNetCore.Mvc;
using ZealandLokaleBooking.Data;
using System; // Tilføj dette for at få adgang til DateTime
using System.Linq; // Tilføj dette for at få adgang til ToList() og LINQ

namespace ZealandLokaleBooking.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var rooms = _context.Rooms.ToList();
            return Ok(rooms); // Returnerer en liste over lokaler i JSON-format
        }
    }
}

