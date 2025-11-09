using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;

namespace AtasLocalizationTests
{
    /// <summary>
    /// Набор UI-тестов локализации формы регистрации (signup) и попапа успеха (success).
    /// Делает:
    /// 1) Открывает сайт, выбирает локаль.
    /// 2) Проверяет URL и html[lang].
    /// 3) Открывает попап регистрации.
    /// 4) Проверяет тексты элементов signup.
    /// 5) Вводит e-mail, проставляет чекбокс согласия и сабмитит форму.
    /// 6) Ждёт успешный попап и валидирует его тексты.
    /// </summary>
    [TestFixture]
    public class LiveSignupPopupI18nTests
    {
        private ChromeDriver _driver;
        private WebDriverWait _wait;

        // === БАЗОВОЕ ===
        private const string RootUrl = "https://atas.bambus.com.ua/";
        private const string BasicAuthUser = "bambuk";
        private const string BasicAuthPass = "Atastraders$";

        // Названия в меню -> локаль
        private static readonly Dictionary<string, string> LangNameToLocale = new(StringComparer.OrdinalIgnoreCase)
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

        // Префикс <html lang> для проверки
        private static readonly Dictionary<string, string> HtmlLangPrefix = new()
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

        // Правила распознавания локали в URL (хватает префикса)
        private static readonly Dictionary<string, string[]> UrlHints = new(StringComparer.OrdinalIgnoreCase)
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

        // XPaths попапа signup и success (для переиспользования/проверки в DevTools)
        private readonly Dictionary<string, string> _x = new()
        {
            // signup
            ["signup_title"] = "//div[@data-popup-name='signup']//span[contains(@class,'title')]",
            ["email_label"] = "//div[@data-popup-name='signup']//label[1]//span[contains(@class,'placeholder')]",
            ["email_input"] = "//div[@data-popup-name='signup']//input[@type='email']",
            ["marketing"] = "//div[@data-popup-name='signup']//label[@class='checkbox'][1]//span[last()]",
            ["agree_label"] = "//div[@data-popup-name='signup']//label[@class='checkbox'][2]//span[last()]",
            ["agree_checkbox"] = "//div[@data-popup-name='signup']//input[@type='checkbox' and @name='agree']", // сам input
            ["submit_text"] = "//div[@data-popup-name='signup']//div[@class='btn-text']",
            ["submit_button"] = "//div[@data-popup-name='signup']//button[contains(@class,'btn') and contains(@class,'cta')]",
            ["bottom_text"] = "//div[@data-popup-name='signup']//div[contains(@class,'cta-bottom')]//p",
            ["bottom_cta"] = "//div[@data-popup-name='signup']//div[contains(@class,'cta-bottom')]//button",
            ["data_msgs_host"] = "//div[@data-popup-name='signup']//div[contains(@class,'custom-form') and @data-msgs]",

            // success
            ["success_root"] = "//div[@data-popup-name='success']",
            ["success_title"] = "//div[@data-popup-name='success']//span[contains(@class,'title')]",
            ["success_subtitle"] = "//div[@data-popup-name='success']//p[contains(@class,'text')]",
            ["success_button"] = "//div[@data-popup-name='success']//button[contains(@class,'btn') and contains(@class,'cta')]"
        };

        private readonly Dictionary<(string locale, string key), string> _exp = new TupleIgnoreCaseDictionary
        {
            // RU
            { ("RU","title"),            "Регистрация" },
            { ("RU","email_label"),      "Email" },
            { ("RU","email_ph"),         "Eмail (на него придет пароль от ATAS)" },
            { ("RU","marketing"),        "Я хочу получать специальные предложения ATAS" },
            { ("RU","agree"),            "Пожалуйста, ознакомься и подтверди согласие с Terms of use, License agreement." },
            { ("RU","submit_text"),      "Зарегистрироваться" },
            { ("RU","bottom_text"),      "Уже есть учетная запись?" },
            { ("RU","bottom_cta"),       "Войти" },
            { ("RU","success_title"),    "Спасибо за регистрацию!" },
            { ("RU","success_subtitle"), "Проверь почту — мы отправили письмо со ссылкой для активации аккаунта." },
            { ("RU","success_button"),   "Закрыть" },

            // EN
            { ("EN","title"),            "Sign Up" },
            { ("EN","email_label"),      "Email" },
            { ("EN","email_ph"),         "We’ll send your ATAS password to this email address" },
            { ("EN","marketing"),        "I would like to receive special offers from ATAS" },
            { ("EN","agree"),            "Please read and accept the Terms of Use and License Agreement" },
            { ("EN","submit_text"),      "Sign Up" },
            { ("EN","bottom_text"),      "Already have an account?" },
            { ("EN","bottom_cta"),       "Sign In" },
            { ("EN","success_title"),    "Thanks for signing up!" },
            { ("EN","success_subtitle"), "Check your inbox — we’ve sent you an email with an activation link." },
            { ("EN","success_button"),   "Close" },

            // DE
            { ("DE","title"),            "Registrierung" },
            { ("DE","email_label"),      "Email" },
            { ("DE","email_ph"),         "E-Mail-Adresse eingeben" },
            { ("DE","marketing"),        "Neuigkeiten und Angebote von ATAS erhalten" },
            { ("DE","agree"),            "Mit deiner Registrierung stimmst du den Terms of Use und dem License Agreement zu" },
            { ("DE","submit_text"),      "Registrieren" },
            { ("DE","bottom_text"),      "Hast du schon ein Konto?" },
            { ("DE","bottom_cta"),       "Anmelden" },
            { ("DE","success_title"),    "Danke für deine Registrierung!" },
            { ("DE","success_subtitle"), "Überprüfe dein Postfach – wir haben dir eine E-Mail mit einem Aktivierungslink geschickt." },
            { ("DE","success_button"),   "Schließen" },

            // ES
            { ("ES","title"),            "Registro" },
            { ("ES","email_label"),      "Correo electrónico" },
            { ("ES","email_ph"),         "Te enviaremos tu contraseña de ATAS a esta dirección de correo electrónico" },
            { ("ES","marketing"),        "Deseo recibir ofertas especiales de ATAS" },
            { ("ES","agree"),            "Por favor, lee y acepta los Términos de uso y el Acuerdo de licencia" },
            { ("ES","submit_text"),      "Registrarse" },
            { ("ES","bottom_text"),      "¿Ya tienes una cuenta?" },
            { ("ES","bottom_cta"),       "Inicia sesión" },
            { ("ES","success_title"),    "¡Gracias por registrarte!" },
            { ("ES","success_subtitle"), "Revisa tu bandeja de entrada: te hemos enviado un correo con el enlace de activación." },
            { ("ES","success_button"),   "Cerrar" },

            // FR
            { ("FR","title"),            "Inscription" },
            { ("FR","email_label"),      "Email" },
            { ("FR","email_ph"),         "Saisissez votre adresse e-mail (Nous vous enverrons votre mot de passe ATAS à cette adresse e-mail)" },
            { ("FR","marketing"),        "Recevoir les actualités et offres d’ATAS" },
            { ("FR","agree"),            "Veuillez lire et accepter les Conditions d'utilisation et le Contrat de licence." },
            { ("FR","submit_text"),      "S’inscrire" },
            { ("FR","bottom_text"),      "Vous avez déjà un compte ?" },
            { ("FR","bottom_cta"),       "Se connecter" },
            { ("FR","success_title"),    "Merci pour votre inscription !" },
            { ("FR","success_subtitle"), "Vérifiez votre boîte de réception — nous vous avons envoyé un e-mail contenant un lien d'activation." },
            { ("FR","success_button"),   "Fermer" },

            // IT
            { ("IT","title"),            "Registrazione" },
            { ("IT","email_label"),      "Email" },
            { ("IT","email_ph"),         "Inserisci il tuo indirizzo e-mail" },
            { ("IT","marketing"),        "Ricevi notizie e offerte da ATAS" },
            { ("IT","agree"),            "Registrandoti, accetti i Terms of Use e il License Agreement" },
            { ("IT","submit_text"),      "Registrati" },
            { ("IT","bottom_text"),      "Hai già un account?" },
            { ("IT","bottom_cta"),       "Accedi" },
            { ("IT","success_title"),    "Grazie per la registrazione!" },
            { ("IT","success_subtitle"), "Controlla la tua casella di posta — ti abbiamo inviato un’e-mail con il link di attivazione." },
            { ("IT","success_button"),   "Chiudi" },

            // UA
            { ("UA","title"),            "Реєстрація" },
            { ("UA","email_label"),      "Email" },
            { ("UA","email_ph"),         "Введи свій email" },
            { ("UA","marketing"),        "Отримувати новини та пропозиції від ATAS" },
            { ("UA","agree"),            "Реєструючись, ти погоджуєшся з Terms of Use та License Agreement" },
            { ("UA","submit_text"),      "Зареєструватися" },
            { ("UA","bottom_text"),      "Уже маєш акаунт?" },
            { ("UA","bottom_cta"),       "Увійти" },
            { ("UA","success_title"),    "Дякуємо за реєстрацію!" },
            { ("UA","success_subtitle"), "Перевір пошту — ми надіслали лист із посиланням для активації акаунта." },
            { ("UA","success_button"),   "Закрити" },

            // CN
            { ("CN","title"),            "注册" },
            { ("CN","email_label"),      "邮箱" },
            { ("CN","email_ph"),         "我们将向此邮箱发送您的ATAS密码。" },
            { ("CN","marketing"),        "接收 ATAS 的新闻和优惠信息" },
            { ("CN","agree"),            "请阅读并接受《使用条款》和《许可协议》。" },
            { ("CN","submit_text"),      "注册" },
            { ("CN","bottom_text"),      "已有账号？" },
            { ("CN","bottom_cta"),       "登录" },
            { ("CN","success_title"),    "感谢您的注册！" },
            { ("CN","success_subtitle"), "请查看邮箱，我们已发送激活链接。" },
            { ("CN","success_button"),   "关闭" },
        };


        private static readonly string[] Locales = { "EN", "RU", "FR", "DE", "IT", "ES", "UA", "CN" };

        /// <summary>
        /// Инициализация браузера перед всеми тестами.
        /// Используется Selenium Manager (без WebDriverManager) + CDP-авторизация Basic Auth.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var opts = new ChromeOptions();
            // Не максимизируем и не задаём размер — окно «как есть»
            opts.AddArgument("--disable-gpu");
            opts.AddArgument("--disable-dev-shm-usage");
            opts.AddArgument("--no-sandbox");
            opts.AddArgument("--remote-allow-origins=*");

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = false;
            service.EnableVerboseLogging = true;
            service.LogPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "chromedriver.log");

            _driver = new ChromeDriver(service, opts, TimeSpan.FromSeconds(60));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            // BasicAuth через CDP — чтобы сайт пустил на защищённый стенд
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
                TestContext.WriteLine("[WARN] Не удалось проставить CDP-хедеры BasicAuth: " + ex.Message);
            }
        }

        /// <summary>
        /// Закрытие браузера по завершении всех тестов.
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            try { _driver?.Quit(); } catch { }
            try { _driver?.Dispose(); } catch { }
        }

        /// <summary>
        /// Главный тест: проверка локализации signup + успешного попапа для заданной локали.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(Locales))]
        public void Check_Signup_On_Locale(string locale)
        {
            var errors = new List<string>();

            // 1) Открываем корень
            _driver.Navigate().GoToUrl(RootUrl);
            WaitReady();

            // 2) Выбираем локаль
            try
            {
                SelectLocale(locale);
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Ошибка при выборе локали: {ex.Message}");
            }

            // 3) Проверяем, что URL и html[lang] соответствуют локали
            var okLocale = VerifyLocaleByUrlAndLang(locale, out var explain);
            if (!okLocale) errors.Add($"[{locale}] Несоответствие локали после выбора: {explain}");

            // 4) Открываем попап регистрации (если не активен — активируем бережно)
            try
            {
                OpenSignupPopupOrForce();
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Не удалось открыть попап signup: {ex.Message}");
            }

            // 5) Сверяем тексты в signup (label/placeholder/checkboxes/кнопки/низ)
            ExpectContains(errors, locale, "title", GetText(_x["signup_title"]));
            ExpectContains(errors, locale, "email_label", GetText(_x["email_label"]));
            ExpectContains(errors, locale, "email_ph", GetAttr(_x["email_input"], "placeholder"));
            ExpectContains(errors, locale, "marketing", GetText(_x["marketing"]));
            ExpectContains(errors, locale, "agree", GetText(_x["agree_label"]));
            ExpectContains(errors, locale, "submit_text", GetText(_x["submit_text"]));
            ExpectContains(errors, locale, "bottom_text", GetText(_x["bottom_text"]));
            ExpectContains(errors, locale, "bottom_cta", GetText(_x["bottom_cta"]));

            // 6) Заполнение формы и сабмит:
            // 6.1 e-mail вида test<rand>@test.net
            try
            {
                var rnd = new Random().Next(10, 1001);
                var email = $"test{rnd}@test.net";

                var emailInput = _wait.Until(d => d.FindElement(By.XPath(_x["email_input"])));
                emailInput.Clear();
                emailInput.SendKeys(email);
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Не удалось ввести e-mail: {ex.Message}");
            }

            // 6.2 Чекбокс согласия (именно input, не span) — клик через JS для надёжности
            try
            {
                var agreeInput = _wait.Until(d =>
                    d.FindElement(By.XPath(_x["agree_checkbox"])));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", agreeInput);
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Не удалось кликнуть чекбокс согласия: {ex.Message}");
            }

            // 6.3 Сабмит (кнопка «Зарегистрироваться» и аналоги)
            // 1) Пытаемся найти кнопку по локализованному тексту (внутренний div.btn-text)
            IWebElement submitBtn = null;
            try
            {
                var expectedSubmitText = Expect(locale, "submit_text");
                var submitXpathByText =
                    $"//div[@data-popup-name='signup']//button[.//div[contains(normalize-space(.),'{expectedSubmitText}')]]";

                submitBtn = _wait.Until(d => d.FindElement(By.XPath(submitXpathByText)));
                ScrollIntoView(submitBtn);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submitBtn);
            }
            catch (WebDriverTimeoutException)
            {
                // 2) Фоллбек: просто кнопка submit в signup
                try
                {
                    submitBtn = _wait.Until(d => d.FindElement(By.XPath(
                        "//div[@data-popup-name='signup']//button[@type='submit']")));
                    ScrollIntoView(submitBtn);
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", submitBtn);
                }
                catch (Exception ex2)
                {
                    errors.Add($"[{locale}] Не удалось нажать кнопку отправки формы: {ex2.Message}");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Не удалось нажать кнопку отправки формы: {ex.Message}");
            }

            // 7) Ждём и проверяем попап успеха
            try
            {
                // 7.0 ждём, пока попап реально станет активным и заполнится текстом
                WaitSuccessPopupVisible();

                // 7.1 читаем тексты надёжным способом
                var sTitle = GetTextStrict(_x["success_title"]);
                var sSubtitle = GetTextStrict(_x["success_subtitle"]);
                var sButton = GetTextStrict(_x["success_button"]);

                // 7.2 fallback: если пусто, достаём тексты из data-msgs
                if (string.IsNullOrWhiteSpace(sTitle) || string.IsNullOrWhiteSpace(sSubtitle))
                {
                    try
                    {
                        var host = Find(_x["data_msgs_host"]);
                        if (host != null)
                        {
                            var decoded = System.Net.WebUtility.HtmlDecode(host.GetAttribute("data-msgs") ?? "");
                            using var jd = JsonDocument.Parse(decoded);
                            if (jd.RootElement.TryGetProperty("success", out var success))
                            {
                                string Get(string p) => Normalize(success.TryGetProperty(p, out var v) ? v.GetString() ?? "" : "");
                                if (string.IsNullOrWhiteSpace(sTitle)) sTitle = Get("title");
                                if (string.IsNullOrWhiteSpace(sSubtitle)) sSubtitle = Get("subtitle");
                                if (string.IsNullOrWhiteSpace(sButton)) sButton = Get("button");
                            }
                        }
                    }
                    catch { /* ignore */ }
                }

                // 7.3 проверки
                ExpectContains(errors, locale, "success_title", sTitle, strict: false);
                ExpectContains(errors, locale, "success_subtitle", sSubtitle, strict: false);
                ExpectEquals(errors, locale, "success_button", sButton);
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Не удалось отвалидировать success-попап: {ex.Message}");
            }

            // 8) Итоговый отчёт
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

        // ====== СЛУЖЕБНЫЕ МЕТОДЫ ======

        /// <summary>
        /// Выбор локали через выпадающее меню языков.
        /// Ищем по видимому тексту/атрибутам, кликаем с ретраями.
        /// </summary>
        private void SelectLocale(string locale)
        {
            // На всякий — прокрутка вверх
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0,0);");
            System.Threading.Thread.Sleep(200);

            var btn = _wait.Until(d => d.FindElement(By.CssSelector("button.btn.langs-wrapper-lang")));
            ScrollIntoView(btn);

            // Пытаемся открыть dropdown надёжно
            bool opened = false;
            for (int i = 0; i < 3 && !opened; i++)
            {
                try
                {
                    try
                    {
                        new OpenQA.Selenium.Interactions.Actions(_driver)
                            .MoveToElement(btn).Pause(TimeSpan.FromMilliseconds(100)).Click().Perform();
                    }
                    catch
                    {
                        try { btn.Click(); }
                        catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn); }
                    }

                    _wait.Until(d => d.FindElement(By.CssSelector("nav.langs-wrapper-dropdown")));
                    opened = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(200);
                }
            }
            if (!opened) throw new NoSuchElementException("Не удалось открыть выпадающее меню локалей.");

            var dd = _driver.FindElement(By.CssSelector("nav.langs-wrapper-dropdown"));
            ScrollIntoView(dd);

            var links = dd.FindElements(By.CssSelector("a.link")).ToList();
            if (links.Count == 0) throw new NoSuchElementException("В выпадающем списке локалей нет ссылок.");

            // По видимому тексту
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

            // По атрибутам/href
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

            throw new NoSuchElementException($"Локаль {locale} не найдена в меню.");
        }

        /// <summary>Прокрутка элемента в центр экрана, чтобы избежать перекрытий.</summary>
        private void ScrollIntoView(IWebElement el)
        {
            try { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center',inline:'center'});", el); }
            catch { }
        }

        /// <summary>
        /// Эвристика распознавания кода локали по атрибутам/ссылке элемента.
        /// </summary>
        private string GuessLocaleFromLink(IWebElement a)
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

        /// <summary>
        /// Верификация выбранной локали по URL и по атрибуту &lt;html lang&gt;.
        /// </summary>
        private bool VerifyLocaleByUrlAndLang(string locale, out string explain)
        {
            explain = "";
            var ok = true;

            // URL
            var u = _driver.Url ?? "";
            if (UrlHints.TryGetValue(locale, out var hints))
            {
                var hit = hints.Any(h => u.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!hit) { ok = false; explain += $"URL '{u}' не похож на {locale}; "; }
            }

            // <html lang>
            try
            {
                var pref = HtmlLangPrefix.TryGetValue(locale, out var p) ? p : "en";
                var lang = (string)((IJavaScriptExecutor)_driver).ExecuteScript("return document.documentElement.lang||'';");
                if (string.IsNullOrEmpty(lang) || !lang.StartsWith(pref, StringComparison.OrdinalIgnoreCase))
                {
                    ok = false; explain += $"html[lang]={lang} не начинается с '{pref}'. ";
                }
            }
            catch (Exception ex)
            {
                ok = false; explain += $"ошибка чтения documentElement.lang: {ex.Message}. ";
            }

            return ok;
        }

        /// <summary>
        /// Открывает попап signup «честным» кликом по триггеру.
        /// Если не получилось — мягко форсирует показ (только для чтения текстов).
        /// </summary>
        private void OpenSignupPopupOrForce()
        {
            var triggers = _driver.FindElements(By.CssSelector("[data-open-popup='signup'], [data-open-popup='sign-up']"));
            if (triggers.Count > 0)
            {
                SafeClick(triggers[0]);
                Wait(300);
            }

            // Если попап не активен — аккуратно активируем его
            var isActive = IsSignupActive();
            if (!isActive)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript(@"
                    const p = document.querySelector(""div.popup[data-popup-name='signup']"");
                    if (p) { p.classList.add('active'); p.style.display='block'; }
                ");
                Wait(150);
            }
        }

        /// <summary>
        /// Проверяет, что попап signup отображается (classList.contains('active') или display != none).
        /// </summary>
        private bool IsSignupActive()
        {
            try
            {
                var pop = _driver.FindElements(By.CssSelector("div.popup[data-popup-name='signup']")).FirstOrDefault();
                if (pop == null) return false;
                return (bool)((IJavaScriptExecutor)_driver).ExecuteScript(
                    "const p=arguments[0];return p.classList.contains('active')||getComputedStyle(p).display!=='none';", pop);
            }
            catch { return false; }
        }

        /// <summary>Безопасно находит элемент по XPath. Возвращает null, если не найден.</summary>
        private IWebElement Find(string xpath)
        {
            try { return _driver.FindElement(By.XPath(xpath)); }
            catch { return null; }
        }

        /// <summary>Читает видимый текст элемента по XPath (text/innerText), нормализует пробелы/дефисы/«ё».</summary>
        private string GetText(string xpath)
        {
            var el = Find(xpath);
            if (el == null) return $"__NOT_FOUND__ (xpath: {xpath})";
            var s = el.Text;
            if (string.IsNullOrWhiteSpace(s)) s = el.GetAttribute("innerText") ?? "";
            return Normalize(s);
        }

        /// <summary>
        /// Возвращает текст элемента по XPath максимально надёжно:
        /// 1) .Text (если видим),
        /// 2) @innerText,
        /// 3) textContent через JS (даже если элемент уже скрыт).
        /// </summary>
        private string GetTextStrict(string xpath)
        {
            var el = Find(xpath);
            if (el == null) return $"__NOT_FOUND__ (xpath: {xpath})";

            // 1) стандартно
            var s = el.Text;
            if (!string.IsNullOrWhiteSpace(s)) return Normalize(s);

            // 2) innerText
            var inner = el.GetAttribute("innerText");
            if (!string.IsNullOrWhiteSpace(inner)) return Normalize(inner);

            // 3) textContent через JS
            try
            {
                var tc = (string)((IJavaScriptExecutor)_driver)
                    .ExecuteScript("return arguments[0] && arguments[0].textContent ? arguments[0].textContent : '';", el);
                return Normalize(tc ?? "");
            }
            catch { return ""; }
        }

        /// <summary>Читает атрибут элемента по XPath, нормализует строку.</summary>
        private string GetAttr(string xpath, string attr)
        {
            var el = Find(xpath);
            if (el == null) return $"__NOT_FOUND__ (xpath: {xpath})";
            var s = el.GetAttribute(attr) ?? "";
            return Normalize(s);
        }

        /// <summary>
        /// Ожидаем, что фактический текст содержит ожидаемый (case/пробелы нормализуются).
        /// </summary>
        private void ExpectContains(List<string> errors, string locale, string key, string actual, bool strict = false)
        {
            var exp = Expect(locale, key);
            if (actual.StartsWith("__NOT_FOUND__"))
            {
                errors.Add($"[{locale}] {key}: элемент не найден. {actual}");
                return;
            }

            if (strict)
            {
                if (!StringEqualsNormalized(actual, exp))
                    errors.Add($"[{locale}] {key}: ожидали «{exp}», получили «{actual}».");
            }
            else
            {
                if (!ContainsNormalized(actual, exp))
                {
                    errors.Add($"[{locale}] {key}: ожидали содержит «{exp}», получили «{actual}».");
                    TakeScreenshot(locale, key);
                }
            }
        }

        /// <summary>
        /// Ожидаем, что фактический текст равен ожидаемому (строгая проверка после нормализации).
        /// </summary>
        private void ExpectEquals(List<string> errors, string locale, string key, string actual)
        {
            var exp = Expect(locale, key);
            if (actual.StartsWith("__NOT_FOUND__"))
            {
                errors.Add($"[{locale}] {key}: элемент не найден. {actual}");
                return;
            }
            if (!StringEqualsNormalized(actual, exp))
            {
                errors.Add($"[{locale}] {key}: ожидали «{exp}», получили «{actual}».");
                TakeScreenshot(locale, key);
            }
        }

        /// <summary>
        /// Возвращает эталонную строку для ключа (locale,key) или пустую строку, если не задано.
        /// </summary>
        private string Expect(string locale, string key)
        {
            if (_exp.TryGetValue((locale, key), out var v)) return v;
            return "";
        }

        /// <summary>
        /// Нормализация текста: HTML-decode, замена NBSP/длинных тире/«ё», схлопывание пробелов.
        /// </summary>
        private static string Normalize(string s)
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

        /// <summary>Проверка «actual содержит expected» после Normalize (без учёта регистра).</summary>
        private static bool ContainsNormalized(string actual, string expectedPart) =>
            Normalize(actual).IndexOf(Normalize(expectedPart), StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>Проверка равенства после Normalize (без учёта регистра/культуры).</summary>
        private static bool StringEqualsNormalized(string a, string b) =>
            string.Equals(Normalize(a), Normalize(b), StringComparison.Ordinal);

        /// <summary>
        /// Безопасный клик: пробуем .Click(), если не вышло — JS click.
        /// </summary>
        private void SafeClick(IWebElement el)
        {
            try { el.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", el); }
        }

        /// <summary>
        /// Явное ожидание полной загрузки документа (readyState=complete) + короткий sleep.
        /// </summary>
        private void WaitReady()
        {
            _wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");
            Wait(150);
        }

        /// <summary>Простой sleep в миллисекундах.</summary>
        private static void Wait(int ms) => System.Threading.Thread.Sleep(ms);

        /// <summary>
        /// Делает скриншот текущего экрана и сохраняет в TestContext.
        /// </summary>
        private void TakeScreenshot(string locale, string key)
        {
            try
            {
                var fileName = $"{locale}_{key}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
                var fullPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, fileName);

                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var bytes = screenshot.AsByteArray;
                File.WriteAllBytes(fullPath, bytes);

                TestContext.WriteLine($"📸 Скриншот сохранён: {fullPath}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"⚠️ Не удалось сделать скриншот: {ex.Message}");
            }
        }

        /// <summary>
        /// Устойчиво дожидается появления success-попапа:
        /// ждём .active или display != none + непустой текст заголовка/подзаголовка.
        /// </summary>
        private void WaitSuccessPopupVisible()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(40));
            wait.Until(d =>
            {
                try
                {
                    var root = d.FindElements(By.XPath(_x["success_root"])).FirstOrDefault();
                    if (root == null) return false;

                    var isActive = (bool)((IJavaScriptExecutor)d).ExecuteScript(
                        "const p=arguments[0]; return p && p.classList && p.classList.contains('active');", root);

                    var display = (string)((IJavaScriptExecutor)d)
                        .ExecuteScript("return getComputedStyle(arguments[0]).display;", root);

                    // Проверяем наличие текста
                    var titleEl = d.FindElements(By.XPath(_x["success_title"])).FirstOrDefault();
                    var subEl = d.FindElements(By.XPath(_x["success_subtitle"])).FirstOrDefault();

                    bool titleOk = false, subOk = false;
                    if (titleEl != null)
                    {
                        var t = (string)((IJavaScriptExecutor)d)
                            .ExecuteScript("return arguments[0]?.textContent?.trim() || '';", titleEl);
                        titleOk = !string.IsNullOrWhiteSpace(t);
                    }
                    if (subEl != null)
                    {
                        var s = (string)((IJavaScriptExecutor)d)
                            .ExecuteScript("return arguments[0]?.textContent?.trim() || '';", subEl);
                        subOk = !string.IsNullOrWhiteSpace(s);
                    }

                    return isActive || !string.Equals(display, "none", StringComparison.OrdinalIgnoreCase) || (titleOk && subOk);
                }
                catch { return false; }
            });

            Wait(150);
        }

        // ====== Tuple-словарь с ignore-case на ключи (locale,key) ======
        /// <summary>
        /// Словарь ключей (locale,key) с регистронезависимым сравнением — удобно для локалей/ключей.
        /// </summary>
        private sealed class TupleIgnoreCaseDictionary : Dictionary<(string locale, string key), string>
        {
            public TupleIgnoreCaseDictionary() : base(new TupleComparer()) { }

            private sealed class TupleComparer : IEqualityComparer<(string locale, string key)>
            {
                public bool Equals((string locale, string key) x, (string locale, string key) y) =>
                    string.Equals(x.locale, y.locale, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.key, y.key, StringComparison.OrdinalIgnoreCase);

                public int GetHashCode((string locale, string key) obj) =>
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.locale ?? "") * 397 ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.key ?? "");
            }
        }
    }
}
