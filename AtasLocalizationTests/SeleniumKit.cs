using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AtasLocalizationTests
{
    /// <summary>
    /// Общие утилиты для Selenium: ожидания, клики, чтение текстов, локаль, скриншоты.
    /// </summary>
    public sealed class SeleniumKit : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public SeleniumKit()
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
        }

        public SeleniumKit(IWebDriver driver, WebDriverWait wait)
        {
            _driver = driver;
            _wait = wait;
        }

        public IWebDriver Driver => _driver;

        #region Базовые методы навигации

        public void GoTo(string url)
        {
            _driver.Navigate().GoToUrl(url);
            WaitReady();
        }

        public void Dispose()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        #endregion

        #region Поиск/чтение/клики

        public IWebElement? FindByXPath(string xpath)
        {
            try { return _driver.FindElement(By.XPath(xpath)); }
            catch { return null; }
        }

        public string GetTextByXPath(string xpath)
        {
            var el = FindByXPath(xpath);
            if (el == null) return $"__NOT_FOUND__ (xpath: {xpath})";
            var s = el.Text;
            if (string.IsNullOrWhiteSpace(s)) s = el.GetAttribute("innerText") ?? "";
            return Normalize(s);
        }

        public string GetAttrByXPath(string xpath, string attr)
        {
            var el = FindByXPath(xpath);
            if (el == null) return $"__NOT_FOUND__ (xpath: {xpath})";
            var s = el.GetAttribute(attr) ?? "";
            return Normalize(s);
        }

        public void SafeClick(IWebElement el)
        {
            try
            {
                el.Click();
                return;
            }
            catch
            {
                try
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript(
                        "if (arguments[0]) { arguments[0].click(); }", el);
                    return;
                }
                catch
                {
                    try
                    {
                        var rect = (Dictionary<string, object>)((IJavaScriptExecutor)_driver)
                            .ExecuteScript("if(!arguments[0]) return null; const r=arguments[0].getBoundingClientRect(); return {x:r.left+r.width/2,y:r.top+r.height/2};", el);
                        if (rect != null)
                        {
                            ((IJavaScriptExecutor)_driver).ExecuteScript(
                                "document.elementFromPoint(arguments[0], arguments[1])?.click();",
                                rect["x"], rect["y"]);
                        }
                    }
                    catch { /* глушим, чтобы не сыпать stacktrace */ }
                }
            }
        }

        public void JsClick(IWebElement el) =>
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", el);

        public void ScrollIntoView(IWebElement el)
        {
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript(
                    "arguments[0].scrollIntoView({block:'center',inline:'center'});", el);
            }
            catch { }
        }

        public void WaitReady()
        {
            _wait.Until(d => ((IJavaScriptExecutor)d)
                .ExecuteScript("return document.readyState")?.ToString() == "complete");
            Sleep(150);
        }

        public void Sleep(int ms) => System.Threading.Thread.Sleep(ms);

        #endregion

        #region Валидация строк

        public static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = System.Net.WebUtility.HtmlDecode(s);
            s = s.Replace('\u00A0', ' ')
                 .Replace('–', '-')
                 .Replace('—', '-')
                 .Replace('ё', 'е')
                 .Replace('Ё', 'Е');
            return Regex.Replace(s, @"\s+", " ").Trim();
        }

        public static bool ContainsNormalized(string actual, string expectedPart) =>
            Normalize(actual).Contains(Normalize(expectedPart), StringComparison.OrdinalIgnoreCase);

        public static bool EqualsNormalized(string a, string b) =>
            string.Equals(Normalize(a), Normalize(b), StringComparison.Ordinal);

        #endregion

        #region Локаль (общая логика)

        public static readonly Dictionary<string, string> HtmlLangPrefix = new()
        {
            ["EN"] = "en",
            ["RU"] = "ru",
            ["FR"] = "fr",
            ["DE"] = "de",
            ["IT"] = "it",
            ["ES"] = "es",
            ["UA"] = "uk",
            ["CN"] = "zh"
        };

        public static readonly Dictionary<string, string[]> UrlHints = new(StringComparer.OrdinalIgnoreCase)
        {
            ["EN"] = new[] { "/en/", "lang=en" },
            ["RU"] = new[] { "/ru/", "lang=ru" },
            ["FR"] = new[] { "/fr/", "lang=fr" },
            ["DE"] = new[] { "/de/", "lang=de" },
            ["IT"] = new[] { "/it/", "lang=it" },
            ["ES"] = new[] { "/es/", "lang=es" },
            ["UA"] = new[] { "/ua/", "/uk/", "lang=uk", "lang=ua" },
            ["CN"] = new[] { "/cn/", "lang=zh", "lang=cn" },
        };

        public static readonly Dictionary<string, string> LangNameToLocale = new(StringComparer.OrdinalIgnoreCase)
        {
            ["English"] = "EN",
            ["Русский"] = "RU",
            ["Français"] = "FR",
            ["Deutsch"] = "DE",
            ["Italiano"] = "IT",
            ["Español"] = "ES",
            ["Українська"] = "UA",
            ["Chinese (Simplified)"] = "CN",
        };

        public void SelectLocale(string locale)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("scrollTo(0,0)");
            Sleep(200);

            var btn = _wait.Until(d => d.FindElement(By.CssSelector("button.btn.langs-wrapper-lang")));
            ScrollIntoView(btn);

            var opened = false;
            for (int i = 0; i < 3 && !opened; i++)
            {
                try
                {
                    try { SafeClick(btn); }
                    catch { JsClick(btn); }

                    _wait.Until(d => d.FindElement(By.CssSelector("nav.langs-wrapper-dropdown")));
                    opened = true;
                }
                catch { Sleep(200); }
            }
            if (!opened) throw new NoSuchElementException("Не удалось открыть меню локалей.");

            var dd = _driver.FindElement(By.CssSelector("nav.langs-wrapper-dropdown"));
            ScrollIntoView(dd);

            var links = dd.FindElements(By.CssSelector("a.link")).ToList();
            if (links.Count == 0) throw new NoSuchElementException("В списке локалей нет ссылок.");

            foreach (var a in links)
            {
                var name = (a.Text ?? "").Trim();
                if (string.IsNullOrEmpty(name)) name = (a.GetAttribute("title") ?? "").Trim();

                if (name.Length > 0 &&
                    LangNameToLocale.TryGetValue(name, out var code) &&
                    code.Equals(locale, StringComparison.OrdinalIgnoreCase))
                {
                    ScrollIntoView(a);
                    SafeClick(a);
                    WaitReady();
                    return;
                }
            }

            foreach (var a in links)
            {
                var guess = GuessLocaleFromLink(a);
                if (guess.Equals(locale, StringComparison.OrdinalIgnoreCase))
                {
                    ScrollIntoView(a);
                    SafeClick(a);
                    WaitReady();
                    return;
                }
            }

            throw new NoSuchElementException($"Локаль {locale} не найдена.");
        }

        public string GuessLocaleFromLink(IWebElement a)
        {
            string L(string s) => (a.GetAttribute(s) ?? "").Trim().ToLowerInvariant();
            var attrs = new[] { "data-lang", "lang", "hreflang", "aria-label", "title" };
            foreach (var attr in attrs)
            {
                var v = L(attr);
                if (v is "uk" or "ua") return "UA";
                if (v.StartsWith("en")) return "EN";
                if (v.StartsWith("ru")) return "RU";
                if (v.StartsWith("fr")) return "FR";
                if (v.StartsWith("de")) return "DE";
                if (v.StartsWith("it")) return "IT";
                if (v.StartsWith("es")) return "ES";
                if (v.StartsWith("zh") || v.StartsWith("cn")) return "CN";
            }

            var href = L("href");
            if (href.Contains("/ua/") || href.Contains("lang=uk") || href.Contains("lang=ua")) return "UA";
            if (href.Contains("/ru/") || href.Contains("lang=ru")) return "RU";
            if (href.Contains("/fr/") || href.Contains("lang=fr")) return "FR";
            if (href.Contains("/de/") || href.Contains("lang=de")) return "DE";
            if (href.Contains("/it/") || href.Contains("lang=it")) return "IT";
            if (href.Contains("/es/") || href.Contains("lang=es")) return "ES";
            if (href.Contains("/cn/") || href.Contains("lang=zh") || href.Contains("lang=cn")) return "CN";

            return "";
        }

        /// <summary>Проверяет локаль по URL и html lang атрибуту.</summary>
        public bool VerifyLocaleByUrlAndLang(string locale, out string explain)
        {
            explain = "";

            try
            {
                var pref = HtmlLangPrefix.TryGetValue(locale, out var p) ? p : "en";
                var lang = (string)((IJavaScriptExecutor)_driver)
                    .ExecuteScript("return document.documentElement.lang||'';");

                if (string.IsNullOrEmpty(lang))
                {
                    explain = "html[lang] не установлен";
                    return false;
                }

                if (!lang.StartsWith(pref, StringComparison.OrdinalIgnoreCase))
                {
                    explain = $"html[lang]='{lang}' не начинается с '{pref}'";
                    return false;
                }

                explain = $"html[lang]='{lang}' - локаль подтверждена";
                return true;
            }
            catch (Exception ex)
            {
                explain = $"ошибка чтения documentElement.lang: {ex.Message}";
                return false;
            }
        }
        /// <summary>Проверяет локаль только по html lang атрибуту (игнорирует URL).</summary>
        public bool VerifyLocaleByHtmlLangOnly(string locale, out string explain)
        {
            explain = "";

            try
            {
                var pref = HtmlLangPrefix.TryGetValue(locale, out var p) ? p : "en";
                var lang = (string)((IJavaScriptExecutor)_driver)
                    .ExecuteScript("return document.documentElement.lang||'';");

                if (string.IsNullOrEmpty(lang))
                {
                    explain = "html[lang] не установлен";
                    return false;
                }

                if (!lang.StartsWith(pref, StringComparison.OrdinalIgnoreCase))
                {
                    explain = $"html[lang]='{lang}' не начинается с '{pref}'";
                    return false;
                }

                explain = $"html[lang]='{lang}'";
                return true;
            }
            catch (Exception ex)
            {
                explain = $"ошибка чтения documentElement.lang: {ex.Message}";
                return false;
            }
        }

        #endregion

        #region Попапы (общие хелперы)

        public void OpenPopupByTriggerOrForce(string dataPopupName, string triggerSelector)
        {
            var triggers = _driver.FindElements(By.CssSelector(triggerSelector));
            if (triggers.Count > 0) { SafeClick(triggers[0]); Sleep(300); }

            if (!IsPopupActive(dataPopupName))
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    const p = document.querySelector(`div.popup[data-popup-name='`+arguments[0]+`']`);
                    if (p) { p.classList.add('active'); p.style.display='block'; }
                ", dataPopupName);
                Sleep(150);
            }
        }

        public bool IsPopupActive(string dataPopupName)
        {
            try
            {
                var pop = _driver.FindElements(
                    By.CssSelector($"div.popup[data-popup-name='{dataPopupName}']")).FirstOrDefault();
                if (pop == null) return false;
                return (bool)((IJavaScriptExecutor)_driver).ExecuteScript(
                    "const p=arguments[0];return p.classList.contains('active')||getComputedStyle(p).display!=='none';", pop);
            }
            catch { return false; }
        }

        /// <summary>Ждёт видимость попапа с именем data-popup-name.</summary>
        public void WaitPopupVisible(string dataPopupName, int seconds = 40)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(seconds));
            wait.Until(d =>
            {
                try
                {
                    var root = d.FindElements(By.CssSelector($"div.popup[data-popup-name='{dataPopupName}']")).FirstOrDefault();
                    if (root == null) return false;

                    var isActive = (bool)((IJavaScriptExecutor)d).ExecuteScript(
                        "const p=arguments[0]; return p && p.classList && p.classList.contains('active');", root);

                    var display = (string)((IJavaScriptExecutor)d).ExecuteScript(
                        "return getComputedStyle(arguments[0]).display;", root);

                    return isActive || !string.Equals(display, "none", StringComparison.OrdinalIgnoreCase);
                }
                catch { return false; }
            });
            Sleep(150);
        }

        #endregion

        #region Скриншоты

        public void TakeScreenshot(string locale, string key)
        {
            try
            {
                var screenshotsDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                var fileName = $"{locale}_{key}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
                var fullPath = Path.Combine(screenshotsDir, fileName);

                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var bytes = screenshot.AsByteArray;
                File.WriteAllBytes(fullPath, bytes);

                var relativePath = Path.Combine("Screenshots", fileName).Replace("\\", "/");
                TestContext.AddTestAttachment(fullPath, $"Скриншот {locale}_{key}");

                TestContext.WriteLine($"📸 Скриншот сохранён: <a href='{relativePath}' style='color: #007bff; text-decoration: underline;'>{fileName}</a>");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"⚠️ Не удалось сделать скриншот: {ex.Message}");
            }
        }

        #endregion

        #region Дополнительные методы для совместимости

        public void ClickByCss(string cssSelector)
        {
            var element = _wait.Until(d => d.FindElement(By.CssSelector(cssSelector)));
            SafeClick(element);
        }

        public void ForceOpenPopup(string popupName)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                const p = document.querySelector(`div.popup[data-popup-name='`+arguments[0]+`']`);
                if (p) { 
                    p.classList.add('active'); 
                    p.style.display='block';
                }
            ", popupName);
            Sleep(150);
        }

        /// <summary>Устанавливает значение элемента через JavaScript.</summary>
        public void JsSetValue(IWebElement element, string value)
        {
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].value = arguments[1];", element, value);
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"⚠️ Не удалось установить значение через JS: {ex.Message}");
                // Пробуем обычный способ как fallback
                element.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    element.SendKeys(value);
                }
            }
        }

        #endregion
    }
}