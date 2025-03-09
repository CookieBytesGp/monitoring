using App.Models.PageEditor;
using Microsoft.AspNetCore.Mvc;

namespace App.ViewComponents
{
    public class TopMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Guid pageId, string saveUrl, string finalSaveUrl)
        {
            var model = new TopMenuViewModel
            {
                PageId = pageId,
                SaveUrl = saveUrl,
                FinalSaveUrl = finalSaveUrl
            };
            return View(model);
        }
    }
    
}
