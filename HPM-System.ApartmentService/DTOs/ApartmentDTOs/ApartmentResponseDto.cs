namespace DTOs.ApartmentDTOs
{
    public class ApartmentResponseDto
    {
        public long Id { get; set; }
        public int Number { get; set; }
        public int NumbersOfRooms { get; set; }
        public decimal ResidentialArea { get; set; }
        public decimal TotalArea { get; set; }
        public int? Floor { get; set; }
        public int HouseId { get; set; }
        public List<ApartmentUserResponseDto> Users { get; set; } = new List<ApartmentUserResponseDto>();
    }
}