namespace HPM_System.ApartmentService.DTOs
{
    public class ApartmentStatisticsDto
    {
        public long ApartmentId { get; set; }
        public int TotalUsers { get; set; }
        public int OwnersCount { get; set; }
        public int TenantsCount { get; set; }
        public int RegisteredCount { get; set; }
        public decimal TotalOwnershipShare { get; set; }
    }
}
