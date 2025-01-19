using System; 
using System.Collections.Generic;

namespace ZealandLokaleBooking.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public Role Role { get; set; } = null!;
        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}