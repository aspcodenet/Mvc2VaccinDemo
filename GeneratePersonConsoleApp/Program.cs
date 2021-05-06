using System;
using Microsoft.EntityFrameworkCore;
using SharedThings;
using SharedThings.Data;

namespace GeneratePersonConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=Mvc1VaccinDemo;Trusted_Connection=True;MultipleActiveResultSets=true");
            var dbContext = new ApplicationDbContext(optionsBuilder.Options);

            var service = new PersonGeneratorService();
            for (int i = 0; i < 20; i++)
            {
                var generatedPerson = service.GenerateFakePerson();
                dbContext.Personer.Add(new Person
                {
                    City = generatedPerson.City,
                    EmailAddress = generatedPerson.EmailAddress,
                    Name = generatedPerson.Name,
                    PersonalNumber = generatedPerson.PersonalNumber,
                    PostalCode = generatedPerson.PostalCode,
                    StreetAddress = generatedPerson.StreetAddress
                });

            }
            dbContext.SaveChanges();



        }
    }
}
