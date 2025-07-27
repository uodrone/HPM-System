namespace HPM_System.ApartmentService.DTOs
{
    public class ApartmentListResponseDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int NumbersOfRooms { get; set; }
        public decimal ResidentialArea { get; set; }
        public decimal TotalArea { get; set; }
        public int? Floor { get; set; }
        public int HouseId { get; set; }
        public int UsersCount { get; set; }
        public int OwnersCount { get; set; }
    }
}