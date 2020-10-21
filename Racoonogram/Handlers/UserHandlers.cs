using System;
using System.Collections.Generic;
using System.Linq;
using Racoonogram.Models;
using Racoonogram.Services;
using System.Net;
using System.Net.Mail;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Data.Entity;
using Racoonogram.Models;
using Racoonogram.Handlers;
using Racoonogram.Services;
using im = Racoonogram.Models.Image;

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Racoonogram.Handlers
{
    public class UserHandlers
    {
        public UserHandlers() { }

        public void Like(int imageId, string userName)
        {
            Like l = new Like
            {
                BuyingDate = DateTime.Now,
                ImageId = imageId
            };
            if (!String.IsNullOrEmpty(userName))
            {
                l.UserId = new UserService().GetUserID(userName);
            }
            new ImageService().LikeAdd(l);
        }

        
        public void SendImgHref(string email, int size, string idOfImg, string ashref)
        {
            /*отправка ссылки на изображение по почте*/
            SmtpClient smtpClient = new SmtpClient("smtp.mail.ru", 25);
            smtpClient.Credentials = new NetworkCredential("rosavtodorcza@mail.ru", "tararaKota1235");
            MailAddress to = new MailAddress(email);
            MailAddress from = new MailAddress("rosavtodorcza@mail.ru", "Фотобанк Raccoonogram");
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Ссылка для скачивания изображения";
            message.IsBodyHtml = true;
            message.Body = "<html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'></head><body><h2>Загрузка изображения - фотобанк Raccoonogram</h2>" +
"<p>Для скачивания фотографии перейдите по ссылке ниже:</p><br><a download href='" + ashref + "/Home/GetImg/" + idOfImg + "' title='Получить приобретенное изображение'>" + ashref + "/Home/GetImg/" + idOfImg + "</a> <hr/><p>Служба поддержки сервиса Raccoonogram</p><br/><p>Возникли вопросы? Напишите нам: Raccoonogram.help@gmail.com</p><p style='text-align:right'>" + DateTime.Now + "</p></body></html>";
            smtpClient.EnableSsl = true;
            smtpClient.Send(message);
            ImgDownload imgDownload = new ImgDownload();
            imgDownload.Id = idOfImg;
            imgDownload.DateLast = DateTime.Now.AddDays(1);

            new ImageService().ImageDownloadAdd(imgDownload);
        }

        public void BuyPlan(string userId, string planId)
        {
            PlanBuying buying;
            if (planId.Contains('s'))
            {
                buying = new PlanService().GetPlanBuying(userId);
                if (buying == null)
                {
                    buying = new PlanBuying
                    {
                        Id_plan = planId,
                        Id_user = userId,
                        MoneyBalance = 0
                    };
                }
                else
                {
                    new PlanService().PlanBuyingDelete(buying);
                }
                buying.MoneyBalance += new PlanService().GetPlanPrice(planId);
                buying.BuyingDate = DateTime.Now;
                buying.isHide = 0;
            }
            else
            {
                buying = new PlanBuying
                {
                    Id_plan = planId,
                    Id_user = userId,
                    BuyingDate = DateTime.Now,
                    isHide = 0
                };
                buying.ImageBalance = new PlanService().GetPlanImages(planId);
            }
            new PlanService().PlanBuyingAdd(buying);
        }
    }
}