namespace ZealandLokaleBooking.Models
{
    public class Room
    {
        // Unikt ID for lokalet
        public int RoomId { get; set; }

        // Navn på lokalet, f.eks. "A101"
        public string RoomName { get; set; } = null!;

        // Kapacitet, dvs. hvor mange personer lokalet kan rumme
        public int Capacity { get; set; }

        // Type af lokale, f.eks. "Klasseværelse", "Mødelokale", etc.
        public string RoomType { get; set; } = null!;

        // Angiver, om lokalet er booket eller ej
        public bool IsBooked { get; set; }

        // ID på brugeren, der har booket lokalet (hvis det er booket)
        public int? BookedByUserId { get; set; }

        // Navigation property til den bruger, der har booket lokalet (hvis det er booket)
        public User? BookedByUser { get; set; }

        // Angiver, om lokalet er et auditorium (true) eller ej (false)
        public bool IsAuditorium { get; set; }

        // Navigation property til de bookinger, der er knyttet til dette lokale
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}