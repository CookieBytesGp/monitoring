using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DTOs.Pagebuilder;
using App.Models;

namespace App.Controllers
{
    public class PageController : Controller
    {
        private readonly HttpClient _httpClient;

        public PageController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Page
        public async Task<IActionResult> Index()
        {
            try
            {
                // Replace with the actual API Gateway URL
                var response = await _httpClient.GetAsync("http://localhost:5000/pagebuilder");

                if (response.IsSuccessStatusCode)
                {
                    var pages = await response.Content.ReadAsAsync<List<PageDTO>>();
                    return View(pages);
                }
                else
                {
                    // Log the status code and reason phrase
                    var statusCode = response.StatusCode;
                    var reasonPhrase = response.ReasonPhrase;
                    // Pass ErrorViewModel to the view
                    var errorViewModel = new ErrorViewModel
                    {
                        ErrorMessage = $"Error: {statusCode} - {reasonPhrase}"
                    };
                    return View("Error", errorViewModel);
                }
            }
            catch (HttpRequestException ex)
            {
                // Log the exception message
                var errorMessage = ex.Message;
                // Pass ErrorViewModel to the view
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = $"Request error: {errorMessage}"
                };
                return View("Error", errorViewModel);
            }
        }
    }

}
