using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace Racoonogram.Handlers
{
    public class AccountHandler
    {
        public AccountHandler() { }

        public string SendForgotPasswordMail(string email, string userName)
        {
            string code = "";
            Random r = new Random();
            char[] charArray = new char[72];
            int z = 0;
            for (char p = 'a'; p <= 'z'; p++)
            { charArray[z] = p; z++; }
            for (char p = '0'; p <= '9'; p++)
            { charArray[z] = p; z++; }
            for (char p = 'A'; p <= 'Z'; p++)
            { charArray[z] = p; z++; }
            for (char p = '0'; p <= '9'; p++)
            { charArray[z] = p; z++; }
            for (int p = 0; p < 30; p++)
            {
                code += charArray[r.Next(0, 71)];
            }
            SmtpClient smtpClient = new SmtpClient("smtp.mail.ru", 25);
            smtpClient.Credentials = new NetworkCredential("rosavtodorcza@mail.ru", "tararaKota1235");

            MailAddress to = new MailAddress(email);
            MailAddress from = new MailAddress("rosavtodorcza@mail.ru", "Фотобанк Raccoonogram");
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Восстановление пароля - фотобанк Racconogram";
            message.IsBodyHtml = true;
            message.Body = "<html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'></head><body><h2>Восстановление пароля - фотобанк Raccoonogram</h2><p>Ваш логин: " + userName + "</p><p>Ключ: " + code +
                "</p><p>В целях безопасности Ваших данных мы не храним Ваш пароль. Вам необходимо придумать новый, после чего доступ к аккаунту будет восстановлен.</p><br><hr/><p>Служба поддержки сервиса Raccoonogram</p><p>Возникли вопросы? Напишите нам: Raccoonogram.help@gmail.com</p><p style='text-align:right'>" + DateTime.Now + "</p></body></html>";
            smtpClient.EnableSsl = true;

            smtpClient.Send(message);
            return code;
        }
    }
}