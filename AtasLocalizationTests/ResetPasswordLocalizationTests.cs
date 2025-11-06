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
    public class ResetPasswordLocalizationTests
    {
        private ChromeDriver _driver;
        private WebDriverWait _wait;

        private const string RootUrl = "https://atas.bambus.com.ua/";
        private const string BasicAuthUser = "bambuk";
        private const string BasicAuthPass = "Atastraders$";

        // Отображаемое имя в меню -> наш код локали
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

        // WPML cookie (на всякий, если сайт ориентируется на неё)
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

        // I18n таблица для попапа "Восстановить пароль"
        private static readonly Dictionary<string, Dictionary<string, string>> I18n = new()
        {
            ["ResetPassword"] = new()
            {
                ["RU.Title"] = "Восстановить пароль",
                ["EN.Title"] = "Reset your password",
                ["DE.Title"] = "Passwort zurücksetzen",
                ["ES.Title"] = "Restablecer contraseña",
                ["FR.Title"] = "Réinitialiser le mot de passe",
                ["IT.Title"] = "Reimposta la password",
                ["UA.Title"] = "Відновити пароль",
                ["CN.Title"] = "重置密码",

                ["RU.Desc"] = "Введи адрес электронной почты, и мы отправим ссылку для создания нового пароля.",
                ["EN.Desc"] = "Enter your email address and we’ll send you a link to create a new password.",
                ["DE.Desc"] = "Gib deine E-Mail-Adresse ein, und wir schicken dir einen Link, um ein neues Passwort zu erstellen.",
                ["ES.Desc"] = "Introduce tu correo electrónico y te enviaremos un enlace para crear una nueva contraseña.",
                ["FR.Desc"] = "Saisis ton adresse e-mail et nous t’enverrons un lien pour créer un nouveau mot de passe.",
                ["IT.Desc"] = "Inserisci il tuo indirizzo e-mail e ti invieremo un link pour creare una nuova password.",
                ["UA.Desc"] = "Введи адресу електронної пошти, і ми надішлемо посилання для створення нового пароля.",
                ["CN.Desc"] = "输入您的邮箱地址，我们会发送一个链接，让您创建新密码。",

                ["RU.EmailLabel"] = "Email",
                ["EN.EmailLabel"] = "Email",
                ["DE.EmailLabel"] = "E-Mail-Adresse",
                ["ES.EmailLabel"] = "Correo electrónico",
                ["FR.EmailLabel"] = "Email",
                ["IT.EmailLabel"] = "Email",
                ["UA.EmailLabel"] = "Електронна пошта",
                ["CN.EmailLabel"] = "邮箱",

                ["RU.EmailPh"] = "Email (на него придет пароль от ATAS)",
                ["EN.EmailPh"] = "Enter your email",
                ["DE.EmailPh"] = "Deine E-Mail-Adresse (der ATAS-Link wird an diese Adresse gesendet)",
                ["ES.EmailPh"] = "Introduce tu correo electrónico",
                ["FR.EmailPh"] = "Ton adresse e-mail (le lien ATAS y sera envoyé)",
                ["IT.EmailPh"] = "La tua e-mail (il link di ATAS arriverà qui)",
                ["UA.EmailPh"] = "Твоя електронна пошта (сюди прийде лист від ATAS)",
                ["CN.EmailPh"] = "您的邮箱地址（ATAS 链接将发送到此邮箱）",

                ["RU.Btn"] = "Сбросить пароль",
                ["EN.Btn"] = "Send reset link",
                ["DE.Btn"] = "Passwort zurücksetzen",
                ["ES.Btn"] = "Restablecer contraseña",
                ["FR.Btn"] = "Réinitialiser le mot de passe",
                ["IT.Btn"] = "Reimposta password",
                ["UA.Btn"] = "Відновити пароль",
                ["CN.Btn"] = "重置密码",

                // Успешное состояние (если будете проверять после submit)
                ["RU.SuccessTitle"] = "Ссылка для сброса пароля отправлена!",
                ["EN.SuccessTitle"] = "Password reset link sent!",
                ["DE.SuccessTitle"] = "Link zum Zurücksetzen des Passworts gesendet!",
                ["ES.SuccessTitle"] = "¡Enlace para restablecer la contraseña enviado!",
                ["FR.SuccessTitle"] = "Lien de réinitialisation du mot de passe envoyé !",
                ["IT.SuccessTitle"] = "Link per reimpostare la password inviato!",
                ["UA.SuccessTitle"] = "Посилання для відновлення пароля надіслано!",
                ["CN.SuccessTitle"] = "我们已发送密码重置链接！",

                ["RU.SuccessDesc"] = "Проверь почту и следуй инструкциям, чтобы задать новый пароль.",
                ["EN.SuccessDesc"] = "Check your inbox and follow the instructions to create a new password.",
                ["DE.SuccessDesc"] = "Überprüfe dein Postfach und folge den Anweisungen, um ein neues Passwort zu erstellen.",
                ["ES.SuccessDesc"] = "Revisa tu bandeja de entrada y sigue las instrucciones para crear una nueva contraseña.",
                ["FR.SuccessDesc"] = "Vérifie ta boîte mail et suis les instructions pour créer un nouveau mot de passe.",
                ["IT.SuccessDesc"] = "Controlla la tua casella di posta e segui le istruzioni per creare una nuova password.",
                ["UA.SuccessDesc"] = "Перевір пошту та дотримуйся інструкцій, щоб створити новий пароль.",
                ["CN.SuccessDesc"] = "请查看邮箱并按照说明创建新密码。",

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

        // ---------- ТЕСТ: один прогон — одна локаль ----------
        [Test]
        [TestCaseSource(nameof(AllLocales))]
        public void ResetPassword_Popup_One_Locale(string locale)
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

            Step("Открыть попап восстановления пароля", () =>
            {
                // 1) Открываем Sign In
                var signIn = _wait.Until(d => d.FindElement(By.CssSelector("a.header-log-in[href='#form-signin']")));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", signIn);
                _wait.Until(d => d.FindElement(By.XPath("//form[@action='/v2/account/signIn']")));
                WaitPopupVisibleRoot();

                // 2) Кликаем Forgot your password?
                var forgot = _wait.Until(d => d.FindElement(By.CssSelector("form[action='/v2/account/signIn'] button.forgot")));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", forgot);

                // 3) Ждём попап Reset password (форма reset)
                _wait.Until(d => d.FindElement(By.CssSelector("form[action='/v2/account/password/reset']")));
                WaitPopupVisibleRoot();
            });

            Step($"Проверить тексты Reset Password ({locale})", () =>
            {
                var soft = new SoftVerify($"ResetPassword-{locale}");
                VerifyResetPasswordTranslations_AllSoft(locale, soft);
                soft.ThrowIfAny(); // соберёт все несоответствия, упадёт в конце
            });
        }

        private static IEnumerable<string> AllLocales()
            => new[] { "EN", "RU", "FR", "DE", "IT", "ES", "UA", "CN" };

        // ---------- Проверка попапа (все проверки «мягкие») ----------
        private void VerifyResetPasswordTranslations_AllSoft(string locale, SoftVerify soft)
        {
            IWebElement root = null;
            try
            {
                root = _wait.Until(d => d.FindElement(
                    By.XPath("//div[contains(@class,'popup-content')]//form[@action='/v2/account/password/reset']/ancestor::div[contains(@class,'popup-content')]")));
                WaitPopupVisibleRoot();
            }
            catch
            {
                soft.Contains("PopupRoot", "FOUND", "NOT FOUND");
            }

            string T(By by, int sec = 15) => root == null ? "" : WaitNonEmptyText(root, by, TimeSpan.FromSeconds(sec));
            string A(By by, string attr) => root == null ? "" : SafeAttr(root, by, attr);

            // Title
            var title = T(By.CssSelector(".neue-40-bold"));
            soft.Contains("Title", I18n["ResetPassword"][$"{locale}.Title"], title);

            // Description
            var desc = T(By.CssSelector("p.description"));
            soft.Contains("Description", I18n["ResetPassword"][$"{locale}.Desc"], desc);

            // Email label
            var emailLabel = T(By.XPath(".//label//span[contains(@class,'placeholder')]"));
            // допускаем "E-mail" vs "Email"
            var expectEmail = I18n["ResetPassword"][$"{locale}.EmailLabel"];
            if (!(SoftEquals(expectEmail, emailLabel) || SoftEquals(expectEmail.Insert(1, "-"), emailLabel)))
                soft.Contains("EmailLabel", expectEmail, emailLabel);

            // Email placeholder
            var emailPh = A(By.CssSelector("form[action='/v2/account/password/reset'] input[type='email']"), "placeholder");
            soft.Equal("EmailPh", I18n["ResetPassword"][$"{locale}.EmailPh"], emailPh);

            // Submit button text
            var btnText = T(By.CssSelector("form[action='/v2/account/password/reset'] .btn-text"));
            soft.Contains("Button", I18n["ResetPassword"][$"{locale}.Btn"], btnText);

            // (опционально) Успешное состояние — если будете жать кнопку и ждать сообщения:
            // var successTitle = T(By.CssSelector("[data-msgs] .success .title"));
            // soft.Contains("SuccessTitle", I18n["ResetPassword"][$"{locale}.SuccessTitle"], successTitle);
            // var successDesc = T(By.CssSelector("[data-msgs] .success .subtitle"));
            // soft.Contains("SuccessDesc", I18n["ResetPassword"][$"{locale}.SuccessDesc"], successDesc);
            // var closeText = T(By.CssSelector("[data-msgs] .success .btn-text"));
            // soft.Contains("Close", I18n["ResetPassword"][$"{locale}.Close"], closeText);
        }

        // ---------- Работа с языковым меню ----------
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

        // ---------- «Умные» ожидания ----------
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
            catch { return ""; } // не роняем тест, вернём пусто — soft-assert зафиксирует
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

        // ---------- Служебное ----------
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
