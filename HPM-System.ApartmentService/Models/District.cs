namespace HPM_System.ApartmentService.Models
{
    public class District
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public District? Parent { get; set; }
    }
}
