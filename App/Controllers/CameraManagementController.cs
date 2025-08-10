using Microsoft.AspNetCore.Mvc;
using CameraService.Services;
using DTOs.Camera;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace App.Controllers
{
    [Authorize]
    public class CameraManagementController : Controller
    {
        private readonly ICameraService _cameraService;

        public CameraManagementController(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        // GET: CameraManagement
        public async Task<IActionResult> Index()
        {
            var result = await _cameraService.GetAllCamerasAsync();
            
            if (result.IsFailed)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                return View(new List<CameraViewModel>());
            }
                
            return View(result.Value);
        }

        // GET: CameraManagement/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _cameraService.GetCameraByIdAsync(id);
            
            if (result.IsFailed)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
                return RedirectToAction(nameof(Index));
            }
                
            return View(result.Value);
        }

        // GET: CameraManagement/Create
        public IActionResult Create()
        {
            ViewBag.CameraTypes = Enum.GetValues(typeof(Domain.Aggregates.Camera.CameraType));
            return View();
        }

        // POST: CameraManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCameraCommand command)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CameraTypes = Enum.GetValues(typeof(Domain.Aggregates.Camera.CameraType));
                return View(command);
            }

            var result = await _cameraService.CreateCameraAsync(command);
            
            if (result.IsFailed)
            {
                ViewBag.CameraTypes = Enum.GetValues(typeof(Domain.Aggregates.Camera.CameraType));
                ViewBag.Error = string.Join(", ", result.Errors);
                return View(command);
            }

            TempData["Success"] = "Camera created successfully!";
            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        // GET: CameraManagement/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _cameraService.GetCameraByIdAsync(id);
            
            if (result.IsFailed)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
                return RedirectToAction(nameof(Index));
            }

            var command = new UpdateCameraCommand
            {
                Id = result.Value.Id,
                Name = result.Value.Name,
                Location = result.Value.Location,
                LocationZone = result.Value.LocationZone
            };
                
            return View(command);
        }

        // POST: CameraManagement/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateCameraCommand command)
        {
            if (id != command.Id)
            {
                TempData["Error"] = "ID mismatch";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(command);
            }

            var result = await _cameraService.UpdateCameraAsync(command);
            
            if (result.IsFailed)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                return View(command);
            }

            TempData["Success"] = "Camera updated successfully!";
            return RedirectToAction(nameof(Details), new { id = command.Id });
        }

        // GET: CameraManagement/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _cameraService.GetCameraByIdAsync(id);
            
            if (result.IsFailed)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
                return RedirectToAction(nameof(Index));
            }
                
            return View(result.Value);
        }

        // POST: CameraManagement/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var result = await _cameraService.DeleteCameraAsync(id);
            
            if (result.IsFailed)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
            }
            else
            {
                TempData["Success"] = "Camera deleted successfully!";
            }
                
            return RedirectToAction(nameof(Index));
        }

        // POST: CameraManagement/Connect/{id}
        [HttpPost]
        public async Task<IActionResult> Connect(Guid id)
        {
            var result = await _cameraService.ConnectCameraAsync(id);
            
            if (result.IsFailed)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
            }
            else
            {
                TempData["Success"] = "Camera connected successfully!";
            }
                
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: CameraManagement/Disconnect/{id}
        [HttpPost]
        public async Task<IActionResult> Disconnect(Guid id)
        {
            var result = await _cameraService.DisconnectCameraAsync(id);
            
            if (result.IsFailed)
            {
                TempData["Error"] = string.Join(", ", result.Errors);
            }
            else
            {
                TempData["Success"] = "Camera disconnected successfully!";
            }
                
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: CameraManagement/Configuration/{id}
        public async Task<IActionResult> Configuration(Guid id)
        {
            var cameraResult = await _cameraService.GetCameraByIdAsync(id);
            if (cameraResult.IsFailed)
            {
                TempData["Error"] = string.Join(", ", cameraResult.Errors);
                return RedirectToAction(nameof(Index));
            }

            var configResult = await _cameraService.GetCameraConfigurationAsync(id);
            if (configResult.IsFailed)
            {
                TempData["Error"] = string.Join(", ", configResult.Errors);
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Camera = cameraResult.Value;
            return View(configResult.Value);
        }

        // POST: CameraManagement/Configuration/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Configuration(Guid id, CameraConfigurationViewModel configuration)
        {
            if (!ModelState.IsValid)
            {
                var cameraResult = await _cameraService.GetCameraByIdAsync(id);
                if (cameraResult.IsSuccess)
                    ViewBag.Camera = cameraResult.Value;
                return View(configuration);
            }

            var result = await _cameraService.UpdateCameraConfigurationAsync(id, configuration);
            
            if (result.IsFailed)
            {
                ViewBag.Error = string.Join(", ", result.Errors);
                var cameraResult = await _cameraService.GetCameraByIdAsync(id);
                if (cameraResult.IsSuccess)
                    ViewBag.Camera = cameraResult.Value;
                return View(configuration);
            }

            TempData["Success"] = "Configuration updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
