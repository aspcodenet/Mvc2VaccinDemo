using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Mvc1VaccinDemo.Services.Krisinformation;
using Mvc1VaccinDemo.ViewModels;

namespace Mvc1VaccinDemo.Controllers
{
    public class HejController : Controller
    {
        private readonly IKrisInfoService _service;

        public HejController(IKrisInfoService service)
        {
            _service = service;
        }



        public class Highscore
        {
            public string Namn { get; set; }
            public int Points { get; set; }
        }
        public IActionResult GetHighscore()
        {
            var l = new List<Highscore>();
            l.Add(new Highscore{Namn="Stefan", Points = 123});
            l.Add(new Highscore { Namn = "Oliver", Points = 124 });
            l.Add(new Highscore { Namn = "Josefine", Points = 133 });
            return Json(l);
        }


        public IActionResult Show(string id)
        {
            if (id == "12345")
            {
                throw new InvalidDataException("aswraasdf");
            }
            var viewModel = new ArticleViewModel();
            var db = _service.GetKrisInformation(id); //"123111"

            viewModel.Text = db.Text;
            viewModel.Displaymode = db.Displaymode;
            viewModel.Text = db.Text;
            viewModel.Emergency = db.Emergency;
            viewModel.Id = db.Id;
            viewModel.ImageUrl = db.ImageUrl;
            viewModel.LinkUrl = db.LinkUrl;
            viewModel.Title = db.Title;

            return View(viewModel);
        }

    }
}