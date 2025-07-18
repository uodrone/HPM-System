﻿using Microsoft.AspNetCore.Identity;

namespace HPM_System.UserService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Patronymic { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int? Age { get; set; }
        // Список автомобилей
        public ICollection<Car> Cars { get; set; } = new List<Car>();
        
    }
}
