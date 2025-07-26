using HPM_System.ApartmentService.Models;

namespace HPM_System.ApartmentService.Models
{
    public class ApartmentUserStatus
    {
        public int Id { get; set; }

        // Ссылка на составной ключ ApartmentUser
        public int ApartmentId { get; set; }
        public int UserId { get; set; }
        public ApartmentUser ApartmentUser { get; set; }

        public int StatusId { get; set; }
        public Status Status { get; set; }
    }
}
