using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IEnumerable<LicenseInfo>> Get(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData();

            var results = await data.GetLicenseInfoForAllStates(lastName, firstName, isDOSearch);

            return results;
        }
    }
}
