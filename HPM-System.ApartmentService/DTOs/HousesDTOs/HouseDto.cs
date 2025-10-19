namespace HPM_System.ApartmentService.DTOs.HousesDTOs
{
    public class HouseDto
    {
        public long Id { get; set; }
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public int Entrances { get; set; }
        public int Floors { get; set; }
        public bool HasGas { get; set; }
        public bool HasElectricity { get; set; }
        public bool HasElevator { get; set; }
        public Guid? HeadId { get; set; }
        public string? PostIndex { get; set; }
        public double? ApartmentsArea { get; set; }
        public double? TotalArea { get; set; }
        public double? LandArea { get; set; }
        public bool IsApartmentBuilding { get; set; }
        public int builtYear { get; set; }
    }
}
