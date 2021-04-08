using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        public IActionResult Index(string q, string sortField, string sortOrder, int page = 1)
        {
           var viewModel = new PersonIndexViewModel();

           var query = _dbContext.Personer
               .Where(r => q == null || r.Name.Contains(q) || r.PersonalNumber.Contains(q));


           //ANTAL POSTER SOM MATCHAR FILTRET
           int totalRowCount = query.Count();

            if (string.IsNullOrEmpty(sortField))
               sortField = "Namn";
           if (string.IsNullOrEmpty(sortOrder))
               sortOrder = "asc";

           if (sortField == "Namn")
           {
               if(sortOrder == "asc") 
                   query = query.OrderBy(y => y.Name);
               else
                   query = query.OrderByDescending(y => y.Name);
            }

           if (sortField == "Email")
           {
               if (sortOrder == "asc")
                    query = query.OrderBy(y => y.EmailAddress);
               else
                   query = query.OrderByDescending(y => y.EmailAddress);
            }

           if (sortField == "Personnummer")
           {
               if (sortOrder == "asc")
                    query = query.OrderBy(y => y.PersonalNumber);
               else
                   query = query.OrderByDescending(y => y.PersonalNumber);

            }

           int pageSize = 10;

           var pageCount = (double)totalRowCount / pageSize;
           viewModel.TotalPages = (int)Math.Ceiling(pageCount);




            //Skip - hoppa över så många
            //Take - sen ta så många

            int howManyRecordsToSkip = (page - 1) * pageSize;  // Sida 1 ->  0

            query = query.Skip(howManyRecordsToSkip).Take(pageSize);


            viewModel.Personer = query
                .Select(person => new PersonViewModel
                {
                    Id = person.Id,
                    Name = person.Name,
                    EmailAddress = person.EmailAddress,
                    PersonalNumber = person.PersonalNumber
                }).ToList();

            viewModel.q = q;
            viewModel.SortOrder = sortOrder;
            viewModel.SortField = sortField;
            viewModel.Page = page;
            viewModel.OppositeSortOrder = sortOrder == "asc" ? "desc" : "asc";
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