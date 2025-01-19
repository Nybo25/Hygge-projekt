using System;

namespace ZealandLokaleBooking.Models
{
    public class Booking
    {
        public int Id { get; set; } // Unik ID for booking
        public int RoomId { get; set; } // ID for det bookede lokale
        public int UserId { get; set; } // ID for brugeren, der lavede bookingen
        public DateTime StartTime { get; set; } // Starttid for bookingen
        public DateTime EndTime { get; set; } // Sluttid for bookingen

        // Navigation properties
        public Room Room { get; set; } = null!; // Det bookede lokale
        public User User { get; set; } = null!; // Den studerende, der lavede bookingen

        // Status og flags
        public bool IsDeleted { get; set; } = false; // Marker som slettet
        public bool IsActive { get; set; } = true;   // Marker som aktiv
        public string Status { get; set; } = "Active"; // Standardstatus: Aktiv

        // Annulleringsinformation
        public int? CancelledBy { get; set; } // ID på den person, der annullerede bookingen
        public DateTime? CancelledDate { get; set; } // Dato og tid for annulleringen

        // Metode til at annullere booking
        public void CancelBooking(int cancelledByUserId)
        {
            Status = "Cancelled";
            IsActive = false;
            CancelledBy = cancelledByUserId;
            CancelledDate = DateTime.Now; // Sætter annulleringsdato til nuværende tidspunkt
        }
    }
}