namespace HPM_System.Models
{
    public class Community
    {
        public int Id { get; set; }
        public string Name { get; set; } // Например, адрес дома
        public int NumberOfUnits { get; set; } // Количество квартир или домов
        public decimal TotalArea { get; set; } // Общая площадь территории
        public int ApartmentsCount { get; set; }

        // Только для многоквартирных домов
        public int? Floors { get; set; }
        public int? Entrances { get; set; }        
        public decimal? ResidentialArea { get; set; }
        public bool HasGas { get; set; }
        public bool HasElevator { get; set; }

        // Главный в сообществе
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
