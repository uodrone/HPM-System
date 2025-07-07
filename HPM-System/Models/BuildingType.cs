namespace HPM_System.Models
{
    public class BuildingType
    {
        public int Id { get; set; }
        public string Type { get; set; } // многоквартирный - block; индивидуальный - house
        public string Description { get; set; }
    }
}
