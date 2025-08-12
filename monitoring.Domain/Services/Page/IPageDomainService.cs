using FluentResults;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monitoring.Domain.Aggregates.Page;
using Domain.Aggregates.Page.ValueObjects;
namespace Monitoring.Domain.Services.Page
{
    /// <summary>
    /// Domain Service برای مدیریت منطق پیچیده صفحات
    /// </summary>
    public interface IPageDomainService
    {
        /// <summary>
        /// اعتبارسنجی ساختار صفحه
        /// </summary>
        /// <param name="page">صفحه مورد بررسی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Result ValidatePageStructure(Monitoring.Domain.Aggregates.Page.Page page);

        /// <summary>
        /// بهینه‌سازی ترتیب المنت‌های صفحه
        /// </summary>
        /// <param name="page">صفحه مورد نظر</param>
        /// <returns>صفحه بهینه‌سازی شده</returns>
        Result<Monitoring.Domain.Aggregates.Page.Page> OptimizeElementOrder(Monitoring.Domain.Aggregates.Page.Page page);

        /// <summary>
        /// بررسی تداخل المنت‌ها در صفحه
        /// </summary>
        /// <param name="page">صفحه مورد بررسی</param>
        /// <returns>لیست المنت‌های متداخل</returns>
        Result<List<BaseElement>> DetectOverlappingElements(Monitoring.Domain.Aggregates.Page.Page page);

        /// <summary>
        /// محاسبه ابعاد کل صفحه بر اساس المنت‌ها
        /// </summary>
        /// <param name="page">صفحه مورد نظر</param>
        /// <returns>ابعاد محاسبه شده</returns>
        Result<(int width, int height)> CalculatePageDimensions(Monitoring.Domain.Aggregates.Page.Page page);

        /// <summary>
        /// تولید پیش‌نمایش صفحه
        /// </summary>
        /// <param name="page">صفحه مورد نظر</param>
        /// <returns>URL پیش‌نمایش</returns>
        Task<Result<string>> GeneratePagePreview(Monitoring.Domain.Aggregates.Page.Page page);
    }
}
