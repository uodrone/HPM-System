namespace HPM_System.ApartmentService.Models
{
    public class House
    {
        long Id { get; set; }
        string City { get; set; }
        string Street { get; set; }
        string Number { get; set; }
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
        public bool IsApartmentBuilding { get; set; } = true;
        public ICollection<Apartment>? Apartments { get; set; }
        public ICollection<District>? Districts { get; set; }

    }
}
