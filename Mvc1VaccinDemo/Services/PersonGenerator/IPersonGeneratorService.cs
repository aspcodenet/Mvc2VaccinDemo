using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Mvc1VaccinDemo.Services.PersonGenerator
{

    public class GeneratedPerson
    {
        public string Name { get; set; }
        public string PersonalNumber { get; set; }
        public string EmailAddress { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public int PostalCode { get; set; }

    }

    public class NameFakePerson
    {
        public string Name { get; set; }
        public string Birth_data { get; set; }
        public string email_u { get; set; }
        public string email_d { get; set; }
        public string Address { get; set; }



    }

    public interface IPersonGeneratorService
    {
        GeneratedPerson GenerateFakePerson();
    }

    class PersonGeneratorService : IPersonGeneratorService
    {
        public GeneratedPerson GenerateFakePerson()
        {
            var client = new HttpClient();
            string result = client.GetStringAsync("https://api.namefake.com/swedish-sweden/random/").Result;
            var p = JsonConvert.DeserializeObject<NameFakePerson>(result);
            return new GeneratedPerson
            {
                Name = p.Name,
                PersonalNumber = GeneratePersonalNumber(p.Birth_data),
                EmailAddress = p.email_u + "@" + p.email_d,
                City = p.Address.Split('\n')[1].Substring(7),
                PostalCode = Convert.ToInt32(p.Address.Split('\n')[1].Substring(0, 6).Replace(" ", "")),
                StreetAddress = p.Address.Split('\n')[0]
            };
        }

        private string GeneratePersonalNumber(string Birth_data)
        {
            //1972-08-03
            return Birth_data.Substring(0, 4) + Birth_data.Substring(5, 2) + Birth_data.Substring(8, 2) +
                   "-1111";
        }
    }
}