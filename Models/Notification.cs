using System;

namespace ZealandLokaleBooking.Models
{
    public class Notification
    {
        public int Id { get; set; } // Unik ID for notifikationen
        public int UserId { get; set; } // Brugeren, der skal modtage notifikationen
        public string Message { get; set; } = null!; // Beskeden
        public bool IsRead { get; set; } = false; // Om beskeden er blevet læst
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Oprettelsesdato
    }
}