namespace HPM_System.Models
{
    public class Building
    {
        public int Id { get; set; }
        public BuildingType Type { get; set; }
        public int NumberBuilding { get; set; } 
        public double AreaBuilding { get; set; } // площадь самого дома 
        public double AreaLiving { get; set; } // жилая площадь дома
        public double AreaLand { get; set; } // площадь придомовой территории 
        public int Floors { get; set; } // этажность 
        public int Entrances { get; set; }  // количество подъездов
        public bool HasGas { get; set; }
        public bool HasElectricity { get; set; }
        public bool HasElevator { get; set; } 
        public ApplicationUser Head { get; set; } // старший по дому
        public ICollection<Apartment> ApartmentsList { get; set; } =  new List<Apartment>();
    }
}
