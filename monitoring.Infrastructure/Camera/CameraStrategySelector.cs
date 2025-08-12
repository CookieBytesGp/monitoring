using FluentResults;
using Microsoft.Extensions.Logging;
using Monitoring.Domain.Aggregates.Camera;
using Monitoring.Domain.Services.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monitoring.Infrastructure.Camera
{
    /// <summary>
    /// سرویس هوشمند انتخاب استراتژی بر اساس تحلیل دوربین
    /// </summary>
    public class CameraStrategySelector
    {
        private readonly ILogger<CameraStrategySelector> _logger;
        
        public CameraStrategySelector(ILogger<CameraStrategySelector> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// تحلیل دوربین و پیشنهاد استراتژی‌های مناسب
        /// </summary>
        public async Task<Result<StrategyAnalysisResult>> AnalyzeCameraAsync(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                if (camera == null)
                    return Result.Fail<StrategyAnalysisResult>("Camera cannot be null");

                _logger.LogInformation("Analyzing camera {CameraName} for strategy selection", camera.Name);

                var analysis = new StrategyAnalysisResult
                {
                    CameraName = camera.Name,
                    CameraType = camera.Type?.Name,
                    AnalysisTimestamp = DateTime.UtcNow
                };

                // تحلیل بر اساس نوع دوربین
                AnalyzeCameraType(camera, analysis);

                // تحلیل بر اساس مکان دوربین
                AnalyzeCameraLocation(camera, analysis);

                // تحلیل بر اساس تنظیمات اتصال
                AnalyzeConnectionInfo(camera, analysis);

                // تحلیل بر اساس Configuration
                AnalyzeConfiguration(camera, analysis);

                // رتبه‌بندی نهایی استراتژی‌ها
                RankStrategies(analysis);

                _logger.LogInformation("Camera analysis completed for {CameraName}. Recommended strategies: {Strategies}",
                    camera.Name, string.Join(", ", analysis.RecommendedStrategies.Take(3)));

                await Task.CompletedTask;
                return Result.Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing camera {CameraName}", camera.Name);
                return Result.Fail<StrategyAnalysisResult>($"Camera analysis failed: {ex.Message}");
            }
        }

        /// <summary>
        /// انتخاب خودکار استراتژی بر اساس URL یا اطلاعات دوربین
        /// </summary>
        public Result<List<string>> AutoSelectStrategies(Monitoring.Domain.Aggregates.Camera.Camera camera)
        {
            try
            {
                var recommendedStrategies = new List<string>();

                // تحلیل URL اتصال
                if (camera.ConnectionInfo?.StreamUrl != null)
                {
                    var url = camera.ConnectionInfo.StreamUrl.ToLower();

                    if (url.StartsWith("rtsp://"))
                        recommendedStrategies.Add("RTSP");
                    else if (url.StartsWith("http://") || url.StartsWith("https://"))
                        recommendedStrategies.Add("HTTP");
                    else if (url.StartsWith("usb://") || url.Contains("/dev/video"))
                        recommendedStrategies.Add("USB");
                }

                // تحلیل نوع دوربین
                if (camera.Type?.Name != null)
                {
                    var typeName = camera.Type.Name.ToLower();

                    if (typeName.Contains("onvif"))
                        recommendedStrategies.AddRange(new[] { "ONVIF", "RTSP", "HTTP" });
                    else if (typeName.Contains("ip") || typeName.Contains("network"))
                    {
                        recommendedStrategies.AddRange(new[] { "ONVIF", "RTSP", "HTTP" });
                    }
                    else if (typeName.Contains("usb") || typeName.Contains("webcam"))
                    {
                        recommendedStrategies.Add("USB");
                    }
                    else if (typeName.Contains("hikvision"))
                    {
                        recommendedStrategies.AddRange(new[] { "HIKVISION_SDK", "RTSP", "ONVIF" });
                    }
                    else if (typeName.Contains("dahua"))
                    {
                        recommendedStrategies.AddRange(new[] { "DAHUA_SDK", "RTSP", "ONVIF" });
                    }
                }

                // تحلیل پورت
                if (camera.Location?.Value != null)
                {
                    var address = camera.Location.Value.ToLower();
                    
                    if (address.Contains(":554"))
                        recommendedStrategies.Add("RTSP");
                    else if (address.Contains(":80") || address.Contains(":8080"))
                        recommendedStrategies.Add("HTTP");
                    else if (address.Contains(":3702"))
                        recommendedStrategies.Add("ONVIF");
                }

                // حذف تکراری و اولویت‌بندی
                var uniqueStrategies = recommendedStrategies.Distinct().ToList();
                
                // اگر هیچ استراتژی خاصی پیدا نشد، همه را اضافه کن
                if (!uniqueStrategies.Any())
                {
                    uniqueStrategies.AddRange(new[] { "ONVIF", "RTSP", "HTTP", "USB" });
                }

                return Result.Ok(uniqueStrategies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in auto strategy selection for camera {CameraName}", camera.Name);
                return Result.Fail<List<string>>($"Auto selection failed: {ex.Message}");
            }
        }

        #region Private Analysis Methods

        private void AnalyzeCameraType(Monitoring.Domain.Aggregates.Camera.Camera camera, StrategyAnalysisResult analysis)
        {
            if (camera.Type?.Name == null) return;

            var typeName = camera.Type.Name.ToLower();
            analysis.CameraTypeAnalysis = typeName;

            // IP Camera
            if (typeName.Contains("ip") || typeName.Contains("network"))
            {
                analysis.StrategyScores["ONVIF"] += 35;  // ONVIF اولویت بالاتر برای IP cameras
                analysis.StrategyScores["RTSP"] += 30;
                analysis.StrategyScores["HTTP"] += 25;
                analysis.Confidence += 0.3f;
            }

            // ONVIF specific
            if (typeName.Contains("onvif"))
            {
                analysis.StrategyScores["ONVIF"] += 50;
                analysis.Confidence += 0.5f;
            }

            // USB Camera
            if (typeName.Contains("usb") || typeName.Contains("webcam") || typeName.Contains("local"))
            {
                analysis.StrategyScores["USB"] += 40;
                analysis.Confidence += 0.4f;
            }

            // Brand-specific
            if (typeName.Contains("hikvision"))
            {
                analysis.StrategyScores["HIKVISION_SDK"] += 40;
                analysis.StrategyScores["RTSP"] += 30;
                analysis.StrategyScores["ONVIF"] += 25;
                analysis.Confidence += 0.35f;
            }

            if (typeName.Contains("dahua"))
            {
                analysis.StrategyScores["DAHUA_SDK"] += 40;
                analysis.StrategyScores["RTSP"] += 30;
                analysis.StrategyScores["ONVIF"] += 25;
                analysis.Confidence += 0.35f;
            }
        }

        private void AnalyzeCameraLocation(Monitoring.Domain.Aggregates.Camera.Camera camera, StrategyAnalysisResult analysis)
        {
            if (camera.Location?.Value == null) return;

            var address = camera.Location.Value.ToLower();
            analysis.LocationAnalysis = address;

            // Local addresses favor USB
            if (address.Contains("localhost") || address.Contains("127.0.0.1") || address.Contains("local"))
            {
                analysis.StrategyScores["USB"] += 20;
            }

            // Network addresses favor network protocols
            if (address.Contains("192.168.") || address.Contains("10.") || address.Contains("172."))
            {
                analysis.StrategyScores["RTSP"] += 15;
                analysis.StrategyScores["HTTP"] += 10;
                analysis.StrategyScores["ONVIF"] += 10;
            }

            // Port analysis
            if (address.Contains(":554"))
            {
                analysis.StrategyScores["RTSP"] += 25;
                analysis.Confidence += 0.2f;
            }
            
            if (address.Contains(":80") || address.Contains(":8080"))
            {
                analysis.StrategyScores["HTTP"] += 20;
                analysis.Confidence += 0.15f;
            }

            if (address.Contains(":3702"))
            {
                analysis.StrategyScores["ONVIF"] += 30;
                analysis.Confidence += 0.25f;
            }
        }

        private void AnalyzeConnectionInfo(Monitoring.Domain.Aggregates.Camera.Camera camera, StrategyAnalysisResult analysis)
        {
            if (camera.ConnectionInfo?.StreamUrl == null) return;

            var streamUrl = camera.ConnectionInfo.StreamUrl.ToLower();
            analysis.ConnectionAnalysis = streamUrl;

            if (streamUrl.StartsWith("rtsp://"))
            {
                analysis.StrategyScores["RTSP"] += 40;
                analysis.Confidence += 0.4f;
            }
            else if (streamUrl.StartsWith("http://") || streamUrl.StartsWith("https://"))
            {
                analysis.StrategyScores["HTTP"] += 35;
                analysis.Confidence += 0.35f;
            }
            else if (streamUrl.StartsWith("usb://") || streamUrl.Contains("/dev/video"))
            {
                analysis.StrategyScores["USB"] += 40;
                analysis.Confidence += 0.4f;
            }

            // Protocol-specific paths
            if (streamUrl.Contains("/onvif/") || streamUrl.Contains("onvif"))
            {
                analysis.StrategyScores["ONVIF"] += 20;
            }

            if (streamUrl.Contains("mjpeg") || streamUrl.Contains("snapshot"))
            {
                analysis.StrategyScores["HTTP"] += 15;
            }
        }

        private void AnalyzeConfiguration(Monitoring.Domain.Aggregates.Camera.Camera camera, StrategyAnalysisResult analysis)
        {
            if (camera.Configuration?.AdditionalSettings == null) return;

            var settings = camera.Configuration.AdditionalSettings;
            analysis.ConfigurationAnalysis = string.Join(", ", settings.Keys);

            // Protocol hints in configuration
            foreach (var setting in settings)
            {
                var key = setting.Key.ToLower();
                var value = setting.Value?.ToLower() ?? "";

                if (key.Contains("protocol") || key.Contains("strategy"))
                {
                    if (value.Contains("onvif"))
                        analysis.StrategyScores["ONVIF"] += 40;
                    else if (value.Contains("rtsp"))
                        analysis.StrategyScores["RTSP"] += 30;
                    else if (value.Contains("http"))
                        analysis.StrategyScores["HTTP"] += 30;
                    else if (value.Contains("usb"))
                        analysis.StrategyScores["USB"] += 30;
                }

                if (key.Contains("device_path") && value.Contains("/dev/video"))
                {
                    analysis.StrategyScores["USB"] += 25;
                }

                if (key.Contains("sdk") || key.Contains("native"))
                {
                    analysis.StrategyScores["HIKVISION_SDK"] += 15;
                    analysis.StrategyScores["DAHUA_SDK"] += 15;
                }
            }
        }

        private void RankStrategies(StrategyAnalysisResult analysis)
        {
            // مرتب‌سازی بر اساس امتیاز
            analysis.RecommendedStrategies = analysis.StrategyScores
                .OrderByDescending(x => x.Value)
                .Where(x => x.Value > 0)
                .Select(x => x.Key)
                .ToList();

            // محاسبه اعتماد نهایی
            var maxScore = analysis.StrategyScores.Values.DefaultIfEmpty(0).Max();
            if (maxScore > 30)
                analysis.Confidence += 0.2f;
            else if (maxScore > 15)
                analysis.Confidence += 0.1f;

            // محدود کردن اعتماد به بازه 0-1
            analysis.Confidence = Math.Min(1.0f, analysis.Confidence);
        }

        #endregion
    }

    /// <summary>
    /// نتیجه تحلیل دوربین برای انتخاب استراتژی
    /// </summary>
    public class StrategyAnalysisResult
    {
        public string CameraName { get; set; }
        public string CameraType { get; set; }
        public DateTime AnalysisTimestamp { get; set; }
        public float Confidence { get; set; } = 0.0f;
        
        public Dictionary<string, int> StrategyScores { get; set; }
        public List<string> RecommendedStrategies { get; set; }

        public StrategyAnalysisResult()
        {
            StrategyScores = new Dictionary<string, int>
            {
                ["RTSP"] = 0,
                ["HTTP"] = 0,
                ["USB"] = 0,
                ["ONVIF"] = 0,
                ["HIKVISION_SDK"] = 0,
                ["DAHUA_SDK"] = 0
            };
            RecommendedStrategies = new List<string>();
        }
        
        public string CameraTypeAnalysis { get; set; }
        public string LocationAnalysis { get; set; }
        public string ConnectionAnalysis { get; set; }
        public string ConfigurationAnalysis { get; set; }
    }
}
