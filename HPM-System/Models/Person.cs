using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace HPM_System.Models
{
    public class Person
    {
        public int ID { get; set; }
        public byte Role { get; set; }
        public string LastName { get; set; }
        public string GivenName { get; set; }
        public string MiddleName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Phone { get; set; }
        public List<Community> CommunitiesList { get; set; }
        public List<Car> CarsList { get; set; }
        public List<Apartment> ApartmentsList { get; set; }

        public Person(
            int iD,
            byte role,
            string lastName,
            string givenName,
            string middleName,
            string phone,
            DateOnly dateOfBirth,
            List<Car> carsList,
            List<Apartment> apartmentsList
            )
        {
            ID = iD;
            Role = role;
            LastName = lastName;
            GivenName = givenName;
            MiddleName = middleName;
            Phone = phone;
            DateOfBirth = dateOfBirth;
            CarsList = carsList;
            ApartmentsList = apartmentsList;
        }
    }
}

