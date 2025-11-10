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


    }
}
