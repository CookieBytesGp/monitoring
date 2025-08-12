//using Microsoft.AspNetCore.SignalR;

//namespace App.Hubs.Adapters
//{
//    public class SignalRCameraNotifications : ICameraNotifications
//    {
//        private readonly IHubContext<CameraHub> _hubContext;

//        public SignalRCameraNotifications(IHubContext<CameraHub> hubContext)
//        {
//            _hubContext = hubContext;
//        }

//        public Task UpdateCameraStatusAsync(int cameraId, bool isActive) =>
//            _hubContext.Clients.All.SendAsync("UpdateCameraStatus", cameraId, isActive);

//        public Task NotifyMotionDetectedAsync(int cameraId, string timestamp) =>
//            _hubContext.Clients.All.SendAsync("MotionDetected", cameraId, timestamp);
//    }
//}

