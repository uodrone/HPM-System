namespace HPM_System.ApartmentService.Models
{
    public class Apartment
    {
        public int Id { get; set; }
        public int CommunityId { get; set; }
        public int Number { get; set; }
        public int NumbersOfRooms { get; set; }
        public decimal ResidentialArea { get; set; }
        public decimal TotalArea { get; set; }
        public int? Floor {  get; set; }
        public List<int> UserId { get; set; }
    }
}
