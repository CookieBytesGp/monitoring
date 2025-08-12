using FluentResults;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monitoring.Domain.Aggregates.Tools;
using Domain.Aggregates.Tools.ValueObjects;

namespace Monitoring.Domain.Services.Tools
{
    /// <summary>
    /// Domain Service برای مدیریت منطق ابزارها و قالب‌ها
    /// </summary>
    public interface IToolsDomainService
    {
        /// <summary>
        /// اعتبارسنجی قالب HTML
        /// </summary>
        /// <param name="template">قالب مورد بررسی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Result ValidateTemplate(Template template);

        /// <summary>
        /// بررسی سازگاری ابزار با نوع المنت
        /// </summary>
        /// <param name="tool">ابزار مورد بررسی</param>
        /// <param name="elementType">نوع المنت</param>
        /// <returns>نتیجه بررسی سازگاری</returns>
        Result<bool> IsToolCompatibleWithElement(Tool tool, string elementType);

        /// <summary>
        /// ترکیب CSS های مختلف ابزار
        /// </summary>
        /// <param name="tool">ابزار مورد نظر</param>
        /// <param name="customCss">CSS سفارشی</param>
        /// <returns>CSS نهایی</returns>
        Result<string> MergeCssStyles(Tool tool, string customCss);

        /// <summary>
        /// تولید کد JavaScript نهایی برای ابزار
        /// </summary>
        /// <param name="tool">ابزار مورد نظر</param>
        /// <param name="customJs">JavaScript سفارشی</param>
        /// <returns>کد JavaScript نهایی</returns>
        Result<string> GenerateFinalJavaScript(Tool tool, string customJs);

        /// <summary>
        /// بررسی وابستگی‌های ابزار
        /// </summary>
        /// <param name="tool">ابزار مورد بررسی</param>
        /// <returns>لیست وابستگی‌ها</returns>
        Result<List<string>> GetToolDependencies(Tool tool);

        /// <summary>
        /// بهینه‌سازی Assets ابزار
        /// </summary>
        /// <param name="tool">ابزار مورد نظر</param>
        /// <returns>ابزار بهینه‌سازی شده</returns>
        Task<Result<Tool>> OptimizeToolAssets(Tool tool);
    }
}
