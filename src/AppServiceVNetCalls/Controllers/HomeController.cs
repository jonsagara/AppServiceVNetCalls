using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using AppServiceVNetCalls.Configuration;
using AppServiceVNetCalls.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AppServiceVNetCalls.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly TestRequestSettings _testRequest;

        public HomeController(IHttpClientFactory httpClientFactory, IOptions<TestRequestSettings> testRequest)
        {
            _httpClient = httpClientFactory.CreateClient();
            _testRequest = testRequest.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> VNetRequest()
        {
            string? body;
            try
            {
                using var requestMsg = new HttpRequestMessage(HttpMethod.Get, _testRequest.VNetEndpoint);
                using var responseMsg = await _httpClient.SendAsync(requestMsg);
                body = await responseMsg.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                body = ex.ToString();
            }

            return Content(body);
        }

        public async Task<IActionResult> NonVNetRequest()
        {
            string? body;
            try
            {
                using var requestMsg = new HttpRequestMessage(HttpMethod.Get, _testRequest.NonVNetEndpoint);
                using var responseMsg = await _httpClient.SendAsync(requestMsg);
                body = await responseMsg.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                body = ex.ToString();
            }

            return Content(body);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
