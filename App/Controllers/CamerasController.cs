using Microsoft.AspNetCore.Mvc;
using CameraService.Services;
using DTOs.Camera;
using System;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CamerasController : ControllerBase
    {
        private readonly ICameraService _cameraService;

        public CamerasController(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        // GET: api/Cameras
        [HttpGet]
        public async Task<IActionResult> GetAllCameras()
        {
            var result = await _cameraService.GetAllCamerasAsync();
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(result.Value);
        }

        // GET: api/Cameras/summaries
        [HttpGet("summaries")]
        public async Task<IActionResult> GetCameraSummaries()
        {
            var result = await _cameraService.GetCameraSummariesAsync();
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(result.Value);
        }

        // GET: api/Cameras/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCamera(Guid id)
        {
            var result = await _cameraService.GetCameraByIdAsync(id);
            
            if (result.IsFailed)
                return NotFound(result.Errors);
                
            return Ok(result.Value);
        }

        // POST: api/Cameras
        [HttpPost]
        public async Task<IActionResult> CreateCamera([FromBody] CreateCameraCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cameraService.CreateCameraAsync(command);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return CreatedAtAction(nameof(GetCamera), new { id = result.Value.Id }, result.Value);
        }

        // PUT: api/Cameras/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCamera(Guid id, [FromBody] UpdateCameraCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cameraService.UpdateCameraAsync(command);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(result.Value);
        }

        // DELETE: api/Cameras/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera(Guid id)
        {
            var result = await _cameraService.DeleteCameraAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return NoContent();
        }

        // POST: api/Cameras/{id}/connect
        [HttpPost("{id}/connect")]
        public async Task<IActionResult> ConnectCamera(Guid id)
        {
            var result = await _cameraService.ConnectCameraAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(new { message = "Camera connected successfully" });
        }

        // POST: api/Cameras/{id}/disconnect
        [HttpPost("{id}/disconnect")]
        public async Task<IActionResult> DisconnectCamera(Guid id)
        {
            var result = await _cameraService.DisconnectCameraAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(new { message = "Camera disconnected successfully" });
        }

        // POST: api/Cameras/{id}/heartbeat
        [HttpPost("{id}/heartbeat")]
        public async Task<IActionResult> UpdateHeartbeat(Guid id)
        {
            var result = await _cameraService.UpdateCameraHeartbeatAsync(id);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(new { message = "Heartbeat updated successfully" });
        }

        // GET: api/Cameras/status/{status}
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetCamerasByStatus(int status)
        {
            if (!Enum.IsDefined(typeof(Domain.Aggregates.Camera.CameraStatus), status))
                return BadRequest("Invalid status value");

            var cameraStatus = (Domain.Aggregates.Camera.CameraStatus)status;
            var result = await _cameraService.GetCamerasByStatusAsync(cameraStatus);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(result.Value);
        }

        // GET: api/Cameras/location/{location}
        [HttpGet("location/{location}")]
        public async Task<IActionResult> GetCamerasByLocation(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return BadRequest("Location cannot be empty");

            var result = await _cameraService.GetCamerasByLocationAsync(location);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(result.Value);
        }

        // GET: api/Cameras/{id}/configuration
        [HttpGet("{id}/configuration")]
        public async Task<IActionResult> GetCameraConfiguration(Guid id)
        {
            var result = await _cameraService.GetCameraConfigurationAsync(id);
            
            if (result.IsFailed)
                return NotFound(result.Errors);
                
            return Ok(result.Value);
        }

        // PUT: api/Cameras/{id}/configuration
        [HttpPut("{id}/configuration")]
        public async Task<IActionResult> UpdateCameraConfiguration(Guid id, [FromBody] CameraConfigurationViewModel configuration)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cameraService.UpdateCameraConfigurationAsync(id, configuration);
            
            if (result.IsFailed)
                return BadRequest(result.Errors);
                
            return Ok(new { message = "Configuration updated successfully" });
        }
    }
}
