using DTOs.User;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FluentResults;
using UserSerivce.Services;
using Persistence;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger, IUnitOfWork unitOfWork) 
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(Result<UserVeiwModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateUser([FromBody] Request viewModel)
        {
            try
            {
                var result = await _userService.CreateUserAsync(
                    viewModel.FirstName,
                    viewModel.LastName,
                    viewModel.UserName,
                    viewModel.Password);

                if (result.IsFailed)
                {
                    return BadRequest(result.Errors);
                }



                // در اینجا می‌توانید عملیات اضافه را انجام دهید، مانند ذخیره در دیتابیس
                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating user");
                return StatusCode(500, "Unexpected error while creating user");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserVeiwModel), StatusCodes.Status200OK)]
        public IActionResult GetUser(Guid id)
        {
            try
            {
                var user = _userService.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting user");
                return StatusCode(500, "Unexpected error while getting user");
            }
        }
    }

}
