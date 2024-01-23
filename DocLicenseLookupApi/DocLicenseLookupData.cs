using Microsoft.Extensions.Options;
using Polly.Retry;
using Polly;
using PuppeteerSharp;
using System.Collections.Concurrent;

namespace DocLicenseLookupApi
{
    public class DocLicenseLookupData
    {
        private readonly string _lastName;
        private readonly string _firstName;
        private readonly bool _isDOSearch;
        private AsyncRetryPolicy _retryPolicy;


        public DocLicenseLookupData(string lastName, string firstName = "", bool isDoSearch = false)
        {
            _lastName = lastName;
            _firstName = firstName;
            _isDOSearch = isDoSearch;

            _retryPolicy = Policy
           .Handle<Exception>() // Specify the type of exceptions to handle
           .WaitAndRetryAsync(
               3, // Number of retries
               retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        }

        public async Task<ConcurrentBag<LicenseInfo>> GetLicenseInfoForAllStates()
        {
            var statesList = GetSearchInfoForAllStates();
            var resultsList = new ConcurrentBag<LicenseInfo>();

            using (var browserService = new BrowserService())
            {
                await browserService.InitializeAsync();

                // Concurrency control
                var maxConcurrentTasks = 20; // Adjust this number based on performance testing
                var semaphore = new SemaphoreSlim(maxConcurrentTasks);


                var tasks = statesList.Select(async state =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var licenseInfo = await GetLicenseInfoFromBasicSite(browserService, state);
                        resultsList.Add(licenseInfo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing {state.StateName}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToList();

                await Task.WhenAll(tasks);
            }

            return resultsList;
        }

   

        public List<SearchInfo> GetSearchInfoForAllStates()
        {
            return new List<SearchInfo>
            {
                GetArizonaLicenseInfo(),
                GetArkansasLicenseInfo(),
                GetCaliforniaLicenseInfo(),
                GetDelawareLicenseInfo(),
              GetDistrictOfColumbiaLicenseInfo(),
                GetFloridaLicenseInfo(),
                GetIdahoLicenseInfo(),
                GetIndianaLicenseInfo(),
                GetIowaLicenseInfo  (),
                GetKansasLicenseInfo(),
                GetKentuckyLicenseInfo(),
                GetLouisianaSiteLicenseInfo(),
               // GetMaineSiteLicenseInfo(),
                //GetMarylandLicenseInfo(),
                GetMassachussettsSiteLicenseInfo(),
                GetMichiganSiteLicenseInfo(),
                GetMinnesotaSiteLicenseInfo(),
                GetMissouriLicenseInfo(),
                GetNevadaLicenseInfo(),
                GetNewHampshireLicenseInfo(),
                GetNewJerseyLicenseInfo(),
                GetNewMexicoLicenseInfo(),
                GetNorthCarolinaLicenseInfo(),
                GetOhioLicenseInfo(),
                //GetPennsylvaniaLicenseInfo(),
                GetRhodeIslandLicenseInfo       (),
                GetTexasLicenseInfo(),
            //    GetVermontLicenseInfo(),
                GetVirginiaLicenseInfo  (),
                GetWestVirginiaLicenseInfo(),
                GetWyomingLicenseInfo(),
            };
        }


        public async Task<LicenseInfo> GetLicenseInfoFromBasicSite(BrowserService browserService, SearchInfo searchInfo)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
            var context = await browserService.Browser.CreateIncognitoBrowserContextAsync();
            try
            {
                var page = await context.NewPageAsync();

                // await page.SetJavaScriptEnabledAsync(false);

            
                        await page.SetRequestInterceptionAsync(true);
                        page.Request += async (sender, e) =>
                        {
                            if (e.Request.ResourceType == ResourceType.StyleSheet ||
                                e.Request.ResourceType == ResourceType.Font ||
                                e.Request.ResourceType == ResourceType.Image || e.Request.ResourceType == ResourceType.Media)
                            {
                                await e.Request.AbortAsync();
                            }
                            else
                            {
                                await e.Request.ContinueAsync();
                            }
                        };
                    
             

                await page.GoToAsync(searchInfo.Url);

                if (!string.IsNullOrEmpty(searchInfo.InitComboBoxSelector))
                {
                    await page.WaitForSelectorAsync(searchInfo.InitComboBoxSelector);
                    await page.FocusAsync(searchInfo.InitComboBoxSelector);
                    await page.Keyboard.TypeAsync(searchInfo.InitComboBoxValue);
                    await page.Keyboard.DownAsync("Enter");
                }

                if (!string.IsNullOrEmpty(searchInfo.InitComboBox2Selector))
                {
                    await page.FocusAsync(searchInfo.InitComboBox2Selector);
                    await page.Keyboard.TypeAsync(searchInfo.InitComboBox2Value);
                    await page.Keyboard.DownAsync("Enter");
                }



                if (!string.IsNullOrEmpty(searchInfo.AcceptUsageTermsBtnSelector))
                {
                    await page.WaitForSelectorAsync(searchInfo.AcceptUsageTermsBtnSelector);
                    await page.ClickAsync(searchInfo.AcceptUsageTermsBtnSelector);
                }

                if (!string.IsNullOrEmpty(searchInfo.SearchTypeInitDropdownSelector))
                    await page.SelectAsync(searchInfo.SearchTypeInitDropdownSelector, searchInfo.SearchTypeInitDropdownValue);

                if (searchInfo.IsSearchButtonHidden)
                {
                    await page.WaitForSelectorAsync(searchInfo.LastNameSelector);
                }
                else
                {
                    await page.WaitForSelectorAsync(searchInfo.SearchButtonSelector);
                }

                await page.FocusAsync(searchInfo.LastNameSelector);
                await page.Keyboard.TypeAsync(searchInfo.LastName);

                if (!string.IsNullOrEmpty(searchInfo.FirstNameSelector))
                {
                    await page.FocusAsync(searchInfo.FirstNameSelector);
                    await page.Keyboard.TypeAsync(searchInfo.FirstName);
                }

                if (!string.IsNullOrEmpty(searchInfo.DropdownSelector))
                    await page.SelectAsync(searchInfo.DropdownSelector, searchInfo.DropdownSelectValue);

                if (!string.IsNullOrEmpty(searchInfo.RadioSelector))
                    await page.ClickAsync(searchInfo.RadioSelector);


                await page.ClickAsync(searchInfo.SearchButtonSelector);

                if (!string.IsNullOrEmpty(searchInfo.SecondSelectDropdown))
                {
                    await page.ClickAsync(searchInfo.SecondSelectDropdown);
                }


                if (!string.IsNullOrEmpty(searchInfo.SecondSearchButtonSelector))
                {
                    await page.WaitForSelectorAsync(searchInfo.SecondSearchButtonSelector);
                    await page.ClickAsync(searchInfo.SecondSearchButtonSelector);
                }

                if (searchInfo.OpensInNewTab)
                {
                    var pages = await browserService.Browser.PagesAsync();
                    page = pages.LastOrDefault();
                }


                if (!string.IsNullOrEmpty(searchInfo.UniqueResultsSelector))
                {
                    await page.WaitForSelectorAsync(searchInfo.UniqueResultsSelector);
                }
                else
                {
                    await page.WaitForSelectorAsync(searchInfo.LicenseNumberSelector);
                }

                string licenseNumber, licenseStatus = string.Empty, licenseExpiration = string.Empty;
                var elLicenseNumber = await page.QuerySelectorAsync(searchInfo.LicenseNumberSelector);
                    licenseNumber = (await (await elLicenseNumber.GetPropertyAsync("textContent")).JsonValueAsync()).ToString() ?? "";

                    if (!string.IsNullOrEmpty(searchInfo.LicenseStatusSelector))
                    {
                        var elLicenseStatus = await page.QuerySelectorAsync(searchInfo.LicenseStatusSelector);
                        licenseStatus = (await (await elLicenseStatus.GetPropertyAsync("textContent")).JsonValueAsync()).ToString()?.Replace(" ", "").Trim() ?? "Not Available";
                    }

                    if (!string.IsNullOrEmpty(searchInfo.LicenseExpirationSelector))
                    {
                        var elLicenseExpiration = await page.QuerySelectorAsync(searchInfo.LicenseExpirationSelector);
                        licenseExpiration = (await (await elLicenseExpiration.GetPropertyAsync("textContent")).JsonValueAsync()).ToString()?.Replace(" ", "").Trim() ?? "Not Available";
                    }

                    return new LicenseInfo
                    {
                        State = searchInfo.StateName.ToUpperInvariant(),
                        LicenseNumber = licenseNumber,
                        LicenseStatus = licenseStatus,
                        LicenseExpiration = licenseExpiration
                    };
                }
                catch (Exception ex)
                {

                    return new LicenseInfo
                    {
                        State = searchInfo.StateName.ToUpperInvariant(),
                        ErrorMessage = ex.Message

                    };
                }
                finally
                {
                    if (context is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

            });
        }

        public SearchInfo GetArizonaLicenseInfo()
        {
            if (_isDOSearch)
                return GetArizonaLicenseDOInfo();

            var searchInfo = new SearchInfo
            {
                Url = "https://azbomprod.azmd.gov/glsuiteweb/clients/azbom/public/webverificationsearch.aspx",
                StateName = "Arizona",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr > td:nth-child(2)",
                SearchButtonSelector = "#ContentPlaceHolder1_btnName",
                LastNameSelector = "#ContentPlaceHolder1_txtLastName",
                FirstNameSelector = "#ContentPlaceHolder1_txtFirstName",
                SecondSearchButtonSelector = "td:nth-child(1) > a",
                OpensInNewTab = true,
                RadioSelector = "#ContentPlaceHolder1_rbName1"
            };

            return searchInfo;
        }

        public SearchInfo GetArizonaLicenseDOInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://azdo.portalus.thentiacloud.net/webs/portal/register/#/",
                StateName = "Arizona",
                LastName = $"{_firstName} {_lastName}",
                LicenseStatusSelector = "tr:last-child > td:nth-child(5)",
                LicenseNumberSelector = "tr:last-child > td:nth-child(1)",
                SearchButtonSelector = "button",
                LastNameSelector = "#keywords",
            };

            return searchInfo;
        }

        public SearchInfo GetPennsylvaniaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://www.pals.pa.gov/#!/page/search",
                StateName = "Pennsylvania",
                LastName = _lastName,
                FirstName = _firstName,
                LicenseStatusSelector = "tr:last-child > td:nth-child(5)",
                LicenseNumberSelector = "tr:last-child > td:nth-child(2)",
                SearchButtonSelector = "button:nth-child(3)",
                LastNameSelector = "#lName",
                FirstNameSelector = "#fName",
            };

            return searchInfo;
        }

        public SearchInfo GetRhodeIslandLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://healthri.mylicense.com/verification/",
                StateName = "Rhode Island",
                LastName = _lastName,
                FirstName = _firstName,
                LicenseStatusSelector = "tr:nth-child(2) > td:nth-child(5) > span",
                LicenseNumberSelector = "tr:nth-child(2) > td:nth-child(2) > span",
                SearchButtonSelector = "#sch_button",
                LastNameSelector = "#t_web_lookup__last_name",
                FirstNameSelector = "#t_web_lookup__first_name",
            };

            return searchInfo;
        }


        public SearchInfo GetCaliforniaLicenseInfo()
        {
            var url = "https://search.dca.ca.gov/";
            var elLicenseNumberSelector = "#lic0";
            var elLicenseStatusSelector = "#\\30  > footer > ul:nth-child(2) > li:nth-child(5)";
            var elLicenseExpirationDateSelector = "#\\30  > footer > ul:nth-child(2) > li:nth-child(7)";
            var firstNameSelector = "#firstName";
            var lastNameSelector = "#lastName";



            return new SearchInfo
            {
                Url = url,
                StateName = "California",
                LastName = _lastName,
                FirstName = _firstName,
                LastNameSelector = lastNameSelector,
                FirstNameSelector = firstNameSelector,
                SearchButtonSelector = "#srchSubmitHome",
                LicenseExpirationSelector = elLicenseExpirationDateSelector,
                LicenseNumberSelector = elLicenseNumberSelector,
                LicenseStatusSelector = elLicenseStatusSelector
            };
        }

        //public  SearchInfo GetColoradoLicenseInfo(IBrowser browser, IPage page, , string searchLicenseNumber)
        //{
        //    var url = "https://apps2.colorado.gov/dora/licensing/lookup/licenselookup.aspx";
        //    var elLicenseNumberSelector = "#Grid1 > tbody > tr > td:nth-child(1)";
        //    var elLicenseStatusSelector = "#Grid1 > tbody > tr > td:nth-child(4) > font";
        //    var elLicenseExpirationDateSelector = "#Grid1 > tbody > tr > td:nth-child(7)";
        //    var firstNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbFirstName_Contact";
        //    var lastNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbLastName_Contact";


        //    await page.GoToAsync(url);
        //    await page.WaitForSelectorAsync(firstNameSelector);
        //    await page.FocusAsync(firstNameSelector);
        //    await page.Keyboard.TypeAsync(firstName);
        //    await page.FocusAsync(lastNameSelector);
        //    await page.Keyboard.TypeAsync(lastName);
        //    await page.ClickAsync("#ctl00_MainContentPlaceHolder_ucLicenseLookup_btnLookup");

        //    var pages = await browser.PagesAsync();
        //    var popup = pages[pages.Length - 2];


        //    //todo:fox page problem
        //    await popup.ClickAsync(".btn-primary");
        //    pages = await browser.PagesAsync();
        //    popup = pages[pages.Length - 1];


        //    var elLicenseNumber = await popup.WaitForSelectorAsync(elLicenseNumberSelector);
        //    var licenseNumber = await (await elLicenseNumber.GetPropertyAsync("textContent")).JsonValueAsync();

        //    var elLicenseStatus = await popup.QuerySelectorAsync(elLicenseStatusSelector);
        //    var licenseStatus = await (await elLicenseStatus.GetPropertyAsync("textContent")).JsonValueAsync();

        //    var elLicenseExpirationDate = await popup.QuerySelectorAsync(elLicenseExpirationDateSelector);
        //    var licenseExpirationDate = await (await elLicenseExpirationDate.GetPropertyAsync("textContent")).JsonValueAsync();

        //    return new LicenseInfo
        //    {
        //        State = "Arizona",
        //        LicenseNumber = licenseNumber?.ToString() ?? string.Empty,
        //        LicenseStatus = licenseStatus?.ToString() ?? string.Empty,
        //        LicenseExpiration = licenseExpirationDate?.ToString() ?? string.Empty
        //    };
        //}

        //public  SearchInfo GetConnecticutLicenseInfo(IBrowser browser, IPage page, , string searchLicenseNumber)
        //{

        //    //todo: exact same as colorado. fix page issue
        //    var url = "https://www.elicense.ct.gov/Lookup/LicenseLookup.aspx";
        //    var elLicenseNumberSelector = "#Grid1 > tbody > tr > td:nth-child(1)";
        //    var elLicenseStatusSelector = "#Grid1 > tbody > tr > td:nth-child(4) > font";
        //    var elLicenseExpirationDateSelector = "#Grid1 > tbody > tr > td:nth-child(7)";
        //    var firstNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbFirstName_Contact";
        //    var lastNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbLastName_Contact";


        //    await page.GoToAsync(url);
        //    await page.WaitForSelectorAsync(firstNameSelector);
        //    await page.FocusAsync(firstNameSelector);
        //    await page.Keyboard.TypeAsync(firstName);
        //    await page.FocusAsync(lastNameSelector);
        //    await page.Keyboard.TypeAsync(lastName);
        //    await page.ClickAsync("#ctl00_MainContentPlaceHolder_ucLicenseLookup_btnLookup");

        //    var pages = await browser.PagesAsync();
        //    var popup = pages[pages.Length - 2];


        //    //todo:fox page problem
        //    await popup.ClickAsync(".btn-primary");
        //    pages = await browser.PagesAsync();
        //    popup = pages[pages.Length - 1];


        //    var elLicenseNumber = await popup.WaitForSelectorAsync(elLicenseNumberSelector);
        //    var licenseNumber = await (await elLicenseNumber.GetPropertyAsync("textContent")).JsonValueAsync();

        //    var elLicenseStatus = await popup.QuerySelectorAsync(elLicenseStatusSelector);
        //    var licenseStatus = await (await elLicenseStatus.GetPropertyAsync("textContent")).JsonValueAsync();

        //    var elLicenseExpirationDate = await popup.QuerySelectorAsync(elLicenseExpirationDateSelector);
        //    var licenseExpirationDate = await (await elLicenseExpirationDate.GetPropertyAsync("textContent")).JsonValueAsync();

        //    return new LicenseInfo
        //    {
        //        State = "Arizona",
        //        LicenseNumber = licenseNumber?.ToString() ?? string.Empty,
        //        LicenseStatus = licenseStatus?.ToString() ?? string.Empty,
        //        LicenseExpiration = licenseExpirationDate?.ToString() ?? string.Empty
        //    };
        //}


        /// <summary>
        /// Works for North Carolina, Florida, New Mexico, Oregon, North Dakota, Maryland, Georgia, and Kansas
        /// </summary>
        /// <param name="page"></param>
        /// <param name="stateName"></param>
        /// <param name="firstName"></param>
        /// <param name=lastName></param>
        /// <returns></returns>
        //public  SearchInfo GetDocFinderLicenseInfo(string stateName, )
        //{

        //    try
        //    {
        //        stateName = stateName.ToUpperInvariant();

        //        var url = "http://docfinder.docboard.org/docfinder.html";
        //        var optionSelector = $@"select";
        //        var elLicenseNumberSelector = @"body > b > b > b > center > b > center > table > tbody > tr:nth-child(3) > td:nth-child(2)";
        //        var elLicenseStatusSelector = "tr:nth-child(6) > td:nth-child(2)";
        //        var srchBtnSelector = @"body > center > table:nth-child(3) > tbody > tr > td:nth-child(1) > table > tbody > tr:nth-child(3) > td > font > form > input[type=submit]:nth-child(6)";
        //        var firstNameSelector = @"[name='medfname']";
        //        var lastNameSelector = @"[name='medlname']";
        //        var secondSrchBtnSelector = @"body > b > b > b > center > form > input[type=submit]:nth-child(7)";



        //        var launchOptions = new LaunchOptions
        //        {
        //            Headless = true // = false for testing
        //        };
        //        await new BrowserFetcher().DownloadAsync();
        //        using var browser = await Puppeteer.LaunchAsync(launchOptions);
        //        using var page = await browser.NewPageAsync();
        //        await page.GoToAsync(url);

        //        await page.QuerySelectorAsync(firstNameSelector);
        //        await page.FocusAsync(firstNameSelector);
        //        await page.Keyboard.TypeAsync(firstName);
        //        await page.FocusAsync(lastNameSelector);
        //        await page.Keyboard.TypeAsync(lastName);
        //        await page.ClickAsync(srchBtnSelector);

        //        await page.WaitForSelectorAsync(optionSelector);
        //        await page.ClickAsync($"option[value*='{stateName}']");
        //        await page.ClickAsync(secondSrchBtnSelector);


        //        var elLicenseNumber = await page.WaitForSelectorAsync(elLicenseNumberSelector);
        //        var licenseNumber = await (await elLicenseNumber.GetPropertyAsync("textContent")).JsonValueAsync();
        //        var elLicenseStatus = await page.QuerySelectorAsync(elLicenseStatusSelector);
        //        var licenseStatus = await (await elLicenseStatus.GetPropertyAsync("textContent")).JsonValueAsync();

        //        return new LicenseInfo
        //        {
        //            State = stateName,
        //            LicenseNumber = licenseNumber?.ToString() ?? string.Empty,
        //            LicenseStatus = licenseStatus?.ToString() ?? string.Empty,
        //            LicenseExpiration = "Unavailable"
        //        };

        //    }
        //    catch (Exception ex)
        //    {
        //        return new LicenseInfo
        //        {
        //            State = stateName,
        //            ErrorMessage = ex.Message
        //        };

        //    }



        //}

        public SearchInfo GetArkansasLicenseInfo()
        {
            var url = "https://www.armedicalboard.org/public/verify/default.aspx";
            var radioSelector = @"#ctl00_MainContentPlaceHolder_ucVerifyLicense_rbVerifyLicenseSearch_1";
            var elLicenseNumberSelector = @"#ctl00_MainContentPlaceHolder_lvResultsLicInfo_ctrl0_lblLicnumInfo";
            var elLicenseStatusSelector = @"#ctl00_MainContentPlaceHolder_lvResultsLicInfo_ctrl0_lblStatusInfo";
            var expirationSelector = @"#ctl00_MainContentPlaceHolder_lvResultsLicInfo_ctrl0_lblEndDateInfo";
            var srchBtnSelector = @"#ctl00_MainContentPlaceHolder_ucVerifyLicense_btnVerifyLicense";
            var lastNameSelector = @"#ctl00_MainContentPlaceHolder_ucVerifyLicense_txtVerifyLicNumLastName";
            var secondSrchBtnSelector = @"#ctl00_MainContentPlaceHolder_gvVerifyLicenseResultsLookup_ctl02_VerifySelectFindLicense";

            var searchInfo = new SearchInfo
            {
                Url = url,
                StateName = "Arkansas",
                LastName = _lastName,
                LicenseExpirationSelector = expirationSelector,
                LicenseStatusSelector = elLicenseStatusSelector,
                LicenseNumberSelector = elLicenseNumberSelector,
                SearchButtonSelector = srchBtnSelector,
                LastNameSelector = lastNameSelector,
                RadioSelector = radioSelector,
                SecondSearchButtonSelector = secondSrchBtnSelector,
            };

            return searchInfo;

        }


        public SearchInfo GetKentuckyLicenseInfo()
        {
            var url = "https://web1.ky.gov/GenSearch/LicenseSearch.aspx?AGY=5";
            var elLicenseNumberSelector = @"#Form1 > div.ky-content > div > div.ky-cm-content > div:nth-child(7) > div.cols2-col2";
            var elLicenseStatusSelector = @"#Form1 > div.ky-content > div > div.ky-cm-content > div:nth-child(8) > div.cols2-col2";
            var expirationSelector = @"#Form1 > div.ky-content > div > div.ky-cm-content > div:nth-child(9) > div.cols2-col2";
            var srchBtnSelector = @"#usLicenseSearch_btnSearch";
            var lastNameSelector = @"#usLicenseSearch_txtField1";

            var searchInfo = new SearchInfo
            {
                Url = "https://web1.ky.gov/GenSearch/LicenseSearch.aspx?AGY=5",
                LastName = _lastName,
                LastNameSelector = lastNameSelector,
                LicenseNumberSelector = elLicenseNumberSelector,
                LicenseExpirationSelector = expirationSelector,
                LicenseStatusSelector = elLicenseStatusSelector,
                SearchButtonSelector = srchBtnSelector,
                StateName = "Kentucky",
            };

            return searchInfo;

        }

        public SearchInfo GetLouisianaSiteLicenseInfo()
        {
            var url = "https://online.lasbme.org/#/verifylicense";

            var elLicenseNumberSelector = "tr:last-child > td:nth-child(2)";
            var elLicenseStatusSelector = @"tr:last-child > td:nth-child(3)";
            // var expirationSelector = @"tr:last-child > td:nth-child(6)";

            var srchBtnSelector = @"input.btn.btn-success";
            //var secondSrchSelector = @"body > div:nth-child(1) > div.middle-content.ng-scope > div > div > div > div.searchoption.mt30.ng-scope > div:nth-child(2) > table > tbody > tr > td:nth-child(8) > input";

            var lastNameSelector = @"div:nth-child(7) > div > input";
            var firstNameSelector = @"div:nth-child(6) > div > input";


            var searchInfo = new SearchInfo
            {
                Url = url,
                StateName = "Louisiana",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseStatusSelector = elLicenseStatusSelector,
                LicenseNumberSelector = elLicenseNumberSelector,
                SearchButtonSelector = srchBtnSelector,
                FirstNameSelector = firstNameSelector,
                LastNameSelector = lastNameSelector,
            };

            return searchInfo;

        }

        public SearchInfo GetMaineSiteLicenseInfo()
        {
            var url = "https://www.pfr.maine.gov/ALMSOnline/ALMSQuery/SearchIndividual.aspx";

            var elLicenseNumberSelector = "tr:last-child > td:nth-child(2)";
            var elLicenseStatusSelector = "tr:last-child > td:nth-child(5)";
            //  var expirationSelector = @"#detailPage > div > div:nth-child(3) > div.DetailGroup.Attributes > div:nth-child(4) > div:nth-child(2)";

            var ddlSelector = @"#scRegulator";

            var srchBtnSelector = @"#btnSearch";
            // var secondSrchSelector = @"#gvLicensees > tbody > tr > td:nth-child(1) > a";

            var lastNameSelector = @"#scLastName";
            var firstNameSelector = @"#scFirstName";

            var searchInfo = new SearchInfo
            {
                Url = url,
                StateName = "Maine",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseStatusSelector = elLicenseStatusSelector,
                LicenseNumberSelector = elLicenseNumberSelector,
                SearchButtonSelector = srchBtnSelector,
                FirstNameSelector = firstNameSelector,
                LastNameSelector = lastNameSelector,
                DropdownSelector = ddlSelector,
                DropdownSelectValue = "",
            };

            return searchInfo;
        }


        public SearchInfo GetMassachussettsSiteLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://madph.mylicense.com/verification/",
                StateName = "Massachussetts",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#t_web_lookup__last_name",
                FirstNameSelector = @"#t_web_lookup__first_name",
                SearchButtonSelector = "#sch_button",
                LicenseNumberSelector = "tr:nth-child(2) > td:nth-child(2) > span",
                LicenseStatusSelector = "tr:nth-child(2) > td:nth-child(4) > span",
                DropdownSelector = @"#t_web_lookup__license_status_name",
                DropdownSelectValue = "Current"
            };

            return searchInfo;
        }

        public SearchInfo GetMichiganSiteLicenseInfo()
        {
            var url = "https://aca-prod.accela.com/MILARA/GeneralProperty/PropertyLookUp.aspx?isLicensee=Y&TabName=APO";

            var elLicenseNumberSelector = @"#ctl00_PlaceHolderMain_licenseeGeneralInfo_lblLicenseeNumber_value";
            var elLicenseStatusSelector = @"#ctl00_PlaceHolderMain_licenseeGeneralInfo_lblBusinessName2_value";
            var expirationSelector = @"#ctl00_PlaceHolderMain_licenseeGeneralInfo_lblExpirationDate_value";
            var srchBtnSelector = @"#ctl00_PlaceHolderMain_btnNewSearch";
            var lastNameSelector = @"#ctl00_PlaceHolderMain_refLicenseeSearchForm_txtLastName";
            var firstNameSelector = @"#ctl00_PlaceHolderMain_refLicenseeSearchForm_txtFirstName";

            var searchInfo = new SearchInfo
            {
                Url = url,
                StateName = "Michigan",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseExpirationSelector = expirationSelector,
                LicenseStatusSelector = elLicenseStatusSelector,
                LicenseNumberSelector = elLicenseNumberSelector,
                SearchButtonSelector = srchBtnSelector,
                FirstNameSelector = firstNameSelector,
                LastNameSelector = lastNameSelector,
            };

            return searchInfo;
        }

        public SearchInfo GetMinnesotaSiteLicenseInfo()
        {
            var url = "http://docfinder.docboard.org/mn/df/mndf.htm";

            var elLicenseNumberSelector = @"body > b > b > b > center > b > center:nth-child(1) > table > tbody > tr:nth-child(2) > td:nth-child(2)";
            var elLicenseStatusSelector = @"body > b > b > b > center > b > center:nth-child(1) > table > tbody > tr:nth-child(4) > td:nth-child(2)";
            var expirationSelector = @"body > b > b > b > center > b > center:nth-child(1) > table > tbody > tr:nth-child(6) > td:nth-child(2)";
            var srchBtnSelector = @"body > blockquote > b > form > font > input[type=submit]:nth-child(17)";
            var lastNameSelector = @"body > blockquote > b > form > font > input[type=text]:nth-child(7)";
            var firstNameSelector = @"body > blockquote > b > form > font > input[type=text]:nth-child(9)";

            var searchInfo = new SearchInfo
            {
                Url = url,
                StateName = "Minnesota",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseExpirationSelector = expirationSelector,
                LicenseStatusSelector = elLicenseStatusSelector,
                LicenseNumberSelector = elLicenseNumberSelector,
                SearchButtonSelector = srchBtnSelector,
                FirstNameSelector = firstNameSelector,
                LastNameSelector = lastNameSelector,
            };

            return searchInfo;
        }

        public SearchInfo GetNewJerseyLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://newjersey.mylicense.com/verification/Search.aspx",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#t_web_lookup__last_name",
                FirstNameSelector = @"#t_web_lookup__first_name",
                SearchButtonSelector = @"#sch_button",
                SecondSearchButtonSelector = @"#datagrid_results__ctl3_name",
                LicenseNumberSelector = @"#license_no",
                LicenseExpirationSelector = @"#expiration_date",
                LicenseStatusSelector = @"#sec_lic_status",
                StateName = "New Jersey"
            };


            return searchInfo;
        }

        public SearchInfo GetOhioLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://elicense.ohio.gov/oh_verifylicense",
                StateName = "Ohio",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = "#j_id0\\:j_id110\\:lastName",
                FirstNameSelector = "#j_id0\\:j_id110\\:firstName",
                SearchButtonSelector = ".searchButton",
                LicenseNumberSelector = "tr:last-child > td.LicenseEndorsementNumber > div",
                LicenseStatusSelector = "tr:last-child > td.Status"

            };

            return searchInfo;
        }



        public SearchInfo GetTexasLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://profile.tmb.state.tx.us/Search.aspx",
                StateName = "Texas",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#BodyContent_tbLastName",
                FirstNameSelector = @"#BodyContent_tbFirstName",
                SearchButtonSelector = @"#BodyContent_btnSearch",
                SecondSearchButtonSelector = @"#BodyContent_gvSearchResults > tbody > tr:nth-child(2) > td:nth-child(1) > a",
                LicenseNumberSelector = @"#BodyContent_lblLicenseNumber",
                LicenseExpirationSelector = @"#BodyContent_lblLicenseExpirationDate",
                LicenseStatusSelector = @"#BodyContent_lblRegStatus",
                AcceptUsageTermsBtnSelector = @"#BodyContent_btnAccept",
                IsActiveLicenseSelector = @"#BodyContent_cbActiveLicensesOnly"
            };

            return searchInfo;
        }

        public SearchInfo GetVermontLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://secure.professionals.vermont.gov/prweb/PRServletCustom/app/NGLPGuestUser_/V9csDxL3sXkkjMC_FR2HrA*/!STANDARD?UserIdentifier=LicenseLookupGuestUser",
                StateName = "Vermont",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = "#\\36 9f331dd",
                FirstNameSelector = "#\\33 86be106",
                SearchButtonSelector = "#RULE_KEY > div > div.content-item.content-field.item-1.remove-top-spacing.remove-left-spacing.flex.flex-row.dataValueRead > span > button",
                LicenseNumberSelector = "td:nth-child(1) > div > span",
                LicenseExpirationSelector = "td:nth-child(7) > div > span",
                LicenseStatusSelector = @"td:nth-child(3) > div > span",
            };

            return searchInfo;
        }

        public SearchInfo GetVirginiaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://dhp.virginiainteractive.org/lookup/index",
                StateName = "Virginia",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#LName",
                FirstNameSelector = @"#FName",
                SearchButtonSelector = @"body > div.body-bg > div.container.body-content > div.panel.panel-info > div.panel-body > form:nth-child(8) > div:nth-child(7) > div:nth-child(3) > input.btn.btn-primary",
                SecondSearchButtonSelector = @"body > div.body-bg > div.container.body-content > div:nth-child(6) > div.panel-body > table > tbody > tr:nth-child(2) > td:nth-child(1) > a",
                LicenseNumberSelector = @"body > div.body-bg > div.container.body-content > div.panel.panel-info > div.panel-body > table > tbody > tr:nth-child(1) > td",
                LicenseExpirationSelector = @"body > div.body-bg > div.container.body-content > div.panel.panel-info > div.panel-body > table > tbody > tr:nth-child(6) > td",
                LicenseStatusSelector = @"body > div.body-bg > div.container.body-content > div.panel.panel-info > div.panel-body > table > tbody > tr:nth-child(7) > td",
                AcceptUsageTermsBtnSelector = @"",
                IsActiveLicenseSelector = @""
            };

            return searchInfo;
        }

        public SearchInfo GetWestVirginiaLicenseInfo()
        {
            if (_isDOSearch)
                return GetWestVirginiaDOLicenseInfo();

            var searchInfo = new SearchInfo
            {
                Url = "https://wvbom.wv.gov/public/search/",
                StateName = "West Virginia",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#inputLastName",
                FirstNameSelector = @"#inputFirstName",
                SearchButtonSelector = @"#licName > div.col-md-offset-5.col-md-3 > button",
                LicenseNumberSelector = @"#frmMain > table:nth-child(3) > tbody > tr > td:nth-child(2)",
                LicenseExpirationSelector = @"#frmMain > table:nth-child(3) > tbody > tr > td:nth-child(5)",
                LicenseStatusSelector = @"#frmMain > table:nth-child(3) > tbody > tr > td:nth-child(3)",
                AcceptUsageTermsBtnSelector = @"",
                IsActiveLicenseSelector = @"",
                SearchTypeInitDropdownSelector = @"#selectType",
                SearchTypeInitDropdownValue = @"Name"

            };

            return searchInfo;
        }

        public SearchInfo GetWestVirginiaDOLicenseInfo()
        {
            //todo: no results found for sw.
            var searchInfo = new SearchInfo
            {
                Url = "https://www.wvbdosteo.org/verify/",
                StateName = "West Virginia",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#lName",
                SearchButtonSelector = @".btn-primary",
                LicenseNumberSelector = @"#printcontent > table:nth-child(1) > tbody > tr > td:nth-child(3)",
                LicenseExpirationSelector = @"#printcontent > table:nth-child(1) > tbody > tr > td:nth-child(4)",
                LicenseStatusSelector = "#printcontent > table:nth-child(1) > tbody > tr > td:nth-child(4)",
                DropdownSelector = @"#licType",
                DropdownSelectValue = @"BoardXP"
            };

            return searchInfo;
        }

        public SearchInfo GetWyomingLicenseInfo()
        {
            //todo: no results found for sw.
            var searchInfo = new SearchInfo
            {
                Url = "https://wybomprod.glsuite.us/GLSuiteWeb/Clients/WYBOM/Public/LicenseeSearch.aspx?SearchType=Physician",
                StateName = "Wyoming",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#ContentPlaceHolder1_txtLastName",
                FirstNameSelector = @"#ContentPlaceHolder1_txtFirstName",
                SearchButtonSelector = @"#ContentPlaceHolder1_btnSubmit",
                LicenseNumberSelector = @"#ContentPlaceHolder1_dtgResults > tbody > tr.Georgia11 > td:nth-child(4)",
                LicenseExpirationSelector = @"#ContentPlaceHolder1_dtgResults > tbody > tr.Georgia11 > td:nth-child(7)"
            };

            return searchInfo;
        }

        public SearchInfo GetDistrictOfColumbiaLicenseInfo()
        {
            //todo: no results found for sw.
            var searchInfo = new SearchInfo
            {
                Url = "https://dohenterprise.my.site.com/ver/s/",
                StateName = "Washington, D.C.",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#LastName",
                FirstNameSelector = @"#FirstName",
                SearchButtonSelector = "div > a:nth-child(15)",
                LicenseNumberSelector = "tr:last-child > td:nth-child(2)",
                LicenseExpirationSelector = "tr:last-child > td:nth-child(6)",
                LicenseStatusSelector = "tr:last-child > td:nth-child(4)",
                DropdownSelector = @"#Status",
                DropdownSelectValue = "Active",
            };

            return searchInfo;
        }

        public SearchInfo GetDelawareLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://delpros.delaware.gov/OH_VerifyLicense",
                StateName = "Delaware",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = "#j_id0\\:j_id111\\:lastName",
                FirstNameSelector = "#j_id0\\:j_id111\\:firstName",
                SearchButtonSelector = @".searchButton",
                SecondSearchButtonSelector = @".expand",
                LicenseNumberSelector = @".expand",
                LicenseStatusSelector = @"td.Status"
            };

            return searchInfo;
        }

        public SearchInfo GetIdahoLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://mylicense.in.gov/everification/",
                StateName = "Indiana",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = "#t_web_lookup__last_name",
                FirstNameSelector = "#t_web_lookup__first_name",
                SearchButtonSelector = @"#sch_button",
                LicenseNumberSelector = "td:nth-child(2) > span",
                LicenseStatusSelector = "td:nth-child(5) > span",
            };

            return searchInfo;
        }

        public SearchInfo GetIndianaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://apps-dopl.idaho.gov/IBOMPublic/LPRBrowser.aspx",
                StateName = "Indiana",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = "#CPH1_txtsrcApplicantLastName",
                FirstNameSelector = "#CPH1_txtsrcApplicantFirstName",
                SearchButtonSelector = @"#CPH1_btnGoFind",
                LicenseNumberSelector = "#CPH1_myDataGrid > tbody > tr.GridItemStyle > td:nth-child(3) > a",
                LicenseStatusSelector = @"#CPH1_myDataGrid > tbody > tr.GridItemStyle > td:nth-child(5)",
                LicenseExpirationSelector = "#CPH1_myDataGrid > tbody > tr.GridItemStyle > td:nth-child(4)",
            };

            return searchInfo;
        }

        public SearchInfo GetIowaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://amanda-portal.idph.state.ia.us/ibm/portal/#/license/license_query",
                StateName = "Iowa",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = "input[name=lastname]",
                FirstNameSelector = "input[name=firstname]",
                SearchButtonSelector = "button.mat-primary",
                LicenseNumberSelector = "mat-cell.mat-cell.cdk-cell.cdk-column-ReferenceFile.mat-column-ReferenceFile.ng-star-inserted > div",
                LicenseStatusSelector = "mat-cell.mat-cell.cdk-cell.cdk-column-StatusDesc.mat-column-StatusDesc.ng-star-inserted > div"

            };


            return searchInfo;
        }

        public SearchInfo GetMissouriLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://pr.mo.gov/licensee-search-division.asp",
                StateName = "Missouri",
                // FirstName = _firstName,
                LastName = $"{_lastName}, {_firstName}",
                LastNameSelector = "div:nth-child(4) > input",
                SearchButtonSelector = "button.btn.btn-primary",
                LicenseNumberSelector = "tr:nth-child(3) > td:nth-child(2)",
                LicenseExpirationSelector = "tr:nth-child(4) > td:nth-child(2)",
                SecondSearchButtonSelector = "tr:nth-child(2) > td:nth-child(5) > a",
                RadioSelector = "#optionsRadios2"


            };

            return searchInfo;
        }

        public SearchInfo GetNevadaLicenseInfo()
        {
            if (_isDOSearch)
                return GetNevadaDOLicenseInfo();


            var searchInfo = new SearchInfo
            {
                Url = "https://nsbme.us.thentiacloud.net/webs/nsbme/register",
                StateName = "Nevada",
                LastName = $"{_firstName} {_lastName}",
                LastNameSelector = "#keywords",
                SearchButtonSelector = "div.input-group.hd-search > div > button",
                LicenseNumberSelector = "div.hd-box-container.profile > div:nth-child(2) > div",
                LicenseStatusSelector = "div.hd-box-container.profile > div:nth-child(4) > div",
                LicenseExpirationSelector = "div.hd-box-container.profile > div:nth-child(6) > div",
                SecondSearchButtonSelector = "tr > td:nth-child(10) > a"
            };


            return searchInfo;
        }


        public SearchInfo GetNevadaDOLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://nsbom.portalus.thentiacloud.net/webs/portal/register/#/",
                StateName = "Nevada",
                // FirstName = _firstName,
                LastName = $"{_firstName} {_lastName}",
                LastNameSelector = "#keywords",
                SearchButtonSelector = "button > span",
                LicenseNumberSelector = "tr > td:nth-child(1)",
                LicenseStatusSelector = "tr > td:nth-child(6)",
                LicenseExpirationSelector = "td:nth-child(7)"
            };


            return searchInfo;
        }

        public SearchInfo GetNewHampshireLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://forms.nh.gov/licenseverification/",
                StateName = "New Hampshire",
                LastName = _lastName,
                FirstName = _firstName,
                LastNameSelector = "#t_web_lookup__last_name",
                FirstNameSelector = "#t_web_lookup__first_name",
                SearchButtonSelector = "#sch_button",
                LicenseNumberSelector = "td:nth-child(4) > span",
                LicenseStatusSelector = "td:nth-child(5) > span"
            };


            return searchInfo;
        }


        public SearchInfo GetNewYorkLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://www.op.nysed.gov/verification-search",
                StateName = "New York",
                LastName = $"{_firstName} {_lastName}",
                LastNameSelector = "#searchInput",
                SearchButtonSelector = "#goButton",
                LicenseNumberSelector = "td:nth-child(4) > span",
                LicenseStatusSelector = "td:nth-child(5) > span",
                InitComboBoxSelector = "#vs12-combobox",
                InitComboBoxValue = "Licensee Name",
                InitComboBox2Selector = "#vs19-combobox",
                InitComboBox2Value = "All",
                IsSearchButtonHidden = true,
            };


            return searchInfo;
        }


        public SearchInfo GetFloridaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://mqa-internet.doh.state.fl.us/MQASearchServices/HealthCareProviders",
                StateName = "florida",
                LastName = _lastName,
                FirstName = _firstName,

                LastNameSelector = "#SearchDto_LastName",
                FirstNameSelector = "#SearchDto_FirstName",
                SearchButtonSelector = "fieldset > p > input",
                LicenseNumberSelector = "tr:last-child > td:nth-child(1) > a",
                LicenseStatusSelector = "tr:last-child > td:nth-child(5)"
            };

            return searchInfo;
        }

        public SearchInfo GetNorthCarolinaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://portal.ncmedboard.org/verification/search.aspx",
                StateName = "North Carolina",
                LastName = _lastName,
                FirstName = _firstName,

                FirstNameSelector = "#txtFirst",
                LastNameSelector = "#txtLast",
                SearchButtonSelector = "#btnSubmit",
                LicenseNumberSelector = "tr:last-child > td:nth-child(4)",
                LicenseStatusSelector = "tr:last-child > td:nth-child(5) > span",
            };

            return searchInfo;
        }

        public SearchInfo GetNewMexicoLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "http://docfinder.docboard.org/nm/",
                StateName = "New mexico",
                LastName = _lastName,
                FirstName = _firstName,

                FirstNameSelector = "input[type=text]:nth-child(7)",
                LastNameSelector = "input[type=text]:nth-child(5)",
                SearchButtonSelector = "input[type=submit]",
                LicenseNumberSelector = "tr:nth-child(2) > td:nth-child(4) > font",
                LicenseStatusSelector = "tr:nth-child(3) > td:nth-child(4) > font",
                LicenseExpirationSelector = "tr:nth-child(5) > td:nth-child(4) > font"
            };


            return searchInfo;
        }

        // public  SearchInfo GetOregonLicenseInfo() => await GetDocFinderLicenseInfo("Oregon", _firstName, lastName);
        //    public  SearchInfo GetNorthDakotaLicenseInfo() => await GetDocFinderLicenseInfo("North Dakota", _firstName, lastName);
        public SearchInfo GetKansasLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://www.kansas.gov/ssrv-ksbhada/search.html",
                StateName = "kansas",
                LastName = _lastName,
                FirstName = _firstName,
                LastNameSelector = "#lastName",
                FirstNameSelector = "#firstName",
                SearchButtonSelector = "#id_submit",
                LicenseNumberSelector = "tr:last-child > td:nth-child(3)",
                LicenseStatusSelector = "tr:last-child > td:nth-child(5)",
            };

            return searchInfo;
        }

        public SearchInfo GetMarylandLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://www.mbp.state.md.us/bpqapp/",
                StateName = "maryland",
                LastName = _lastName,
                FirstName = _firstName,
                LastNameSelector = "#LastName",
                SearchButtonSelector = "#btnLastName",
                LicenseNumberSelector = "#Lic_no",
                LicenseStatusSelector = "#Lic_Status",
                LicenseExpirationSelector = "#Expiration_Date",
                SecondSelectDropdown = "#listbox_Names > option",
                SecondSearchButtonSelector = "#Btn_Name"
            };


            return searchInfo;
        }
    }

}


