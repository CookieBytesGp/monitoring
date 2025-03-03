using Microsoft.AspNetCore.Mvc;

namespace App.ViewComponents
{
    public class TopMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            // You can pass any model or data here
            return View();
        }
    }
}
