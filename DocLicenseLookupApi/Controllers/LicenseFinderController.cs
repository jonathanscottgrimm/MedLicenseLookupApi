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

        [HttpGet]
        [Route("GetGroupOne")]
        public async Task<List<LicenseInfo>> GetGroupOne(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupOne()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupTwo")]
        public async Task<List<LicenseInfo>> GetGroupTwo(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupTwo()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupThree")]
        public async Task<List<LicenseInfo>> GetGroupThree(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupThree()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupFour")]
        public async Task<List<LicenseInfo>> GetGroupFour(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupFour()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupFive")]
        public async Task<List<LicenseInfo>> GetGroupFive(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupFive()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupSix")]
        public async Task<List<LicenseInfo>> GetGroupSix(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupSix()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupSeven")]
        public async Task<List<LicenseInfo>> GetGroupSeven(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupSeven()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupEight")]
        public async Task<List<LicenseInfo>> GetGroupEight(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupEight()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupNine")]
        public async Task<List<LicenseInfo>> GetGroupNine(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupNine()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }

        [HttpGet]
        [Route("GetGroupTen")]
        public async Task<List<LicenseInfo>> GetGroupTen(string lastName, string firstName = "", bool isDOSearch = false)
        {
            var data = new DocLicenseLookupData(lastName, firstName, isDOSearch);
            var licenseInfoList = (await data.GetGroupTen()).OrderBy(x => x.State).ToList();

            return licenseInfoList;
        }
    }
}