namespace HPM_System.Models
{
    public class Car
    {
        public int Id { get; set; }

        public string? Mark { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string Number { get; set; }

        // Владелец автомобиля
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
