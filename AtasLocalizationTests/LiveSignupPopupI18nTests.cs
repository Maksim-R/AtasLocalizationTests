using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;

namespace AtasLocalizationTests
{
    /// <summary>
    /// Тест локализации signup + signin по всем локалям.
    /// </summary>
    [TestFixture]
    public class LiveSignupPopupI18nTests
    {
        private SeleniumKit _kit = default!;
        private SignupPopupPage _signup = default!;
        private SigninPopupPage _signin = default!;

        private const string RootUrl = "https://atas.bambus.com.ua/";
        private const string BasicAuthUser = "bambuk";
        private const string BasicAuthPass = "Atastraders$";

        private static readonly string[] Locales = { "EN", "RU", "FR", "DE", "IT", "ES", "UA", "CN" };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _kit = new SeleniumKit();
            _signup = new SignupPopupPage(_kit);
            _signin = new SigninPopupPage(_kit);

            // BasicAuth через CDP
            try
            {
                var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{BasicAuthUser}:{BasicAuthPass}"));
                var driver = (ChromeDriver)_kit.Driver;
                driver.ExecuteCdpCommand("Network.enable", new Dictionary<string, object>());
                driver.ExecuteCdpCommand("Network.setExtraHTTPHeaders", new Dictionary<string, object>
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
            _kit?.Dispose();
        }

        // ---------- SIGNUP ----------

        [Test]
        [TestCaseSource(nameof(Locales))]
        public void Check_Signup_On_Locale(string locale)
        {
            var errors = new List<string>();

            // 1) Открыть сайт и выбрать локаль
            _kit.GoTo(RootUrl);
            try { _kit.SelectLocale(locale); }
            catch (Exception ex) { errors.Add($"[{locale}] Выбор локали: {ex.Message}"); }

            if (!_kit.VerifyLocaleByUrlAndLang(locale, out var explain))
                errors.Add($"[{locale}] Локаль не подтверждена: {explain}");

            // 2) Открыть попап регистрации
            try { _signup.Open(); }
            catch (Exception ex) { errors.Add($"[{locale}] Открытие signup: {ex.Message}"); }

            // 3) Проверка статичных текстов signup
            var (title, emailLabel, emailPh, marketing, agreeLabel, submitText, bottomText, bottomCta)
                = _signup.ReadSignupTexts();

            ExpectContains(errors, locale, "title", title);
            ExpectContains(errors, locale, "email_label", emailLabel);
            ExpectContains(errors, locale, "email_ph", emailPh);
            ExpectContains(errors, locale, "marketing", marketing);
            ExpectContains(errors, locale, "agree", agreeLabel);
            ExpectContains(errors, locale, "submit_text", submitText);
            ExpectContains(errors, locale, "bottom_text", bottomText);
            ExpectContains(errors, locale, "bottom_cta", bottomCta);

            // 4) Заполнить и отправить форму
            try
            {
                var rnd = new Random().Next(10, 1001);
                _signup.FillAndSubmit($"test{rnd}@test.net");
            }
            catch (Exception ex)
            {
                _kit.TakeScreenshot(locale, "submit_error");
                errors.Add($"[{locale}] Сабмит формы: {ex.Message}");
            }

            // 5) Ждать и проверить success
            try
            {
                _signup.WaitAndValidateSuccess(locale, errors, 45);
                _signup.ClickCloseButton();
            }
            catch (Exception ex)
            {
                _kit.TakeScreenshot(locale, "success_wait_fail");
                errors.Add($"[{locale}] Ожидание/валидация success: {ex.Message}");
            }

            // 6) Отчёт
            if (errors.Count > 0)
            {
                TestContext.WriteLine("⚠️ Несоответствия:");
                foreach (var e in errors) TestContext.WriteLine(" - " + e);
                Assert.Fail($"{errors.Count} несоответств(ия/ий) для локали {locale}.");
            }
            else
            {
                TestContext.WriteLine($"✅ [{locale}] SIGNUP OK");
            }
        }

        // ---------- SIGNIN ----------

        [Test]
        [TestCaseSource(nameof(Locales))]
        public void Check_Signin_On_Locale(string locale)
        {
            var errors = new List<string>();

            _kit.GoTo(RootUrl);
            try { _kit.SelectLocale(locale); }
            catch (Exception ex) { errors.Add($"[{locale}] Ошибка выбора локали: {ex.Message}"); }

            if (!_kit.VerifyLocaleByUrlAndLang(locale, out var explain))
                errors.Add($"[{locale}] Локаль не подтверждена: {explain}");

            // 1) Открываем форму авторизации
            try
            {
                _signin.Open();
                _kit.TakeScreenshot(locale, "form_opened");
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Ошибка открытия формы входа: {ex.Message}");
                return; // Не продолжаем если форму не удалось открыть
            }

            // 2) Проверяем тексты
            _signin.ValidateTexts(locale, errors);

            // 3) Невалидный вход
            try
            {
                TestContext.WriteLine($"[{locale}] Выполняем невалидный вход...");
                _signin.Login("wrong" + Guid.NewGuid().ToString("N")[..5] + "@test.net", "wrongpass");
                _kit.TakeScreenshot(locale, "after_invalid_login");

                // Проверяем ошибку
                if (!_signin.IsErrorVisible())
                {
                    errors.Add($"[{locale}] Ошибка авторизации не отображается после невалидного входа");
                }
                else
                {
                    var err = _signin.GetErrorText();
                    var expError = Translations.Exp.TryGetValue((locale, "signin_error"), out var v) ? v : "";

                    if (string.IsNullOrEmpty(err) || err.StartsWith("__NOT_FOUND__"))
                    {
                        errors.Add($"[{locale}] Текст ошибки авторизации не найден");
                    }
                    else if (!SeleniumKit.ContainsNormalized(err, expError))
                    {
                        errors.Add($"[{locale}] Текст ошибки авторизации: ожидали '{expError}', получили '{err}'");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Ошибка при невалидном входе: {ex.Message}");
                _kit.TakeScreenshot(locale, "invalid_login_error");
            }

            // 4) Подготавливаем форму для валидного входа - УПРОЩЕННАЯ ЛОГИКА
            TestContext.WriteLine($"[{locale}] Подготавливаем форму для валидного входа...");

            // Просто закрываем и заново открываем форму, чтобы гарантированно очистить все поля
            try
            {
                _signin.Close();
                _kit.Sleep(2000);
                _signin.Open();
                _kit.Sleep(2000);
                _kit.TakeScreenshot(locale, "form_prepared_for_valid_login");
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Ошибка подготовки формы для валидного входа: {ex.Message}");
                _kit.TakeScreenshot(locale, "form_preparation_error");
            }

            // 5) Валидный вход
            try
            {
                TestContext.WriteLine($"[{locale}] Выполняем валидный вход...");

                // Убеждаемся, что форма открыта
                if (!_signin.IsFormOpen())
                {
                    errors.Add($"[{locale}] Форма авторизации не открыта перед валидным входом");
                }
                else
                {
                    // Выполняем вход с валидными данными
                    _signin.Login("test560@test.net", "NjEPWB!1w");
                    _kit.TakeScreenshot(locale, "after_valid_login");

                    // Ждем и проверяем результат
                    _kit.Sleep(3000);

                    // Проверяем, что форма закрылась (успешный вход)
                    if (_signin.IsFormOpen())
                    {
                        // Если форма осталась открытой, проверяем есть ли ошибка
                        if (_signin.IsErrorVisible())
                        {
                            var errorText = _signin.GetErrorText();
                            errors.Add($"[{locale}] Ошибка при валидном входе: {errorText}");
                        }
                        else
                        {
                            errors.Add($"[{locale}] Форма не закрылась после валидного входа (возможно неверные данные)");
                        }
                    }
                    else
                    {
                        TestContext.WriteLine($"[{locale}] Форма успешно закрылась - вход выполнен");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"[{locale}] Ошибка при валидном входе: {ex.Message}");
                _kit.TakeScreenshot(locale, "valid_login_error");
            }

            // Форматируем вывод ошибок
            if (errors.Count > 0)
            {
                var errorMessage = string.Join(Environment.NewLine, errors);
                Assert.Fail($"Найдено {errors.Count} несоответствий для локали {locale}:{Environment.NewLine}{errorMessage}");
            }
            else
            {
                TestContext.WriteLine($"✅ [{locale}] SIGNIN OK");
            }
        }

        // ------- локальные проверки текста -------
        private static void ExpectContains(List<string> errors, string locale, string key, string actual)
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
            }
        }
    }
}