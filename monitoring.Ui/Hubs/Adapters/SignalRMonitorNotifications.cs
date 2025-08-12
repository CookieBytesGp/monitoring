//using Microsoft.AspNetCore.SignalR;
//using Monitoring.Application.Interfaces.Realtime;

//namespace App.Hubs.Adapters
//{
//    public class SignalRMonitorNotifications : IMonitorNotifications
//    {
//        private readonly IHubContext<MonitorHub> _hubContext;

//        public SignalRMonitorNotifications(IHubContext<MonitorHub> hubContext)
//        {
//            _hubContext = hubContext;
//        }

//        public Task UpdateMonitorStatusAsync(int monitorId, bool isActive) =>
//            _hubContext.Clients.All.SendAsync("UpdateMonitorStatus", monitorId, isActive);

//        public Task UpdateMonitorContentAsync(int monitorId, string content) =>
//            _hubContext.Clients.All.SendAsync("UpdateMonitorContent", monitorId, content);
//    }
//}

