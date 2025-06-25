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
        //public List<Community> CommunitiesList { get; set; } TODO
        public List<string> CarsList { get; set; }
        public List<Flat> FlatsList { get; set; }

        public Person(
            int iD,
            byte role,
            string lastName,
            string givenName,
            string middleName,
            string phone,
            DateOnly dateOfBirth,
            List<string> carsList, 
            List<Flat> flatsList
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
            FlatsList = flatsList;
        }
    }
}

