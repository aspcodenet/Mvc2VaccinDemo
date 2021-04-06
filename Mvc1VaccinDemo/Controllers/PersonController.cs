using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mvc1VaccinDemo.Data;
using Mvc1VaccinDemo.Services.Krisinformation;
using Mvc1VaccinDemo.Services.PersonGenerator;
using Mvc1VaccinDemo.ViewModels;

namespace Mvc1VaccinDemo.Controllers
{
    public class PersonController : BaseController
    {
        private readonly IPersonGeneratorService _personGeneratorService;

        public PersonController(ApplicationDbContext dbContext, IKrisInfoService krisInfoService,
            IPersonGeneratorService personGeneratorService)
            : base(dbContext, krisInfoService)
        {
            _personGeneratorService = personGeneratorService;
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult BulkAdd()
        {
            var viewModel = new PersonBulkAddViewModel();
            viewModel.Antal = 0;
            return View(viewModel);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult BulkAdd(PersonBulkAddViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                for (int i = 0; i < viewModel.Antal; i++)
                {
                    var generatedPerson = _personGeneratorService.GenerateFakePerson();
                    _dbContext.Personer.Add(new Person
                    {
                        City = generatedPerson.City,
                        EmailAddress = generatedPerson.EmailAddress,
                        Name = generatedPerson.Name,
                        PersonalNumber = generatedPerson.PersonalNumber,
                        PostalCode = generatedPerson.PostalCode,
                        StreetAddress = generatedPerson.StreetAddress
                    });
                }

                _dbContext.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(viewModel);
        }


        // GET
        [Authorize(Roles = "Admin, Nurse")]
        public IActionResult Index(string q)
        {
           var viewModel = new PersonIndexViewModel();

            viewModel.Personer = _dbContext.Personer
                .Where(r => q == null || r.Name.Contains(q) || r.PersonalNumber.Contains(q))
                .Select(person => new PersonViewModel
                {
                    Id = person.Id,
                    Name = person.Name,
                    EmailAddress = person.EmailAddress,
                    PersonalNumber = person.PersonalNumber
                }).ToList();

            return View(viewModel);
        }


        [Authorize(Roles = "Admin, Nurse")]
        public IActionResult _SelectPerson(int id)
        {
            var viewModel = new SelectPersonViewModel();
            var p = _dbContext.Personer.Include(p=>p.VaccineringsFas).First(r => r.Id == id);

            viewModel.Name = p.Name;
            viewModel.PersonalNumber = p.PersonalNumber;
            viewModel.NextVaccinDate = p.PreliminaryNextVaccinDate.HasValue
                ? p.PreliminaryNextVaccinDate.Value.ToString("yyyy-MM-dd")
                : "";
            viewModel.VaccineringsFas = p.VaccineringsFas.Name;
            return View(viewModel);
        }



        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int Id)
        {
            var viewModel = new PersonEditViewModel();

            var dbPerson = _dbContext.Personer.Include(r=>r.VaccineringsFas).First(r => r.Id == Id);

            viewModel.Id = dbPerson.Id;
            viewModel.Name = dbPerson.Name;
            viewModel.EmailAddress = dbPerson.EmailAddress;
            viewModel.PersonalNumber = dbPerson.PersonalNumber;
            viewModel.City = dbPerson.City;
            viewModel.PostalCode = dbPerson.PostalCode;
            viewModel.PreliminaryNextVaccinDate = dbPerson.PreliminaryNextVaccinDate;
            viewModel.StreetAddress = dbPerson.StreetAddress;
            
            viewModel.SelectedVaccineringsFasId = dbPerson.VaccineringsFas.Id;
            viewModel.AllaVaccineringsFaser = GetAllVaccineringsFaserAsListItems(); 

            return View(viewModel);
        }

        private List<SelectListItem> GetAllVaccineringsFaserAsListItems()
        {
            var list = new List<SelectListItem>();
            list.AddRange(_dbContext.VaccineringsFaser.Select(r=>new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }));
            return list;

        }
    }
}