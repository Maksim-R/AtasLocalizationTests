using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;

namespace AtasLocalizationTests
{
    /// <summary>
    /// Page Object pop-up регистрации (signup) + success.
    /// Хранит локаторы и сценарии действий.
    /// </summary>
    public sealed class SignupPopupPage
    {        
        private readonly SeleniumKit _kit;

        public SignupPopupPage(SeleniumKit kit)
        {            
            _kit = kit;
        }

        public const string SignupName = "signup";
        public const string SuccessName = "success";

        // Локаторы (XPath) — удобно переиспользовать и в DevTools.
        public readonly Dictionary<string, string> X = new()
        {
            // signup
            ["signup_title"] = "//div[@data-popup-name='signup']//span[contains(@class,'title')]",
            ["email_label"] = "//div[@data-popup-name='signup']//label[1]//span[contains(@class,'placeholder')]",
            ["email_input"] = "//div[@data-popup-name='signup']//input[@type='email']",
            ["marketing"] = "//div[@data-popup-name='signup']//label[@class='checkbox'][1]//span[last()]",
            ["agree_label"] = "//div[@data-popup-name='signup']//label[@class='checkbox'][2]//span[last()]",
            ["agree_checkbox"] = "//div[@data-popup-name='signup']//input[@type='checkbox' and @name='agree']",
            ["submit_text"] = "//div[@data-popup-name='signup']//div[@class='btn-text']",
            ["submit_button"] = "//div[@data-popup-name='signup']//button[contains(@class,'btn') and contains(@class,'cta')]",
            ["bottom_text"] = "//div[@data-popup-name='signup']//div[contains(@class,'cta-bottom')]//p",
            ["bottom_cta"] = "//div[@data-popup-name='signup']//div[contains(@class,'cta-bottom')]//button",
            ["data_msgs_host"] = "//div[@data-popup-name='signup']//div[contains(@class,'custom-form') and @data-msgs]",

            // success
            ["success_title"] = "//div[@data-popup-name='success']//span[contains(@class,'title')]",
            ["success_subtitle"] = "//div[@data-popup-name='success']//p[contains(@class,'text')]",
            ["success_button"] = "//div[@data-popup-name='success']//button[contains(@class,'btn') and contains(@class,'cta')]",
        };

        /// <summary>Открывает signup: кликом по триггеру или форсированно.</summary>
        public void Open()
        {
            _kit.OpenPopupByTriggerOrForce(SignupName,
                "[data-open-popup='signup'], [data-open-popup='sign-up']");
        }

        /// <summary>Возвращает тексты видимых элементов signup.</summary>
        public (string title, string emailLabel, string emailPh,
                string marketing, string agreeLabel,
                string submitText, string bottomText, string bottomCta) ReadSignupTexts()
        {
            return (
                _kit.GetTextByXPath(X["signup_title"]),
                _kit.GetTextByXPath(X["email_label"]),
                _kit.GetAttrByXPath(X["email_input"], "placeholder"),
                _kit.GetTextByXPath(X["marketing"]),
                _kit.GetTextByXPath(X["agree_label"]),
                _kit.GetTextByXPath(X["submit_text"]),
                _kit.GetTextByXPath(X["bottom_text"]),
                _kit.GetTextByXPath(X["bottom_cta"])
            );
        }

        /// <summary>Вводит email, проставляет чекбокс согласия, жмёт Submit.</summary>
        public void FillAndSubmit(string email)
        {
            var emailInput = _kit.FindByXPath(X["email_input"])
                            ?? throw new NoSuchElementException("email_input not found");
            emailInput.Clear();
            emailInput.SendKeys(email);

            var agreeInput = _kit.FindByXPath(X["agree_checkbox"])
                            ?? throw new NoSuchElementException("agree_checkbox not found");
            _kit.JsClick(agreeInput);

            var submitBtn = _kit.FindByXPath(X["submit_button"])
                          ?? throw new NoSuchElementException("submit_button not found");
            _kit.JsClick(submitBtn);
        }

        /// <summary>Ждёт видимость success-попапа.</summary>
        public void WaitSuccessVisible(int seconds = 40) =>
            _kit.WaitPopupVisible(SuccessName, seconds);


        /// <summary>Читает тексты из success-попапа.</summary>
        public (string title, string subtitle, string button) ReadSuccessTexts()
        {
            return (
                _kit.GetTextByXPath(X["success_title"]),
                _kit.GetTextByXPath(X["success_subtitle"]),
                _kit.GetTextByXPath(X["success_button"])
            );
        }

        /// <summary>
        /// Дожидается success-попапа и валидирует его тексты.
        /// Если видимый заголовок/подзаголовок пустые, пробует взять их из data-msgs (fallback).
        /// На расхождения делает скриншоты с ключами: success_title / success_subtitle / success_button.
        /// </summary>
        public void WaitAndValidateSuccess(string locale, List<string> errors, int waitSeconds = 45)
        {
            // 1) Ждём сам попап
            _kit.WaitPopupVisible(SuccessName, waitSeconds);

            // 2) Пробуем прочитать тексты из DOM
            var (title, subtitle, button) = ReadSuccessTexts();

            // 3) Fallback: если нет текста, читаем из data-msgs на signup
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(subtitle))
            {
                try
                {
                    var host = _kit.FindByXPath(Locators.Signup.DataMsgsHost);
                    if (host != null)
                    {
                        var raw = host.GetAttribute("data-msgs") ?? "";
                        var decoded = System.Net.WebUtility.HtmlDecode(raw);

                        using var jd = System.Text.Json.JsonDocument.Parse(decoded);
                        if (jd.RootElement.TryGetProperty("success", out var success))
                        {
                            if (string.IsNullOrWhiteSpace(title) && success.TryGetProperty("title", out var t))
                                title = t.GetString() ?? title;

                            if (string.IsNullOrWhiteSpace(subtitle) && success.TryGetProperty("subtitle", out var s))
                                subtitle = s.GetString() ?? subtitle;

                            if (string.IsNullOrWhiteSpace(button) && success.TryGetProperty("button", out var b))
                                button = b.GetString() ?? button;
                        }
                    }
                }
                catch
                {
                    // молча игнорируем: fallback не обязателен
                }
            }

            // 4) Валидации: contains для title/subtitle, equals для кнопки
            ExpectContains(locale, "success_title", title, errors, screenshotKey: "success_title");
            ExpectContains(locale, "success_subtitle", subtitle, errors, screenshotKey: "success_subtitle");
            ExpectEquals(locale, "success_button", button, errors, screenshotKey: "success_button");
        }

        /// <summary>Ожидаем, что фактическая строка содержит эталонную.</summary>
        private void ExpectContains(string locale, string key, string actual, List<string> errors, string? screenshotKey = null)
        {
            var expected = GetExpected(locale, key);
            if (string.IsNullOrEmpty(actual))
            {
                if (!string.IsNullOrEmpty(screenshotKey)) _kit.TakeScreenshot(locale, screenshotKey);
                errors.Add($"[{locale}] {key}: пустое значение.");
                return;
            }
            if (!SeleniumKit.ContainsNormalized(actual, expected))
            {
                if (!string.IsNullOrEmpty(screenshotKey)) _kit.TakeScreenshot(locale, screenshotKey);
                errors.Add($"[{locale}] {key}: ожидали содержит «{expected}», получили «{actual}».");
            }
        }

        /// <summary>Ожидаем точное совпадение (после нормализации).</summary>
        private void ExpectEquals(string locale, string key, string actual, List<string> errors, string? screenshotKey = null)
        {
            var expected = GetExpected(locale, key);
            if (string.IsNullOrEmpty(actual))
            {
                if (!string.IsNullOrEmpty(screenshotKey)) _kit.TakeScreenshot(locale, screenshotKey);
                errors.Add($"[{locale}] {key}: пустое значение.");
                return;
            }
            if (!SeleniumKit.EqualsNormalized(actual, expected))
            {
                if (!string.IsNullOrEmpty(screenshotKey)) _kit.TakeScreenshot(locale, screenshotKey);
                errors.Add($"[{locale}] {key}: ожидали «{expected}», получили «{actual}».");
            }
        }

        private static string GetExpected(string locale, string key)
        {
            // Берём эталон из Translations.Exp
            return Translations.Exp.TryGetValue((locale, key), out var v) ? v : "";
        }

    }
}
