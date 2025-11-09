using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AtasLocalizationTests
{
    /// <summary>
    /// Тонкий тест локализации signup + success с использованием PageObject и SeleniumKit.
    /// </summary>
    [TestFixture]
    public class LiveSignupPopupI18nTests
    {
        private ChromeDriver _driver = default!;
        private WebDriverWait _wait = default!;
        private SeleniumKit _kit = default!;

        private const string RootUrl = "https://atas.bambus.com.ua/";
        private const string BasicAuthUser = "bambuk";
        private const string BasicAuthPass = "Atastraders$";

        private static readonly string[] Locales = { "EN", "RU", "FR", "DE", "IT", "ES", "UA", "CN" };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var opts = new ChromeOptions();
            opts.AddArgument("--disable-gpu");
            opts.AddArgument("--disable-dev-shm-usage");
            opts.AddArgument("--no-sandbox");
            opts.AddArgument("--remote-allow-origins=*");

            var service = ChromeDriverService.CreateDefaultService();
            service.EnableVerboseLogging = true;
            service.LogPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "chromedriver.log");

            _driver = new ChromeDriver(service, opts, TimeSpan.FromSeconds(60));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            _kit = new SeleniumKit(_driver, _wait);

            // BasicAuth через CDP
            try
            {
                var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{BasicAuthUser}:{BasicAuthPass}"));
                _driver.ExecuteCdpCommand("Network.enable", new Dictionary<string, object>());
                _driver.ExecuteCdpCommand("Network.setExtraHTTPHeaders", new Dictionary<string, object>
                {
                    ["headers"] = new Dictionary<string, object> { ["Authorization"] = $"Basic {token}" }
                });
            }
            catch (Exception ex)
            {
                TestContext.WriteLine("[WARN] CDP BasicAuth failed: " + ex.Message);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        [Test]
        [TestCaseSource(nameof(Locales))]
        public void Check_Signup_On_Locale(string locale)
        {
            var errors = new List<string>();
            var page = new SignupPopupPage(_kit);

            // 1) Открыть сайт и выбрать локаль
            _driver.Navigate().GoToUrl(RootUrl);
            _kit.WaitReady();

            try { _kit.SelectLocale(locale); }
            catch (Exception ex) { errors.Add($"[{locale}] Выбор локали: {ex.Message}"); }

            // 2) Проверка URL + <html lang>
            if (!_kit.VerifyLocaleByUrlAndLang(locale, out var explain))
                errors.Add($"[{locale}] Локаль не подтверждена: {explain}");

            // 3) Открыть попап регистрации
            try { page.Open(); }
            catch (Exception ex) { errors.Add($"[{locale}] Открытие signup: {ex.Message}"); }

            // 4) Проверка статичных текстов signup
            var (title, emailLabel, emailPh, marketing, agreeLabel, submitText, bottomText, bottomCta)
                = page.ReadSignupTexts();

            ExpectContains(errors, locale, "title", title);
            ExpectContains(errors, locale, "email_label", emailLabel);
            ExpectContains(errors, locale, "email_ph", emailPh);
            ExpectContains(errors, locale, "marketing", marketing);
            ExpectContains(errors, locale, "agree", agreeLabel);
            ExpectContains(errors, locale, "submit_text", submitText);
            ExpectContains(errors, locale, "bottom_text", bottomText);
            ExpectContains(errors, locale, "bottom_cta", bottomCta);

            // 5) Заполнить и отправить форму
            try
            {
                var rnd = new Random().Next(10, 1001);
                page.FillAndSubmit($"test{rnd}@test.net");
            }
            catch (Exception ex)
            {
                _kit.TakeScreenshot(locale, "submit_error");
                errors.Add($"[{locale}] Сабмит формы: {ex.Message}");
            }

            // 6) Ждать и проверить success (одним методом страницы)
            try
            {
                page.WaitAndValidateSuccess(locale, errors, 45);
            }
            catch (Exception ex)
            {
                _kit.TakeScreenshot(locale, "success_wait_fail");
                errors.Add($"[{locale}] Ожидание/валидация success: {ex.Message}");
            }

            // 7) Отчёт
            if (errors.Count > 0)
            {
                TestContext.WriteLine("⚠️ Несоответствия:");
                foreach (var e in errors) TestContext.WriteLine(" - " + e);
                Assert.Fail($"{errors.Count} несоответств(ия/ий) для локали {locale}.");
            }
            else
            {
                TestContext.WriteLine($"✅ [{locale}] Все проверки пройдены.");
            }
        }

        #region Локальные ассерты (используют Translations.Exp + SeleniumKit)

        private static void ExpectContains(List<string> errors, string locale, string key, string actual, string? screenshotOnErrorKey = null)
        {
            var exp = Translations.Exp.TryGetValue((locale, key), out var v) ? v : "";
            if (actual.StartsWith("__NOT_FOUND__"))
            {
                errors.Add($"[{locale}] {key}: элемент не найден. {actual}");
                return;
            }
            if (!SeleniumKit.ContainsNormalized(actual, exp))
            {
                errors.Add($"[{locale}] {key}: ожидали содержит «{exp}», получили «{actual}».");
                TestContext.Out?.WriteLine($"DEBUG {locale}.{key}: «{actual}»");
                if (screenshotOnErrorKey != null)
                {
                    // снимок экрана для конкретной ошибки
                    try
                    {
                        var drv = TestContext.CurrentContext?.Test?.Properties?["WebDriver"] as ITakesScreenshot;
                    }
                    catch { /* игнор */ }
                }
            }
        }

        private void ExpectEquals(List<string> errors, string locale, string key, string actual, string? screenshotOnErrorKey = null)
        {
            var exp = Translations.Exp.TryGetValue((locale, key), out var v) ? v : "";
            if (actual.StartsWith("__NOT_FOUND__"))
            {
                errors.Add($"[{locale}] {key}: элемент не найден. {actual}");
                return;
            }
            if (!SeleniumKit.EqualsNormalized(actual, exp))
            {
                errors.Add($"[{locale}] {key}: ожидали «{exp}», получили «{actual}».");
                _kit.TakeScreenshot(locale, $"mismatch_{key}");
            }
        }

        #endregion
    }
}
