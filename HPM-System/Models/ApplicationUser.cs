using Microsoft.AspNetCore.Identity;

namespace HPM_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Patronymic { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public int? Age { get; set; }
        public int PersonRoleId { get; set; }
        public PersonRole Role { get; set; }

        public ICollection<Community> Communities { get; set; } = new List<Community>();

        // Список квартир
        public ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();

        // Список автомобилей
        public ICollection<Car> Cars { get; set; } = new List<Car>();
        
    }
}
