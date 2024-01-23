using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DocLicenseLookupApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LicenseFinderController : Controller
    {
        private readonly ILogger<LicenseFinderController> _logger;

        public LicenseFinderController(ILogger<LicenseFinderController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetLicenses")]
        public async Task<ResponseWrapper> Get(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var sw = Stopwatch.StartNew();

            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);

            var response = new ResponseWrapper
            {
                LicenseInfo = await data.GetLicenseInfoForAllStates()
            };

            sw.Stop();

            response.ExecutionTime = (int)sw.Elapsed.TotalSeconds;

     
            return response;
        }
    }
}
