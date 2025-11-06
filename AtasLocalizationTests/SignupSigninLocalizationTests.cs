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

namespace AtasLocalizationTests
{
    [TestFixture]
    public class SignupSigninLocalizationTests
    {
        private ChromeDriver _driver;
        private WebDriverWait _wait;

        private const string BasicAuthUser = "bambuk";
        private const string BasicAuthPass = "Atastraders$";

        private static readonly Dictionary<string, string> LocalePath = new()
        {
            ["EN"] = "",
            ["RU"] = "ru/",
            ["FR"] = "fr/",
            ["DE"] = "de/",
            ["IT"] = "it/",
            ["ES"] = "es/",
            ["CN"] = "cn/",
            ["UA"] = "ua/"
        };

        // соответствие локали значению cookie WPML
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
            },

            ["SignUp"] = new()
            {
                ["RU.Title"] = "Sign Up",
                ["EN.Title"] = "Sign Up",
                ["DE.Title"] = "Sign Up",
                ["ES.Title"] = "Sign Up",
                ["FR.Title"] = "Sign Up",
                ["IT.Title"] = "Sign Up",
                ["UA.Title"] = "Sign Up",
                ["CN.Title"] = "Sign Up",

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

                ["RU.SignUpBtn"] = "Зарегистрироваться",
                ["EN.SignUpBtn"] = "Sign up",
                ["DE.SignUpBtn"] = "Registrieren",
                ["ES.SignUpBtn"] = "Registrarse",
                ["FR.SignUpBtn"] = "S’inscrire",
                ["IT.SignUpBtn"] = "Registrati",
                ["UA.SignUpBtn"] = "Зареєструватися",
                ["CN.SignUpBtn"] = "注册",
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
            _wait  = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
        }

        [TearDown]
        public void TearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        [Test]
        [TestCaseSource(nameof(AllLocales))]
        public void Signup_And_Signin_Popups_Localized_Correctly(string locale)
        {
            var url = $"https://{BasicAuthUser}:{Uri.EscapeDataString(BasicAuthPass)}@atas.bambus.com.ua/" + LocalePath[locale];
            Log($"Открываем {url}");
            _driver.Navigate().GoToUrl(url);

            EnsureLanguageCookie(locale); // привести контент к нужной локали

            // ===== Регистрация =====
            Step("Открыть попап Sign Up", OpenSignup);
            Step($"Проверка переводов Sign Up ({locale})", () =>
            {
                var soft = new SoftVerify("SignUp");
                VerifySignupTranslations(locale, soft);
                soft.ThrowIfAny();
            });
            Step("Заполнить и отправить форму Sign Up", () => FillAndSubmitSignup(locale));
            Step("Скриншот следующего экрана после Sign Up", () => TakeScreenshot($"SignUp_{locale}.png"));
            SafePressEsc();

            // ===== Авторизация =====
            Step("Открыть попап Sign In", OpenSigninFromHeader);
            Step($"Проверка переводов Sign In ({locale})", () =>
            {
                var soft = new SoftVerify("SignIn");
                VerifySigninTranslations(locale, soft);
                soft.ThrowIfAny();
            });
        }

        private static IEnumerable<string> AllLocales()
            => new[] { "EN", "RU", "FR", "DE", "IT", "ES", "CN", "UA" };

        // ---------- helpers & logging ----------
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
                SavePopupHtmlForDebug($"{Path.GetFileNameWithoutExtension(fname)}.html");
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

        private void SavePopupHtmlForDebug(string fileName)
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

        // ---------- Язык (cookie) ----------
        private void EnsureLanguageCookie(string locale)
        {
            if (!WpmlCookie.TryGetValue(locale, out var code))
                code = "en";

            try { _driver.Manage().Cookies.DeleteCookieNamed("wp-wpml_current_language"); } catch { }

            _driver.Manage().Cookies.AddCookie(new Cookie(
                "wp-wpml_current_language",
                code,
                "atas.bambus.com.ua",
                "/",
                DateTime.UtcNow.AddDays(1)));

            _driver.Navigate().Refresh();
        }

        // ---------- Открытие попапов ----------
        private void OpenSignup()
        {
            var startBtn = _wait.Until(d => d.FindElement(By.CssSelector("a.btn[href='#form-signup'], a.btn.btn--gradient[href='#form-signup']")));
            ClickByJs(startBtn);

            SwitchToPopupFrameIfAny();
            WaitPopupRoot("signup");
        }

        private void OpenSigninFromHeader()
        {
            var signIn = _wait.Until(d => d.FindElement(By.CssSelector("a.header-log-in[href='#form-signin'], a[href='#form-signin']")));
            Log($"Кликаем Sign In: {signIn.Text}");
            ClickByJs(signIn);

            SwitchToPopupFrameIfAny();
            WaitPopupRoot("signin");
        }

        // ---------- Верификация переводов ----------
        private void VerifySignupTranslations(string locale, SoftVerify soft)
        {
            var root = WaitPopupRoot("signup");

            // Title
            var title = TextWithin(root,
                By.CssSelector(".neue-40-bold.title, .popup-title, h2, .title"),
                By.XPath(".//*[self::h2 or self::span][contains(.,'Sign')]")
            );
            soft.Contains("Title", I18n["SignUp"][$"{locale}.Title"], title);

            // Email label
            var emailLabel = TextWithin(root,
                By.XPath(".//label[.//span[contains(@class,'placeholder')]]//span[contains(@class,'api-wrapper')]"),
                By.XPath(".//*[self::label or self::span][contains(translate(., 'EMAILЕМАІL', 'emaileмаіл'), 'email')]")
            );
            soft.Contains("EmailLabel", I18n["SignUp"][$"{locale}.EmailLabel"], emailLabel);

            // Email placeholder
            var email = root.FindElement(By.CssSelector("input[type='email'], input[name*='email' i]"));
            soft.Equal("EmailPh", I18n["SignUp"][$"{locale}.EmailPh"], email.GetAttribute("placeholder") ?? "");

            // Button text
            var signUpBtn = FirstDisplayed(root,
                By.CssSelector("button[type='submit']"),
                By.CssSelector("[role='button']"),
                By.XPath(".//button|.//a")
            );
            soft.Contains("Button", I18n["SignUp"][$"{locale}.SignUpBtn"], signUpBtn.Text.Trim());
        }

        private void VerifySigninTranslations(string locale, SoftVerify soft)
        {
            var root = WaitPopupRoot("signin");

            var title = TextWithin(root,
                By.CssSelector(".neue-40-bold.title, .popup-title, h2, .title"),
                By.XPath(".//*[self::h2 or self::span]")
            );
            soft.Contains("Title", I18n["SignIn"][$"{locale}.Title"], title);

            var emailLabel = TextWithin(root,
                By.XPath(".//*[self::label or self::span][contains(.,'mail') or contains(.,'邮箱') or contains(.,'Еmail')]")
            );
            soft.Contains("EmailLabel", I18n["SignIn"][$"{locale}.EmailLabel"], emailLabel);

            var email = root.FindElements(By.CssSelector("input[type='email'], input[name*='email' i]")).FirstOrDefault();
            if (email != null)
                soft.Equal("EmailPh", I18n["SignIn"][$"{locale}.EmailPh"], email.GetAttribute("placeholder") ?? "");
            else
                Log("Внимание: поле email не найдено в Sign In");

            var pass = root.FindElements(By.CssSelector("input[type='password'], input[name*='pass' i]")).FirstOrDefault();
            if (pass != null)
            {
                var passLabel = TextWithin(root,
                    By.XPath(".//*[self::label or self::span][contains(.,'Pass') or contains(.,'Пароль') or contains(.,'密码')]")
                );
                soft.Contains("PassLabel", I18n["SignIn"][$"{locale}.PassLabel"], passLabel);
                soft.Equal("PassPh", I18n["SignIn"][$"{locale}.PassPh"], pass.GetAttribute("placeholder") ?? "");
            }
            else
                Log("Внимание: поле password не найдено в Sign In");

            var forgot = TextWithin(root,
                By.XPath(".//a[contains(@href,'forgot') or contains(.,'?') or contains(.,'Забыл') or contains(.,'Passwort vergessen') or contains(.,'Mot de passe') or contains(.,'Забули') or contains(.,'忘记')]")
            );
            if (!string.IsNullOrEmpty(forgot))
                soft.Contains("Forgot", I18n["SignIn"][$"{locale}.Forgot"], forgot);

            var noAcc = TextWithin(root,
                By.XPath(".//*[contains(text(),'No account') or contains(text(),'Нет учетной записи') or contains(text(),'Noch kein Konto') or contains(text(),'Pas de compte') or contains(text(),'Немає облікового запису') or contains(text(),'没有账号')]")
            );
            if (!string.IsNullOrEmpty(noAcc))
                soft.Contains("NoAcc", I18n["SignIn"][$"{locale}.NoAcc"], noAcc);

            var signUpLink = TextWithin(root,
                By.XPath(".//a[contains(@href,'signup') or contains(.,'Sign up') or contains(.,'Зарегистр') or contains(.,'Registr') or contains(.,'S’inscrire') or contains(.,'注册')]")
            );
            if (!string.IsNullOrEmpty(signUpLink))
                soft.Contains("SignUpLink", I18n["SignIn"][$"{locale}.SignUpLink"], signUpLink);

            var signInBtn = TextWithin(root,
                By.CssSelector("button[type='submit'], [role='button']")
            );
            if (!string.IsNullOrEmpty(signInBtn))
                soft.Contains("Button", I18n["SignIn"][$"{locale}.SignInBtn"], signInBtn);
        }

        // ---------- Действия с формой Sign Up ----------
        private void FillAndSubmitSignup(string _locale)
        {
            var root = WaitPopupRoot("signup");
            var form = root.FindElements(By.CssSelector("form")).FirstOrDefault() ?? root;

            var email = form.FindElement(By.CssSelector("input[type='email'], input[name*='email' i]"));
            var value = RandomEmail();
            Log($"Ввод email: {value}");
            email.Clear();
            email.SendKeys(value);

            foreach (var cb in form.FindElements(By.CssSelector("input[type='checkbox']")))
                if (!cb.Selected) cb.Click();

            var submit = FirstDisplayed(form,
                By.CssSelector("button[type='submit']"),
                By.CssSelector("[role='button']")
            );
            _wait.Until(_ => submit.Enabled);
            Log("Жмём Sign Up");
            ClickByJs(submit);

            _wait.Until(_ =>
            {
                try
                {
                    return !root.Displayed ||
                           root.FindElements(By.CssSelector(".success, .error, .custom-form[data-msgs], [data-state*='success'], [data-state*='error']")).Count > 0;
                }
                catch { return true; }
            });
        }

        // ---------- Вспомогательные ----------
        private void ClickByJs(IWebElement el)
            => ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", el);

        private void SwitchToPopupFrameIfAny()
        {
            try
            {
                var frames = _driver.FindElements(By.CssSelector("iframe[src*='signup'], iframe[src*='signin'], iframe[id*='signup'], iframe[id*='signin']"));
                if (frames.Count > 0) _driver.SwitchTo().Frame(frames[0]);
            }
            catch { }
        }

        private IWebElement WaitPopupRoot(string kind /* "signup" | "signin" */)
        {
            var locators = new By[]
            {
                By.CssSelector($"#form-{kind}"),
                By.CssSelector($"[id*='form-{kind}']"),
                By.CssSelector($"[data-popup-name*='{kind}']"),
                By.CssSelector($".popup-content[data-popup*='{kind}']"),
                By.CssSelector($".popup[data-popup*='{kind}']"),
            };

            foreach (var by in locators)
            {
                try
                {
                    var root = _wait.Until(d =>
                    {
                        var el = d.FindElements(by).FirstOrDefault();
                        return (el != null && el.Displayed) ? el : null;
                    });
                    if (root != null) return root;
                }
                catch { }
            }
            throw new WebDriverTimeoutException($"Не найден попап '{kind}' по ожидаемым локаторам.");
        }

        private string TextWithin(IWebElement scope, params By[] locators)
        {
            foreach (var by in locators)
            {
                try
                {
                    var el = _wait.Until(_ =>
                    {
                        var candidate = scope.FindElements(by).FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
                        return candidate;
                    });
                    if (el != null) return el.Text.Trim();
                }
                catch { }
            }
            return string.Empty;
        }

        private IWebElement FirstDisplayed(IWebElement scope, params By[] locators)
        {
            foreach (var by in locators)
            {
                var el = scope.FindElements(by).FirstOrDefault(e => e.Displayed);
                if (el != null) return el;
            }
            throw new NoSuchElementException($"Не найден видимый элемент по: {string.Join(" | ", locators.Select(l => l.ToString()))}");
        }

        private static string RandomEmail()
            => $"qatest_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..6]}@example.com";

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

        private void SafePressEsc()
        {
            try { new Actions(_driver).SendKeys(Keys.Escape).Perform(); } catch { }
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
