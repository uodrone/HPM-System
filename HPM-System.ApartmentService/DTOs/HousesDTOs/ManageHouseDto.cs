using System.ComponentModel.DataAnnotations;

namespace HPM_System.ApartmentService.DTOs.HousesDTOs
{
    public class ManageHouseDto
    {
        [Required(ErrorMessage = "Город обязателен")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Улица обязательна")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Номер дома обязателен")]
        public string Number { get; set; } = string.Empty;

        public int Entrances { get; set; }

        public int Floors { get; set; }

        public bool HasGas { get; set; }

        public bool HasElectricity { get; set; }

        public bool HasElevator { get; set; }

        public string? PostIndex { get; set; }

        public double? ApartmentsArea { get; set; }

        public double? TotalArea { get; set; }

        public double? LandArea { get; set; }

        public bool IsApartmentBuilding { get; set; } = true;
    }
}
