using bingo_Tech.IServices;
using Microsoft.AspNetCore.Mvc;

namespace bingo_Tech.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICallLogService _callLogService;

        public HomeController(ICallLogService callLogService)
        {
            _callLogService = callLogService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CallHistory()
        {
            var history = await _callLogService.GetCallHistoryAsync();
            return View(history);
        }
    }
}