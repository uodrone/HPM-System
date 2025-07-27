using System.Text.Json.Serialization;

namespace HPM_System.ApartmentService.Models
{
    public class Apartment
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int NumbersOfRooms { get; set; }
        public decimal ResidentialArea { get; set; }
        public decimal TotalArea { get; set; }
        public int? Floor {  get; set; }
        public int HouseId { get; set; }
        [JsonIgnore]
        public ICollection<ApartmentUser> Users { get; set; } = new List<ApartmentUser>();
    }
}
