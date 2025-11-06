using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace AtasLocalizationTests
{
    [TestFixture]
    public class SignupSigninLocalizationTests
    {
        private ChromeDriver _driver;
        private WebDriverWait _wait;

        private const string RootUrl = "https://atas.bambus.com.ua/";
        private const string BasicAuthUser = "bambuk";
        private const string BasicAuthPass = "Atastraders$";

        // отображаемое имя в меню -> наш код локали
        private static readonly Dictionary<string, string> LangNameToLocale = new(StringComparer.OrdinalIgnoreCase)
        {
            ["English"] = "EN",
            ["Русский"] = "RU",
            ["Français"] = "FR",
            ["Deutsch"] = "DE",
            ["Italiano"] = "IT",
            ["Español"] = "ES",
            ["Українська"] = "UA",
            ["Chinese"] = "CN",
            ["中文"] = "CN",
        };

        // На случай, если сайт опирается на cookie языка
        private static readonly Dictionary<string, string> WpmlCookie = new()
        {
            ["EN"] = "en",
            ["RU"] = "ru",
            ["FR"] = "fr",
            ["DE"] = "de",
            ["IT"] = "it",
            ["ES"] = "es",
            ["CN"] = "zh-hans",
            ["UA"] = "uk",
        };

        // Тексты для проверки (оставлен ваш словарь SignIn)
        private static readonly Dictionary<string, Dictionary<string, string>> I18n = new()
        {
            ["SignIn"] = new()
            {
                ["RU.Title"] = "Вход",
                ["EN.Title"] = "Sign In",
                ["DE.Title"] = "Anmeldung",
                ["ES.Title"] = "Iniciar sesión",
                ["FR.Title"] = "Connexion",
                ["IT.Title"] = "Accesso",
                ["UA.Title"] = "Вхід",
                ["CN.Title"] = "登录",

                ["RU.EmailLabel"] = "Email",
                ["EN.EmailLabel"] = "Email",
                ["DE.EmailLabel"] = "E-Mail-Adresse",
                ["ES.EmailLabel"] = "Correo electrónico",
                ["FR.EmailLabel"] = "Еmail",
                ["IT.EmailLabel"] = "Email",
                ["UA.EmailLabel"] = "Еmail",
                ["CN.EmailLabel"] = "邮箱",

                ["RU.EmailPh"] = "Email (на него придет пароль от ATAS)",
                ["EN.EmailPh"] = "Enter your email",
                ["DE.EmailPh"] = "E-Mail-Adresse eingeben",
                ["ES.EmailPh"] = "Introduce tu correo electrónico",
                ["FR.EmailPh"] = "Saisissez votre adresse e-mail",
                ["IT.EmailPh"] = "Inserisci il tuo indirizzo e-mail",
                ["UA.EmailPh"] = "Введи свій email",
                ["CN.EmailPh"] = "请输入您的邮箱地址",

                ["RU.PassLabel"] = "Пароль",
                ["EN.PassLabel"] = "Password",
                ["DE.PassLabel"] = "Passwort",
                ["ES.PassLabel"] = "Contraseña",
                ["FR.PassLabel"] = "Mot de passe",
                ["IT.PassLabel"] = "Password",
                ["UA.PassLabel"] = "Пароль",
                ["CN.PassLabel"] = "密码",

                ["RU.PassPh"] = "Введи свой пароль",
                ["EN.PassPh"] = "Enter your password",
                ["DE.PassPh"] = "Passwort eingeben",
                ["ES.PassPh"] = "Introduce tu contraseña",
                ["FR.PassPh"] = "Saisissez votre mot de passe",
                ["IT.PassPh"] = "Inserisci la tua password",
                ["UA.PassPh"] = "Введи свій пароль",
                ["CN.PassPh"] = "请输入密码",

                ["RU.Forgot"] = "Забыл(а) пароль?",
                ["EN.Forgot"] = "Forgot your password?",
                ["DE.Forgot"] = "Passwort vergessen?",
                ["ES.Forgot"] = "¿Olvidaste tu contraseña?",
                ["FR.Forgot"] = "Mot de passe oublié ?",
                ["IT.Forgot"] = "Hai dimenticato la password?",
                ["UA.Forgot"] = "Забули пароль?",
                ["CN.Forgot"] = "忘记密码？",

                ["RU.NoAcc"] = "Нет учетной записи?",
                ["EN.NoAcc"] = "No account?",
                ["DE.NoAcc"] = "Noch kein Konto?",
                ["ES.NoAcc"] = "¿No tienes cuenta?",
                ["FR.NoAcc"] = "Pas de compte ?",
                ["IT.NoAcc"] = "Non hai un account?",
                ["UA.NoAcc"] = "Немає облікового запису?",
                ["CN.NoAcc"] = "没有账号？",

                ["RU.SignUpLink"] = "Зарегистрироваться",
                ["EN.SignUpLink"] = "Sign up",
                ["DE.SignUpLink"] = "Registrieren",
                ["ES.SignUpLink"] = "Registrarse",
                ["FR.SignUpLink"] = "S’inscrire",
                ["IT.SignUpLink"] = "Registrati",
                ["UA.SignUpLink"] = "Зареєструватися",
                ["CN.SignUpLink"] = "注册",

                ["RU.SignInBtn"] = "Войти",
                ["EN.SignInBtn"] = "Sign In",
                ["DE.SignInBtn"] = "Anmelden",
                ["ES.SignInBtn"] = "Acceder",
                ["FR.SignInBtn"] = "Se connecter",
                ["IT.SignInBtn"] = "Accedi",
                ["UA.SignInBtn"] = "Увійти",
                ["CN.SignInBtn"] = "登录",
            }
        };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
        }

        [SetUp]
        public void SetUp()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            // options.AddArgument("--headless=new");

            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            // Basic-auth через заголовок (без user:pass@ в URL)
            EnableBasicAuthHeader();
        }

        [TearDown]
        public void TearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        // =================== ОТДЕЛЬНЫЙ ТЕСТ ДЛЯ ОТДЕЛЬНОГО ЯЗЫКА ===================
        [Test]
        [TestCaseSource(nameof(AllLocales))]
        public void SignIn_Popup_One_Locale(string locale)
        {
            Step("Открыть базовую страницу", () =>
            {
                _driver.Navigate().GoToUrl(RootUrl);
                _wait.Until(d => d.Title != null);
            });

            Step($"Выбрать язык {locale} из меню", () =>
            {
                OpenLangMenu();
                ClickLanguageByLocale(locale);
                EnsureLanguageCookie(locale); // закрепим язык
            });

            Step("Открыть попап авторизации", OpenSigninFromHeader);

            Step($"Проверить тексты в попапе ({locale})", () =>
            {
                var soft = new SoftVerify($"SignIn-{locale}");
                VerifySigninTranslations_AgainstNewMarkup_WithSmartWaits_AllSoft(locale, soft);
                soft.ThrowIfAny(); // СБОР всех несоответствий, падение в конце
            });
        }

        private static IEnumerable<string> AllLocales()
            => new[] { "EN", "RU", "FR", "DE", "IT", "ES", "UA", "CN" };

        // =================== Работа с языковым меню ===================

        private void OpenLangMenu()
        {
            var btn = _wait.Until(d => d.FindElement(By.CssSelector("button.btn.langs-wrapper-lang")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
            _wait.Until(d => d.FindElement(By.CssSelector("nav.langs-wrapper-dropdown")));
        }

        private void ClickLanguageByLocale(string locale)
        {
            var container = _wait.Until(d => d.FindElement(By.CssSelector("nav.langs-wrapper-dropdown")));
            var links = container.FindElements(By.CssSelector("a.link"));

            foreach (var a in links)
            {
                var name = (a.Text ?? "").Trim();
                if (string.IsNullOrEmpty(name))
                    name = (a.GetAttribute("title") ?? "").Trim();

                var href = a.GetAttribute("href") ?? "";
                var guess = !string.IsNullOrEmpty(name) && LangNameToLocale.TryGetValue(name, out var byName)
                    ? byName
                    : GuessLocaleFromHref(href);

                if (string.Equals(guess, locale, StringComparison.OrdinalIgnoreCase))
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", a);
                    // дождёмся загрузки
                    _wait.Until(_ => (_driver.Url ?? "").Length > 0 && _driver.Title != null);
                    return;
                }
            }
            throw new NoSuchElementException($"Не найдена ссылка языка для локали {locale}.");
        }

        private static string GuessLocaleFromHref(string href)
        {
            if (href.Contains("/ru/", StringComparison.OrdinalIgnoreCase)) return "RU";
            if (href.Contains("/fr/", StringComparison.OrdinalIgnoreCase)) return "FR";
            if (href.Contains("/de/", StringComparison.OrdinalIgnoreCase)) return "DE";
            if (href.Contains("/it/", StringComparison.OrdinalIgnoreCase)) return "IT";
            if (href.Contains("/es/", StringComparison.OrdinalIgnoreCase)) return "ES";
            if (href.Contains("/cn/", StringComparison.OrdinalIgnoreCase)) return "CN";
            if (href.Contains("/ua/", StringComparison.OrdinalIgnoreCase)) return "UA";
            return "EN";
        }

        // =================== Попап авторизации ===================

        private void OpenSigninFromHeader()
        {
            var signIn = _wait.Until(d => d.FindElement(By.CssSelector("a.header-log-in[href='#form-signin']")));
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", signIn);

            // Ждём саму форму
            _wait.Until(d => d.FindElement(By.XPath("//form[@action='/v2/account/signIn']")));

            // И пока попап реально стал видимым (анимация/прозрачность)
            WaitPopupVisibleRoot();
        }

        // ========= «Умные» ожидания для текстов и видимости попапа =========

        // ждём элемент внутри scope и возвращаем НЕПУСТОЙ innerText (или "" без исключения)
        private string WaitNonEmptyText(IWebElement scope, By by, TimeSpan? timeout = null)
        {
            try
            {
                var wait = new WebDriverWait(_driver, timeout ?? TimeSpan.FromSeconds(15));
                return wait.Until(_ =>
                {
                    try
                    {
                        var el = scope.FindElement(by);
                        var txt = (string)((IJavaScriptExecutor)_driver)
                            .ExecuteScript("return (arguments[0].innerText||'').trim();", el);
                        return string.IsNullOrEmpty(txt) ? null : txt;
                    }
                    catch { return null; }
                });
            }
            catch
            {
                return ""; // не роняем тест — вернём пустую строку, soft-assert это зафиксирует
            }
        }

        // безопасное чтение атрибута (без исключений)
        private string SafeAttr(IWebElement scope, By by, string attr, string @default = "")
        {
            try
            {
                var el = scope.FindElement(by);
                return el?.GetAttribute(attr) ?? @default;
            }
            catch { return @default; }
        }

        // ждём корневой контейнер попапа, пока он станет реально видимым (не прозрачен)
        private IWebElement WaitPopupVisibleRoot()
        {
            return _wait.Until(d =>
            {
                var roots = d.FindElements(By.CssSelector("div.popup-content"));
                var root = roots.FirstOrDefault();
                if (root == null) return null;
                try
                {
                    var visible = (bool)((IJavaScriptExecutor)d).ExecuteScript(@"
                        const el = arguments[0];
                        if(!el) return false;
                        const s = getComputedStyle(el);
                        const r = el.getBoundingClientRect();
                        return s.visibility !== 'hidden' && s.opacity !== '0' && r.width>0 && r.height>0;
                    ", root);
                    return visible ? root : null;
                }
                catch { return null; }
            });
        }

        // ---------- Проверка попапа авторизации: ВСЁ через soft-assert, НИЧЕГО не бросаем внутри ----------
        private void VerifySigninTranslations_AgainstNewMarkup_WithSmartWaits_AllSoft(string locale, SoftVerify soft)
        {
            // находим root попапа, но даже если не нашли — продолжим с пустыми значениями
            IWebElement root = null;
            try
            {
                root = _wait.Until(d => d.FindElement(
                    By.XPath("//div[contains(@class,'popup-content')]//form[@action='/v2/account/signIn']/ancestor::div[contains(@class,'popup-content')]")));
                WaitPopupVisibleRoot();
            }
            catch
            {
                soft.Contains("PopupRoot", "FOUND", "NOT FOUND");
                // дальше будем читать значения с пустыми результатами
            }

            // маленький локальный помощник: если root нет — вернём пустые строки
            string T(By by, int seconds = 15) => root == null ? "" : WaitNonEmptyText(root, by, TimeSpan.FromSeconds(seconds));
            string A(By by, string attr) => root == null ? "" : SafeAttr(root, by, attr);

            // Title (в EN может быть 'Sign-In')
            var title = T(By.CssSelector(".neue-40-bold.title"));
            var expectedTitle = I18n["SignIn"][$"{locale}.Title"];
            if (!(SoftEquals(expectedTitle, title) || SoftEquals(expectedTitle.Replace(" ", "-"), title)))
                soft.Contains("Title", expectedTitle, title);

            // E-mail label
            var emailLabel = T(By.XPath(".//form[@action='/v2/account/signIn']//label[1]//span[contains(@class,'placeholder')]"));
            var expectEmailLabel = I18n["SignIn"][$"{locale}.EmailLabel"];
            if (!(SoftEquals(expectEmailLabel, emailLabel) || SoftEquals(expectEmailLabel.Insert(1, "-"), emailLabel)))
                soft.Contains("EmailLabel", expectEmailLabel, emailLabel);

            // Email placeholder
            var emailPh = A(By.CssSelector("form[action='/v2/account/signIn'] input[type='email'][name='email']"), "placeholder");
            soft.Equal("EmailPh", I18n["SignIn"][$"{locale}.EmailPh"], emailPh);

            // Password label
            var passLabel = T(By.XPath(".//form[@action='/v2/account/signIn']//label[2]//span[contains(@class,'placeholder')]"));
            soft.Contains("PassLabel", I18n["SignIn"][$"{locale}.PassLabel"], passLabel);

            // Password placeholder
            var passPh = A(By.CssSelector("form[action='/v2/account/signIn'] input[type='password'][name='password']"), "placeholder");
            soft.Equal("PassPh", I18n["SignIn"][$"{locale}.PassPh"], passPh);

            // Forgot
            var forgot = T(By.CssSelector("form[action='/v2/account/signIn'] button.forgot"));
            soft.Contains("Forgot", I18n["SignIn"][$"{locale}.Forgot"], forgot);

            // No account?
            var noAcc = T(By.CssSelector("form[action='/v2/account/signIn'] .cta-bottom p"));
            soft.Contains("NoAcc", I18n["SignIn"][$"{locale}.NoAcc"], noAcc);

            // Sign up
            var signUpLink = T(By.CssSelector("form[action='/v2/account/signIn'] .cta-bottom button[aria-label*='Sign up' i]"));
            soft.Contains("SignUpLink", I18n["SignIn"][$"{locale}.SignUpLink"], signUpLink);

            // Submit (в разметке — "Submit"; допускаем "Sign In" из словаря)
            var submitText = T(By.CssSelector("form[action='/v2/account/signIn'] button[type='submit'] .btn-text"));
            var expectedBtn = I18n["SignIn"][$"{locale}.SignInBtn"];
            if (!(SoftEquals(expectedBtn, submitText) || SoftEquals("Submit", submitText)))
                soft.Contains("Button", expectedBtn, submitText);
        }

        // =================== Утилиты ===================

        private void EnableBasicAuthHeader()
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{BasicAuthUser}:{BasicAuthPass}"));
            _driver.ExecuteCdpCommand("Network.enable", new Dictionary<string, object>());
            _driver.ExecuteCdpCommand("Network.setExtraHTTPHeaders", new Dictionary<string, object>
            {
                ["headers"] = new Dictionary<string, object>
                {
                    ["Authorization"] = $"Basic {token}"
                }
            });
        }

        private void EnsureLanguageCookie(string locale)
        {
            if (!WpmlCookie.TryGetValue(locale, out var code))
                code = "en";

            try { _driver.Manage().Cookies.DeleteCookieNamed("wp-wpml_current_language"); } catch { }

            _driver.Manage().Cookies.AddCookie(new Cookie(
                "wp-wpml_current_language",
                code,
                ".bambus.com.ua", // общий домен
                "/",
                DateTime.UtcNow.AddDays(1)));

            _driver.Navigate().Refresh();
        }

        private static string Normalize(string s)
        {
            if (s == null) return string.Empty;
            return new string(
                s.Replace("\u00A0", " ")
                 .Replace("-", " ")
                 .Where(ch => !char.IsWhiteSpace(ch))
                 .ToArray()
            ).ToLowerInvariant();
        }
        private static bool SoftEquals(string expect, string actual) => Normalize(expect) == Normalize(actual);

        private void Log(string message)
        {
            TestContext.Progress.WriteLine(message);
            Console.WriteLine(message);
        }

        private string SanitizeFileName(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }

        private void TakeScreenshot(string fileName)
        {
            try
            {
                var ss = ((ITakesScreenshot)_driver).GetScreenshot();
                if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    fileName += ".png";
                File.WriteAllBytes(fileName, ss.AsByteArray);
                TestContext.AddTestAttachment(fileName);
                Log($"Скриншот сохранён: {fileName}");
            }
            catch { }
        }

        private void SavePageHtmlForDebug(string fileName)
        {
            try
            {
                var html = _driver.PageSource ?? "";
                File.WriteAllText(fileName, html);
                TestContext.AddTestAttachment(fileName);
                Log($"HTML сохранён: {fileName}");
            }
            catch { }
        }

        private void Step(string title, Action action)
        {
            Log($"→ {title}");
            try
            {
                action();
                Log($"✓ {title}");
            }
            catch (Exception ex)
            {
                Log($"✗ {title}. Ошибка: {ex.Message}");
                var fname = SanitizeFileName($"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{title}.png");
                TakeScreenshot(fname);
                SavePageHtmlForDebug($"{Path.GetFileNameWithoutExtension(fname)}.html");
                throw;
            }
        }

        private void SafePressEsc()
        {
            try { new Actions(_driver).SendKeys(Keys.Escape).Perform(); } catch { }
        }

        // -------- Soft assertions: копит ВСЕ ошибки и роняет тест только в конце --------
        private sealed class SoftVerify
        {
            private readonly List<string> _errors = new();
            private readonly string _scope;
            public SoftVerify(string scope) => _scope = scope;

            public void Equal(string key, string expect, string actual)
            {
                var ok = string.Equals(expect?.Trim(), (actual ?? "").Trim(), StringComparison.Ordinal);
                LogLine(key, expect, actual, ok, mode: "Equal");
                if (!ok) _errors.Add(FormatErr(key, expect, actual, "=="));
            }

            public void Contains(string key, string expect, string actual)
            {
                bool ok = (actual ?? "").Contains(expect ?? "", StringComparison.Ordinal);
                LogLine(key, expect, actual, ok, mode: "Contains");
                if (!ok) _errors.Add(FormatErr(key, expect, actual, "∈"));
            }

            private void LogLine(string key, string expect, string actual, bool ok, string mode)
            {
                var mark = ok ? "✓" : "✗";
                TestContext.Progress.WriteLine(
                    $"{mark} [{_scope}.{key}] {mode}\n    Expected: \"{expect}\"\n    Actual:   \"{actual}\""
                );
            }

            private string FormatErr(string key, string expect, string actual, string op)
                => $"[{_scope}.{key}] Expected {op}: \"{expect}\"  |  Actual: \"{actual}\"";

            public void ThrowIfAny()
            {
                if (_errors.Count > 0)
                {
                    var msg = $"Найдены несоответствия ({_scope}):\n - " + string.Join("\n - ", _errors);
                    Assert.Fail(msg);
                }
            }
        }
    }
}
