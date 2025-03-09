using Microsoft.AspNetCore.Mvc;
using DTOs.Pagebuilder;

namespace App.ViewComponents
{
    public class EditingElementViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(BaseElementDTO model)
        {
            return View(model);
        }
    }
}
