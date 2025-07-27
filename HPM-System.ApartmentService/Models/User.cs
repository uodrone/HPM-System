﻿using System.Text.Json.Serialization;

namespace HPM_System.ApartmentService.Models
{
    public class User
    {
        public int Id { get; set; }
        [JsonIgnore]
        public ICollection<ApartmentUser> Apartments { get; set; } = new List<ApartmentUser>();
    }
}