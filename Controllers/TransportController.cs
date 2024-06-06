using BIA601_HW.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace BIA601_HW.Controllers
{
    public class TransportController : Controller
    {
        private readonly OptimizationService _optimizationService;

        public TransportController(OptimizationService optimizationService)
        {
            _optimizationService = optimizationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new TransportInputViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(TransportInputViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = _optimizationService.ProcessTransportData(model);
                ViewBag.Result = result;
                return View("Result", model);
            }

            return View(model);
        }
    }
}
