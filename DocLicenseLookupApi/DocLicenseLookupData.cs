﻿using Polly.Retry;
using Polly;
using PuppeteerSharp;
using System.Collections.Concurrent;
using AntiCaptchaAPI;
using System.Diagnostics;
using TwoCaptcha.Captcha;
using System.Reflection.Metadata;

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
        (retryAttempt) =>
        {
            Debug.WriteLine($"Retrying attempt {retryAttempt}.");
            return TimeSpan.FromSeconds(0); // Exponential back-off
        },
        onRetry: (exception, timespan) =>
        {
            Debug.WriteLine($"Retry scheduled after {timespan.TotalSeconds} seconds due to: {exception.Message}");
        });
        }


        private async Task<IEnumerable<LicenseInfo>> GetLicenseInfoForSelectedStates(List<SearchInfo> statesList)
        {
            var resultsList = new List<LicenseInfo>();

            using (var browserService = new BrowserService())
            {
                await browserService.InitializeAsync();

                var maxConcurrentTasks = 20;
                var semaphore = new SemaphoreSlim(maxConcurrentTasks);

                var tasks = statesList.Select(async state =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var licenseInfo = await GetLicenseInfoFromBasicSite(browserService, state);
                        resultsList.AddRange(licenseInfo);
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


        public async Task<IEnumerable<LicenseInfo>> GetGroupOne()
        {
            var statesList = GetGroupOneSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupTwo()
        {
            var statesList = GetGroupTwoSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupThree()
        {
            var statesList = GetGroupThreeSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupFour()
        {
            var statesList = GetGroupFourSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupFive()
        {
            var statesList = GetGroupFiveSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupSix()
        {
            var statesList = GetGroupSixSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupSeven()
        {
            var statesList = GetGroupSevenSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupEight()
        {
            var statesList = GetGroupEightSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupNine()
        {
            var statesList = GetGroupNineSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        public async Task<IEnumerable<LicenseInfo>> GetGroupTen()
        {
            var statesList = GetGroupTenSearchInfo();

            return await GetLicenseInfoForSelectedStates(statesList);
        }

        private List<SearchInfo> GetGroupOneSearchInfo()
        {
            return new List<SearchInfo>
           {
                GetAlabamaLicenseInfo(),
                GetAlaskaLicenseInfo(),
                GetArizonaLicenseInfo(),
                GetArkansasLicenseInfo(),
                GetCaliforniaLicenseInfo()
            };
        }

        private List<SearchInfo> GetGroupTwoSearchInfo()
        {
            return new List<SearchInfo>
            {
                GetColoradoLicenseInfo(),
                GetConnecticutLicenseInfo(),
                GetDelawareLicenseInfo(),
                GetDistrictOfColumbiaLicenseInfo(),
                GetFloridaLicenseInfo()
            };
        }

        private List<SearchInfo> GetGroupThreeSearchInfo()
        {
            return new List<SearchInfo>
            {
                GetGeorgiaLicenseInfo(),
                GetHawaiiLicenseInfo(),

                GetIllinoisLicenseInfo(),
                GetIndianaLicenseInfo(),
            };
        }
        //todo: fix group 4
        private List<SearchInfo> GetGroupFourSearchInfo()
        {
            return new List<SearchInfo>
           {
              GetIowaLicenseInfo(),
              GetKansasLicenseInfo(),
              GetKentuckyLicenseInfo(),
              GetLouisianaSiteLicenseInfo(),
               GetMaineSiteLicenseInfo(),
            };
        }

        //tdo: michigan and minnesota failing on multi search
        private List<SearchInfo> GetGroupFiveSearchInfo()
        {
            return new List<SearchInfo>
           {
              GetMarylandLicenseInfo(),
           GetMassachussettsSiteLicenseInfo(),
              GetMichiganSiteLicenseInfo(),
          GetMinnesotaSiteLicenseInfo(),
          GetMississippiLicenseInfo(),
            };
        }

        //todo: fix group six on multi result search. nevADA failing
        private List<SearchInfo> GetGroupSixSearchInfo()
        {
            return new List<SearchInfo>
           {
                GetMissouriLicenseInfo(),

         GetNebraskaLicenseInfo(),
             GetNevadaLicenseInfo(),
              GetNewHampshireLicenseInfo(),
            };
        }

        private List<SearchInfo> GetGroupSevenSearchInfo()
        {
            return new List<SearchInfo>
           {
                GetNewJerseyLicenseInfo(),
                GetNewMexicoLicenseInfo(),
          GetNewYorkLicenseInfo(),
           GetNorthCarolinaLicenseInfo(),
        
            };
        }

        private List<SearchInfo> GetGroupEightSearchInfo()
        {
            return new List<SearchInfo>
           {
                    GetOhioLicenseInfo(),
        GetPennsylvaniaLicenseInfo(),
              GetRhodeIslandLicenseInfo(),
         GetSouthCarolinaLicenseInfo(),
          GetTennesseeLicenseInfo()
            };
        }

        private List<SearchInfo> GetGroupNineSearchInfo()
        {
            return new List<SearchInfo>
           {
                  GetTexasLicenseInfo(),
             GetUtahLicenseInfo(),
               GetVermontLicenseInfo(),
                 GetNorthDakotaLicenseInfo(),

             GetWestVirginiaLicenseInfo(),
            };
        }

        private List<SearchInfo> GetGroupTenSearchInfo()
        {
            return new List<SearchInfo>
           {
                  GetMontanaLicenseInfo(),

              GetWyomingLicenseInfo(),
              GetWisconsinLicenseInfo(),
              
                GetVirginiaLicenseInfo(),
                   GetIdahoLicenseInfo(),

            };
        }



        public async Task<List<LicenseInfo>> GetLicenseInfoFromBasicSite(BrowserService browserService, SearchInfo searchInfo)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var context = await browserService.Browser.CreateIncognitoBrowserContextAsync();
                var licenses = new List<LicenseInfo>();
                try
                {
                    var page = await context.NewPageAsync();

                    // await page.SetJavaScriptEnabledAsync(false);

                    if (!searchInfo.HasImageRecaptcha &&
                    searchInfo.StateName.ToLowerInvariant() != "new york"
                    && searchInfo.StateName.ToLowerInvariant() != "vermont"
                    && searchInfo.StateName.ToLowerInvariant() != "washington, d.c."
                    && searchInfo.StateName.ToLowerInvariant() != "maine"
                     && searchInfo.StateName.ToLowerInvariant() != "connecticut"
                     && searchInfo.StateName.ToLowerInvariant() != "alaska"
                     && searchInfo.StateName.ToLowerInvariant() != "pennsylvania")
                    {
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
                    }
                    //    await page.SetViewportAsync(new ViewPortOptions { Width = 1280, Height = 800 });
                    await page.GoToAsync(searchInfo.Url);



                    if (searchInfo.StateName.ToLower() == "connecticut")
                        await page.ReloadAsync();

                    if (searchInfo.StateName.ToLower() == "new york")
                    {
                        try
                        {
                            await page.EvaluateExpressionAsync("document.querySelector('body > div:nth-child(17) > div:nth-child(1)').click()");
                        }
                        catch { }

                        try
                        {
                            await page.EvaluateExpressionAsync("document.querySelector('body > div:nth-child(18) > div:nth-child(1)').click()");
                        }
                        catch { }

                        try
                        {
                            await page.EvaluateExpressionAsync("document.querySelector('body > div:nth-child(19) > div:nth-child(1)').click()");
                            ;
                        }
                        catch { }
                    }
                    //      await page.EvaluateExpressionAsync("document.querySelector('body > div:nth-child(19)').click()");

                    if (searchInfo.HasInitModal)
                    {
                        await Task.Delay(1000);
                        await page.WaitForSelectorAsync(searchInfo.AcceptTermsAndConditionsCheckboxSelector);
                        await page.ClickAsync(searchInfo.AcceptTermsAndConditionsCheckboxSelector);

                        await page.Keyboard.PressAsync("Enter");
                        await Task.Delay(1000);
                        //await page.ClickAsync(searchInfo.AcceptUsageTermsBtnSelector);
                    }

                    if (searchInfo.HasInitRecaptcha2)
                    {
                        await SolveRecaptcha2(page, searchInfo.Url, searchInfo.SiteKey, searchInfo.StateName, searchInfo.RecaptchaCallback, searchInfo.IsRecaptchaAfterSearchBtnClick, searchInfo.CustomReptchaResponseId);
                    }


                    if (!string.IsNullOrEmpty(searchInfo.InitComboBoxSelector))
                    {
                        await page.WaitForSelectorAsync(searchInfo.InitComboBoxSelector);
                        await page.FocusAsync(searchInfo.InitComboBoxSelector);
                        await page.Keyboard.TypeAsync(searchInfo.InitComboBoxValue);
                        await page.Keyboard.DownAsync("Enter");
                        await Task.Delay(1000);
                    }

                    if (!string.IsNullOrEmpty(searchInfo.InitComboBox2Selector))
                    {
                        await page.FocusAsync(searchInfo.InitComboBox2Selector);
                        await page.Keyboard.TypeAsync(searchInfo.InitComboBox2Value);
                        await page.Keyboard.DownAsync("Enter");
                        await Task.Delay(1000);
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
                        await Task.Delay(1000);
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

                    if (searchInfo.HasImageRecaptcha)
                        await SolveImageRecaptcha(page, searchInfo.CaptchaAnswerSelector, searchInfo.StateName);

                    if (searchInfo.HasRecaptcha2)
                        await SolveRecaptcha2(page, searchInfo.Url, searchInfo.SiteKey, searchInfo.StateName, searchInfo.RecaptchaCallback, searchInfo.IsRecaptchaAfterSearchBtnClick, searchInfo.CustomReptchaResponseId);

                    if (searchInfo.HasRecaptcha3)
                        await SolveRecaptcha3(page, searchInfo.Url, searchInfo.SiteKey, searchInfo.StateName, searchInfo.RecaptchaCallback, searchInfo.ActionName);



                    if (searchInfo.StateName.ToLowerInvariant() != "maine" && searchInfo.StateName.ToLowerInvariant() != "vermont")
                    {
                        if (searchInfo.StateName.ToLower() == "pennsylvania")
                            await Task.Delay(1000);

                        await page.ClickAsync(searchInfo.SearchButtonSelector);

                        if (searchInfo.StateName.ToLower() == "montana")
                            await Task.Delay(15000);
                    }
                    else
                    {
                        await Task.Delay(5000);
                        await page.EvaluateExpressionAsync($"document.querySelector('{searchInfo.SearchButtonSelector}').click();");
                        await Task.Delay(5000);
                    }

                    if (searchInfo.IsRecaptchaAfterSearchBtnClick)
                    {
                        try
                        {
                            await page.EvaluateExpressionAsync($@"{searchInfo.RecaptchaCallback};");
                        }
                        catch { }
                    }

                    if (searchInfo.HasPostSearchBtnRecaptcha2)
                    {
                        await SolveRecaptcha2(page, searchInfo.Url, searchInfo.SiteKey, searchInfo.StateName, searchInfo.RecaptchaCallback, searchInfo.IsRecaptchaAfterSearchBtnClick, searchInfo.CustomReptchaResponseId);

                        await page.ClickAsync(searchInfo.PostSearchBtnRecaptcha2ContinueButton);
                    }

                    if (!string.IsNullOrEmpty(searchInfo.SecondSelectDropdown))
                    {
                        await page.WaitForSelectorAsync(searchInfo.SecondSelectDropdown);
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

                    if (!searchInfo.IsTable)
                    {
                        string licenseNumber, licenseStatus = string.Empty, licenseExpiration = string.Empty, providerName = string.Empty;
                        var elLicenseNumber = await page.QuerySelectorAsync(searchInfo.LicenseNumberSelector);
                        licenseNumber = (await (await elLicenseNumber.GetPropertyAsync("textContent")).JsonValueAsync()).ToString()?.Replace(" ", "").Trim() ?? "Not Available";

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

                        if (!string.IsNullOrEmpty(searchInfo.ProviderNameSelector))
                        {
                            var elProviderName = await page.QuerySelectorAsync(searchInfo.ProviderNameSelector);
                            providerName = (await (await elProviderName.GetPropertyAsync("textContent")).JsonValueAsync()).ToString()?.Replace(" ", "").Trim() ?? "Not Available";
                        }



                        licenses.Add(
                            new LicenseInfo
                            {
                                State = searchInfo.StateName.ToUpperInvariant(),
                                Name = providerName,
                                LicenseNumber = licenseNumber,
                                LicenseStatus = licenseStatus,
                                LicenseExpiration = licenseExpiration
                            });
                    }
                    else
                    {
                        // Initialize lists to hold the values for each column
                        List<string> licenseNumbers = new List<string>(), licenseStatuses = new List<string>(), licenseExpirations = new List<string>(); List<string> providerNames = new List<string>();

                        if (!string.IsNullOrEmpty(searchInfo.ProviderNameSelector))
                        {
                            var elNames = await page.QuerySelectorAllAsync(searchInfo.ProviderNameSelector);
                            foreach (var el in elNames)
                            {
                                var name = (await (await el.GetPropertyAsync("textContent")).JsonValueAsync()).ToString()?.Replace(" ", "").Trim() ?? "Not Available";
                                if (!name.Contains("1"))
                                    providerNames.Add(name);
                            }
                        }

                        // Select all elements for license numbers
                        var elLicenseNumbers = await page.QuerySelectorAllAsync(searchInfo.LicenseNumberSelector);
                        foreach (var el in elLicenseNumbers)
                        {
                            var licenseNumber = (await (await el.GetPropertyAsync("textContent")).JsonValueAsync()).ToString()?.Replace(" ", "").Trim() ?? "Not Available";

                            licenseNumbers.Add(licenseNumber);
                        }

                        // Select all elements for license status if the selector is not empty
                        if (!string.IsNullOrEmpty(searchInfo.LicenseStatusSelector))
                        {
                            var elLicenseStatuses = await page.QuerySelectorAllAsync(searchInfo.LicenseStatusSelector);
                            foreach (var el in elLicenseStatuses)
                            {
                                var licenseStatus = (await (await el.GetPropertyAsync("textContent")).JsonValueAsync()).ToString()?.Replace(" ", "").Trim() ?? "Not Available";

                                licenseStatuses.Add(licenseStatus);
                            }
                        }

                        // Select all elements for license expiration if the selector is not empty
                        if (!string.IsNullOrEmpty(searchInfo.LicenseExpirationSelector))
                        {
                            var elLicenseExpirations = await page.QuerySelectorAllAsync(searchInfo.LicenseExpirationSelector);
                            foreach (var el in elLicenseExpirations)
                            {
                                var licenseExpiration = (await (await el.GetPropertyAsync("textContent")).JsonValueAsync()).ToString()?.Replace(" ", "").Trim() ?? "Not Available";

                                licenseExpirations.Add(licenseExpiration);
                            }
                        }

                        for (int i = 0; i < licenseNumbers.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(licenseNumbers[i]))
                            {
                                licenses.Add(new LicenseInfo
                                {
                                    State = searchInfo.StateName.ToUpperInvariant(),
                                    Name = providerNames[i],
                                    LicenseNumber = licenseNumbers[i],
                                    LicenseStatus = i < licenseStatuses.Count ? licenseStatuses[i] : "Not Available", // Check if there's a status for each number
                                    LicenseExpiration = i < licenseExpirations.Count ? licenseExpirations[i] : "Not Available" // Check if there's an expiration date for each number
                                });
                            }
                        }
                    }
                }


                catch (Exception ex)
                {
                    throw;

                    //return new LicenseInfo
                    //{
                    //    State = searchInfo.StateName.ToUpperInvariant(),
                    //    ErrorMessage = ex.Message

                    //};
                }
                finally
                {
                    if (context is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                return licenses;
            });
        }

        private async Task SolveImageRecaptcha(IPage page, string captchaInputSelector, string stateName)
        {
            //var captcha = new AntiCaptcha("fe348e4a8a96a206a483b6ea98ee3751");

            var twoCaptcha = new TwoCaptcha.TwoCaptcha("bb743d81179f6439cb71e645192ad2cd");


            await page.FocusAsync(captchaInputSelector);

            var screenshot = await page.ScreenshotBase64Async();

            var normal = new Normal();
            normal.SetBase64(screenshot);
            normal.SetCaseSensitive(true);

            await twoCaptcha.Solve(normal);

            await page.FocusAsync(captchaInputSelector);
            await page.Keyboard.TypeAsync(normal.Code);
        }

        public async Task SolveRecaptcha2(IPage page, string url, string siteKey, string stateName, string recaptchaCallback = "", bool isCallBackAfterSearchBtnClick = false, string customRecaptchaId = "")
        {
            var captcha = new AntiCaptcha("fe348e4a8a96a206a483b6ea98ee3751");
            AntiCaptchaResult reCaptchaResult = await captcha.SolveReCaptchaV2(siteKey, url);

            if (string.IsNullOrEmpty(customRecaptchaId))
            {

                if (!string.IsNullOrEmpty(recaptchaCallback) && !isCallBackAfterSearchBtnClick)
                {
                    await page.EvaluateExpressionAsync(@$"document.querySelector('#g-recaptcha-response').value = '{reCaptchaResult.Response}';");
                    await page.EvaluateExpressionAsync(@$"document.querySelector('#g-recaptcha-response').innerHTML = '{reCaptchaResult.Response}';{recaptchaCallback};");
                }
                else
                {
                    await page.EvaluateExpressionAsync(@$"document.querySelector('#g-recaptcha-response').value = '{reCaptchaResult.Response}';");
                    await page.EvaluateExpressionAsync(@$"document.querySelector('#g-recaptcha-response').innerHTML = '{reCaptchaResult.Response}';");
                }
            }
            else
            {
                await page.EvaluateExpressionAsync(@$"document.querySelector('{customRecaptchaId}').value = '{reCaptchaResult.Response}';");
                await page.EvaluateExpressionAsync(@$"document.querySelector('{customRecaptchaId}').innerHTML = '{reCaptchaResult.Response}';");
            }
        }

        public async Task SolveRecaptcha3(IPage page, string url, string siteKey, string stateName, string recaptchaCallback = "", string actionName = "")
        {
            var captcha = new AntiCaptcha("fe348e4a8a96a206a483b6ea98ee3751");

            var reCaptchaResult = await captcha.SolveReCaptchaV3(siteKey, url, 0.5, actionName);



            if (stateName.ToLowerInvariant() != "utah")
            {
                var value = await page.EvaluateExpressionAsync(@$"document.querySelector('#g-recaptcha-response').value = '{reCaptchaResult.Response}';");

                var innerHTML = await page.EvaluateExpressionAsync(@$"document.querySelector('#g-recaptcha-response').innerHTML = '{reCaptchaResult.Response}'; {recaptchaCallback}");
            }
            else
            {
                var value = await page.EvaluateExpressionAsync(@$"document.querySelector('#g-recaptcha-response-name').value = '{reCaptchaResult.Response}'; {recaptchaCallback};");
            }
        }

        public SearchInfo GetAlabamaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://abme.igovsolution.net/online/Lookups/Individual_Lookup.aspx",
                StateName = "Alabama",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr > td:nth-child(2)",
                SearchButtonSelector = "#ctl00_cntbdy_btn_search",
                LastNameSelector = "#ctl00_cntbdy_txt_lastname",
                FirstNameSelector = "#ctl00_cntbdy_txt_firstname",
                LicenseExpirationSelector = "tr > td:nth-child(6)",
                LicenseStatusSelector = "tr > td:nth-child(4)",
                ProviderNameSelector = "tr > td:nth-child(1)",
                HasRecaptcha2 = true,
                SiteKey = "6LchcFEUAAAAAJdfnpZDr9hVzyt81NYOspe29k",
                RecaptchaCallback = @"correctCaptcha();",
                IsTable = true,
            };

            return searchInfo;
        }

        public SearchInfo GetAlaskaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://www.commerce.alaska.gov/cbp/main/search/professional",
                StateName = "Alaska",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr > td.deptGridViewActionCell > a",
                SearchButtonSelector = "#search",
                LastNameSelector = "#OwnerEntityName",
                LicenseExpirationSelector = "tr > td:nth-child(6)",
                LicenseStatusSelector = "tr > td:nth-child(5)",
                ProviderNameSelector = "td:nth-child(4)",
                SiteKey = @"6LdZ7-UUAAAAACLkIDeI7ahpbRvUahh4Onk9yF1J",
                HasPostSearchBtnRecaptcha2 = true,
                PostSearchBtnRecaptcha2ContinueButton = "div.deptModal > div.deptModalContainer > div > div > a",
                IsTable = true


            };

            return searchInfo;
        }

        public SearchInfo GetNorthDakotaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://www.ndbom.org/public/find_verify/verify.asp",
                StateName = "North Dakota",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr > td:nth-child(3)",
                SearchButtonSelector = "#divSearch > fieldset.submit > button",
                LastNameSelector = "#lastName",
                FirstNameSelector = "#firstName",
                LicenseStatusSelector = "tr > td:nth-child(4)",
                ProviderNameSelector = "tr > td.name > a",
                IsTable = true,
                SiteKey = @"6LddpAsTAAAAAI4oir3oUkc4eAZo40OabK1Yu_rq",
                RecaptchaCallback = @"showSearch();",
                HasInitRecaptcha2 = true
            };

            return searchInfo;
        }


        public SearchInfo GetMontanaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://ebizws.mt.gov/PUBLICPORTAL/searchform?mylist=licenses",
                StateName = "Montana",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr > td:nth-child(1) > a",
                SearchButtonSelector = "#submitbtn",
                LastNameSelector = "span:nth-child(3) > input[type=text]",
                FirstNameSelector = "span:nth-child(2) > input[type=text]",
                LicenseExpirationSelector = "tr > td:nth-child(4)",
                LicenseStatusSelector = "tr > td:nth-child(3)",
                ProviderNameSelector = "tr > td:nth-child(6)",
                IsTable = true,
                CaptchaAnswerSelector = "#verif",
                HasImageRecaptcha = true
            };

            return searchInfo;
        }

        public SearchInfo GetIllinoisLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://online-dfpr.micropact.com/lookup/licenselookup.aspx",
                StateName = "Illinois",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr:nth-child(1) > td:nth-child(4)",
                SearchButtonSelector = "#btnLookup",
                LastNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbLastName_Contact",
                FirstNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbFirstName_Contact",
                LicenseExpirationSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_gvSearchResults > tbody > tr > td:nth-child(7)",
                LicenseStatusSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_gvSearchResults > tbody > tr > td:nth-child(3)",
                ProviderNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_gvSearchResults > tbody > tr > td:nth-child(2)",
                IsTable = true,
                CaptchaAnswerSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_CaptchaSecurity1_txtCAPTCHA",
                HasImageRecaptcha = true,
                RadioSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_lbMultipleCredentialTypePrefix > option:nth-child(34)",
            };

            return searchInfo;
        }

        public SearchInfo GetWisconsinLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://licensesearch.wi.gov/#panel1",
                StateName = "Wisconsin",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr > td:nth-child(1)",
                SearchButtonSelector = "#IndividualSearch",
                LastNameSelector = "#lastName",
                FirstNameSelector = "#firstName",
                LicenseExpirationSelector = "tr > td:nth-child(6)",
                // LicenseStatusSelector = "tr:nth-child(1) > td:nth-child(4)",
                ProviderNameSelector = "tr > td:nth-child(3)",
                IsTable = true,
                HasRecaptcha2 = true,
                SiteKey = "6LfXEbEUAAAAAHu-jvb4evjNCyg700VKwnnx6Vyi",
            };

            return searchInfo;
        }

        public SearchInfo GetSouthCarolinaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://verify.llronline.com/LicLookup/Med/Med.aspx",
                StateName = "South Carolina",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "#ctl00_ContentPlaceHolder2_gv_results > tbody > tr > td:nth-child(1) > a",
                ProviderNameSelector = "#ctl00_ContentPlaceHolder2_gv_results > tbody > tr > td:nth-child(5)",
                IsTable = true,
                SearchButtonSelector = "tr:nth-child(5) > td.tdrightside > button",
                LastNameSelector = "#ctl00_ContentPlaceHolder1_UserInputGen1_txt_lastName",
                FirstNameSelector = "#ctl00_ContentPlaceHolder1_UserInputGen1_txt_firstName",
                HasRecaptcha2 = true,
                SiteKey = @"6Lc2X-saAAAAAPC6HatgHFOd8rCxCl-2yPTh44PN",
                RecaptchaCallback = @"onSubmit()",
                IsRecaptchaAfterSearchBtnClick = true
            };

            return searchInfo;
        }

        public SearchInfo GetUtahLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://secure.utah.gov/llv/search/index.html",
                StateName = "Utah",
                LastName = $"{_firstName} {_lastName}",
                LicenseNumberSelector = "tr > td:nth-child(4)>a",
                SearchButtonSelector = "input[type=submit]:nth-child(1)",
                LastNameSelector = "#fullName",
                LicenseStatusSelector = "tr > td:nth-child(5)",
                ProviderNameSelector = "tr > td:nth-child(1) > a",
                IsTable = true,
                HasRecaptcha2 = true,
                SiteKey = @"6LcQUqIUAAAAAG7lgG1BfDlhvVUuFP26QsY4Eq6_",
                CustomReptchaResponseId = @"#g-recaptcha-response-name"
            };

            return searchInfo;
        }

        public SearchInfo GetGeorgiaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://gcmb.mylicense.com/verification/",
                StateName = "Georgia",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr > td:nth-child(4) > span",
                SearchButtonSelector = "#sch_button",
                LastNameSelector = "#t_web_lookup__last_name",
                FirstNameSelector = "#t_web_lookup__first_name",
                LicenseStatusSelector = "tr>td:nth-child(3)>span",
                ProviderNameSelector = "#datagrid_results > tbody > tr > td:nth-child(1) > table > tbody > tr:nth-child(1) > td",
                IsTable = true,
                HasRecaptcha2 = true,
                SiteKey = @"6Ldp57EUAAAAABWjdLVKT-QThpxati6v0KV8azOS",
            };

            return searchInfo;
        }

        public SearchInfo GetHawaiiLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://mypvl.dcca.hawaii.gov/public-license-search/",
                StateName = "Hawaii",
                FirstName = _firstName,
                LastName = $"{_firstName} {_lastName}",
                LicenseNumberSelector = "tr > td:nth-child(2) > a",
                //SearchButtonSelector = "#business_individual-captcha-btn",
                LastNameSelector = "#business_individual",
                HasRecaptcha2 = true,
                SiteKey = @"6LfE564ZAAAAAHmW1_6SbaG2P8-EqV_RhtHpMR80",
                AcceptUsageTermsBtnSelector = "#nav-business-individual-tab",
                CustomReptchaResponseId = "#g-recaptcha-response-1",
                SearchButtonSelector = "#business_individual-captcha-btn",
                ProviderNameSelector = "tr > td.sorting_1 > a",
                IsTable = true,
            };

            return searchInfo;
        }

        public SearchInfo GetMississippiLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://gateway.msbml.ms.gov/verification/search.aspx",
                StateName = "Mississippi",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr>td:nth-child(2)",
                SearchButtonSelector = "#btnSubmit",
                LastNameSelector = "#txtLast",
                FirstNameSelector = "#txtFirst",
                LicenseStatusSelector = "tr > td:nth-child(4) > span",
                HasRecaptcha2 = true,
                SiteKey = @"6LcOFXcdAAAAAJ-_adyQGC-KARdFfJP461lcbuL0",
                ProviderNameSelector = "tr > td:nth-child(1) > a",
                IsTable = true,
            };

            return searchInfo;
        }

        public SearchInfo GetNebraskaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://www.nebraska.gov/LISSearch/search.cgi",
                StateName = "Nebraska",
                FirstName = _firstName,
                LastName = _lastName,
                LicenseNumberSelector = "tr > td:nth-child(3)",
                SearchButtonSelector = "#submit",
                LastNameSelector = "#last_name",
                FirstNameSelector = "#first_name",
                LicenseExpirationSelector = "tr > td:nth-child(4)",
                LicenseStatusSelector = "tr > td:nth-child(2)",
                HasRecaptcha2 = true,
                SiteKey = @"6LcmsP4SAAAAAJeHxpx9VA7CeZq_9gf74M8tJVra",
                AcceptUsageTermsBtnSelector = "#radio1",
                ProviderNameSelector = "tr > th > a",
                IsTable = true
            };

            return searchInfo;
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
                ProviderNameSelector = "#ContentPlaceHolder1_dtgGeneral_lblLeftColumnEntName_0 > b",
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
                LicenseStatusSelector = "tr > td:nth-child(5)",
                LicenseNumberSelector = "tr > td:nth-child(1)",
                ProviderNameSelector = "tr > td:nth-child(3)",
                SearchButtonSelector = "button",
                LastNameSelector = "#keywords",
                IsTable = true
            };

            return searchInfo;
        }

        public SearchInfo GetPennsylvaniaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = @"https://www.pals.pa.gov/#!/page/search",
                StateName = "Pennsylvania",
                LastName = _lastName,
                FirstName = _firstName,
                LicenseStatusSelector = "tr > td:nth-child(5)",
                LicenseNumberSelector = "tr > td:nth-child(2)",
                ProviderNameSelector = "tr > td:nth-child(1) > a",
                IsTable = true,
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
                LicenseStatusSelector = "#datagrid_results > tbody > tr > td:nth-child(5) > span",
                LicenseNumberSelector = "#datagrid_results > tbody > tr > td:nth-child(2) > span",
                ProviderNameSelector = "#datagrid_results > tbody > tr > td:nth-child(1)",
                IsTable = true,
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
                LicenseStatusSelector = elLicenseStatusSelector,
                ProviderNameSelector = "#\\30  > footer > ul:nth-child(2) > li:nth-child(1) > h3"
            };
        }

        public SearchInfo GetColoradoLicenseInfo()
        {
            var url = "https://apps2.colorado.gov/dora/licensing/lookup/licenselookup.aspx";
            var elLicenseNumberSelector = "tr > td:nth-child(3)";
            var elLicenseStatusSelector = "tr > td:nth-child(4)";
            var firstNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbFirstName_Contact";
            var lastNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbLastName_Contact";


            return new SearchInfo
            {
                StateName = "Colorado",
                Url = url,
                FirstName = _firstName,
                LastName = _lastName,
                FirstNameSelector = firstNameSelector,
                LastNameSelector = lastNameSelector,
                SearchButtonSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_btnLookup",
                LicenseNumberSelector = elLicenseNumberSelector,
                LicenseStatusSelector = elLicenseStatusSelector,
                ProviderNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_gvSearchResults > tbody > tr > td:nth-child(2)",
                IsTable = true
            };

        }

        public SearchInfo GetConnecticutLicenseInfo()
        {

            var url = "https://www.elicense.ct.gov/Lookup/LicenseLookup.aspx";
            var elLicenseNumberSelector = "tr > td:nth-child(3)";
            var elLicenseStatusSelector = "tr > td:nth-child(5)";
            var firstNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbFirstName_Contact";
            var lastNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_ctl03_tbLastName_Contact";

            return new SearchInfo
            {
                StateName = "Connecticut",
                Url = url,
                FirstName = _firstName,
                LastName = _lastName,
                FirstNameSelector = firstNameSelector,
                LastNameSelector = lastNameSelector,
                LicenseStatusSelector = elLicenseStatusSelector,
                LicenseNumberSelector = elLicenseNumberSelector,
                SearchButtonSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_btnLookup",
                ProviderNameSelector = "#ctl00_MainContentPlaceHolder_ucLicenseLookup_gvSearchResults > tbody > tr > td:nth-child(2)",
                IsTable = true
            };
        }

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
                ProviderNameSelector = "#ctl00_MainContentPlaceHolder_lvResults_ctrl0_lblPhyname"
            };

            return searchInfo;

        }

        public SearchInfo GetKentuckyLicenseInfo()
        {
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
                ProviderNameSelector = "div:nth-child(3) > div.cols2-col2"
            };

            return searchInfo;

        }

        public SearchInfo GetLouisianaSiteLicenseInfo()
        {
            var url = "https://online.lasbme.org/#/verifylicense";
            var elLicenseNumberSelector = "tr > td:nth-child(2)";
            var elLicenseStatusSelector = "tr > td:nth-child(3)";
            var srchBtnSelector = "input.btn.btn-success";
            var lastNameSelector = "form > div:nth-child(7) > div > input";
            var firstNameSelector = "form > div:nth-child(6) > div > input";

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
                ProviderNameSelector = "tr > td:nth-child(1)",
                IsTable = true
            };

            return searchInfo;
        }

        public SearchInfo GetMaineSiteLicenseInfo()
        {
            var url = "https://www.pfr.maine.gov/ALMSOnline/ALMSQuery/SearchIndividual.aspx";
            var elLicenseNumberSelector = "tr > td:nth-child(2)";
            var elLicenseStatusSelector = "tr > td:nth-child(5)";
            var ddlSelector = @"#scRegulator";
            var srchBtnSelector = @"#btnSearch";
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
                IsTable = true,
                ProviderNameSelector = "tr > td:nth-child(1) > a"
            };

            return searchInfo;
        }

        public SearchInfo GetMassachussettsSiteLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://findmydoctor.mass.gov/",
                StateName = "Massachussetts",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#physician-last-name-input",
                FirstNameSelector = @"#physician-first-name-input",
                SearchButtonSelector = "div.search-criteria-wrapper > div:nth-child(5) > button",
                LicenseNumberSelector = "div > span > hyperlink-cell-renderer > a",
                LicenseStatusSelector = "div:nth-child(4) > div > span",
                ProviderNameSelector = "div > div > div > div:nth-child(2) > div > span",
                IsTable = true
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
                ProviderNameSelector = "#ctl00_PlaceHolderMain_licenseeGeneralInfo_lblContactName_value"
            };

            return searchInfo;
        }

        //MN site being updated

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
                ProviderNameSelector = "#full_name",
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
                LicenseNumberSelector = "tr > td.LicenseEndorsementNumber > div",
                LicenseStatusSelector = "tr > td.Status",
                ProviderNameSelector = "tr > td.Name.sorting_1",
                IsTable = true
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
                ProviderNameSelector = "#BodyContent_lblHeaderName",
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
                ProviderNameSelector = "td:nth-child(4) > div",
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
                ProviderNameSelector = "tr:nth-child(3) > td",
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
                LicenseNumberSelector = @"#form > table > tbody > tr > td:nth-child(3)",
                LicenseExpirationSelector = @"#form > table > tbody > tr > td:nth-child(4)",
                LicenseStatusSelector = @"#form > table > tbody > tr > td:nth-child(5)",
                ProviderNameSelector = "#form > table > tbody > tr > td:nth-child(1) > a",
                IsTable = true,
                AcceptUsageTermsBtnSelector = @"",
                IsActiveLicenseSelector = @"",
                SearchTypeInitDropdownSelector = @"#selectType",
                SearchTypeInitDropdownValue = @"Name"

            };

            return searchInfo;
        }

        public SearchInfo GetWestVirginiaDOLicenseInfo()
        {
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
                ProviderNameSelector = "#printcontent > table:nth-child(1) > tbody > tr > td:nth-child(1)",
                DropdownSelector = @"#licType",
                DropdownSelectValue = @"BoardXP"
            };

            return searchInfo;
        }

        public SearchInfo GetWyomingLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://wybomprod.glsuite.us/GLSuiteWeb/Clients/WYBOM/Public/LicenseeSearch.aspx?SearchType=Physician",
                StateName = "Wyoming",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#ContentPlaceHolder1_txtLastName",
                FirstNameSelector = @"#ContentPlaceHolder1_txtFirstName",
                SearchButtonSelector = @"#ContentPlaceHolder1_btnSubmit",
                LicenseNumberSelector = @"#ContentPlaceHolder1_dtgResults > tbody > tr > td:nth-child(4):not(.th)",
                LicenseExpirationSelector = @"#ContentPlaceHolder1_dtgResults > tbody > tr > td:nth-child(7):not(.th)",
                ProviderNameSelector = "#ContentPlaceHolder1_dtgResults > tbody > tr > td:nth-child(1):not(.th)",
                IsTable = true
            };

            return searchInfo;
        }

        public SearchInfo GetDistrictOfColumbiaLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://dohenterprise.my.site.com/ver/s/",
                StateName = "Washington, D.C.",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = @"#LastName",
                FirstNameSelector = @"#FirstName",
                SearchButtonSelector = "div > a:nth-child(15)",
                LicenseNumberSelector = "tr > td:nth-child(2)",
                LicenseExpirationSelector = "tr > td:nth-child(6)",
                LicenseStatusSelector = "tr > td:nth-child(4)",
                ProviderNameSelector = "tr > td:nth-child(1) > a",
                DropdownSelector = @"#Status",
                DropdownSelectValue = "Active",
                IsTable = true,
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
                //SecondSearchButtonSelector = @".expand",
                LicenseNumberSelector = @"tr > td.LicenseEndorsementNumber > div",
                LicenseStatusSelector = @" tr > td.Status",
                ProviderNameSelector = @"tr > td.Name.sorting_1",
                IsTable = true
            };

            return searchInfo;
        }

        public SearchInfo GetIndianaLicenseInfo()
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
                LicenseNumberSelector = "#datagrid_results > tbody > tr > td:nth-child(2) > span",
                LicenseStatusSelector = "#datagrid_results > tbody > tr > td:nth-child(5) > span",
                IsTable = true,
                ProviderNameSelector = "#datagrid_results > tbody > tr > td:nth-child(1) > table > tbody > tr:nth-child(1) > td"
            };

            return searchInfo;
        }

        public SearchInfo GetIdahoLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://apps-dopl.idaho.gov/IBOMPublic/LPRBrowser.aspx",
                StateName = "Idaho",
                FirstName = _firstName,
                LastName = _lastName,
                LastNameSelector = "#CPH1_txtsrcApplicantLastName",
                FirstNameSelector = "#CPH1_txtsrcApplicantFirstName",
                SearchButtonSelector = @"#CPH1_btnGoFind",
                LicenseNumberSelector = "#CPH1_myDataGrid > tbody > tr.GridItemStyle > td:nth-child(3) > a",
                LicenseStatusSelector = @"#CPH1_myDataGrid > tbody > tr.GridItemStyle > td:nth-child(5)",
                LicenseExpirationSelector = "#CPH1_myDataGrid > tbody > tr.GridItemStyle > td:nth-child(4)",
                ProviderNameSelector = "#CPH1_myDataGrid > tbody > tr.GridItemStyle > td:nth-child(2)",
                IsTable = true,
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
                ProviderNameSelector = "mat-row:nth-child(2) > mat-cell.mat-cell.cdk-cell.cdk-column-NameLast.mat-column-NameLast.ng-star-inserted > div",
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
                LastName = $"{_lastName}, {_firstName}",
                LastNameSelector = "div:nth-child(4) > input",
                SearchButtonSelector = "button.btn.btn-primary",
                LicenseNumberSelector = "tr:nth-child(3) > td:nth-child(2)",
                LicenseExpirationSelector = "tr:nth-child(4) > td:nth-child(2)",
                SecondSearchButtonSelector = "tr:nth-child(2) > td:nth-child(5) > a",
                RadioSelector = "#optionsRadios2",
                ProviderNameSelector = "tr:nth-child(1) > td:nth-child(2)"
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
                LicenseNumberSelector = "tr > td:nth-child(1)",
                LicenseStatusSelector = "tr > td:nth-child(8)",
                ProviderNameSelector = "tr > td:nth-child(3)",
                IsTable = true
                //  LicenseExpirationSelector = "div.hd-box-container.profile > div:nth-child(6) > div",
                // SecondSearchButtonSelector = "tr > td:nth-child(10) > a"
            };

            return searchInfo;
        }

        public SearchInfo GetNevadaDOLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://nsbom.portalus.thentiacloud.net/webs/portal/register/#/",
                StateName = "Nevada",
                LastName = $"{_firstName} {_lastName}",
                LastNameSelector = "#keywords",
                SearchButtonSelector = "button > span",
                LicenseNumberSelector = "tr > td:nth-child(1)",
                LicenseStatusSelector = "tr > td:nth-child(6)",
                LicenseExpirationSelector = "td:nth-child(7)",
                ProviderNameSelector = "tr > td:nth-child(2)",
                IsTable = true
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
                LicenseStatusSelector = "td:nth-child(5) > span",
                ProviderNameSelector = "#datagrid_results > tbody > tr >td:nth-child(1)"
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
                LicenseNumberSelector = "tr > td:nth-child(1) > a",
                ProviderNameSelector = "tr > td:nth-child(2)",
                IsTable = true,
                InitComboBoxSelector = "#vs12-combobox",
                InitComboBoxValue = "Licensee Name",
                InitComboBox2Selector = "#vs19-combobox",
                InitComboBox2Value = "All Professions (ALL)",
                IsSearchButtonHidden = true,
                HasInitModal = true,
                AcceptTermsAndConditionsCheckboxSelector = "#edit-i-agree-to-the-terms-and-condition",
                HasRecaptcha2 = true,
                SiteKey = @"6LdTmh0pAAAAAEcCfQ6zY2lKvj62CF3TryPCGWYm",
            };

            return searchInfo;
        }

        public SearchInfo GetTennesseeLicenseInfo()
        {
            var searchInfo = new SearchInfo
            {
                Url = "https://apps.health.tn.gov/Licensure/default.aspx",
                StateName = "Tennessee",
                LastName = _lastName,
                FirstName = _firstName,
                LastNameSelector = "#ctl00_PageContent_txtLastName",
                FirstNameSelector = "#ctl00_PageContent_txtFirstName",
                SearchButtonSelector = "#ctl00_PageContent_btnSubmit",
                LicenseNumberSelector = "tr > td:nth-child(3) > p",
                ProviderNameSelector = "#ctl00_PageContent_dlstLicensure > tbody > tr > td > table > tbody > tr > td:nth-child(2)",
                IsTable = true,
                HasImageRecaptcha = true,
                CaptchaAnswerSelector = "span:nth-child(2) > input[type=text]"
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
                LicenseNumberSelector = "tr > td:nth-child(1) > a",
                LicenseStatusSelector = "tr > td:nth-child(5)",
                ProviderNameSelector = "tr > td:nth-child(2)",
                IsTable = true,
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
                LicenseNumberSelector = "tr > td:nth-child(4)",
                LicenseStatusSelector = "tr > td:nth-child(5) > span",
                ProviderNameSelector = "tr > td:nth-child(2) > a",
                IsTable = true
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
                LicenseNumberSelector = "tr > td:nth-child(3)",
                LicenseStatusSelector = "tr > td:nth-child(5)",
                ProviderNameSelector = "tr > td:nth-child(1) > a",
                IsTable = true
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
                SecondSearchButtonSelector = "#Btn_Name",
                ProviderNameSelector = "#Name"
            };

            return searchInfo;
        }
    }
}


