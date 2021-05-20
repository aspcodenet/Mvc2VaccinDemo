using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Mvc1VaccinDemo.Data;
using Mvc1VaccinDemo.Services.Krisinformation;
using Mvc1VaccinDemo.ViewModels;
using SharedThings;
using SharedThings.Data;

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

           var searchClient = new SearchClient(new Uri("https://stefanpersonsearch.search.windows.net"),
               "personerna", new AzureKeyCredential("F4B85AB85B8662E8B66EACDF1B98E581"));


           int pageSize = 10;

            var searchOptions = new SearchOptions
           {
               OrderBy = { "City desc" }, // Build from sortField och sortOrder
               Skip = (page-1)*10,
               Size = pageSize,
               IncludeTotalCount = true
           };


           var searchResult = searchClient.Search<PersonInAzure>(q, searchOptions);


            //var query = _dbContext.Personer
            //   .Where(r => q == null || r.Name.Contains(q) || r.PersonalNumber.Contains(q));


           //ANTAL POSTER SOM MATCHAR FILTRET
           long totalRowCount = searchResult.Value.TotalCount.Value;

           // if (string.IsNullOrEmpty(sortField))
           //    sortField = "Namn";
           //if (string.IsNullOrEmpty(sortOrder))
           //    sortOrder = "asc";

           //if (sortField == "Namn")
           //{
           //    if(sortOrder == "asc") 
           //        query = query.OrderBy(y => y.Name);
           //    else
           //        query = query.OrderByDescending(y => y.Name);
           // }

           //if (sortField == "Email")
           //{
           //    if (sortOrder == "asc")
           //         query = query.OrderBy(y => y.EmailAddress);
           //    else
           //        query = query.OrderByDescending(y => y.EmailAddress);
           // }

           //if (sortField == "Personnummer")
           //{
           //    if (sortOrder == "asc")
           //         query = query.OrderBy(y => y.PersonalNumber);
           //    else
           //        query = query.OrderByDescending(y => y.PersonalNumber);

           // }


           var pageCount = (double)totalRowCount / pageSize;
           viewModel.TotalPages = (int)Math.Ceiling(pageCount);




            //Skip - hoppa över så många
            //Take - sen ta så många
            var listOfIds = searchResult.Value.GetResults().Select(r => Convert.ToInt32(r.Document.Id)).ToList();

            // 12, 56,3,45,678

            //viewModel.Personer = _dbContext.Personer.Where(q=>listOfIds.Contains(q.Id))
            //    .Select(person => new PersonViewModel
            //    {
            //        Id = person.Id,
            //        Name = person.Name,
            //        EmailAddress = person.EmailAddress,
            //        PersonalNumber = person.PersonalNumber
            //    }).ToList();

            //viewModel.Personer = query
            //    .Select(person => new PersonViewModel
            //    {
            //        Id = person.Id,
            //        Name = person.Name,
            //        EmailAddress = person.EmailAddress,
            //        PersonalNumber = person.PersonalNumber
            //    }).ToList();

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
            viewModel.VaccineringsFas = p.VaccineringsFas?.Name;
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
            viewModel.Description = dbPerson.Description;


            if (dbPerson.VaccineringsFas != null)
                viewModel.SelectedVaccineringsFasId = dbPerson.VaccineringsFas.Id;
            viewModel.AllaVaccineringsFaser = GetAllVaccineringsFaserAsListItems(); 

            return View(viewModel);
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int Id, PersonEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dbPerson = _dbContext.Personer.Include(r => r.VaccineringsFas).First(r => r.Id == Id);

                dbPerson.Name = model.Name;
                dbPerson.EmailAddress = model.EmailAddress;
                dbPerson.PersonalNumber = model.PersonalNumber;
                dbPerson.City = model.City;
                dbPerson.PostalCode = model.PostalCode;
                dbPerson.PreliminaryNextVaccinDate = model.PreliminaryNextVaccinDate;
                dbPerson.StreetAddress = model.StreetAddress;
                dbPerson.Description = model.Description;

                UpdateSearchIndex(dbPerson);

                _dbContext.SaveChanges();
                return RedirectToAction("Index");

            }
            model.AllaVaccineringsFaser = GetAllVaccineringsFaserAsListItems();
            return View(model);

        }

        private void UpdateSearchIndex(Person person)
        {
            var searchClient = new SearchClient(new Uri("https://stefanpersonsearch.search.windows.net"),
                "personerna", new AzureKeyCredential("F4B85AB85B8662E8B66EACDF1B98E581"));

            var batch = new IndexDocumentsBatch<PersonInAzure>();
            var personInAzure = new PersonInAzure
            {
                City = person.City,
                Description = person.Description,
                Id = person.Id.ToString(),
                Namn = person.Name,
                StreetAddress = person.StreetAddress
            };
            batch.Actions.Add(new IndexDocumentsAction<PersonInAzure>(IndexActionType.MergeOrUpload,
                personInAzure));

            IndexDocumentsResult result = searchClient.IndexDocuments(batch);
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