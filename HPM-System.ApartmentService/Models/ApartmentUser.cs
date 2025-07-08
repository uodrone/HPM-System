namespace HPM_System.ApartmentService.Models
{
    public class ApartmentUser
    {
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
