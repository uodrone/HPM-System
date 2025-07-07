namespace HPM_System.UserService.Models
{
    public class Car
    {
        public int Id { get; set; }

        public string? Mark { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string Number { get; set; }

        // Владелец автомобиля
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
