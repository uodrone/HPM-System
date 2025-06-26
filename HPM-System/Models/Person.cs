using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace HPM_System.Models
{
    public class Person
    {
        public int ID { get; set; }
        public PersonRole Role { get; set; }
        public string LastName { get; set; }
        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Phone { get; set; }
        public List<Community> CommunitiesList { get; set; }
        public List<Apartment> ApartmentsList { get; set; }
        public List<Car> CarsList { get; set; }
    }
}

