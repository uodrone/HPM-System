namespace HPM_System.Models
{
    public class Flat
    {
        public int ID { get; set; }
        public double Area { get; set; }
        public int Resident { get; set; }
        public byte Entrance { get; set; }
        public int Floor { get; set; }

        public Flat(int iD, double area, int resident, byte entrance, int floor)
        {
            ID = iD;
            Area = area;
            Resident = resident;
            Entrance = entrance;
            Floor = floor;
        }
    }
}

