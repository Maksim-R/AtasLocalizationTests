using System;
using System.Collections.Generic;
using OpenQA.Selenium;

namespace AtasLocalizationTests
{
    /// <summary>
    /// Page Object попапа авторизации (signin)
    /// </summary>
    public sealed class SigninPopupPage
    {
        private readonly SeleniumKit _kit;

        public SigninPopupPage(SeleniumKit kit)
        {
            _kit = kit;
        }

        // --- Локаторы (XPath) ---
        public readonly Dictionary<string, string> X = new()
        {
            ["trigger"] = "//a[contains(@class,'header-log-in')]",
            ["title"] = Locators.Signin.Title,
            ["email_input"] = Locators.Signin.EmailInput,
            ["email_placeholder"] = Locators.Signin.EmailInput + "/@placeholder",
            ["password_label"] = Locators.Signin.PasswordLabel,
            ["password_input"] = Locators.Signin.PasswordInput,
            ["password_placeholder"] = Locators.Signin.PasswordInput + "/@placeholder",
            ["forgot_btn"] = Locators.Signin.ForgotButton,
            ["submit_btn"] = Locators.Signin.SubmitButton,
            ["submit_text"] = Locators.Signin.SubmitText,
            ["bottom_text"] = Locators.Signin.BottomText,
            ["bottom_cta"] = Locators.Signin.BottomCtaButton,
            ["error_label"] = Locators.Signin.ErrorLabel,
            ["close_btn"] = Locators.Signin.CloseButton
        };

        // --- Методы ---

        /// <summary>Открывает попап авторизации.</summary>
        public void Open()
        {
            // 1) честная попытка: клик по кнопке "Войти" в шапке
            try
            {
                _kit.ClickByCss("a.header-log-in[href='#form-signin']");
                _kit.WaitPopupVisible("signin", 5);
                return;
            }
            catch { }

            // 2) альтернативные триггеры
            try
            {
                _kit.ClickByCss("[data-open-popup='signin']");
                _kit.WaitPopupVisible("signin", 5);
                return;
            }
            catch { }

            // 3) форс-показ
            _kit.ForceOpenPopup("signin");
            _kit.WaitPopupVisible("signin", 10);
        }

        /// <summary>Закрывает попап авторизации.</summary>
        public void Close()
        {
            try
            {
                var closeBtn = _kit.FindByXPath(X["close_btn"]);
                if (closeBtn is not null)
                {
                    _kit.JsClick(closeBtn);
                    _kit.Sleep(1000);
                }
            }
            catch { }
        }

        /// <summary>Очищает поля формы.</summary>
        public void ClearForm()
        {
            var emailInput = _kit.FindByXPath(X["email_input"]);
            var passInput = _kit.FindByXPath(X["password_input"]);

            if (emailInput is not null)
            {
                emailInput.Clear();
                // Дополнительно очищаем через JavaScript на случай если Clear() не работает
                _kit.JsSetValue(emailInput, "");
            }

            if (passInput is not null)
            {
                passInput.Clear();
                _kit.JsSetValue(passInput, "");
            }

            _kit.Sleep(500);
        }

        /// <summary>Читает тексты интерфейса формы авторизации.</summary>
        public (string title, string emailPh, string passwordLabel, string passwordPh,
                string forgotText, string submitText, string bottomText, string bottomCta)
            ReadTexts()
        {
            var passwordLabel = _kit.GetTextByXPath(X["password_label"]);

            if (passwordLabel.StartsWith("__NOT_FOUND__"))
            {
                var altPasswordLabel = "//div[@data-popup-name='signin']//label[.//input[@type='password']]//span[contains(@class,'placeholder')]";
                passwordLabel = _kit.GetTextByXPath(altPasswordLabel);
            }

            return (
                _kit.GetTextByXPath(X["title"]),
                _kit.GetAttrByXPath(X["email_input"], "placeholder"),
                passwordLabel,
                _kit.GetAttrByXPath(X["password_input"], "placeholder"),
                _kit.GetTextByXPath(X["forgot_btn"]),
                _kit.GetTextByXPath(X["submit_text"]),
                _kit.GetTextByXPath(X["bottom_text"]),
                _kit.GetTextByXPath(X["bottom_cta"])
            );
        }

        /// <summary>Проверяет перевод с эталоном.</summary>
        public void ValidateTexts(string locale, List<string> errors)
        {
            var (title, emailPh, passLabel, passPh, forgot, submit, bottomText, bottomCta) = ReadTexts();

            Expect(locale, "заголовок формы входа", "signin_title", title, errors);
            Expect(locale, "плейсхолдер email", "signin_email_ph", emailPh, errors);
            Expect(locale, "метка пароля", "signin_pass_label", passLabel, errors);
            Expect(locale, "плейсхолдер пароля", "signin_pass_ph", passPh, errors);
            Expect(locale, "кнопка 'Забыли пароль'", "signin_forgot", forgot, errors);
            Expect(locale, "текст кнопки входа", "signin_submit", submit, errors);
            Expect(locale, "текст внизу формы", "signin_bottom_text", bottomText, errors);
            Expect(locale, "кнопка внизу формы", "signin_bottom_cta", bottomCta, errors);
        }

        private static void Expect(string locale, string fieldName, string key, string actual, List<string> errors)
        {
            var expected = Translations.Exp.TryGetValue((locale, key), out var v) ? v : "";
            if (actual.StartsWith("__NOT_FOUND__"))
            {
                errors.Add($"[{locale}] Поле '{fieldName}': элемент не найден");
                return;
            }
            if (!SeleniumKit.ContainsNormalized(actual, expected))
            {
                errors.Add($"[{locale}] Поле '{fieldName}': ожидали '{expected}', получили '{actual}'");
            }
        }

        /// <summary>Авторизация с указанными данными.</summary>
        public void Login(string email, string password)
        {
            // Очищаем форму перед вводом новых данных
            ClearForm();

            var emailInput = _kit.FindByXPath(X["email_input"]);
            var passInput = _kit.FindByXPath(X["password_input"]);

            if (emailInput is null)
                throw new Exception("Не найдено поле email");
            if (passInput is null)
                throw new Exception("Не найдено поле пароля");

            // Убеждаемся, что поля доступны для ввода
            if (!emailInput.Enabled || !passInput.Enabled)
                throw new Exception("Поля формы недоступны для ввода");

            // Вводим email - более надежный способ
            emailInput.Click();
            _kit.Sleep(200);
            emailInput.Clear();
            _kit.Sleep(200);
            emailInput.SendKeys(email);
            _kit.Sleep(300);

            // Вводим пароль
            passInput.Click();
            _kit.Sleep(200);
            passInput.Clear();
            _kit.Sleep(200);
            passInput.SendKeys(password);
            _kit.Sleep(300);

            // Нажимаем кнопку входа
            var btn = _kit.FindByXPath(X["submit_btn"]);
            if (btn is null)
                throw new Exception("Не найдена кнопка отправки формы");

            // Проверяем, что кнопка доступна
            if (!btn.Enabled)
                throw new Exception("Кнопка входа недоступна");

            // Кликаем и ждем
            _kit.JsClick(btn);
            _kit.Sleep(3000); // Увеличили время ожидания обработки
        }

        /// <summary>Проверяет, открыта ли форма авторизации.</summary>
        public bool IsFormOpen()
        {
            return _kit.IsPopupActive("signin");
        }

        /// <summary>Переоткрывает форму авторизации.</summary>
        public void ReopenForm()
        {
            Close();
            _kit.Sleep(1000);
            Open();
            _kit.Sleep(1000);
        }

        /// <summary>Возвращает текст ошибки авторизации.</summary>
        public string GetErrorText()
        {
            _kit.Sleep(2000);
            var errorText = _kit.GetTextByXPath(X["error_label"]);

            if (errorText.StartsWith("__NOT_FOUND__"))
            {
                var altErrorLocator = "//div[@data-popup-name='signin']//div[contains(@class,'error') and @data-error]";
                errorText = _kit.GetTextByXPath(altErrorLocator);
            }

            return errorText;
        }

        /// <summary>Проверяет, видна ли ошибка авторизации.</summary>
        public bool IsErrorVisible()
        {
            var errorElement = _kit.FindByXPath(X["error_label"]);
            if (errorElement is null)
            {
                errorElement = _kit.FindByXPath("//div[@data-popup-name='signin']//div[contains(@class,'error') and @data-error]");
            }

            if (errorElement is null) return false;

            try
            {
                return errorElement.Displayed && !string.IsNullOrEmpty(errorElement.Text?.Trim());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Проверяет состояние формы авторизации.</summary>
        public (bool emailEnabled, bool passwordEnabled, bool submitEnabled) GetFormState()
        {
            var emailInput = _kit.FindByXPath(X["email_input"]);
            var passInput = _kit.FindByXPath(X["password_input"]);
            var submitBtn = _kit.FindByXPath(X["submit_btn"]);

            return (
                emailEnabled: emailInput?.Enabled ?? false,
                passwordEnabled: passInput?.Enabled ?? false,
                submitEnabled: submitBtn?.Enabled ?? false
            );
        }
    }
}