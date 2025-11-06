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
    public class SignupLocalizationTests
    {
        private ChromeDriver _driver;
        private WebDriverWait _wait;

        private const string RootUrl = "https://atas.bambus.com.ua/";
        private const string BasicAuthUser = "bambuk";
        private const string BasicAuthPass = "Atastraders$";

        // Название в меню -> код локали
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

        // WPML cookie
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

        // I18n для попапа Регистрации
        private static readonly Dictionary<string, Dictionary<string, string>> I18n = new()
        {
            ["SignUp"] = new()
            {
                // Заголовок
                ["RU.Title"] = "Регистрация",
                ["EN.Title"] = "Sign Up",
                ["DE.Title"] = "Registrierung",
                ["ES.Title"] = "Registro",
                ["FR.Title"] = "Inscription",
                ["IT.Title"] = "Registrazione",
                ["UA.Title"] = "Реєстрація",
                ["CN.Title"] = "注册",

                // Email label
                ["RU.EmailLabel"] = "Email",
                ["EN.EmailLabel"] = "Email",
                ["DE.EmailLabel"] = "Email",
                ["ES.EmailLabel"] = "Correo electrónico",
                ["FR.EmailLabel"] = "Email",
                ["IT.EmailLabel"] = "Email",
                ["UA.EmailLabel"] = "Email",
                ["CN.EmailLabel"] = "邮箱",

                // Placeholder email
                ["RU.EmailPh"] = "Eмail (на него придет пароль от ATAS)",
                ["EN.EmailPh"] = "We’ll send your ATAS password to this email address",
                ["DE.EmailPh"] = "E-Mail-Adresse eingeben",
                ["ES.EmailPh"] = "Te enviaremos tu contraseña de ATAS a esta dirección de correo electrónico",
                ["FR.EmailPh"] = "Saisissez votre adresse e-mail",
                ["IT.EmailPh"] = "Inserisci il tuo indirizzo e-mail",
                ["UA.EmailPh"] = "Введи свій email",
                ["CN.EmailPh"] = "我们将向此邮箱发送您的ATAS密码。",

                // Чекбоксы
                ["RU.Marketing"] = "Я хочу получать специальные предложения",
                ["EN.Marketing"] = "I would like to receive special offers from ATAS",
                ["DE.Marketing"] = "Neuigkeiten und Angebote von ATAS erhalten",
                ["ES.Marketing"] = "Deseo recibir ofertas especiales de ATAS",
                ["FR.Marketing"] = "Recevoir les actualités et offres d’ATAS",
                ["IT.Marketing"] = "Ricevi notizie e offerte da ATAS",
                ["UA.Marketing"] = "Отримувати новини та пропозиції від ATAS",
                ["CN.Marketing"] = "接收 ATAS 的新闻和优惠信息",

                ["RU.Agree"] = "Пожалуйста, ознакомься и подтверди согласие с",
                ["EN.Agree"] = "Please read and accept the Terms of Use and License Agreement",
                ["DE.Agree"] = "stimmst du den Terms of Use und dem License Agreement zu",
                ["ES.Agree"] = "Por favor, lee y acepta los Términos de uso y el Acuerdo de licencia",
                ["FR.Agree"] = "tu acceptes les Terms of Use et le License Agreement",
                ["IT.Agree"] = "accetti i Terms of Use e il License Agreement",
                ["UA.Agree"] = "погоджуєшся з Terms of Use та License Agreement",
                ["CN.Agree"] = "请阅读并接受《使用条款》和《许可协议》",

                // Кнопка отправки
                ["RU.Btn"] = "Зарегистрироваться",
                ["EN.Btn"] = "Sign Up",
                ["DE.Btn"] = "Registrieren",
                ["ES.Btn"] = "Registrarse",
                ["FR.Btn"] = "S’inscrire",
                ["IT.Btn"] = "Registrati",
                ["UA.Btn"] = "Зареєструватися",
                ["CN.Btn"] = "注册",

                // Нижний блок
                ["RU.BottomQ"] = "Уже есть учетная запись",
                ["EN.BottomQ"] = "Already have an account",
                ["DE.BottomQ"] = "Hast du schon ein Konto",
                ["ES.BottomQ"] = "¿Ya tienes una cuenta",
                ["FR.BottomQ"] = "Tu as déjà un compte",
                ["IT.BottomQ"] = "Hai già un account",
                ["UA.BottomQ"] = "Уже маєш акаунт",
                ["CN.BottomQ"] = "已有账号",

                ["RU.BottomSignIn"] = "Войти",
                ["EN.BottomSignIn"] = "Sign In",
                ["DE.BottomSignIn"] = "Anmelden",
                ["ES.BottomSignIn"] = "Inicia sesión",
                ["FR.BottomSignIn"] = "Connexion",
                ["IT.BottomSignIn"] = "Accedi",
                ["UA.BottomSignIn"] = "Увійти",
                ["CN.BottomSignIn"] = "登录",

                // Success-попап
                ["RU.SuccessTitle"] = "Спасибо за регистрацию!",
                ["EN.SuccessTitle"] = "Thanks for signing up!",
                ["DE.SuccessTitle"] = "Danke für deine Registrierung!",
                ["ES.SuccessTitle"] = "¡Gracias por registrarte!",
                ["FR.SuccessTitle"] = "Merci pour ton inscription !",
                ["IT.SuccessTitle"] = "Grazie per la registrazione!",
                ["UA.SuccessTitle"] = "Дякуємо за реєстрацію!",
                ["CN.SuccessTitle"] = "感谢您的注册！",

                ["RU.SuccessDesc"] = "Проверь почту — мы отправили письмо со ссылкой для активации аккаунта.",
                ["EN.SuccessDesc"] = "Check your inbox — we’ve sent you an email with an activation link.",
                ["DE.SuccessDesc"] = "Überprüfe dein Postfach – wir haben dir eine E-Mail mit einem Aktivierungslink geschickt.",
                ["ES.SuccessDesc"] = "Revisa tu bandeja de entrada: te hemos enviado un correo con el enlace de activación.",
                ["FR.SuccessDesc"] = "Vérifie ta boîte mail — nous t’avons envoyé un e-mail avec un lien d’activation.",
                ["IT.SuccessDesc"] = "Controlla la tua casella di posta — ti abbiamo inviato un’e-mail con il link di attivazione.",
                ["UA.SuccessDesc"] = "Перевір пошту — ми надіслали лист із посиланням для активації акаунта.",
                ["CN.SuccessDesc"] = "请查看邮箱，我们已发送激活链接。",

                ["RU.Close"] = "Закрыть",
                ["EN.Close"] = "Close",
                ["DE.Close"] = "Schließen",
                ["ES.Close"] = "Cerrar",
                ["FR.Close"] = "Fermer",
                ["IT.Close"] = "Chiudi",
                ["UA.Close"] = "Закрити",
                ["CN.Close"] = "关闭",
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

            EnableBasicAuthHeader(); // basic auth заголовком
        }

        [TearDown]
        public void TearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        // ---------- Один прогон = одна локаль ----------
        [Test]
        [TestCaseSource(nameof(AllLocales))]
        public void SignUp_Popup_One_Locale(string locale)
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
                EnsureLanguageCookie(locale);
            });

            Step("Открыть попап регистрации", OpenSignupPopup);

            Step($"Проверить тексты попапа регистрации ({locale})", () =>
            {
                var soft = new SoftVerify($"SignUp-{locale}");
                VerifySignupTranslations_AllSoft(locale, soft);
                soft.ThrowIfAny();
            });

            Step("Ввести валидный email, отметить чекбоксы и отправить", () =>
            {
                SubmitSignupFormWithEmail(RandomEmail());
            });

            Step($"Проверить success-попап «Спасибо за регистрацию!» ({locale})", () =>
            {
                var soft = new SoftVerify($"SignUp-Success-{locale}");
                VerifySignupSuccess_AllSoft(locale, soft);
                soft.ThrowIfAny();
            });

            Step("Закрыть success-попап", CloseSuccessPopupIfAny);
        }

        private static IEnumerable<string> AllLocales()
            => new[] { "EN", "RU", "FR", "DE", "IT", "ES", "UA", "CN" };

        // ---------- Открытие попапа регистрации ----------
        private void OpenSignupPopup()
        {
            // 1) Прямая кнопка Sign Up
            var btn = _driver.FindElements(By.CssSelector("a[href*='signup' i], [data-open-popup='signup'], a.btn[href='#form-signup']"))
                             .FirstOrDefault();
            if (btn == null)
            {
                // 2) Через Sign In → Sign up
                var signIn = _wait.Until(d => d.FindElement(By.CssSelector("a.header-log-in[href='#form-signin']")));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", signIn);
                _wait.Until(d => d.FindElement(By.XPath("//form[@action='/v2/account/signIn']")));
                WaitPopupVisibleRoot();

                btn = _wait.Until(d =>
                    d.FindElements(By.CssSelector("[data-open-popup='signup'], .cta-bottom button"))
                     .FirstOrDefault(x => ((x.Text ?? "").Trim().Length > 0)));
            }

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
            _wait.Until(d => d.FindElement(By.CssSelector("form[action='/v2/customers/signUp']")));
            WaitPopupVisibleRoot();
        }

        // ---------- Проверка попапа регистрации ----------
        private void VerifySignupTranslations_AllSoft(string locale, SoftVerify soft)
        {
            IWebElement root = null;
            try
            {
                root = _wait.Until(d => d.FindElement(
                    By.XPath("//div[contains(@class,'popup-content')]//form[@action='/v2/customers/signUp']/ancestor::div[contains(@class,'popup-content')]")));
                WaitPopupVisibleRoot();
            }
            catch
            {
                soft.Contains("PopupRoot", "FOUND", "NOT FOUND");
            }

            string T(By by, int sec = 15) => root == null ? "" : WaitNonEmptyText(root, by, TimeSpan.FromSeconds(sec));
            string A(By by, string attr) => root == null ? "" : SafeAttr(root, by, attr);

            // Title
            var title = T(By.CssSelector(".neue-40-bold.title"));
            soft.Contains("Title", I18n["SignUp"][$"{locale}.Title"], title);

            // Email label
            var emailLabel = T(By.XPath(".//form[@action='/v2/customers/signUp']//label[1]//span[contains(@class,'placeholder')]"));
            soft.Contains("EmailLabel", I18n["SignUp"][$"{locale}.EmailLabel"], emailLabel);

            // Email placeholder
            var emailPh = A(By.CssSelector("form[action='/v2/customers/signUp'] input[type='email']"), "placeholder");
            soft.Equal("EmailPh", I18n["SignUp"][$"{locale}.EmailPh"], emailPh);

            // Marketing checkbox label
            var marketing = T(By.XPath(".//label[contains(@class,'checkbox')][1]//span[normalize-space()][last()]"));
            soft.Contains("Marketing", I18n["SignUp"][$"{locale}.Marketing"], marketing);

            // Agree checkbox text
            var agree = T(By.XPath(".//label[contains(@class,'checkbox')][2]//span[normalize-space()][last()]"));
            soft.Contains("Agree", I18n["SignUp"][$"{locale}.Agree"], agree);

            // Submit button
            var btnText = T(By.CssSelector("form[action='/v2/customers/signUp'] .btn-text"));
            soft.Contains("Button", I18n["SignUp"][$"{locale}.Btn"], btnText);

            // Bottom block
            var bottomQ = T(By.CssSelector("form[action='/v2/customers/signUp'] .cta-bottom p"));
            soft.Contains("Bottom.Question", I18n["SignUp"][$"{locale}.BottomQ"], bottomQ);

            var bottomSignIn = T(By.CssSelector("form[action='/v2/customners/signUp'] .cta-bottom button")); // опечатка селектора исправим ниже
            if (string.IsNullOrEmpty(bottomSignIn))
            {
                bottomSignIn = T(By.CssSelector("form[action='/v2/customers/signUp'] .cta-bottom button"));
            }
            soft.Contains("Bottom.SignIn", I18n["SignUp"][$"{locale}.BottomSignIn"], bottomSignIn);
        }

        // ---------- Ввод email, чекбоксы, отправка ----------
        private void SubmitSignupFormWithEmail(string email)
        {
            var form = _wait.Until(d => d.FindElement(By.CssSelector("form[action='/v2/customers/signUp']")));

            // email
            var emailInput = form.FindElement(By.CssSelector("input[type='email']"));
            emailInput.Clear();
            emailInput.SendKeys(email);

            // чекбоксы (оба)
            var checkboxes = form.FindElements(By.CssSelector("input[type='checkbox']"));
            foreach (var cb in checkboxes)
            {
                try
                {
                    if (!cb.Selected)
                    {
                        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", cb);
                    }
                }
                catch { /* ignore */ }
            }

            // ждём, пока кнопка станет активной
            var submitBtn = form.FindElement(By.CssSelector("button[type='submit']"));
            _wait.Until(_ =>
            {
                try
                {
                    return string.IsNullOrEmpty(submitBtn.GetAttribute("disabled"));
                }
                catch { return false; }
            });

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submitBtn);
        }

        // ---------- Проверка success-попапа «Спасибо за регистрацию!» ----------
        private void VerifySignupSuccess_AllSoft(string locale, SoftVerify soft)
        {
            // Вариант 1: отдельный success-попап .popup-content.flex-v с data-popup-title/text
            IWebElement successRoot = null;
            try
            {
                successRoot = _wait.Until(d =>
                    d.FindElements(By.CssSelector("div.popup-content.flex-v")).FirstOrDefault(
                        r => r.FindElements(By.CssSelector("[data-popup-title], .title")).Any()));
            }
            catch { /* ignore */ }

            // Вариант 2: success внутри блока с data-msgs (у формы)
            if (successRoot == null)
            {
                try
                {
                    successRoot = _wait.Until(d =>
                        d.FindElements(By.CssSelector("div.popup-content"))
                         .FirstOrDefault(x => x.FindElements(By.CssSelector("[data-msgs] .success")).Any()));
                }
                catch { /* ignore */ }
            }

            if (successRoot == null)
            {
                soft.Contains("Success.PopupRoot", "FOUND", "NOT FOUND");
                return;
            }

            WaitPopupVisibleRoot();

            string T(By by, int sec = 20) => WaitNonEmptyText(successRoot, by, TimeSpan.FromSeconds(sec));

            // Title
            var sTitle = T(By.CssSelector("[data-popup-title], .title"));
            soft.Contains("Success.Title", I18n["SignUp"][$"{locale}.SuccessTitle"], sTitle);

            // Description
            var sDesc = T(By.CssSelector("[data-popup-text], .text, .subtitle, .success .subtitle"));
            soft.Contains("Success.Description", I18n["SignUp"][$"{locale}.SuccessDesc"], sDesc);

            // Close
            var closeText = T(By.CssSelector("button[data-popup-close], .success button, .popup-content button.btn"));
            soft.Contains("Success.Close", I18n["SignUp"][$"{locale}.Close"], closeText);
        }

        // ---------- Закрыть success-попап ----------
        private void CloseSuccessPopupIfAny()
        {
            var closeBtn = _driver.FindElements(By.CssSelector("button[data-popup-close], .popup-content.flex-v button, .popup-content button.btn"))
                                  .FirstOrDefault();
            if (closeBtn != null)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", closeBtn);
                // подождём исчезновения
                _wait.Until(_ =>
                {
                    try
                    {
                        var still = _driver.FindElements(By.CssSelector("div.popup-content.flex-v")).Any();
                        return !still;
                    }
                    catch { return true; }
                });
            }
        }

        // ---------- Меню языков ----------
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

        // ---------- «Умные» ожидания/утилиты ----------
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
            catch { return ""; }
        }

        private string SafeAttr(IWebElement scope, By by, string attr, string @default = "")
        {
            try
            {
                var el = scope.FindElement(by);
                return el?.GetAttribute(attr) ?? @default;
            }
            catch { return @default; }
        }

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

        private static string RandomEmail()
        {
            var guid = Guid.NewGuid().ToString("N").Substring(0, 6);
            return $"qatest_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{guid}@example.com";
        }

        private void EnableBasicAuthHeader()
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{BasicAuthUser}:{BasicAuthPass}"));
            _driver.ExecuteCdpCommand("Network.enable", new Dictionary<string, object>());
            _driver.ExecuteCdpCommand("Network.setExtraHTTPHeaders", new Dictionary<string, object>
            {
                ["headers"] = new Dictionary<string, object> { ["Authorization"] = $"Basic {token}" }
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
                ".bambus.com.ua",
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

        // -------- Soft assertions --------
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
