using HPM_System.ApartmentService.Models;

namespace HPM_System.ApartmentService.Models
{
    public class ApartmentUserStatus
    {
        public int Id { get; set; }
        public int ApartmentUserId { get; set; }
        public ApartmentUser ApartmentUser { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; }
    }
}
