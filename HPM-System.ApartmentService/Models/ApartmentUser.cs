namespace HPM_System.ApartmentService.Models
{
    public class ApartmentUser
    {
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        // Доля собственности (например, 0.5 — 50%)
        public decimal Share { get; set; }
        // Статусы (владелец, жилец, прописан и т.д.)
        public ICollection<ApartmentUserStatus> Statuses { get; set; } = new List<ApartmentUserStatus>();
    }
}
