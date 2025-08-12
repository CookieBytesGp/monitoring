using FluentResults;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monitoring.Domain.Services
{
    /// <summary>
    /// Domain Service مشترک برای منطق عمومی سیستم
    /// </summary>
    public interface ISharedDomainService
    {
        /// <summary>
        /// اعتبارسنجی URL
        /// </summary>
        /// <param name="url">URL مورد بررسی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Result ValidateUrl(string url);

        /// <summary>
        /// تولید ID منحصر به فرد
        /// </summary>
        /// <param name="prefix">پیشوند اختیاری</param>
        /// <returns>ID تولید شده</returns>
        string GenerateUniqueId(string prefix = null);

        /// <summary>
        /// اعتبارسنجی آدرس IP
        /// </summary>
        /// <param name="ipAddress">آدرس IP</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Result ValidateIpAddress(string ipAddress);

        /// <summary>
        /// اعتبارسنجی پورت شبکه
        /// </summary>
        /// <param name="port">شماره پورت</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Result ValidateNetworkPort(int port);

        /// <summary>
        /// تبدیل حجم فایل به فرمت قابل خواندن
        /// </summary>
        /// <param name="sizeInBytes">حجم به بایت</param>
        /// <returns>حجم به فرمت قابل خواندن</returns>
        string FormatFileSize(long sizeInBytes);

        /// <summary>
        /// تبدیل مدت زمان به فرمت قابل خواندن
        /// </summary>
        /// <param name="timeSpan">مدت زمان</param>
        /// <returns>مدت زمان به فرمت قابل خواندن</returns>
        string FormatDuration(TimeSpan timeSpan);

        /// <summary>
        /// محاسبه هش فایل
        /// </summary>
        /// <param name="fileContent">محتوای فایل</param>
        /// <returns>هش محاسبه شده</returns>
        Task<Result<string>> CalculateFileHashAsync(byte[] fileContent);

        /// <summary>
        /// اعتبارسنجی نام فایل
        /// </summary>
        /// <param name="fileName">نام فایل</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Result ValidateFileName(string fileName);

        /// <summary>
        /// تولید Slug از متن
        /// </summary>
        /// <param name="text">متن ورودی</param>
        /// <returns>Slug تولید شده</returns>
        string GenerateSlug(string text);
    }
}
