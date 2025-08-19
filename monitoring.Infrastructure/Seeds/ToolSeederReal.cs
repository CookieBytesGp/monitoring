using Monitoring.Domain.Aggregates.Tools;
using Domain.Aggregates.Tools.ValueObjects;
using Domain.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monitoring.Infrastructure.Persistence;

namespace monitoring.Infrastructure.Seeds
{
    public class ToolSeederReal
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ToolSeederReal> _logger;

        public ToolSeederReal(DatabaseContext context, ILogger<ToolSeederReal> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting Tool seeding...");

                // Check if tools already exist
                if (await _context.Tools.AnyAsync())
                {
                    _logger.LogInformation("Tools already exist, skipping seeding");
                    return;
                }

                var tools = GetDefaultTools();
                
                foreach (var tool in tools)
                {
                    await _context.Tools.AddAsync(tool);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully seeded {tools.Count} tools");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding tools");
                throw;
            }
        }

        private List<Tool> GetDefaultTools()
        {
            var tools = new List<Tool>();

            // ابزارهای اصلی
            tools.Add(CreateVideoTool());           // ویدیو
            tools.Add(CreateFlowingTextTool());     // متن روان
            tools.Add(CreateClockTool());           // ساعت
            tools.Add(CreateWebPageTool());         // صفحه وب
            tools.Add(CreateWeatherTool());         // اب و هوا
            tools.Add(CreateTvTool());              // تلوزیون
            tools.Add(CreateImageTool());           // عکس
            tools.Add(CreateTextTool());            // متن
            tools.Add(CreateHeaderTool());          // سربرگ
            tools.Add(CreateCountdownTool());       // شمارشگر معکوس
            tools.Add(CreateGifTool());             // تصویر متحرک
            tools.Add(CreateDayCounterTool());      // روزشمار
            tools.Add(CreateCalendarTool());        // تقویم
            tools.Add(CreateDigitalClockTool());    // ساعت دیجیتال

            return tools;
        }

        // 1. ویدیو
        private Tool CreateVideoTool()
        {
            var templates = new List<Template>();
            
            var videoTemplate = Template.Create(
                "<div class='video-wrapper'><video class='video-element' controls autoplay muted loop><source src='{{src}}' type='video/mp4'>مرورگر شما از ویدیو پشتیبانی نمی‌کند.</video></div>",
                new Dictionary<string, string>
                {
                    { "video-wrapper", "video-container" },
                    { "video-element", "responsive-video" }
                },
                ".video-container { position: relative; width: 100%; max-width: 800px; margin: 0 auto; }" +
                ".responsive-video { width: 100%; height: auto; border-radius: 8px; box-shadow: 0 4px 15px rgba(0,0,0,0.2); }" +
                ".responsive-video:hover { transform: scale(1.02); transition: transform 0.3s ease; }"
            );

            templates.Add(videoTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateVideo("/videos/sample.mp4", "ویدیو پیش‌فرض").Value
            };

            var tool = Tool.Create(
                "video",
                "class VideoTool { constructor(element) { this.element = element; this.setupControls(); } setupControls() { this.element.querySelector('video').addEventListener('loadeddata', () => console.log('Video loaded')); } }",
                "video",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 2. متن روان
        private Tool CreateFlowingTextTool()
        {
            var templates = new List<Template>();
            
            var flowingTextTemplate = Template.Create(
                "<div class='flowing-text-container'><div class='flowing-text' data-speed='{{speed}}'>{{content}}</div></div>",
                new Dictionary<string, string>
                {
                    { "flowing-text-container", "marquee-wrapper" },
                    { "flowing-text", "scrolling-text" }
                },
                ".marquee-wrapper { width: 100%; overflow: hidden; background: linear-gradient(90deg, #667eea 0%, #764ba2 100%); padding: 10px 0; }" +
                ".scrolling-text { display: inline-block; white-space: nowrap; animation: scroll-left 15s linear infinite; color: white; font-size: 18px; font-weight: bold; }" +
                "@keyframes scroll-left { 0% { transform: translateX(100%); } 100% { transform: translateX(-100%); } }"
            );

            templates.Add(flowingTextTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateText("متن روان نمونه - این متن به صورت پیوسته حرکت می‌کند").Value
            };

            var tool = Tool.Create(
                "flowing-text",
                "class FlowingTextTool { constructor(element) { this.element = element; this.setupAnimation(); } setupAnimation() { const text = this.element.querySelector('.scrolling-text'); const speed = text.dataset.speed || 15; text.style.animationDuration = speed + 's'; } }",
                "flowing-text",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 3. ساعت
        private Tool CreateClockTool()
        {
            var templates = new List<Template>();
            
            var clockTemplate = Template.Create(
                "<div class='analog-clock'><div class='clock-face'><div class='hour-hand'></div><div class='minute-hand'></div><div class='second-hand'></div><div class='center-dot'></div></div></div>",
                new Dictionary<string, string>
                {
                    { "analog-clock", "clock-container" },
                    { "clock-face", "clock-display" }
                },
                ".clock-container { width: 200px; height: 200px; margin: 20px auto; }" +
                ".clock-display { width: 100%; height: 100%; border: 8px solid #333; border-radius: 50%; position: relative; background: white; }" +
                ".hour-hand, .minute-hand, .second-hand { position: absolute; background: #333; transform-origin: bottom center; }" +
                ".hour-hand { width: 4px; height: 50px; top: 50px; left: 98px; }" +
                ".minute-hand { width: 2px; height: 70px; top: 30px; left: 99px; }" +
                ".second-hand { width: 1px; height: 80px; top: 20px; left: 99.5px; background: red; }" +
                ".center-dot { width: 12px; height: 12px; background: #333; border-radius: 50%; position: absolute; top: 94px; left: 94px; }"
            );

            templates.Add(clockTemplate.Value);

            var defaultAssets = new List<Asset>();

            var tool = Tool.Create(
                "clock",
                "class ClockTool { constructor(element) { this.element = element; this.updateClock(); setInterval(() => this.updateClock(), 1000); } updateClock() { const now = new Date(); const hours = now.getHours() % 12; const minutes = now.getMinutes(); const seconds = now.getSeconds(); const hourAngle = (hours * 30) + (minutes * 0.5); const minuteAngle = minutes * 6; const secondAngle = seconds * 6; this.element.querySelector('.hour-hand').style.transform = `rotate(${hourAngle}deg)`; this.element.querySelector('.minute-hand').style.transform = `rotate(${minuteAngle}deg)`; this.element.querySelector('.second-hand').style.transform = `rotate(${secondAngle}deg)`; } }",
                "clock",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 4. صفحه وب
        private Tool CreateWebPageTool()
        {
            var templates = new List<Template>();
            
            var webPageTemplate = Template.Create(
                "<div class='webpage-container'><iframe class='webpage-frame' src='{{url}}' frameborder='0' scrolling='{{scrolling}}'></iframe></div>",
                new Dictionary<string, string>
                {
                    { "webpage-container", "iframe-wrapper" },
                    { "webpage-frame", "embedded-page" }
                },
                ".iframe-wrapper { width: 100%; height: 400px; border: 2px solid #ddd; border-radius: 8px; overflow: hidden; }" +
                ".embedded-page { width: 100%; height: 100%; border: none; }" +
                ".iframe-wrapper:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.15); }"
            );

            templates.Add(webPageTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateText("https://www.google.com").Value
            };

            var tool = Tool.Create(
                "webpage",
                "class WebPageTool { constructor(element) { this.element = element; this.setupIframe(); } setupIframe() { const iframe = this.element.querySelector('iframe'); iframe.onload = () => console.log('Webpage loaded'); } }",
                "webpage",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 5. آب و هوا
        private Tool CreateWeatherTool()
        {
            var templates = new List<Template>();
            
            var weatherTemplate = Template.Create(
                "<div class='weather-widget'><div class='weather-header'><h3>{{city}}</h3></div><div class='weather-body'><div class='temperature'>{{temp}}°C</div><div class='weather-icon'>{{icon}}</div><div class='weather-desc'>{{description}}</div></div></div>",
                new Dictionary<string, string>
                {
                    { "weather-widget", "weather-container" }
                },
                ".weather-container { background: linear-gradient(135deg, #74b9ff, #0984e3); color: white; padding: 20px; border-radius: 15px; text-align: center; min-width: 200px; }" +
                ".weather-header h3 { margin: 0 0 10px 0; font-size: 18px; }" +
                ".temperature { font-size: 36px; font-weight: bold; margin: 10px 0; }" +
                ".weather-icon { font-size: 48px; margin: 10px 0; }" +
                ".weather-desc { font-size: 14px; opacity: 0.9; }"
            );

            templates.Add(weatherTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateText("{\"city\":\"تهران\",\"temp\":25,\"icon\":\"☀️\",\"description\":\"آفتابی\"}").Value
            };

            var tool = Tool.Create(
                "weather",
                "class WeatherTool { constructor(element) { this.element = element; this.updateWeather(); setInterval(() => this.updateWeather(), 300000); } updateWeather() { fetch('/api/weather').then(response => response.json()).then(data => this.displayWeather(data)).catch(err => console.error('Weather update failed:', err)); } displayWeather(data) { /* Update weather display */ } }",
                "weather",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 6. تلویزیون
        private Tool CreateTvTool()
        {
            var templates = new List<Template>();
            
            var tvTemplate = Template.Create(
                "<div class='tv-container'><div class='tv-screen'><video class='tv-video' autoplay muted loop><source src='{{channel}}' type='video/mp4'></video><div class='tv-overlay'><div class='channel-info'>{{channelName}}</div></div></div><div class='tv-controls'><button class='tv-btn prev'>◀</button><button class='tv-btn next'>▶</button></div></div>",
                new Dictionary<string, string>
                {
                    { "tv-container", "television-widget" },
                    { "tv-screen", "tv-display" }
                },
                ".television-widget { background: #2d3436; padding: 20px; border-radius: 15px; }" +
                ".tv-display { position: relative; background: black; border-radius: 10px; overflow: hidden; }" +
                ".tv-video { width: 100%; height: 300px; object-fit: cover; }" +
                ".tv-overlay { position: absolute; bottom: 0; left: 0; right: 0; background: linear-gradient(transparent, rgba(0,0,0,0.7)); color: white; padding: 10px; }" +
                ".channel-info { font-size: 14px; }" +
                ".tv-controls { display: flex; justify-content: center; gap: 10px; margin-top: 15px; }" +
                ".tv-btn { background: #636e72; color: white; border: none; padding: 10px 15px; border-radius: 5px; cursor: pointer; }"
            );

            templates.Add(tvTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateVideo("/channels/channel1.mp4", "شبکه ۱").Value
            };

            var tool = Tool.Create(
                "tv",
                "class TvTool { constructor(element) { this.element = element; this.channels = ['/channels/channel1.mp4', '/channels/channel2.mp4']; this.currentChannel = 0; this.setupControls(); } setupControls() { this.element.querySelector('.prev').onclick = () => this.changeChannel(-1); this.element.querySelector('.next').onclick = () => this.changeChannel(1); } changeChannel(direction) { this.currentChannel += direction; if (this.currentChannel < 0) this.currentChannel = this.channels.length - 1; if (this.currentChannel >= this.channels.length) this.currentChannel = 0; this.element.querySelector('video source').src = this.channels[this.currentChannel]; this.element.querySelector('video').load(); } }",
                "tv",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 7. عکس
        private Tool CreateImageTool()
        {
            var templates = new List<Template>();
            
            var imageTemplate = Template.Create(
                "<div class='image-container'><img class='display-image' src='{{src}}' alt='{{alt}}' /><div class='image-overlay'><div class='image-caption'>{{caption}}</div></div></div>",
                new Dictionary<string, string>
                {
                    { "image-container", "photo-frame" },
                    { "display-image", "responsive-image" }
                },
                ".photo-frame { position: relative; display: inline-block; border-radius: 12px; overflow: hidden; box-shadow: 0 8px 25px rgba(0,0,0,0.15); }" +
                ".responsive-image { width: 100%; height: auto; display: block; transition: transform 0.3s ease; }" +
                ".photo-frame:hover .responsive-image { transform: scale(1.05); }" +
                ".image-overlay { position: absolute; bottom: 0; left: 0; right: 0; background: linear-gradient(transparent, rgba(0,0,0,0.8)); color: white; padding: 15px; }" +
                ".image-caption { font-size: 14px; text-align: center; }"
            );

            templates.Add(imageTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateImage("/images/sample.jpg", "تصویر نمونه").Value
            };

            var tool = Tool.Create(
                "image",
                "class ImageTool { constructor(element) { this.element = element; this.setupImageEvents(); } setupImageEvents() { const img = this.element.querySelector('img'); img.addEventListener('click', () => this.openLightbox()); } openLightbox() { /* Lightbox functionality */ } }",
                "image",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 8. متن
        private Tool CreateTextTool()
        {
            var templates = new List<Template>();
            
            var textTemplate = Template.Create(
                "<div class='text-element' contenteditable='true' data-placeholder='متن خود را وارد کنید'>{{content}}</div>",
                new Dictionary<string, string>
                {
                    { "text-element", "editable-text" }
                },
                ".editable-text { font-family: 'IRANSans', Arial, sans-serif; color: #333; line-height: 1.6; padding: 10px; border: 2px dashed transparent; border-radius: 6px; min-height: 40px; }" +
                ".editable-text:hover { border-color: #007bff; background-color: #f8f9fa; }" +
                ".editable-text:focus { outline: none; border-color: #007bff; background-color: #fff; box-shadow: 0 0 8px rgba(0,123,255,0.25); }" +
                ".editable-text:empty:before { content: attr(data-placeholder); color: #999; font-style: italic; }"
            );

            templates.Add(textTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateText("متن ساده قابل ویرایش").Value
            };

            var tool = Tool.Create(
                "text",
                "class TextTool { constructor(element) { this.element = element; this.setupEditing(); } setupEditing() { const textEl = this.element.querySelector('.editable-text'); textEl.addEventListener('input', () => this.saveContent()); } saveContent() { /* Auto-save functionality */ } }",
                "text",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 9. سربرگ
        private Tool CreateHeaderTool()
        {
            var templates = new List<Template>();
            
            var headerTemplate = Template.Create(
                "<header class='page-header'><div class='header-content'><h1 class='header-title'>{{title}}</h1><p class='header-subtitle'>{{subtitle}}</p></div><div class='header-decoration'></div></header>",
                new Dictionary<string, string>
                {
                    { "page-header", "main-header" },
                    { "header-content", "header-text" }
                },
                ".main-header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 20px; text-align: center; position: relative; overflow: hidden; }" +
                ".header-text { position: relative; z-index: 2; }" +
                ".header-title { font-size: 2.5rem; font-weight: bold; margin: 0 0 10px 0; }" +
                ".header-subtitle { font-size: 1.2rem; opacity: 0.9; margin: 0; }" +
                ".header-decoration { position: absolute; top: -50%; right: -10%; width: 200px; height: 200px; background: rgba(255,255,255,0.1); border-radius: 50%; }"
            );

            templates.Add(headerTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateText("{\"title\":\"عنوان اصلی\",\"subtitle\":\"زیرعنوان توضیحی\"}").Value
            };

            var tool = Tool.Create(
                "header",
                "class HeaderTool { constructor(element) { this.element = element; this.makeEditable(); } makeEditable() { const title = this.element.querySelector('.header-title'); const subtitle = this.element.querySelector('.header-subtitle'); title.contentEditable = true; subtitle.contentEditable = true; } }",
                "header",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 10. شمارشگر معکوس
        private Tool CreateCountdownTool()
        {
            var templates = new List<Template>();
            
            var countdownTemplate = Template.Create(
                "<div class='countdown-widget'><div class='countdown-title'>{{title}}</div><div class='countdown-timer'><div class='time-unit'><span class='time-value' id='days'>00</span><span class='time-label'>روز</span></div><div class='time-unit'><span class='time-value' id='hours'>00</span><span class='time-label'>ساعت</span></div><div class='time-unit'><span class='time-value' id='minutes'>00</span><span class='time-label'>دقیقه</span></div><div class='time-unit'><span class='time-value' id='seconds'>00</span><span class='time-label'>ثانیه</span></div></div></div>",
                new Dictionary<string, string>
                {
                    { "countdown-widget", "countdown-container" }
                },
                ".countdown-container { background: linear-gradient(45deg, #ff6b6b, #ff8e53); color: white; padding: 30px; border-radius: 15px; text-align: center; }" +
                ".countdown-title { font-size: 1.5rem; font-weight: bold; margin-bottom: 20px; }" +
                ".countdown-timer { display: flex; justify-content: center; gap: 15px; }" +
                ".time-unit { display: flex; flex-direction: column; align-items: center; }" +
                ".time-value { font-size: 2.5rem; font-weight: bold; line-height: 1; }" +
                ".time-label { font-size: 0.8rem; opacity: 0.9; margin-top: 5px; }"
            );

            templates.Add(countdownTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateText("{\"title\":\"تا نوروز\",\"targetDate\":\"2025-03-20T00:00:00\"}").Value
            };

            var tool = Tool.Create(
                "countdown",
                "class CountdownTool { constructor(element) { this.element = element; this.targetDate = new Date('2025-03-20T00:00:00'); this.updateCountdown(); setInterval(() => this.updateCountdown(), 1000); } updateCountdown() { const now = new Date(); const diff = this.targetDate - now; if (diff <= 0) return; const days = Math.floor(diff / (1000 * 60 * 60 * 24)); const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)); const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60)); const seconds = Math.floor((diff % (1000 * 60)) / 1000); this.element.querySelector('#days').textContent = days.toString().padStart(2, '0'); this.element.querySelector('#hours').textContent = hours.toString().padStart(2, '0'); this.element.querySelector('#minutes').textContent = minutes.toString().padStart(2, '0'); this.element.querySelector('#seconds').textContent = seconds.toString().padStart(2, '0'); } }",
                "countdown",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 11. تصویر متحرک (GIF)
        private Tool CreateGifTool()
        {
            var templates = new List<Template>();
            
            var gifTemplate = Template.Create(
                "<div class='gif-container'><img class='animated-gif' src='{{src}}' alt='{{alt}}' /><div class='gif-controls'><button class='gif-pause'>⏸️</button><button class='gif-play' style='display:none;'>▶️</button></div></div>",
                new Dictionary<string, string>
                {
                    { "gif-container", "gif-player" },
                    { "animated-gif", "gif-image" }
                },
                ".gif-player { position: relative; display: inline-block; border-radius: 10px; overflow: hidden; }" +
                ".gif-image { width: 100%; height: auto; display: block; }" +
                ".gif-controls { position: absolute; top: 10px; right: 10px; display: flex; gap: 5px; }" +
                ".gif-controls button { background: rgba(0,0,0,0.7); color: white; border: none; padding: 5px 8px; border-radius: 5px; cursor: pointer; font-size: 12px; }" +
                ".gif-player:hover .gif-controls { opacity: 1; }"
            );

            templates.Add(gifTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateImage("/gifs/sample.gif", "تصویر متحرک").Value
            };

            var tool = Tool.Create(
                "gif",
                "class GifTool { constructor(element) { this.element = element; this.setupControls(); } setupControls() { const pauseBtn = this.element.querySelector('.gif-pause'); const playBtn = this.element.querySelector('.gif-play'); const img = this.element.querySelector('img'); pauseBtn.onclick = () => { img.style.animationPlayState = 'paused'; pauseBtn.style.display = 'none'; playBtn.style.display = 'block'; }; playBtn.onclick = () => { img.style.animationPlayState = 'running'; playBtn.style.display = 'none'; pauseBtn.style.display = 'block'; }; } }",
                "gif",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 12. روزشمار
        private Tool CreateDayCounterTool()
        {
            var templates = new List<Template>();
            
            var dayCounterTemplate = Template.Create(
                "<div class='day-counter-widget'><div class='counter-header'><h3>{{title}}</h3><p class='event-date'>{{eventDate}}</p></div><div class='days-count'><span class='days-number'>{{daysCount}}</span><span class='days-text'>روز گذشته</span></div></div>",
                new Dictionary<string, string>
                {
                    { "day-counter-widget", "day-counter" }
                },
                ".day-counter { background: linear-gradient(135deg, #a8edea 0%, #fed6e3 100%); padding: 25px; border-radius: 15px; text-align: center; color: #333; }" +
                ".counter-header h3 { margin: 0 0 5px 0; font-size: 1.3rem; }" +
                ".event-date { margin: 0 0 20px 0; font-size: 0.9rem; opacity: 0.8; }" +
                ".days-count { display: flex; flex-direction: column; align-items: center; }" +
                ".days-number { font-size: 3rem; font-weight: bold; color: #2d3436; line-height: 1; }" +
                ".days-text { font-size: 1rem; margin-top: 5px; }"
            );

            templates.Add(dayCounterTemplate.Value);

            var defaultAssets = new List<Asset>
            {
                Asset.CreateText("{\"title\":\"از تولدم\",\"eventDate\":\"2024-01-01\"}").Value
            };

            var tool = Tool.Create(
                "day-counter",
                "class DayCounterTool { constructor(element) { this.element = element; this.eventDate = new Date('2024-01-01'); this.updateCounter(); setInterval(() => this.updateCounter(), 86400000); } updateCounter() { const now = new Date(); const diffTime = Math.abs(now - this.eventDate); const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24)); this.element.querySelector('.days-number').textContent = diffDays; } }",
                "day-counter",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 13. تقویم
        private Tool CreateCalendarTool()
        {
            var templates = new List<Template>();
            
            var calendarTemplate = Template.Create(
                "<div class='calendar-widget'><div class='calendar-header'><button class='cal-prev'>‹</button><h3 class='cal-month-year'>{{monthYear}}</h3><button class='cal-next'>›</button></div><div class='calendar-grid'><div class='cal-days-header'><span>ش</span><span>ی</span><span>د</span><span>س</span><span>چ</span><span>پ</span><span>ج</span></div><div class='cal-days'></div></div></div>",
                new Dictionary<string, string>
                {
                    { "calendar-widget", "persian-calendar" }
                },
                ".persian-calendar { background: white; border: 1px solid #e0e0e0; border-radius: 12px; padding: 20px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); max-width: 300px; }" +
                ".calendar-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }" +
                ".cal-prev, .cal-next { background: #007bff; color: white; border: none; border-radius: 50%; width: 30px; height: 30px; cursor: pointer; }" +
                ".cal-month-year { margin: 0; font-size: 1.1rem; }" +
                ".cal-days-header { display: grid; grid-template-columns: repeat(7, 1fr); gap: 5px; margin-bottom: 10px; }" +
                ".cal-days-header span { text-align: center; font-weight: bold; padding: 8px; color: #666; }" +
                ".cal-days { display: grid; grid-template-columns: repeat(7, 1fr); gap: 2px; }" +
                ".cal-day { text-align: center; padding: 8px; cursor: pointer; border-radius: 4px; }" +
                ".cal-day:hover { background: #f0f0f0; }" +
                ".cal-day.today { background: #007bff; color: white; }"
            );

            templates.Add(calendarTemplate.Value);

            var defaultAssets = new List<Asset>();

            var tool = Tool.Create(
                "calendar",
                "class CalendarTool { constructor(element) { this.element = element; this.currentDate = new Date(); this.generateCalendar(); this.setupNavigation(); } generateCalendar() { /* Calendar generation logic */ } setupNavigation() { this.element.querySelector('.cal-prev').onclick = () => this.previousMonth(); this.element.querySelector('.cal-next').onclick = () => this.nextMonth(); } previousMonth() { this.currentDate.setMonth(this.currentDate.getMonth() - 1); this.generateCalendar(); } nextMonth() { this.currentDate.setMonth(this.currentDate.getMonth() + 1); this.generateCalendar(); } }",
                "calendar",
                templates,
                defaultAssets
            );

            return tool.Value;
        }

        // 14. ساعت دیجیتال
        private Tool CreateDigitalClockTool()
        {
            var templates = new List<Template>();
            
            var digitalClockTemplate = Template.Create(
                "<div class='digital-clock-widget'><div class='time-display'><span class='hours'>{{hours}}</span><span class='separator'>:</span><span class='minutes'>{{minutes}}</span><span class='separator'>:</span><span class='seconds'>{{seconds}}</span></div><div class='date-display'><span class='weekday'>{{weekday}}</span><span class='date'>{{date}}</span></div></div>",
                new Dictionary<string, string>
                {
                    { "digital-clock-widget", "digital-clock" }
                },
                ".digital-clock { background: linear-gradient(45deg, #2c3e50, #34495e); color: #00ff00; padding: 25px; border-radius: 15px; text-align: center; font-family: 'Courier New', monospace; box-shadow: inset 0 0 20px rgba(0,0,0,0.3); }" +
                ".time-display { font-size: 2.5rem; font-weight: bold; margin-bottom: 10px; }" +
                ".separator { animation: blink 1s infinite; }" +
                "@keyframes blink { 0%, 50% { opacity: 1; } 51%, 100% { opacity: 0; } }" +
                ".date-display { font-size: 1rem; opacity: 0.8; }" +
                ".weekday { margin-left: 10px; }"
            );

            templates.Add(digitalClockTemplate.Value);

            var defaultAssets = new List<Asset>();

            var tool = Tool.Create(
                "digital-clock",
                "class DigitalClockTool { constructor(element) { this.element = element; this.updateClock(); setInterval(() => this.updateClock(), 1000); } updateClock() { const now = new Date(); const hours = now.getHours().toString().padStart(2, '0'); const minutes = now.getMinutes().toString().padStart(2, '0'); const seconds = now.getSeconds().toString().padStart(2, '0'); const weekdays = ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه']; const weekday = weekdays[now.getDay()]; const date = now.toLocaleDateString('fa-IR'); this.element.querySelector('.hours').textContent = hours; this.element.querySelector('.minutes').textContent = minutes; this.element.querySelector('.seconds').textContent = seconds; this.element.querySelector('.weekday').textContent = weekday; this.element.querySelector('.date').textContent = date; } }",
                "digital-clock",
                templates,
                defaultAssets
            );

            return tool.Value;
        }
    }
}
