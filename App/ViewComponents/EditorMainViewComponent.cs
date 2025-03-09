using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using DTOs.Pagebuilder;
using App.Models.PageEditor;

namespace App.ViewComponents
{
    public class EditorMainViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(dynamic data)
        {
            var viewModel = new EditorMainViewModel
            {
                PageId = data.pageId,
                Elements = data.elements as List<BaseElementDTO> ?? new List<BaseElementDTO>()
            };

            return View(viewModel);
        }
    }
}
