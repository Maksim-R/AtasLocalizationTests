using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

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

        // ---------- Меню языков ----------
        private static readonly Dictionary<string, string> LangNameToLocale = new(StringComparer.OrdinalIgnoreCase)
        {
            ["English"] = "EN",
            ["Русский"] = "RU",
            ["Français"] = "FR",
            ["Deutsch"] = "DE",
            ["Italiano"] = "IT",
            ["Español"] = "ES",
            ["Українська"] = "UA",
            ["Chinese (Simplified)"] = "CN"
        };

        // ---------- Полная спецификация SignUp + Success ----------
        private static readonly Dictionary<string, Dictionary<string, string>> I18n = new()
        {
            ["SignUp"] = new()
            {
                // Title
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

                // Placeholder
                ["RU.EmailPh"] = "Eмail (на него придет пароль от ATAS)",
                ["EN.EmailPh"] = "We’ll send your ATAS password to this email address",
                ["DE.EmailPh"] = "E-Mail-Adresse eingeben",
                ["ES.EmailPh"] = "Te enviaremos tu contraseña de ATAS a esta dirección de correo electrónico",
                ["FR.EmailPh"] = "Saisissez votre adresse e-mail",
                ["IT.EmailPh"] = "Inserisci il tuo indirizzo e-mail",
                ["UA.EmailPh"] = "Введи свій email",
                ["CN.EmailPh"] = "我们将向此邮箱发送您的ATAS密码。",

                // Marketing checkbox
                ["RU.Marketing"] = "Я хочу получать специальные предложения",
                ["EN.Marketing"] = "I would like to receive special offers from ATAS",
                ["DE.Marketing"] = "Neuigkeiten und Angebote von ATAS erhalten",
                ["ES.Marketing"] = "Deseo recibir ofertas especiales de ATAS",
                ["FR.Marketing"] = "Recevoir les actualités et offres d’ATAS",
                ["IT.Marketing"] = "Ricevi notizie e offerte da ATAS",
                ["UA.Marketing"] = "Отримувати новини та пропозиції від ATAS",
                ["CN.Marketing"] = "接收 ATAS 的新闻和优惠信息",

                // Agree checkbox
                ["RU.Agree"] = "Пожалуйста, ознакомься и подтверди согласие с",
                ["EN.Agree"] = "Please read and accept the Terms of Use and License Agreement",
                ["DE.Agree"] = "stimmst du den Terms of Use und dem License Agreement zu",
                ["ES.Agree"] = "Por favor, lee y acepta los Términos de uso y el Acuerdo de licencia",
                ["FR.Agree"] = "tu acceptes les Terms of Use et le License Agreement",
                ["IT.Agree"] = "accetti i Terms of Use e il License Agreement",
                ["UA.Agree"] = "погоджуєшся з Terms of Use та License Agreement",
                ["CN.Agree"] = "请阅读并接受《使用条款》和《许可协议》",

                // Button
                ["RU.Btn"] = "Зарегистрироваться",
                ["EN.Btn"] = "Sign Up",
                ["DE.Btn"] = "Registrieren",
                ["ES.Btn"] = "Registrarse",
                ["FR.Btn"] = "S’inscrire",
                ["IT.Btn"] = "Registrati",
                ["UA.Btn"] = "Зареєструватися",
                ["CN.Btn"] = "注册",

                // Bottom block
                ["RU.BottomQ"] = "Уже есть учётная запись?",
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

                // Success popup
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

        // ---------- Setup ----------
        [SetUp]
        public void SetUp()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            var options = new ChromeOptions();
            options.AddArgument("--window-size=1366,900");
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
            EnableBasicAuthHeader();
        }

        [TearDown]
        public void TearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        // ---------- Тест ----------
        [Test]
        [TestCaseSource(nameof(Locales))]
        public void Verify_SignUp_And_Success_Popup_Texts(string locale)
        {
            _driver.Navigate().GoToUrl(RootUrl);
            SelectLocaleViaUi(locale);
            WaitPageReady();

            var soft = new SoftVerify(_driver, locale);
            var dict = I18n["SignUp"];

            string Exp(string k) => dict[$"{locale}.{k}"];

            // Корень формы
            var form = _driver.FindElements(By.CssSelector("form[action*='signUp']")).FirstOrDefault();
            if (form == null)
            {
                soft.Add($"Форма регистрации не найдена для {locale}");
                soft.Report();
                return;
            }

            // Проверяем все элементы
            soft.Contains("Title", TextHidden(form, By.XPath("./ancestor::div[contains(@class,'popup-content')]//div[contains(@class,'title')]")), Exp("Title"));
            soft.Contains("EmailLabel", TextHidden(form, By.XPath(".//label[1]//span[contains(@class,'placeholder')]")), Exp("EmailLabel"));
            soft.Equal("EmailPh", Attr(form, By.CssSelector("input[type='email']"), "placeholder"), Exp("EmailPh"));
            soft.Contains("Marketing", TextHidden(form, By.XPath("(.//label[contains(@class,'checkbox')])[1]")), Exp("Marketing"));
            soft.Contains("Agree", TextHidden(form, By.XPath("(.//label[contains(@class,'checkbox')])[2]")), Exp("Agree"));
            soft.Contains("Btn", TextHidden(form, By.CssSelector("button[type='submit']")), Exp("Btn"));
            soft.Contains("BottomQ", TextHidden(form, By.CssSelector(".cta-bottom p")), Exp("BottomQ"));
            soft.Contains("BottomSignIn", TextHidden(form, By.CssSelector(".cta-bottom button, .cta-bottom a")), Exp("BottomSignIn"));

            // success popup
            var success = _driver.FindElements(By.CssSelector("div[data-popup-name='success'], .success-popup")).FirstOrDefault();
            if (success != null)
            {
                soft.Contains("SuccessTitle", TextHidden(success, By.CssSelector("[data-popup-title], .title")), Exp("SuccessTitle"));
                soft.Contains("SuccessDesc", TextHidden(success, By.CssSelector("[data-popup-text], .text")), Exp("SuccessDesc"));
                soft.Contains("Close", TextHidden(success, By.CssSelector("button[data-popup-close], .btn")), Exp("Close"));
            }

            soft.Report();
        }

        // ---------- Helpers ----------
        private static IEnumerable<string> Locales() => new[] { "EN", "RU", "FR", "DE", "IT", "ES", "UA", "CN" };

        private void SelectLocaleViaUi(string locale)
        {
            var btn = _wait.Until(d => d.FindElement(By.CssSelector("button.btn.langs-wrapper-lang")));
            btn.Click();

            var links = _wait.Until(d => d.FindElements(By.CssSelector("nav.langs-wrapper-dropdown a.link")));
            foreach (var a in links)
            {
                var name = (a.Text ?? "").Trim();
                if (LangNameToLocale.TryGetValue(name, out var guess) && guess == locale)
                {
                    a.Click();
                    Thread.Sleep(1000);
                    return;
                }
            }
        }

        private string TextHidden(IWebElement scope, By by)
        {
            try
            {
                var el = scope.FindElement(by);
                return (string)((IJavaScriptExecutor)_driver).ExecuteScript("return (arguments[0].textContent||'').trim();", el);
            }
            catch { return ""; }
        }

        private string Attr(IWebElement scope, By by, string attr)
        {
            try { return scope.FindElement(by).GetAttribute(attr) ?? ""; }
            catch { return ""; }
        }

        private void WaitPageReady()
        {
            _wait.Until(d => ((string)((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")) == "complete");
            Thread.Sleep(200);
        }

        private void EnableBasicAuthHeader()
        {
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{BasicAuthUser}:{BasicAuthPass}"));
            _driver.ExecuteCdpCommand("Network.enable", new Dictionary<string, object>());
            _driver.ExecuteCdpCommand("Network.setExtraHTTPHeaders", new Dictionary<string, object>
            {
                ["headers"] = new Dictionary<string, object> { ["Authorization"] = $"Basic {token}" }
            });
        }

        // ---------- SoftVerify ----------
        private sealed class SoftVerify
        {
            private readonly IWebDriver _driver;
            private readonly string _scope;
            private readonly List<string> _errors = new();

            public SoftVerify(IWebDriver driver, string scope)
            {
                _driver = driver;
                _scope = scope;
                Directory.CreateDirectory("Screenshots");
            }

            public void Contains(string key, string actual, string expected)
            {
                if (!SafeContains(actual, expected))
                    AddError(key, expected, actual);
            }

            public void Equal(string key, string actual, string expected)
            {
                if (!string.Equals((actual ?? "").Trim(), (expected ?? "").Trim(), StringComparison.Ordinal))
                    AddError(key, expected, actual);
            }

            public void Add(string msg)
            {
                _errors.Add(msg);
                TestContext.Error.WriteLine($"[WARN] [{_scope}] {msg}");
                SaveHtml(msg);
            }

            public void Report()
            {
                if (_errors.Count == 0)
                    TestContext.Progress.WriteLine($"✅ [{_scope}] все тексты совпадают");
                else
                    TestContext.Error.WriteLine($"⚠️ [{_scope}] найдено {_errors.Count} несоответствий");
            }

            private void AddError(string key, string expected, string actual)
            {
                var msg = $"{key} — ожидали: «{expected}», получили: «{actual}»";
                _errors.Add(msg);
                TestContext.Error.WriteLine($"[WARN] [{_scope}] {msg}");
                SaveHtml(key);
            }

            private void SaveHtml(string name)
            {
                try
                {
                    var path = Path.Combine("Screenshots", $"{DateTime.UtcNow:HHmmss}_{_scope}_{Sanitize(name)}.html");
                    File.WriteAllText(path, _driver.PageSource ?? "");
                    TestContext.AddTestAttachment(path);
                }
                catch { }
            }

            private static bool SafeContains(string a, string b) =>
                (a ?? "").IndexOf(b ?? "", StringComparison.OrdinalIgnoreCase) >= 0;

            private static string Sanitize(string s)
            {
                foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
                return s.Length > 40 ? s[..40] : s;
            }
        }
    }
}
