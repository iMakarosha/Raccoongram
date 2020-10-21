using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Racoonogram.Models;
using Racoonogram.Handlers;
using Racoonogram.Services;

using System.Net;
using System.Net.Mail;

using System.Security.Cryptography;


namespace Racoonogram.Controllers
{
    /**/
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.BackImage = new ImageHandler().GenereteRandomBack();
            ViewBag.Images = new HomeHandler().GetMainpageImages();
            IQueryable<Querys> q = new UserService().GetPopularQuerys();
            ViewBag.MinHeight = "padding: 15%;";
            return View(q);
        }
        

        public JsonResult Like(int id)
        {
            try
            {
                new UserHandlers().Like(id, User.Identity.IsAuthenticated ? User.Identity.Name : "");
                return Json("Yes");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult AuthorProfile(string id="")
        {
            AuthorAndAllImages UserAndImages = new AuthorAndAllImages();
            UserAndImages.User = new UserService().GetUser(id);
            IEnumerable<Image> allUserImages = new ImageService().GetImagesByUserId(UserAndImages.User.Id);
            foreach (Image i in allUserImages)
            {
                i.Url = i.ImageId + "_sm.jpg";
            }
            UserAndImages.ImagesUser = allUserImages.ToList();
            ViewBag.Logo = new ImageHandler().RenderUserLogo(UserAndImages.User.Id);
            ViewBag.photos = new ImageService().GetImagesByUserId(UserAndImages.User.Id).Count();
            ViewBag.likes = new ImageService().GetLikesCount(id);
            ViewBag.deal = new UserService().GetOrdersCount(id);
            return View(UserAndImages);
        }

        public ActionResult AllImUser()
        {
            return PartialView();
        }

        public ActionResult UserSearch(int page = 1)
        {
            int pageSize = 24;
            IEnumerable<UserSearch> users = new UserService().GetUsersSearch(pageSize);

            if (users.Count() <= 0)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "NotFound",
                    message = "Авторы не найдены :( Зарегистрируйтесь и станьте первым! :D"
                });
            }
            ViewBag.BackImage = new ImageHandler().GenereteRandomBack();

            foreach (UserSearch user in users)
            {
                user.UrlLogo = new ImageHandler().RenderUserLogo(user.Id,"small");
                user.SiteShort = new ImageHandler().RenderSiteShort(user.Site);
            }
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = new UserService().GetUsersCount() };

            return View(new PaginationClassForUsers { PageInfoPag = pageInfo, userSearches = users });
        }

        [HttpPost]
        public ActionResult UserSearchPartial(string searchstring, string order, int page = 0)
        {
            int pageSize = 16;

            IEnumerable<UserSearch> users = new UserService().GetUsersSearch(searchstring, order, page, pageSize);
            
            if (users.Count() <= 0)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "NotFound",
                    message = "Авторы по данному запросу не найдены"
                });
            }
            foreach(UserSearch user in users)
            {
                user.UrlLogo = new ImageHandler().RenderUserLogo(user.Id, "small");
                user.SiteShort = new ImageHandler().RenderSiteShort(user.Site);
            }
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = new UserService().GetUsersCount(searchstring) };

            return PartialView(new PaginationClassForUsers { PageInfoPag = pageInfo, userSearches = users });
        }

        public ActionResult PhotoGalery(string category = "", string querys = "", int page = 1)
        {
            ViewBag.BackImage = new ImageHandler().GenereteRandomBack();
            ViewBag.MinHeight = "min-height: 200px;padding: 2% 10%;";
            if (category != "")
            {
                ViewBag.GetString = "category";
                ViewBag.StringValue = category;
            }
            else if (querys != "")
            {
                ViewBag.GetString = "querys";
                ViewBag.StringValue = querys;

            }
            return View();
        }

        public ActionResult GetCategories()
        {
            return PartialView(new ImageService().GetCategories().Take(30).ToList());
        }

        [HttpGet]
        public ActionResult ImagePreview(int id)
        {
            try
            {
                Image image = new ImageService().GetImage(id);
                image.Url = image.ImageId + "_sm.jpg";
                ViewBag.keywords = image.KeyWords.Split(' ');
                ViewBag.keywords = image.KeyWords.Split(' ');
                ViewBag.authorLogo = new ImageHandler().RenderUserLogo(image.ApplicationUserId, "small");
                if (image.Price > 0)
                    ViewBag.NameController = "ImageBuy";
                else ViewBag.NameController = "ImageDownload";
                ViewBag.BigUrl = image.ImageId + "_normal.jpg";
                if (image.Description == "" || image.Description==null) ViewBag.Header = "Image #" + image.ImageId.ToString();
                else if (image.Description.Length > 45) ViewBag.Header = image.Description.Substring(0, 45) + "...";
                else ViewBag.Header = image.Description;

                if (User.Identity.IsAuthenticated)
                {
                    UserIdAndEmail idAndEmail = new UserService().GetUserIdAndEmail(User.Identity.Name);
                    ViewBag.Email = idAndEmail.Email;
                    //if (image.Price > 0)
                    //{
                    //    ViewBag.hasPlan = new PlanService().GetPlanRest(idAndEmail.Id, image.Price);
                    //}
                    //else 
                    ViewBag.hasPlan = 1;
                }
                return View(image);
            }
            catch(Exception ex)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "NotFound",
                    message = "Изображение не найдено"
                });
            }
        }
        
        [HttpPost]
        public JsonResult ImageDownload(string email = "")
        {
            return Json("ImageDownload");
        }

        [HttpPost]
        public ActionResult LikeThisImage(string imneed, int idi)
        {
            IEnumerable<Image> images = new ImageService().GetSimilarImages(imneed, idi).Take(12).ToList();
            foreach (Image i in images)
            {
                i.Url = "/Content/Content-image/" + i.ImageId + "_sm.jpg";
            }
            ViewBag.CountPages = images.Count();
            return PartialView(images);
        }

        [HttpPost]
        public ActionResult ImageSearch(string keywords, int page = 1, string iscategory = "", string Colors = null, string Orient = null, string OrderBy="", int pageSize = 12)
        {
            IEnumerable<Image> allimages = new ImageService().GetSearchedImages(keywords, page, iscategory, Colors, Orient).ToList();

            if (allimages.Count() <= 0)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action="NotFound",
                    message = "Изображения по данному запросу не найдены"
                });
            }

            switch (OrderBy)
            {
                case "o2":
                    allimages = allimages.OrderBy(i => i.ImageId).Skip((page - 1) * pageSize).Take(pageSize);
                    break;
                case "o3":
                    allimages = allimages.OrderBy(i => i.Price).Skip((page - 1) * pageSize).Take(pageSize);
                    break;
                case "o4":
                    allimages = allimages.OrderByDescending(i => i.Price).Skip((page - 1) * pageSize).Take(pageSize);
                    break;
                default:
                    allimages = allimages.OrderByDescending(i => i.ImageId).Skip((page - 1) * pageSize).Take(pageSize);
                    break;
            }

            if (keywords.Length > 2)
            {
                new UserService().QueryHistoryAdd(new QueryHistory()
                {
                    QueryDate = DateTime.Now,
                    QuerySting = keywords
                });
            }

            foreach (Image i in allimages)
            {
                i.Url = i.ImageId + "_sm.jpg";
            }

            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = allimages.Count() };

            return PartialView(new PaginationClass { PageInfoPag = pageInfo, ImagesPag = allimages });
        }

        public ActionResult NotFound(string message)
        {
            ViewBag.Message = message;
            return PartialView();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page";
            return View();
        }

        public ActionResult Questions()
        {
            ViewBag.Message = "Questions";
            return View();
        }

        public ActionResult Agreement()
        {
            ViewBag.Message = "Agreement";
            return View();
        }

        public ActionResult License()
        {
            ViewBag.Message = "License";
            return View();
        }

        public ActionResult BuyPlan()
        {
            ViewBag.Message = "Приобрести план";
            return View();
        }

        [HttpGet]
        public ActionResult BuyPlanForm(string radio_val = "s1")
        {
            return GetPlanPage(radio_val);
        }

        private ActionResult GetPlanPage(string radio_val)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("user"))
            {
                switch (radio_val)
                {
                    case "s2":
                        ViewBag.Price = 25;
                        break;
                    case "s3":
                        ViewBag.Price = 50;
                        break;
                    case "p1":
                        ViewBag.Price = 49;
                        break;
                    case "p2":
                        ViewBag.Price = 99;
                        break;
                    case "p3":
                        ViewBag.Price = 179;
                        break;
                    default:
                        ViewBag.Price = 10;
                        break;
                }
                ViewBag.Message = "Приобрести план";
                ViewBag.PlanId = radio_val;
                ViewBag.PlanName = radio_val;
                return View();
            }
            else
            {
                return RedirectToAction("Register", "Account");
            }
        }

        [HttpPost]
        public ActionResult BuyPlanForm(string planId, int price=10, string login="", string nomerC="", string dataC ="", string radio_val="")
        {

            if (login == ""||nomerC=="" || dataC=="")
            {
                ModelState.AddModelError("login", "Необходимо заполнить все поля!");
                return GetPlanPage(planId);
            }
            else
            {
                var userId = new UserService().GetUserID(login);
                if (userId !=null)
                {
                    /*платежная система*/
                    new UserHandlers().BuyPlan(userId, planId);
                }
                else
                {
                    ModelState.AddModelError("login", "Введенного Вами логина не существует в базе!");
                    return GetPlanPage(planId);
                }
                return RedirectToAction("Index","Manage");
            }
        }

        public ActionResult GetImg(string Id)
        {
            ViewBag.Message = "Приобрести план";
            var plan = new ImageService().GetImageDownload(Id);
            try
            {
                if (plan.DateLast > DateTime.Now)
                {
                    ViewBag.ID = Id + ".jpg";
                    return View();
                }
                else
                {
                    ViewBag.ISIMG = "Время действия ссылки истекло! Получите письмо с новой ссылкой в личном кабинете";
                    ViewBag.ID = "no";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ISIMG = "Время действия ссылки истекло! Получите письмо с новой ссылкой в личном кабинете";
                ViewBag.ID = "no";
                return View();
            }
        }
    }
}