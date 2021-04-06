using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mvc1VaccinDemo.Data;
using Mvc1VaccinDemo.Services.Krisinformation;
using Mvc1VaccinDemo.ViewModels;

namespace Mvc1VaccinDemo.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly IKrisInfoService _krisInfoService;

        public BaseController(ApplicationDbContext dbContext, IKrisInfoService krisInfoService)
        {
            _dbContext = dbContext;
            _krisInfoService = krisInfoService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["SenasteArtiklar"] = _krisInfoService.GetAllKrisInformation().Take(5).ToList();

            ViewData["AllaFaser"] = _dbContext.VaccineringsFaser
                .Select(dbVacc => new FaserIndexViewModel.FasViewModel
                {
                    Id = dbVacc.Id,
                    Name = dbVacc.Name
                }).ToList();

            base.OnActionExecuting(context);
        }

    }
}