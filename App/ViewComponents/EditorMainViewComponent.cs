using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using DTOs.Pagebuilder;

namespace App.ViewComponents
{
    public class EditorMainViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<BaseElementDTO> model)
        {
            return View(model);
        }
    }
}
