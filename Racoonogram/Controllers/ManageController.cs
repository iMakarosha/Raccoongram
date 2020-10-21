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


using System.Net;
using System.Net.Mail;

using System.Security.Cryptography;

namespace Racoonogram.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        const string subscriptionKey = "c0e64c91c915499197d95e7243e31625";
        const string uriBase =
            "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/analyze";

        I_U_Models uimdb = new I_U_Models();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        DateTime date = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));

        public ManageController() { }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult ImageManager()
        {
            ViewBag.List = new SelectList(new ImageService().GetCategoryListItems().ToList(), "Category", "Category");
            IEnumerable<Racoonogram.Models.Image> images = new ImageService().GetImagesByUserId(User.Identity.GetUserId()).ToList();
            foreach (var item in images)
            {
                item.Url = item.ImageId + "_sm.jpg";
            }
            return View(images);
        }

        [HttpPost]
        public ActionResult EditImage(string imneed)
        {
            ImageUnload imageToEdit = new ImageService().GetImageUnload(Convert.ToInt32(imneed));
            if (imageToEdit == null)
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "NotFound",
                    message = "Изображение не найдено"
                });
            imageToEdit.Url = "/Content/Content-image/" + imageToEdit.Id + "_sm.jpg";
            return PartialView(imageToEdit);
        }

        [HttpPost]
        public ActionResult SaveImageChanges(ImageUnload image)
        {
            new ImageService().SaveImage(image);
            return RedirectToAction("Index");
        }


        /*проверка изображения искусственный интелект*/
        /// <summary>
        /// Gets the analysis of the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file to analyze.</param>
        /// 

        [HttpPost]
        public async Task<JsonResult> CheckImage(HttpPostedFileBase Upload/*string imageFilePath*/)
        {
            string fileName = System.IO.Path.GetFileName(Request.Files[0].FileName);
            //save file
            string imageFilePath = Server.MapPath("~/Content/MidTerm/") + fileName;
            Request.Files[0].SaveAs(imageFilePath);
            string contentString;
            try
            {
                HttpClient client = new HttpClient();
                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters =
                    "visualFeatures=Categories,Description,Color,Adult";//ВОТ ТУТ ДОБАВЛЯТЬ ПАРАМЕТРЫ, КОТОРЫЕ НУЖНЫ

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Make the REST API call.
                    response = await client.PostAsync(uri, content);
                }

                // Get the JSON response.
                contentString = await response.Content.ReadAsStringAsync();
                //var jld = JsonPrettyPrint(contentString);
                // Display the JSON response.
                System.IO.File.Delete(imageFilePath);
                //string[]names = contentString.
                return Json(contentString);
            }
            catch (Exception e)
            {
                contentString = e.Message;
                return Json(contentString);
            }
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        /// <summary>
        /// Formats the given JSON string by adding line breaks and indents.
        /// </summary>
        /// <param name="json">The raw JSON string to format.</param>
        /// <returns>The formatted JSON string.</returns>
        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            string INDENT_STRING = "    ";
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(
                                item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(
                                item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(
                                item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
        

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public JsonResult Upload()
        {
            Racoonogram.Models.Image i = new Racoonogram.Models.Image();
            if (Request.Form["Category"] == null || Request.Form["Category"] == "")
            {
                return Json("! Поле 'Категория' является обязательным для заполнения");
            }
            i.Category = Request.Form["Category"].ToLower();
            if (Request.Form["KeyWords"] == null || Request.Form["KeyWords"] == "")
            {
                return Json("! Поле 'Ключевые слова' является обязательным для заполнения");
            }
            i.KeyWords = Request.Form["KeyWords"].ToLower();
            if (Request.Form["Description"] != null&&Request.Form["Description"] != "")
            {
                i.Description = Request.Form["Description"].Substring(0, 1).ToUpper()+ Request.Form["Description"].Substring(1); 
            }
            string Price = Request.Form["Price"];
            if (Price.IndexOf(".") >= 0) Price = Price.Replace('.', ',');
            if (Price == "" || !(Double.TryParse(Price, out double ass)))
            {
                i.Price = 0;
            }
            else if (Convert.ToDouble(Price) > 6)
            {
                return Json("! Согласно условиям работы сайта, стоимость изображения не может превышать 6$.");
            }
            else i.Price = Convert.ToDouble(Price);
            i.ApplicationUserId = User.Identity.GetUserId();
            i.Date = DateTime.Now;
            i.Colors = Request.Form["Colors"].ToLower();
            i.IsBlack = Convert.ToBoolean(Request.Form["IsBlack"]);
            i.Orient = Request.Form["Orient"];

            new ImageHandler().UploadImage(Request.Files, i);

            return Json("Изображения успешно загружены");
        }

        [HttpPost]
        public JsonResult DeletePhoto(string imid)
        {
            try
            {
                new ImageHandler().DeleteImage(Convert.ToInt32(Request.Form[0]));
                return Json("allgood");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Index()
        {
            if (User.IsInRole("admin")) { return RedirectToAction("IndexAdmin");}
            else if (User.IsInRole("author")){ return RedirectToAction("IndexAuthor"); }
            else if (User.IsInRole("user")) { return RedirectToAction("IndexUser"); }
            else return HttpNotFound();
        }

        [Authorize(Roles = "admin")]
        public ActionResult IndexAdmin()
        {
            return View();
        }

        [Authorize(Roles ="author")]
        public ActionResult IndexAuthor()
        {
            AuthorAndAllImages UserAndImages = new AuthorAndAllImages();
            string userId = User.Identity.GetUserId();
            UserAndImages.User = new UserService().GetUser(userId);
            IEnumerable<Models.Image> allUserImages = new ImageService().GetImagesByUserId(UserAndImages.User.Id);
            foreach (Models.Image i in allUserImages)
            {
                i.Url = i.ImageId + "_sm.jpg";
            }
            UserAndImages.ImagesUser = allUserImages.ToList();
            if (System.IO.File.Exists(Server.MapPath("~/Content/User-logo/") + User.Identity.GetUserId() + ".jpg"))
            {
                ViewBag.Logo = "/Content/User-logo/" + User.Identity.GetUserId() + ".jpg";
            }
            else ViewBag.Logo = "/Content/User-logo/raccoon.jpg";
            ViewBag.ShortUrl = new ImageHandler().RenderSiteShort(UserAndImages.User.Site);
            ViewBag.photos = new ImageService().GetImagesByUserId(UserAndImages.User.Id).Count();
            ViewBag.likes = new ImageService().GetLikesCount(userId);
            ViewBag.deal = new UserService().GetOrdersCount(userId);
            return View(UserAndImages);
        }

        [Authorize(Roles = "user")]
        public ActionResult IndexUser()
        {
            UserAndImagesAndPlans userAndImages = new UserAndImagesAndPlans();
            userAndImages.Id = User.Identity.GetUserId();
            userAndImages.buyings = new UserService().GetBuyings(userAndImages.Id).ToList();
            userAndImages.buyingsAll = new UserService().GetBuyingsAll(userAndImages.Id).ToList();
            try
            {
                userAndImages.likings = new ImageService().GetUserLikes(userAndImages.Id).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.d = ex.Message;
            }
            userAndImages.User = new UserService().GetUser(userAndImages.Id);
            userAndImages.ImNews1 = new ImageService().GetImagesForUserProfile("news", 8, 0).ToList();
            userAndImages.ImNews2 = new ImageService().GetImagesForUserProfile("news", 8, 8).ToList();
            userAndImages.ImFree = new ImageService().GetImagesForUserProfile("free", 5, 0).ToList();
            userAndImages.querysPopulars = new UserService().GetQuerysPopulars(5).ToList();
            ViewBag.Logo = "/Content/User-logo/"+userAndImages.Id+".jpg";

            return View(userAndImages);
        }


        [HttpPost]
        public JsonResult ImageBuy(string email, string optradio, int ImageId, string ApplicationUserId)
        {
            if (email == null || email == "")
                return Json("<b style='color:red'>Поле Email обязательно для заполнения</b>");
            if (optradio == null || optradio == "")
                return Json("<b style='color:red'>Выберите размер загружаемой фотографии</b>");
            int size = Convert.ToInt32(optradio.Substring(0, optradio.Length - 2));
            try
            {
                Order orders = new UserService().GetOrder(email, ImageId, size);
                if (orders == null)
                {
                    string idOfImg = new ImageHandler().GetFile(ImageId, size);
                    if (idOfImg.IndexOf("System.IO.FileNotFoundException: ") > -1) {
                        return Json("<p style='font-weight:bold;color:red;text-align:center;'>Мы сожалеем, данное изображение удалено правообладателем.<p>");
                    }
                    else
                    {
                        new UserService().AddOrder(new Order
                        {
                            ImageId = ImageId,
                            ApplicationUserId = ApplicationUserId,
                            BuyerEmail = email,
                            Size = size,
                            Price = new ImageService().GetImagePrice(ImageId),
                            BuyingDate = DateTime.Now,
                            IsHide = 0
                        });
                        //string buyerId = new UserService().GetUserID(email, "email");
                        //PlanBuying buying = new PlanService().GetPlanBuying(buyerId, order.Price);
                        //if (buying.ImageBalance.ToString() != null && buying.ImageBalance>0)
                        //{
                        //    buying.ImageBalance -= 1;
                        //}
                        //else buying.MoneyBalance -= order.Price;
                        return sendHrefImg(email, size, idOfImg);
                    }
                }
                else
                {
                    return Redownload(orders.OrderId);
                }
            }
            catch(Exception ex)
            {
                return Json(ex.Message);
            }

        }

        private JsonResult sendHrefImg(string email, int size, string idOfImg)
        {
            try
            {
                new UserHandlers().SendImgHref(email, size, idOfImg, Request.Url.ToString().Substring(0, Request.Url.ToString().LastIndexOf((String)RouteData.Values["controller"].ToString())));
                return Json(size);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult ImgHide(int Id)
        {
            try
            {
                new ImageService().HideImage(Id);
                return Json("Ok");
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return Json("false");
            }
        }

        [HttpPost]
        public JsonResult PlanHide(int Id)
        {
            try
            {
                new PlanService().HidePlan(Id);
                return Json("Ok");
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return Json("false");
            }
        }

        [HttpPost]
        public JsonResult Redownload(int orderid)
        {
            Order order = new UserService().GetOrder(orderid);
            if (order != null)
            { 
                string idOfImg = new ImageHandler().GetFile(order.ImageId, order.Size);
                if (idOfImg.IndexOf("System.IO.FileNotFoundException: ") > -1) {
                    return Json("<p style='font-weight:bold;color:red;text-align:center;'>Мы сожалеем, данное изображение удалено правообладателем.<p>");
                }
                else
                {
                    return sendHrefImg(new UserService().GetUserEmail(User.Identity.GetUserId()), order.Size, idOfImg);
                }
            }
            return Json("error");
        }

        //СТАТИСТИКА
        public ActionResult Statistics()
        {
            return View(new StatisticHandler().GetStatistic(User.Identity.GetUserId()));
        }

        public ActionResult StatisticsBuyLike(string orderBy1 = "", string orderBy2 = "", string orderBy3 = "", string tab = "f1", string count="10")//async
        {
            IEnumerable<StatisticsBuyLike> buyLikes = new StatisticHandler().GetStatisticBuyLikes(User.Identity.GetUserId(), orderBy1, orderBy2, orderBy3, tab, count);
            //если есть упорядочивание

            switch (tab)
            {
                case "f2":
                    switch (orderBy1)
                    {
                        case "По дате":
                            ViewBag.th2 = "Дата"; ViewBag.th3 = "Кол-во скачиваний"; ViewBag.th4 = "Скачано на сумму"; ViewBag.th5 = "Последнее из них:"; ViewBag.count = 3;
                            break;

                        case "По e-mail":
                            ViewBag.th2 = "E-mail"; ViewBag.th3 = "Количество картинок"; ViewBag.th4 = "Кол-во скачиваний"; ViewBag.th5 = "Дата посл. покупки"; ViewBag.count = 5;
                            break;

                        default:
                            ViewBag.th2 = "Категория"; ViewBag.th3 = "Стоимость"; ViewBag.th4 = "Кол-во скачиваний"; ViewBag.th5 = "Дата посл. покупки"; ViewBag.count = 5;
                            break;
                    }
                    break;
                case "f3":
                    switch (orderBy3)
                    {
                        case "По ID картинки":
                            ViewBag.th2 = "Категория"; ViewBag.th3 = "Кол-во лайков"; ViewBag.th4 = "Дата посл.лайка"; ViewBag.count = 4;
                            break;
                        case "По категориям":
                            ViewBag.th2 = "Категория"; ViewBag.th3 = "Кол-во лайков"; ViewBag.th4 = "Дата посл.лайка"; ViewBag.count = 4;
                            break;
                        default:
                            ViewBag.th2 = "Категория"; ViewBag.th3 = "Дата лайка"; ViewBag.count = 2;
                            break;
                    }
                    break;
                default:
                    ViewBag.count = 5; ViewBag.th2 = "Размер"; ViewBag.th3 = "E-mail"; ViewBag.th4 = "Стоимость"; ViewBag.th5 = "Дата покупки";
                    break;
            }

            return PartialView(buyLikes);
        }

        public ActionResult StatisticsDownloaders()//async
        {
            StatisticsDownloaders statisticsDownloaders = new StatisticHandler().GetStatisticDownloaders(User.Identity.GetUserId());
            return PartialView(statisticsDownloaders);
        }

        public ActionResult StatisticsMoney()//async
        {
            return PartialView();
        }

        /*common top modals*/
        public ActionResult BestImCarousel(string typeHD = "heart")//async
        {
            IEnumerable<BestImagesL> bestImagesL = new ImageService().GetBestImages(date, typeHD, 12).ToList();
            if (typeHD == "heart")
            {
                ViewBag.LO = "Лайков за неделю";
                ViewBag.gif = "(по количеству лайков)";
            }
            else
            {
                ViewBag.LO = "Скачиваний за неделю";
                ViewBag.gif = "(по количеству загрузок)";
            }
            foreach (var item in bestImagesL)
            {
                item.Url = "/Content/Content-image/" + item.Id + ".jpg";
            }
            return PartialView(bestImagesL);
        }

        public ActionResult BestAuthor()
        {
            return PartialView(new StatisticHandler().GetStatisticAutors(date));
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> AccountEditor(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен."
                : message == ManageMessageId.SetPasswordSuccess ? "Пароль задан."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Настроен поставщик двухфакторной проверки подлинности."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : message == ManageMessageId.AddPhoneSuccess ? "Ваш номер телефона добавлен."
                : message == ManageMessageId.RemovePhoneSuccess ? "Ваш номер телефона удален."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Создание и отправка маркера
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Ваш код безопасности: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Отправка SMS через поставщик SMS для проверки номера телефона
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // Это сообщение означает наличие ошибки; повторное отображение формы
            ModelState.AddModelError("", "Не удалось проверить телефон");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeProfile(UserAccountEditor userEdit)
        {
            if (!ModelState.IsValid)
            {
                if (!ModelState.IsValidField("UserName"))
                    ModelState.AddModelError("UserName", "Необходим логин длиной от 5 до 18 символов");
                if (!ModelState.IsValidField("Email"))
                    ModelState.AddModelError("Email", "Поле обязательно для заполнения");
                    ViewBag.Logo = "/Content/User-logo/" + userEdit.Id + ".jpg";

                return View();
            }
            var jl = userEdit.logo_file;
            if (userEdit.logo_file != null)
            {
                try
                {
                    new ImageHandler().UpdateUserLogo(User.Identity.GetUserId(), userEdit.logo_file);
                }
                catch (Exception ex)
                {
                    string err = ex.Message;
                }
            }

            new UserService().SaveUser(userEdit, User.Identity.GetUserId());
            
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UploadW(HttpPostedFileBase upload,string asss)
        {
            if (upload != null)
            {
                // получаем имя файла
            }
            return RedirectToAction("Index");
        }

        //редактирование профиля данные пользователя аккаунт
        public ActionResult ChangeProfile()
        {
            //UserEditor userEditor = new UserEditor();
            UserAccountEditor applicationUser = new UserService().GetUserAccountInfo(User.Identity.GetUserId().ToString());
            ViewBag.Logo = "/Content/User-logo/" + applicationUser.Id + ".jpg";
            return View(applicationUser);
        }
        

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // Это сообщение означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "Внешнее имя входа удалено."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Запрос перенаправления к внешнему поставщику входа для связывания имени входа текущего пользователя
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }
    }
}