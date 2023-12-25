namespace DocLicenseLookupApi
{
    public class LicenseInfo
    {
        public string State { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseStatus { get; set; }
        public string LicenseExpiration { get; set; }
        public string ErrorMessage { get; set; }
    }
}
