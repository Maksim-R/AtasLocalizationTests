namespace AtasLocalizationTests
{
    public static class Locators
    {
        // --- SIGNUP POPUP ---
        public static class Signup
        {
            public const string Root = "//div[@data-popup-name='signup']";
            public const string Title = Root + "//span[contains(@class,'title')]";
            public const string EmailLabel = Root + "//label[1]//span[contains(@class,'placeholder')]";
            public const string EmailInput = Root + "//input[@type='email']";
            public const string Marketing = Root + "//label[contains(@class,'checkbox')][1]//span[last()]";
            public const string AgreeLabel = Root + "//label[contains(@class,'checkbox')][2]//span[last()]";
            public const string AgreeCheckbox = Root + "//input[@type='checkbox' and @name='agree']";
            public const string SubmitButton = Root + "//button[contains(@class,'btn') and contains(@class,'cta')]";
            public const string SubmitText = Root + "//div[@class='btn-text']";
            public const string BottomText = Root + "//div[contains(@class,'cta-bottom')]//p";
            public const string BottomCta = Root + "//div[contains(@class,'cta-bottom')]//button";
            public const string DataMsgsHost = Root + "//div[contains(@class,'custom-form') and @data-msgs]";
        }

        // --- SUCCESS POPUP ---
        public static class Success
        {
            public const string Root = "//div[@data-popup-name='success']";
            public const string Title = Root + "//span[contains(@class,'title')]";
            public const string Subtitle = Root + "//p[contains(@class,'text')]";
            public const string Button = Root + "//button[contains(@class,'btn') and contains(@class,'cta')]";
        }

        // --- Общие элементы ---
        public static class Lang
        {
            public const string Button = "button.btn.langs-wrapper-lang";
            public const string Dropdown = "nav.langs-wrapper-dropdown";
            public const string Links = "nav.langs-wrapper-dropdown a.link";
        }

        // --- SIGNIN POPUP ---
        public static class Signin
        {
            public const string Root = "//div[@data-popup-name='signin']";
            public const string Title = Root + "//span[contains(@class,'title')]";
            public const string Form = Root + "//form[@action='/v2/account/signIn' and @method='post']";
            public const string ErrorLabel = Root + "//div[@class='error_label' and @data-error]";
            public const string EmailLabel = Root + "//label[.//input[@type='email']]/span[contains(@class,'placeholder')]";
            public const string EmailInput = Root + "//input[@type='email' and @name='email']";
            public const string PasswordLabel = Root + "//label[.//input[@type='password']]/span[contains(@class,'placeholder')]";
            public const string PasswordInput = Root + "//input[@type='password' and @name='password']";
            public const string ShowPasswordButton = Root + "//button[contains(@class,'show-password')]";
            public const string ForgotButton = Root + "//button[(contains(@class,'forgot') or @data-open-popup='reset')]";
            public const string SubmitButton = Root + "//button[@type='submit' and contains(@class,'btn')]";
            public const string SubmitText = SubmitButton + "//div[@class='btn-text']";
            public const string BottomText = Root + "//div[contains(@class,'cta-bottom')]//p";
            public const string BottomCtaButton = Root + "//div[contains(@class,'cta-bottom')]//button";
            public const string BottomCtaText = BottomCtaButton + "[normalize-space()]";
            public const string CloseButton = Root + "//button[@data-popup-close]";
        }

        // --- Popup names for compatibility ---
        public static class Popups
        {
            public const string Signin = "signin";
            public const string Signup = "signup";
            public const string Success = "success";
        }
    }
}