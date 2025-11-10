using System;
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

            // --- Signin popup translations ---
            { ("RU", "signin_title"), "Вход" },
            { ("EN", "signin_title"), "Sign In" },
            { ("DE", "signin_title"), "Anmeldung" },
            { ("ES", "signin_title"), "Iniciar sesión" },
            { ("FR", "signin_title"), "Connexion" },
            { ("IT", "signin_title"), "Accesso" },
            { ("UA", "signin_title"), "Вхід" },
            { ("CN", "signin_title"), "登录" },

            { ("RU", "signin_email_ph"), "Email (на него придет пароль от ATAS)" },
            { ("EN", "signin_email_ph"), "Enter your email" },
            { ("DE", "signin_email_ph"), "E-Mail-Adresse eingeben" },
            { ("ES", "signin_email_ph"), "Introduce tu correo electrónico" },
            { ("FR", "signin_email_ph"), "Saisissez votre adresse e-mail" },
            { ("IT", "signin_email_ph"), "Inserisci il tuo indirizzo e-mail" },
            { ("UA", "signin_email_ph"), "Введи свій email" },
            { ("CN", "signin_email_ph"), "请输入您的邮箱地址" },

            { ("RU", "signin_pass_label"), "Пароль" },
            { ("EN", "signin_pass_label"), "Password" },
            { ("DE", "signin_pass_label"), "Passwort" },
            { ("ES", "signin_pass_label"), "Contraseña" },
            { ("FR", "signin_pass_label"), "Mot de passe" },
            { ("IT", "signin_pass_label"), "Password" },
            { ("UA", "signin_pass_label"), "Пароль" },
            { ("CN", "signin_pass_label"), "密码" },

            { ("RU", "signin_pass_ph"), "Введи свой пароль" },
            { ("EN", "signin_pass_ph"), "Enter your password" },
            { ("DE", "signin_pass_ph"), "Passwort eingeben" },
            { ("ES", "signin_pass_ph"), "Introduce tu contraseña" },
            { ("FR", "signin_pass_ph"), "Saisissez votre mot de passe" },
            { ("IT", "signin_pass_ph"), "Inserisci la tua password" },
            { ("UA", "signin_pass_ph"), "Введи свій пароль" },
            { ("CN", "signin_pass_ph"), "请输入密码" },

            { ("RU", "signin_forgot"), "Забыл(а) пароль?" },
            { ("EN", "signin_forgot"), "Forgot your password?" },
            { ("DE", "signin_forgot"), "Passwort vergessen?" },
            { ("ES", "signin_forgot"), "¿Olvidaste tu contraseña?" },
            { ("FR", "signin_forgot"), "Mot de passe oublié ?" },
            { ("IT", "signin_forgot"), "Hai dimenticato la password?" },
            { ("UA", "signin_forgot"), "Забули пароль?" },
            { ("CN", "signin_forgot"), "忘记密码？" },

            { ("RU", "signin_submit"), "Войти" },
            { ("EN", "signin_submit"), "Sign In" },
            { ("DE", "signin_submit"), "Anmelden" },
            { ("ES", "signin_submit"), "Acceder" },
            { ("FR", "signin_submit"), "Se connecter" },
            { ("IT", "signin_submit"), "Accedi" },
            { ("UA", "signin_submit"), "Увійти" },
            { ("CN", "signin_submit"), "登录" },

            { ("RU", "signin_bottom_text"), "Нет учетной записи?" },
            { ("EN", "signin_bottom_text"), "No account?" },
            { ("DE", "signin_bottom_text"), "Noch kein Konto?" },
            { ("ES", "signin_bottom_text"), "¿No tienes cuenta?" },
            { ("FR", "signin_bottom_text"), "Pas de compte ?" },
            { ("IT", "signin_bottom_text"), "Non hai un account?" },
            { ("UA", "signin_bottom_text"), "Немає облікового запису?" },
            { ("CN", "signin_bottom_text"), "没有账号？" },

            { ("RU", "signin_bottom_cta"), "Зарегистрироваться" },
            { ("EN", "signin_bottom_cta"), "Sign up" },
            { ("DE", "signin_bottom_cta"), "Registrieren" },
            { ("ES", "signin_bottom_cta"), "Registrarse" },
            { ("FR", "signin_bottom_cta"), "S’inscrire" },
            { ("IT", "signin_bottom_cta"), "Registrati" },
            { ("UA", "signin_bottom_cta"), "Зареєструватися" },
            { ("CN", "signin_bottom_cta"), "注册" },

            { ("RU", "signin_error"), "Неверный логин или пароль!" },
            { ("EN", "signin_error"), "Invalid username or password!" },
            { ("DE", "signin_error"), "Ungültiger Benutzername oder Passwort!" },
            { ("ES", "signin_error"), "¡Nombre de usuario o contraseña incorrectos!" },
            { ("FR", "signin_error"), "Nom d'utilisateur ou mot de passe invalide !" },
            { ("IT", "signin_error"), "Nome utente o password non validi!" },
            { ("UA", "signin_error"), "Невірний логін або пароль!" },
            { ("CN", "signin_error"), "用户名或密码无效！" },
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
