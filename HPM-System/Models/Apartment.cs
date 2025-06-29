namespace HPM_System.Models
{
    public class Apartment
    {
        public int Id { get; set; }
        public decimal Area { get; set; }
        public int? Floor { get; set; }
        public int? NumberOfRooms { get; set; }
        public string Address { get; set; }
        // Владелец квартиры
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        // ID сообщества, к которому относится квартира
        public int CommunityId { get; set; }
        public Community Community { get; set; }
    }
}
