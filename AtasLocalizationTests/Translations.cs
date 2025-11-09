using System.Collections.Generic;

namespace AtasLocalizationTests
{
    /// <summary>
    /// Эталонные строки из ТЗ. Используются во всех тестах.
    /// </summary>
    public static class Translations
    {
        public static readonly Dictionary<(string locale, string key), string> Exp = new TupleIgnoreCaseDictionary
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
    }

    /// <summary>Словарь (locale,key) с регистронезависимыми ключами.</summary>
    public sealed class TupleIgnoreCaseDictionary : Dictionary<(string locale, string key), string>
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
