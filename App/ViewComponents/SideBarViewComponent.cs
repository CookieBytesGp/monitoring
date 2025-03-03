using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using App.Services;

namespace App.ViewComponents
{
    public class SideBarViewComponent : ViewComponent
    {
        private readonly ToolService _toolService;

        public SideBarViewComponent(ToolService toolService)
        {
            _toolService = toolService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tools = await _toolService.GetToolsAsync();
            return View(tools);
        }
    }
}
