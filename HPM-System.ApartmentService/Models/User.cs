namespace HPM_System.ApartmentService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }

        public ICollection<ApartmentUser> Apartments { get; set; } = new List<ApartmentUser>();
    }
}