namespace HPM_System.ApartmentService.DTOs.HousesDTOs
{
    public class HouseOwnerDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<int> ApartmentNumbers { get; set; } = new List<int>();
    }
}