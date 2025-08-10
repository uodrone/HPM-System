using System.Text.Json.Serialization;

namespace HPM_System.ApartmentService.Models
{
    public class ApartmentUser
    {
        public long ApartmentId { get; set; }
        [JsonIgnore]
        public Apartment Apartment { get; set; }
        public Guid UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        // Доля собственности (например, 0.5 — 50%)
        public decimal Share { get; set; }
        // Статусы (владелец, жилец, прописан и т.д.)
        [JsonIgnore]
        public ICollection<ApartmentUserStatus> Statuses { get; set; } = new List<ApartmentUserStatus>();
    }
}
