using System.Collections.Generic;

namespace HPM_System.Models
{
    public class Building
    {
        public int Id { get;}
        public BuildingType Type { get;}
        public int NumberBuilding { get;}
        public double AreaBuilding { get;} // площадь самого дома 
        public double AreaLiving { get;} // жилая площадь дома
        public double AreaLand { get;} // площадь придомовой территории 
        public int Floors { get;} // этажность 
        public int Entrances { get;}  // количество подъездов
        public bool HasGas { get;}
        public bool HasElectricity { get;}
        public bool HasElevator { get;}
        public ApplicationUser Head { get;} // старший по дому
        public ICollection<Apartment> ApartmentsList { get;}

        public Building(
            int id,
            BuildingType type,
            int numberBuilding,
            double areaBuilding,
            double areaLiving,
            double areaLand,
            int floors,
            int entrances,
            bool hasGas,
            bool hasElectricity,
            bool hasElevator,
            ApplicationUser head,
            ICollection<Apartment> apartmentsList)
        {
            Id = id;
            Type = type;
            NumberBuilding = numberBuilding;
            AreaBuilding = areaBuilding;
            AreaLiving = areaBuilding;
            AreaLand = areaLand;
            Floors = floors;
            Entrances = entrances;
            HasGas = hasGas;
            HasElectricity = hasElectricity;
            HasElevator = hasElevator;
            Head = head;
            ApartmentsList = apartmentsList;
        }
        public static (Building Building, string Error) Create(
            int id,
            BuildingType type,
            int numberBuilding,
            double areaBuilding,
            double areaLiving,
            double areaLand,
            int floors,
            int entrances,
            bool hasGas,
            bool hasElectricity,
            bool hasElevator,
            ApplicationUser head,
            ICollection<Apartment> apartmentsList)
        {
            var error = string.Empty;
            // добавить валидацию
            var building = new Building(id, type, numberBuilding, areaBuilding, areaLiving, 
                areaLand, floors, entrances, hasGas, hasElectricity, hasElevator, head, apartmentsList = new List<Apartment>());
            return (building, error);
        }
        
    }

}
