using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using Racoonogram.Models;

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Racoonogram.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        const string subscriptionKey = "f4081285918f4874a517552daec09e67";
        const string uriBase =
            "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/analyze";


        ApplicationDbContext db = new ApplicationDbContext();
        I_U_Models uimdb = new I_U_Models();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;



        DateTime date = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));

        public ManageController()
        {
        }

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
            SelectList listOfCat = new SelectList(db.Images.GroupBy(i => i.Category).Select(group => new { Category = group.Key }).ToList(), "Category", "Category");

            ////var listOfCat = from i in db.Images
            //                group i by i.Category into j
            //                orderby j
            //                select new Categories { CategoryName = j.Key };
            ViewBag.List = listOfCat;
            string userid = User.Identity.GetUserId();
            IEnumerable<Racoonogram.Models.Image> images = db.Images.Select(i => i).Where(i => i.ApplicationUserId == userid).OrderByDescending(i => i.ImageId).ToList();
            foreach (var item in images)
            {
                item.Url = item.ImageId + "_sm.jpg";
            }
            return View(images);
        }
        /*загрузка изображений*/
        //[HttpPost]
        //public JsonResult Upload(HttpPostedFileBase Upload)
        //{
        //    if (Upload != null)
        //    {
        //        string fileName = System.IO.Path.GetFileName(Upload.FileName);
        //        Upload.SaveAs(Server.MapPath("~/Content/Content-image/" + fileName));
        //    }
        //    return Json("all good");
        //}

        //[HttpPost]
        //public ActionResult Upload(string Category, string Price, HttpPostedFileBase Upload)
        //{
        //    if (Upload != null)
        //    {
        //        //получаем имя файла
        //        string fileName = System.IO.Path.GetFileName(Upload.FileName);
        //        //сохраняем файл в папку в проекте
        //        Upload.SaveAs(Server.MapPath("~/Content/Content-image/" + fileName));
        //    }
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public ActionResult EditImage(string imneed)
        {
            int Id = Convert.ToInt32(imneed);
            ImageUnload imageToEdit = db.Images.Select(u => new ImageUnload
            {
                Id = u.ImageId,
                Category = u.Category,
                KeyWords = u.KeyWords,
                Description = u.Description,
                Price = u.Price
            }).Where(i => i.Id == Id).FirstOrDefault();
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
            Racoonogram.Models.Image imEdit = db.Images.Select(i => i).Where(i => i.ImageId == image.Id).FirstOrDefault();
            imEdit.Category = image.Category;
            imEdit.KeyWords = image.KeyWords;
            imEdit.Description = image.Description;
            imEdit.Price = image.Price;
            db.SaveChanges();
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
                //return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(contentString);
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
            //public JsonResult Upload(HttpFileCollection Files, string Category)
            //{
            //    foreach (string file in Files)
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
            if (Request.Form["Description"] != null)
            {
                i.Description = Request.Form["Description"];
            }
            //var asdfl = Convert.ToDouble(Request.Form["Price"]);
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
            string fullPath = Server.MapPath("~/Content/Content-image/");
            foreach (string file in Request.Files)
            {
                var upload = Request.Files[file];
                if (upload != null)
                {
                    System.Drawing.Image waterMarkIm;
                    System.Drawing.Bitmap bit;
                    try
                    {
                        //get filename
                        string fileName = System.IO.Path.GetFileName(upload.FileName);
                        //save file
                        string fullPathName = Server.MapPath("~/Content/MidTerm/") + fileName;
                        upload.SaveAs(fullPathName);
                        db.Images.Add(i);
                        db.SaveChanges();
                        string newName = fullPath + i.ImageId + ".jpg";
                        if (System.IO.File.Exists(newName))
                        {
                            System.IO.File.Delete(newName);
                            //System.IO.File.Create(newName);
                            //System.IO.File.Replace(fullPathName, newName, Server.MapPath("~/Content/Rez-copy/") + i.ImageId + ".jpg");
                        }
                        //System.IO.File.Move(fullPathName, newName);
                        
                        System.Drawing.Image imNormal = System.Drawing.Image.FromFile(fullPathName);
                        imNormal.Save(newName);
                        System.Drawing.Image imNormalSmall = RezizeImage(imNormal, 400,240);



                        /*ДОБАВЛЕНИЕ ВОДЯНОГО ЗНАКА*/
                        
                        waterMarkIm = System.Drawing.Image.FromFile(Server.MapPath("~/Content/Images/")+ "Watermark_.png");
                            int h = imNormalSmall.Height;
                            int w = 400;
                            bit = new Bitmap(w, h);
                            using (Graphics g = Graphics.FromImage(bit))
                            {
                                g.DrawImage(imNormalSmall, 0, 0, w, h);
                                g.DrawImage(waterMarkIm, 10, ((imNormalSmall.Height - waterMarkIm.Height) / 2), waterMarkIm.Width, waterMarkIm.Height);
                            }




                        bit.Save(fullPath + i.ImageId + "_sm.jpg");

                        imNormalSmall = RezizeImage(imNormalSmall, 200,40);
                        imNormalSmall.Save(fullPath + i.ImageId + "_xs.jpg");
                        imNormal = null;
                        System.IO.File.Delete(fullPathName);
                    }
                    catch (Exception ex)
                    {
                        string werrrrrt = ex.Message;
                    }
                }
            }
            //string cat = Request.Form[0];
            return Json("Изображения успешно загружены");
        }

        [HttpPost]
        public JsonResult DeletePhoto(string imid)
        {
            //return Json("allgood");
            int ruh = Convert.ToInt32(Request.Form[0]);
            try
            {
                Models.Image im = db.Images.Select(i => i).Where(i => i.ImageId == ruh).FirstOrDefault();
                if (im != null)
                    db.Images.Remove(im);
                db.SaveChanges();
                if (System.IO.File.Exists(Server.MapPath("~/Content/Content-image/") + ruh + ".jpg"))
                {
                    System.IO.File.Delete(Server.MapPath("~/Content/Content-image/") + ruh + ".jpg");
                    System.IO.File.Delete(Server.MapPath("~/Content/Content-image/") + ruh + "_sm.jpg");
                    System.IO.File.Delete(Server.MapPath("~/Content/Content-image/") + ruh + "_xs.jpg");

                }

                return Json("allgood");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }

        }

        public ActionResult Index()
        {
            AuthorAndAllImages UserAndImages = new AuthorAndAllImages();
            string userId = User.Identity.GetUserId();
            UserAndImages.User = db.Users.Select(u => u).Where(u => u.Id == userId).FirstOrDefault();
            IEnumerable<Models.Image> allUserImages = db.Images.Where(i => i.ApplicationUserId == UserAndImages.User.Id).OrderByDescending(i => i.ImageId);
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
            ViewBag.ShortUrl = HomeController.RenderSiteShort(UserAndImages.User.Site);
            ViewBag.photos = db.Images.Where(i => i.ApplicationUserId == userId).Select(i => i.ImageId).Count();
            ViewBag.likes = (from l in db.Likes
                             join i in db.Images on l.ImageId equals i.ImageId
                             where i.ApplicationUserId == userId
                             select l.LikeId).Count();
            ViewBag.deal = db.Orders.Where(o => o.ApplicationUserId == userId).Select(o => o.OrderId).Count();
            return View(UserAndImages);

        }


        //СТАТИСТИКА
        public ActionResult Statistics()
        {
            List<BestBuyers> bests;
            string userId = User.Identity.GetUserId();
            ForStatistics statistics = new ForStatistics();
            ForStatisticsMy statisticsMy = new ForStatisticsMy();
            try
            {
                statisticsMy.CountDownloads = db.Orders.Where(o => o.ApplicationUserId == userId).Count();
            }
            catch (Exception ex)
            {
                statisticsMy.CountDownloads = 0;
            }

            try
            {
                statisticsMy.CountLikes = (from l in db.Likes
                                           join i in db.Images on l.ImageId equals i.ImageId
                                           where i.ApplicationUserId == userId
                                           select l.LikeId).Count();
            }
            catch (Exception ex)
            {
                statisticsMy.CountLikes = 0;
            }

            try
            {
                statisticsMy.SumTotal = db.Orders.Where(o => o.ApplicationUserId == userId).Select(o => o.Price).Sum().ToString();
            }
            catch (Exception ex)
            {
                statisticsMy.SumTotal = "0";
            }
            if (statisticsMy.SumTotal.ToString().IndexOf(',') >= 0)
            {
                statisticsMy.SumTotal = statisticsMy.SumTotal.ToString().Replace(',', '.');
            }
            bests = new List<BestBuyers>();

            try
            {
                List<string> buyersEmail = db.Orders.Where(o => o.ApplicationUserId == userId).GroupBy(o => o.BuyerEmail).OrderBy(o => o.Count()).Select(j => j.Key).ToList();
                List<string> buyersSum = new List<string>();
                int q = 0;
                double sum = 0;
                //statisticsMy.bestBuyers = buyers.GroupBy(e=>e.Email);
                foreach (var item in buyersEmail)
                {
                    q++;
                    if (q < 4)
                    {
                        BestBuyers b = new BestBuyers();
                        b.Email = item;
                        b.Sum = db.Orders.Where(o => o.BuyerEmail == item).Sum(o => o.Price).ToString().Replace(',', '.');
                        bests.Add(b);
                    }
                    else
                    {
                        sum += db.Orders.Where(o => o.BuyerEmail == item).Sum(o => o.Price);
                    }
                }
                if (sum > 0)
                {
                    BestBuyers b = new BestBuyers();
                    b.Email = "Другие";
                    b.Sum = sum.ToString().Replace(',', '.');
                    bests.Add(b);
                }
                statisticsMy.bestBuyers = bests;
            }
            catch (Exception ex)
            {
                statisticsMy.bestBuyers = null;
            }

            statistics.forStatisticsMy = statisticsMy;
            ForStatisticsCommon statisticsCommon = new ForStatisticsCommon();
            statisticsCommon.I = 3947;
            DateTime date = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
            statisticsCommon.CommonSum = db.Orders.Where(o => o.BuyingDate > date).Select(o => o.Price).Sum().ToString().Replace(',', '.');
            IEnumerable<QuerysPopular> querys1 = db.QueryHistories.Select(z => new QuerysPopular
            {
                QueryStr = z.QuerySting,
                CountThisSearch = db.QueryHistories.Where(o => o.QuerySting == z.QuerySting).Select(o => o.Id).Count()

            }).OrderByDescending(f => f.CountThisSearch);
            statisticsCommon.SearchWords = querys1.GroupBy(j => j.QueryStr).Take(16).ToList();


            statistics.forStatisticsCommon = statisticsCommon;
            //int asdf = lll.Count();
            return View(statistics);
        }






        public ActionResult StatisticsBuyLike(string orderBy1 = "", string orderBy2 = "", string orderBy3 = "", string tab = "f1", string count="10")//async
        {
            IEnumerable<StatisticsBuyLike> buyLikes;
            string userid = User.Identity.GetUserId();
            StatisticsBuyLike statisticsBuyLike = new StatisticsBuyLike();
            //если есть упорядочивание
            
            
            switch (tab) {
                case "f2":
                    switch (orderBy1){
                        //case "По стоимости":
                        //    buyLikes = db.Orders.Where(i => i.ApplicationUserId == userid).GroupBy(i => i.Price).Select(j => new StatisticsBuyLike
                        //    {
                        //        Var1 = j.Key.ToString(),
                        //        integer = j.Key,
                        //        Id = db.Images.Where(im => im.ApplicationUserId == userid && im.Price == j.Key).Select(im => im.ImageId).FirstOrDefault(),
                        //        Var2 = db.Orders.Where(im => im.ApplicationUserId == userid && im.Price == j.Key).Select(im => im.ImageId).Count().ToString(),
                        //        Var3 = db.Orders.Where(o => o.ApplicationUserId==userid && o.ImageId == j.Key).Select(o => o.OrderId).Count().ToString(),
                        //        Date = db.Orders.Where(o => o.ApplicationUserId == userid && o.ImageId == j.Key).Select(o => o.BuyingDate).OrderByDescending(o => o).FirstOrDefault()
                        //    }).ToList();
                        //    foreach(var item in buyLikes)
                        //    {
                        //        item.Url = "/Content/Content-image/" + item.Id + "_xs.jpg";
                        //    }
                        //    if (orderBy2 == "По убыванию")
                        //    {
                        //        buyLikes = buyLikes.OrderByDescending(i => i.integer);
                        //    }
                        //    else
                        //    {
                        //        buyLikes = buyLikes.OrderBy(i => i.integer);
                        //    }
                        //    ViewBag.th2 = "Стоимость"; ViewBag.th3 = "Количество скачанных картинок"; ViewBag.th4 = "Кол-во скачиваний"; ViewBag.th5 = "Дата посл. покупки";
                        //    ViewBag.count = 5;
                        //    break;

                        case "По дате":
                            var s1 = db.Orders.Where(i => i.ApplicationUserId == userid).GroupBy(i => i.BuyingDate).Select(i => i.Key);
                            List<DateTime> dates = new List<DateTime>();
                            foreach(var item in s1)
                            {
                                if (!dates.Contains(item.Date))
                                    dates.Add(item.Date);    
                            }
                            var jl = dates;
                            List<StatisticsBuyLike> buyLikesList = new List<StatisticsBuyLike>();
                            DateTime small, big;
                            foreach (var item in dates)
                            {
                                StatisticsBuyLike ddd = new Models.StatisticsBuyLike();
                                ddd.Date = item;
                                big = ddd.Date.AddDays(1); small = ddd.Date;
                                ddd.Var1 = item.ToString();

                                ddd.integer = db.Orders.Where(im => im.ApplicationUserId == userid && im.BuyingDate >= small && im.BuyingDate < big).Select(im => im.ImageId).OrderByDescending(im => im).FirstOrDefault();
                                ddd.Var2 = db.Orders.Where(im => im.ApplicationUserId == userid && im.BuyingDate >= small && im.BuyingDate < big).Select(im => im.OrderId).Count().ToString();
                                ddd.Var3 = db.Orders.Where(im => im.ApplicationUserId == userid && im.BuyingDate >= small && im.BuyingDate < big).Select(im => im.Price).Sum().ToString();
                                ddd.Url = "/Content/Content-image/" + ddd.integer.ToString() + "_xs.jpg"; ;
                                buyLikesList.Add(ddd);
                            }
                            buyLikes = buyLikesList;
                            if (orderBy2 == "По убыванию")
                            {
                                buyLikes = buyLikes.OrderByDescending(i => i.Date);
                            }
                            else
                            {
                                buyLikes = buyLikes.OrderBy(i => i.Date);
                            }
                            ViewBag.th2 = "Дата"; ViewBag.th3 = "Кол-во скачиваний"; ViewBag.th4 = "Скачано на сумму"; ViewBag.th5 = "Последнее из них:";
                            ViewBag.count = 3;
                            break;

                        case "По e-mail":
                            buyLikes = db.Orders.Where(i => i.ApplicationUserId == userid).GroupBy(i => i.BuyerEmail).Select(j => new StatisticsBuyLike
                            {
                                Var1 = j.Key.ToString(),
                                Id = db.Orders.Where(o => o.ApplicationUserId == userid && o.BuyerEmail== j.Key).Select(o => o.ImageId).FirstOrDefault(),
                                Var2 = db.Orders.Where(o => o.ApplicationUserId == userid && o.BuyerEmail == j.Key).Select(o => o.ImageId).Count().ToString(),
                                Var3 = db.Orders.Where(o => o.ApplicationUserId == userid && o.BuyerEmail == j.Key).Select(o => o.Price).Sum().ToString(),
                                Date = db.Orders.Where(o => o.ApplicationUserId == userid && o.BuyerEmail == j.Key).Select(o => o.BuyingDate).OrderByDescending(o => o).FirstOrDefault()
                            }).ToList();
                            foreach (var item in buyLikes)
                            {
                                item.Url = "/Content/Content-image/" + item.Id + "_xs.jpg";
                            }
                            if (orderBy2 == "По убыванию")
                            {
                                buyLikes = buyLikes.OrderByDescending(i => i.Var1);
                            }
                            else
                            {
                                buyLikes = buyLikes.OrderBy(i => i.Var1);
                            }
                            ViewBag.th2 = "E-mail"; ViewBag.th3 = "Количество картинок"; ViewBag.th4 = "Кол-во скачиваний"; ViewBag.th5 = "Дата посл. покупки";
                            ViewBag.count = 5;
                            break;

                        default:
                            buyLikes = db.Orders.Where(i => i.ApplicationUserId == userid).GroupBy(i => i.ImageId).Select(j => new StatisticsBuyLike
                            {
                                Id = j.Key,
                                Url = "/Content/Content-image/" + j.Key + "_xs.jpg",
                                Var1 = db.Images.Where(im => im.ImageId == j.Key).Select(im => im.Category).FirstOrDefault(),
                                Var2 = db.Images.Where(im => im.ImageId == j.Key).Select(im => im.Price).FirstOrDefault().ToString(),
                                Var3 = db.Orders.Where(o => o.ImageId == j.Key).Select(o => o.OrderId).Count().ToString(),
                                Date = db.Orders.Where(o => o.ImageId == j.Key).Select(o => o.BuyingDate).OrderByDescending(o => o).FirstOrDefault()
                            }).ToList();
                            if(orderBy2=="По убыванию")
                            {
                                buyLikes = buyLikes.OrderByDescending(i => i.Id);
                            }
                            else
                            {
                                buyLikes = buyLikes.OrderBy(i => i.Id);
                            }
                            ViewBag.th2 = "Категория"; ViewBag.th3 = "Стоимость"; ViewBag.th4 = "Кол-во скачиваний"; ViewBag.th5 = "Дата посл. покупки";
                            ViewBag.count = 5;
                            break;
                    }
                    break;
                case "f3":

                     switch (orderBy3)
                    {
                        case "По ID картинки":
                            List<StatisticsBuyLike> statistics = new List<StatisticsBuyLike>();
                            var s = (from l in db.Likes
                                    join i in db.Images on l.ImageId equals i.ImageId
                                    where i.ApplicationUserId == userid
                                    select l.ImageId).GroupBy(j=>j).ToList();
                            foreach(var item in s)
                            {
                                StatisticsBuyLike st = new StatisticsBuyLike
                                {
                                    Id = item.Key,
                                    Date = db.Likes.Where(l => l.ImageId == item.Key).OrderByDescending(l => l.BuyingDate).Select(l => l.BuyingDate).FirstOrDefault(),
                                    Url = "/Content/Content-image/" + item.Key + "_xs.jpg",
                                    Var2 = db.Likes.Where(l => l.ImageId == item.Key).Select(l => l.LikeId).Count().ToString(),
                                    Var1 = db.Images.Where(l => l.ImageId == item.Key).Select(l => l.Category).FirstOrDefault()
                                };
                                statistics.Add(st);
                            }
                            buyLikes = statistics;
                            ViewBag.th2 = "Категория"; ViewBag.th3 = "Кол-во лайков"; ViewBag.th4 = "Дата посл.лайка";
                            ViewBag.count = 4;

                            //if (orderBy2 == "По убыванию")
                            //{
                            //    buyLikes = buyLikes.OrderByDescending(i => i.Id);
                            //}
                            //else
                            //{
                            //    buyLikes = buyLikes.OrderBy(i => i.Id);
                            //}
                            break;
                        case "По категориям":
                            statistics = new List<StatisticsBuyLike>();
                            var d = db.Likes.Select(j => db.Images.Where(i=>i.ImageId==j.ImageId&& i.ApplicationUserId == userid).Select(i=>i.Category).FirstOrDefault()).GroupBy(j => j).ToList();
                            foreach (var item in d)
                            {
                                if (item.Key != null)
                                {
                                    StatisticsBuyLike st = new StatisticsBuyLike
                                    {
                                        Var1 = item.Key,
                                        Id = (from l in db.Likes
                                             join i in db.Images on l.ImageId equals i.ImageId
                                             where i.Category == item.Key && i.ApplicationUserId == userid
                                             select l.ImageId).FirstOrDefault(),
                                        Var2 = (from l in db.Likes
                                                join i in db.Images on l.ImageId equals i.ImageId
                                                where i.Category == item.Key && i.ApplicationUserId == userid
                                                select l.LikeId).Count().ToString()
                                    };
                                    st.Date = db.Likes.Where(l => l.ImageId == st.Id).Select(l => l.BuyingDate).FirstOrDefault();
                                    st.Url = "/Content/Content-image/" + st.Id + "_xs.jpg";

                                    statistics.Add(st);
                                }
                            }
                            buyLikes = statistics;
                            ViewBag.th2 = "Категория"; ViewBag.th3 = "Кол-во лайков"; ViewBag.th4 = "Дата посл.лайка";
                            ViewBag.count = 4;
                            break;
                        default:
                            buyLikes = (from l in db.Likes
                                        join i in db.Images on l.ImageId equals i.ImageId
                                        where i.ApplicationUserId == userid
                                        select new StatisticsBuyLike
                                        {
                                            Id = l.ImageId,
                                            Url = "/Content/Content-image/" + l.ImageId + "_xs.jpg",
                                            Var1 = db.Images.Where(i => i.ImageId == l.ImageId).Select(i => i.Category).FirstOrDefault(),
                                            Var2 = 1.ToString(),
                                Date = l.BuyingDate
                            }).ToList();
                            //if (orderBy2 == "По убыванию")
                            //{
                            //    buyLikes = buyLikes.OrderByDescending(i => i.Id);
                            //}
                            //else
                            //{
                            //    buyLikes = buyLikes.OrderBy(i => i.Id);
                            //}
                            ViewBag.th2 = "Категория"; ViewBag.th3 = "Дата лайка";
                            ViewBag.count = 2;
                            break;

                    }

                    switch (orderBy1)
                    {
                        case "По категории":
                            if (orderBy2 == "По убыванию")
                                buyLikes = buyLikes.OrderByDescending(j => j.Var1);
                            else buyLikes = buyLikes.OrderBy(j => j.Var1);
                            break;
                        case "По кол-ву лайков":
                            if (orderBy2 == "По убыванию")
                                buyLikes = buyLikes.OrderByDescending(j => Convert.ToInt32(j.Var2));
                            else buyLikes = buyLikes.OrderBy(j => Convert.ToInt32(j.Var2));
                            break;
                        case "По дате посл.лайка":
                            if (orderBy2 == "По убыванию")
                                buyLikes = buyLikes.OrderByDescending(j => j.Date);
                            else buyLikes = buyLikes.OrderBy(j => j.Date);
                            break;
                        default:
                            if (orderBy2 == "По убыванию")
                                buyLikes = buyLikes.OrderByDescending(j => j.Id);
                            else buyLikes = buyLikes.OrderBy(j => j.Id);
                            break;
                    }
                    //ViewBag.count = 4;
                    break;
                default:
                    {
                        buyLikes = db.Orders.Where(o => o.ApplicationUserId == userid).Select(j=>new StatisticsBuyLike
                        {
                            Id = j.ImageId,
                            Url = "/Content/Content-image/" + j.ImageId+"_xs.jpg",
                            Var1 = j.Size.ToString(),
                            Var2 = j.BuyerEmail,
                            Var3 = j.Price.ToString(),
                            Date = j.BuyingDate
                        }).OrderByDescending(o => o.Date);
                        //foreach (var item in buyLikes)
                        //{
                        //    item.Url = "/Content/Content-image/" + item.Id + "_xs.jpg";//так делать нельзя!!! Надо сделать новую модель!!!
                        //}
                        if (orderBy1 != "" || orderBy2 != "")
                        {
                            switch (orderBy1)
                            {
                                case "По стоимости":
                                    if (orderBy2 == "По убыванию")
                                        buyLikes = buyLikes.OrderByDescending(o => o.Var3);
                                    else buyLikes = buyLikes.OrderBy(o => o.Var3);
                                    break;
                                case "По размеру":
                                    if (orderBy2 == "По убыванию")
                                        buyLikes = buyLikes.OrderByDescending(o => o.Var1);
                                    else buyLikes = buyLikes.OrderBy(o => o.Var1);
                                    break;
                                case "По e-mail":
                                    if (orderBy2 == "По убыванию")
                                        buyLikes = buyLikes.OrderByDescending(o => o.Var2);
                                    else buyLikes = buyLikes.OrderBy(o => o.Var2);
                                    break;

                                case "По Id картинки":
                                    if (orderBy2 == "По убыванию")
                                        buyLikes = buyLikes.OrderByDescending(o => o.Id);
                                    else buyLikes = buyLikes.OrderBy(o => o.Id);
                                    break;
                                default:
                                    if (orderBy2 == "По возрастанию")
                                        buyLikes = buyLikes.OrderBy(o => o.Date);
                                    else buyLikes = buyLikes.OrderByDescending(o => o.Date);
                                    break;
                            }
                        }
                        ViewBag.count = 5;
                        ViewBag.th2 = "Размер"; ViewBag.th3 = "E-mail"; ViewBag.th4 = "Стоимость"; ViewBag.th5 = "Дата покупки";
                    }
                    break;
            }
            if (count!="Все")
            {
                buyLikes = buyLikes.Take(Convert.ToInt32(count));
            }
            return PartialView(buyLikes);

        }
        public ActionResult StatisticsDownloaders()//async
        {
            StatisticsDownloaders statisticsDownloaders = new Models.StatisticsDownloaders();

            string userId = User.Identity.GetUserId();
            List<BestBuyers> bests = new List<BestBuyers>();
            try
            {
                List<string> buyersEmail = db.Orders.Where(o => o.ApplicationUserId == userId).GroupBy(o => o.BuyerEmail).OrderBy(o => o.Count()).Select(j => j.Key).ToList();
                List<string> buyersSum = new List<string>();
                List<string> buyersCount = new List<string>();
                //statisticsMy.bestBuyers = buyers.GroupBy(e=>e.Email);
                foreach (var item in buyersEmail)
                {
                    BestBuyers b = new BestBuyers();
                    b.Email = item;
                    b.Sum = db.Orders.Where(o => o.BuyerEmail == item).Sum(o => o.Price).ToString().Replace(',', '.');
                    b.CountD = db.Orders.Where(o => o.BuyerEmail == item).Count();

                    bests.Add(b);
                }
                statisticsDownloaders.bestBuyers = bests;

            }
            catch (Exception ex)
            {
                statisticsDownloaders.bestBuyers = null;
            }
            return PartialView(statisticsDownloaders);
        }
        public ActionResult StatisticsMoney()//async
        {
            return PartialView();
        }


        /*common top modals*/
        public ActionResult BestImCarousel(string typeHD= "heart")//async
        {
            string userId = User.Identity.GetUserId();
            IEnumerable<BestImagesL> bestImagesL;
            if (typeHD == "heart")
            {
                bestImagesL = db.Images.Select(u => new BestImagesL
                {
                    Id = u.ImageId,
                    Category = u.Category,
                    UserId = u.ApplicationUserId,
                    UserName = db.Users.Where(us => us.Id == u.ApplicationUserId).Select(us => us.UserName).FirstOrDefault(),
                    LikesCount = db.Likes.Where(i => i.ImageId == u.ImageId && i.BuyingDate > date).Select(i => i.ImageId).Count()
                }).OrderByDescending(u => u.LikesCount).Take(12).ToList();
                ViewBag.LO = "Лайков за неделю";
                ViewBag.gif = "(по количеству лайков)";
            }
            else
            {
                bestImagesL = db.Images.Select(u => new BestImagesL
                {
                    Id = u.ImageId,
                    Category = u.Category,
                    UserId = u.ApplicationUserId,
                    UserName = db.Users.Where(us => us.Id == u.ApplicationUserId).Select(us => us.UserName).FirstOrDefault(),
                    LikesCount = db.Orders.Where(i => i.ImageId == u.ImageId && i.BuyingDate > date).Select(i => i.ImageId).Count()
                }).OrderByDescending(u => u.LikesCount).Take(12).ToList();
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
            IEnumerable<BestUser> bestUsersLike = db.Users.Select(bestu => new BestUser
            {
                Id = bestu.Id,
                UserName = bestu.UserName,
                Photos = db.Images.Where(i => i.ApplicationUserId == bestu.Id).Select(i => i.ImageId).Count(),
                PhotosAtWeek = db.Images.Where(i => i.ApplicationUserId == bestu.Id && i.Date > date).Select(i => i.ImageId).Count(),
                OrdersAtWeek = db.Orders.Where(o => o.ApplicationUserId == bestu.Id && o.BuyingDate > date).Select(o => o.OrderId).Count(),
                LikesAtWeek = (from l in db.Likes
                               join i in db.Images on l.ImageId equals i.ImageId
                               where i.ApplicationUserId == bestu.Id
                               select l.LikeId).Count()
            }).OrderByDescending(u => u.LikesAtWeek).Take(17).ToList();
            foreach (var item in bestUsersLike)
            {
                if (System.IO.File.Exists(Server.MapPath("~/Content/User-logo/") + item.Id + "_small.jpg"))
                {
                    item.UrlLogo = "/Content/User-logo/" + item.Id + "_small.jpg";
                }
                else item.UrlLogo = "/Content/User-logo/reserve-logo(1).jpg";
            }

            IEnumerable<BestUser> bestUsersDownload = db.Users.Select(bestu => new BestUser
            {
                Id = bestu.Id,
                UserName = bestu.UserName,
                Photos = db.Images.Where(i => i.ApplicationUserId == bestu.Id).Select(i => i.ImageId).Count(),
                PhotosAtWeek = db.Images.Where(i => i.ApplicationUserId == bestu.Id && i.Date > date).Select(i => i.ImageId).Count(),
                OrdersAtWeek = db.Orders.Where(o => o.ApplicationUserId == bestu.Id && o.BuyingDate > date).Select(o => o.OrderId).Count(),
                LikesAtWeek = (from l in db.Likes
                               join i in db.Images on l.ImageId equals i.ImageId
                               where i.ApplicationUserId == bestu.Id
                               select l.LikeId).Count()
            }).OrderByDescending(u => u.OrdersAtWeek).Take(17).ToList();
            foreach (var item in bestUsersDownload)
            {
                if (System.IO.File.Exists(Server.MapPath("~/Content/User-logo/") + item.Id + "_small.jpg"))
                {
                    item.UrlLogo = "/Content/User-logo/" + item.Id + "_small.jpg";
                }
                else item.UrlLogo = "/Content/User-logo/reserve-logo(1).jpg";
            }

            BestAuthors bestAuthors = new BestAuthors();
            bestAuthors.bestUsersLike = bestUsersLike;
            bestAuthors.bestUsersDownload = bestUsersDownload;
            return PartialView(bestAuthors);
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
        //        public ActionResult ChangeProfile(object sender, EventArgs e)

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
                    //get filename
                    string fileName = System.IO.Path.GetFileName(userEdit.logo_file.FileName);
                    //save file

                    string fullPathName = Server.MapPath("~/Content/MidTerm/") + fileName;
                    userEdit.logo_file.SaveAs(fullPathName);

                    string newName = Server.MapPath("~/Content/User-logo/") + User.Identity.GetUserId() + ".jpg";
                    if (System.IO.File.Exists(newName))
                    {
                        System.IO.File.Delete(newName);
                        //System.IO.File.Create(newName);
                        //System.IO.File.Replace(fullPathName, newName, Server.MapPath("~/Content/Rez-copy/") + i.ImageId + ".jpg");
                    }
                    //System.IO.File.Move(fullPathName, newName);


                    //System.Drawing.Image mi = System.Drawing.Image.FromFile(@"C:\Users\SuperUser1\Pictures\1920ивыше\2.jpg");
                    //System.Drawing.Image img = RezizeImage(mi, 103, 32);
                    //img.Save("IMAGELOCATION.png", System.Drawing.Imaging.ImageFormat.Jpeg);
                    System.Drawing.Image imNormal = System.Drawing.Image.FromFile(fullPathName);
                    imNormal = RezizeImage(imNormal, 300);
                    imNormal.Save(newName);
                    imNormal = RezizeImage(imNormal, 80);
                    imNormal.Save(Server.MapPath("~/Content/User-logo/") + User.Identity.GetUserId() + "_small.jpg");
                    System.IO.File.Delete(fullPathName);

                }
                catch (Exception ex)
                {
                    string err = ex.Message;
                }

            }

            string id = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Select(u => u).Where(u => u.Id == id).First();
            user.UserName = userEdit.UserName;
            user.Site = userEdit.Site;
            user.Email = userEdit.Email;
            user.PhoneNumber = userEdit.PhoneNumber;
            user.Status = userEdit.Status;
            user.Additionally = userEdit.Additionally;

            db.SaveChanges();



            string jsdf = userEdit.UserName;
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
            string sasdf = User.Identity.GetUserId().ToString();
            UserAccountEditor applicationUser = db.Users.Select(us => new UserAccountEditor
            {
                Id = us.Id,
                UserName = us.UserName,
                Site = us.Site,
                Email = us.Email,
                PhoneNumber = us.PhoneNumber,
                Additionally = us.Additionally,
                Status = us.Status
            }).Where(u => u.Id == sasdf).FirstOrDefault();
            //if (System.IO.File.Exists(Server.MapPath("~/Content/User-logo/" + applicationUser.Id + ".jpg"))){
                ViewBag.Logo = "/Content/User-logo/" + applicationUser.Id + ".jpg";
            //}
            //else
            //{
            //    ViewBag.Logo = "/Content/User-logo/raccoon.jpg";
            //}
            return View(applicationUser);
        }
        
        //public ActionResult ChangeProfile()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }
        //}



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




        /*сжатие изображений image compress*/
        private System.Drawing.Image RezizeImage(System.Drawing.Image img, int maxWidth, int maxHeight=0)
        {
            if (maxHeight == 0) maxHeight = maxWidth;
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Bitmap cpy = CommonBlock(img, xRatio, yRatio);
                return cpy;
            }
        }
        private System.Drawing.Bitmap CommonBlock(System.Drawing.Image img, double xRatio, double yRatio)
        {
            Double ratio = Math.Min(xRatio, yRatio);

            int nnx = (int)Math.Floor(img.Width / ratio);
            int nny = (int)Math.Floor(img.Height / ratio);
            Bitmap cpy = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
            using (Graphics gr = Graphics.FromImage(cpy))
            {
                gr.Clear(Color.Transparent);

                // This is said to give best quality when resizing images
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                gr.DrawImage(img,
                    new Rectangle(0, 0, nnx, nny),
                    new Rectangle(0, 0, img.Width, img.Height),
                    GraphicsUnit.Pixel);
            }
            return cpy;
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