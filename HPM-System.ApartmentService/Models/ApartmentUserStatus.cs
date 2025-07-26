using HPM_System.ApartmentService.Models;
using System.Text.Json.Serialization;

namespace HPM_System.ApartmentService.Models
{
    public class ApartmentUserStatus
    {
        public int Id { get; set; }

        // Ссылка на составной ключ ApartmentUser
        public int ApartmentId { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public ApartmentUser ApartmentUser { get; set; }

        public int StatusId { get; set; }
        [JsonIgnore]
        public Status Status { get; set; }
    }
}
