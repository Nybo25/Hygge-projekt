using Microsoft.AspNetCore.Mvc.RazorPages;
using ZealandLokaleBooking.Data;
using ZealandLokaleBooking.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace Zealand_Lokale_Booking.Pages
{
    public class StudentDashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public StudentDashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ZealandLokaleBooking.Models.Booking> Bookings { get; set; }
        public List<Notification> Notifications { get; set; }

        public void OnGet()
        {
            var userEmail = User.Identity.Name;

            // Hent brugerens ID baseret på email
            var userId = _context.Users
                .Where(u => u.Email == userEmail)
                .Select(u => u.UserId)
                .FirstOrDefault();

            if (userId != 0)
            {
                // Hent brugerens aktive bookinger
                Bookings = _context.Bookings
                    .Where(b => b.UserId == userId && b.IsActive)
                    .Include(b => b.Room)
                    .ToList();

                // Hent brugerens nye notifikationer
                Notifications = _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                // Marker notifikationer som læst
                foreach (var notification in Notifications)
                {
                    notification.IsRead = true;
                }

                _context.SaveChanges();
            }
            else
            {
                Bookings = new List<ZealandLokaleBooking.Models.Booking>();
                Notifications = new List<Notification>();
            }
        }
    }
}