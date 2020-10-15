using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Racoonogram.Models;


using System.Net;
using System.Net.Mail;

using System.Security.Cryptography;


namespace Racoonogram.Controllers
{
    /**/
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        I_U_Models iumodel = new I_U_Models();
        public ActionResult Index()
        {
            IEnumerable<Image> l = db.Images.Select(i => i).OrderByDescending(i => i.Date).Take(12).ToList();
            GenereteRandomBack();
            foreach (Image i in l)
            {
                i.Url = i.ImageId + "_sm.jpg";
            }
            ViewBag.Images = l;
            IQueryable<Querys> q = (from f in db.QueryHistories
                                   group f by f.QuerySting into j
                                   select new Querys() {  QueryStr = j.Key }).Take(30);

            ViewBag.MinHeight = "padding: 15%;";
            return View(q);
        }

        private void GenereteRandomBack()
        {
            Random r = new Random();
            int rand; /*Image backI;*/
            string[] arrayImages = new string[3];
            int[] l = new int[3] { 0, 0, 0 };
            var bestImages = (from f in db.Likes
                              group f by f.ImageId into j
                              select new
                              {
                                  name = j.Key,
                                  count = db.Likes.Where(i => i.ImageId == j.Key).Select(i => i.LikeId).Count()
                              }).OrderByDescending(j => j.count).Take(15).ToList();
            for (int j = 0; j < 3; j++)
            {
            gotome: rand = r.Next(0, bestImages.Count()-1);
            rand = Convert.ToInt32(bestImages[rand].name);
            if (System.IO.File.Exists(Server.MapPath("~/Content/Content-image/" + rand + ".jpg")))
            {
                    if (l[0] != rand && l[1] != rand)
                    {
                        arrayImages[j] = "/Content/Content-image/" + rand + ".jpg";
                        l[j] = rand;
                    }
                    else goto gotome;
            }
            else arrayImages[j] = "/Content/Images/help-back.jpg";
            
                //if (backI.ApplicationUserId != null)
                //{
                //    ViewBag.BackAuthor = backI.User.UserName;
                //    ViewBag.BackAuthorHref = backI.ApplicationUserId;
                //}
            }
            ViewBag.BackImage = arrayImages;
            //ViewBag.likes = db.Likes.Where(i => i.ImageId == 1).Select(i => i.LikeId).Count();

        }
        public JsonResult Like(int id)
        {
            try
            {
                Like l = new Like
                {
                    BuyingDate = DateTime.Now,
                    ImageId = id
                };
                if (User.Identity.IsAuthenticated)
                {
                    string name = User.Identity.Name;
                    l.UserId = db.Users.Where(u => u.UserName == name).Select(u=>u.Id).FirstOrDefault();
                }
                db.Likes.Add(l);
           
                db.SaveChanges();
                return Json("Yes");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }


        public ActionResult AuthorProfile(string id="")
        {
            // ApplicationUser user = db.Users.Select(u => u).Where(u => u.Id == id).FirstOrDefault();
            AuthorAndAllImages UserAndImages = new AuthorAndAllImages();
            UserAndImages.User = db.Users.Select(u => u).Where(u => u.Id == id).FirstOrDefault();
            IEnumerable<Image> allUserImages = db.Images.Where(i => i.ApplicationUserId == UserAndImages.User.Id).OrderByDescending(i => i.ImageId);
            foreach (Image i in allUserImages)
            {
                i.Url = i.ImageId + "_sm.jpg";
            }
            UserAndImages.ImagesUser = allUserImages.ToList();
            ViewBag.Logo = RenderUserLogo(UserAndImages.User.Id);
            ViewBag.photos = db.Images.Where(i => i.ApplicationUserId == id).Select(i => i.ImageId).Count();
            ViewBag.likes = (from l in db.Likes
                             join i in db.Images on l.ImageId equals i.ImageId
                             where i.ApplicationUserId == id
                             select l.LikeId).Count();
            ViewBag.deal = db.Orders.Where(o => o.ApplicationUserId == id).Select(o => o.OrderId).Count();
            return View(UserAndImages);
        }

        private string RenderUserLogo(string UserId, string size="normal")
        {
            string end;
            if (size == "normal") end  = ".jpg";
            else end = "_small.jpg";

                string exPath = Server.MapPath("~/Content/User-logo/") + UserId + end;
            if (System.IO.File.Exists(exPath))
            {
                return "/Content/User-logo/" + UserId + end;
            }
            else
            {
                Random r = new Random();
                return "/Content/User-logo/reserve-logo(" + r.Next(1,4) + ").jpg";
            }
        }

        public ActionResult AllImUser()
        {
            return PartialView();
            //int  pageSize= 12;
            //try
            //{
            //    IEnumerable<Image> allimages = db.Images.Where(i => i.ApplicationUserId == "cbdbd8dc-eeab-43f3-89a9-123a8c525c5b").ToList();
            //    foreach (Image i in allimages)
            //    {
            //        i.Url = i.ImageId + ".jpg";
            //    }
            //    //IEnumerable<Image> allimages = db.Images.Where(i => i.ApplicationUserId== "cbdbd8dc-eeab-43f3-89a9-123a8c525c5b").ToList();
            //    //IEnumerable<Image> allImagesForPag = allimages.OrderBy(i => i.ImageId).Skip((1 - 1) * pageSize).Take(pageSize);


            //    //if (allimages.Count() <= 0)
            //    //{
            //    //    return RedirectToRoute(new
            //    //    {
            //    //        controller = "Home",
            //    //        action = "NotFound",
            //    //        message = "Изображения данного автора не найдены"
            //    //    });
            //    //}


            //    //foreach (Image i in allImagesForPag)
            //    //{
            //    //    i.Url = i.ImageId + ".jpg";
            //    //}
            //    //PageInfo pageInfo = new PageInfo { PageNumber =1, PageSize = pageSize, TotalItems = allimages.Count() };
            //    //PaginationClass pagclass = new PaginationClass { PageInfoPag = pageInfo, ImagesPag = allImagesForPag };

            //    //return PartialView(pagclass); ;
            //    return PartialView(allimages);
            //}
            //catch (Exception ex)
            //{
            //    return HttpNotFound();
            //}

            //if (allimages.Count() <= 0)
            //{
            //    return RedirectToRoute(new
            //    {
            //        controller = "Home",
            //        action = "NotFound",
            //        message = "Изображения данного автора не найдены"
            //    });
            //}


            //foreach (Image i in allImagesForPag)
            //{
            //    i.Url = i.ImageId + ".jpg";
            //}
            //PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = allimages.Count() };
            //PaginationClass pagclass = new PaginationClass { PageInfoPag = pageInfo, ImagesPag = allImagesForPag };

            //return PartialView(pagclass);
        }
        public ActionResult UserSearch(int page = 1)
        {
            int pageSize = 24;
            //var rolee = db.Roles.Where(r=>r.Id=="2").Select(r => r).FirstOrDefault();

            //IEnumerable<UserSearch> users = db.Users.Where(q=>q.Roles.Contains(rolee)).Select(q => new UserSearch
            //{
            //    Id = q.Id,
            //    UserName = q.UserName,
            //    Site = q.Site,
            //    CountPubl = q.Images.Select(i => i.ImageId).Count(),
            //    CountFollow = (from l in db.Likes
            //                   join i in db.Images on l.ImageId equals i.ImageId
            //                   where i.ApplicationUserId == q.Id
            //                   select l.LikeId).Count()
            //}).OrderByDescending(i => i.UserName).Take(pageSize).ToList();
            IEnumerable<UserSearch> users = db.Users.Select(q => new UserSearch
            {
                Id = q.Id,
                UserName = q.UserName,
                Site = q.Site,
                CountPubl = q.Images.Select(i => i.ImageId).Count(),
                CountFollow = (from l in db.Likes
                               join i in db.Images on l.ImageId equals i.ImageId
                               where i.ApplicationUserId == q.Id
                               select l.LikeId).Count()
            }).Where(q=>q.CountPubl>0).OrderByDescending(i => i.UserName).Take(pageSize).ToList();


            if (users.Count() <= 0)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "NotFound",
                    message = "Авторы не найдены :( Зарегистрируйтесь и станьте первым! :D"
                });
            }
            foreach(UserSearch user in users)
            {
                user.UrlLogo = RenderUserLogo(user.Id,"small");
                user.SiteShort = RenderSiteShort(user.Site);
            }
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = db.Users.Select(i => i.UserName).Count() };
            PaginationClassForUsers pagclass = new PaginationClassForUsers { PageInfoPag = pageInfo, userSearches = users };



            GenereteRandomBack();
            return View(pagclass);
        }

        public static string RenderSiteShort(string Site)
        {
            if (Site != null)
            {
                if (Site.Length <= 30)
                    return Site;
                else return Site.Substring(0, 30) + "...";
            }
            return null;
        }

        [HttpPost]
        public ActionResult UserSearchPartial(string searchstring, string order, int page = 0)
        {
            //if (searchstring == "" || searchstring==" ") {
            //    return RedirectToRoute(new
            //    {
            //        controller = "Home",
            //        action = "NotFound",
            //        message = "Пустая строка запроса"
            //    });  }
            IEnumerable<UserSearch> users;
            int pageSize = 16;
            switch (order)
            {
                case "follow":
                    users = db.Users.Select(q => new UserSearch
                    {
                        Id = q.Id,
                        UserName = q.UserName,
                        Site = q.Site,
                        CountPubl = q.Images.Select(i=>i.ImageId).Count(),
                        CountFollow = (from l in db.Likes
                                       join i in db.Images on l.ImageId equals i.ImageId
                                       where i.ApplicationUserId == q.Id
                                       select l.LikeId).Count()
                    }).OrderByDescending(i => i.CountFollow).Where(i=>i.UserName.Contains(searchstring)).Take(pageSize).ToList();
                    break;
                case "images":
                    users = db.Users.Select(q => new UserSearch
                    {
                        Id = q.Id,
                        UserName = q.UserName,
                        Site = q.Site,
                        CountPubl = q.Images.Select(i => i.ImageId).Count(),
                        CountFollow = (from l in db.Likes
                                       join i in db.Images on l.ImageId equals i.ImageId
                                       where i.ApplicationUserId == q.Id
                                       select l.LikeId).Count()
                    }).OrderByDescending(i => i.CountPubl).Where(i => i.UserName.Contains(searchstring)).Take(pageSize).ToList();
                    break;
                default:
                    users = db.Users.Select(q => new UserSearch
                    {
                        Id = q.Id,
                        UserName = q.UserName,
                        Site = q.Site,
                        CountPubl = q.Images.Select(i => i.ImageId).Count(),
                        CountFollow = (from l in db.Likes
                                       join i in db.Images on l.ImageId equals i.ImageId
                                       where i.ApplicationUserId == q.Id
                                       select l.LikeId).Count()
                    }).OrderBy(i => i.UserName).Where(i => i.UserName.Contains(searchstring)).Take(pageSize).ToList();
                    break;

            }
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
                user.UrlLogo = RenderUserLogo(user.Id, "small");
                user.SiteShort = RenderSiteShort(user.Site);
            }
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = db.Users.Where(i=>i.UserName.Contains(searchstring)).Select(i => i.UserName).Count() };
            PaginationClassForUsers pagclass = new PaginationClassForUsers { PageInfoPag = pageInfo, userSearches = users};


            return PartialView(pagclass);
        }



        public ActionResult PhotoGalery(string category = "", string querys = "", int page = 1)
        {
            GenereteRandomBack();
            ViewBag.MinHeight = "min-height: 200px;padding: 2% 10%;";
            //int pageSize = 12;
            //IEnumerable<Image> imagesPag = db.Images.Select(i => i).OrderBy(i=>i.ImageId).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            //PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = db.Images.Select(i => i).Count() };
            //PaginationClass pagclass = new PaginationClass { PageInfoPag = pageInfo, ImagesPag = imagesPag };
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
            //else
            //{
            //    //var allCategory = db.Images.GroupBy(i => i.Category).Select(i = new { Name = j.Key, Count = j.Count() });
            //    //а тут делаем поиск и получаем блоки категорий
            //    var allCategory = db.Images.GroupBy(i => i.Category).Select(j=>new
            //    {
            //        categ = j.Key,
            //        url = db.Images.Where(im=>im.Category==j.Key).Select(im=>im.ImageId).FirstOrDefault()+"_sm.jpg"
            //    }).ToList();
            //    ViewBag.TitleZ = "Популярные категории";
            //    ViewBag.categories = allCategory;
            //}

            return View();//pagclass
        }
        public ActionResult GetCategories()
        {
            IEnumerable<GetCategories> allCategory = db.Images.GroupBy(i => i.Category).Select(j => new GetCategories
            {
                CategoryName = j.Key,
                Url = db.Images.Where(im => im.Category == j.Key).Select(im => im.ImageId).FirstOrDefault() + "_sm.jpg"
            }).Take(30).ToList();
            return PartialView(allCategory);
        }





        [HttpGet]
        public ActionResult ImagePreview(int id)
        {
            try
            {
                Image image = db.Images.Select(i => i).Where(i => i.ImageId == id).FirstOrDefault();
                // IEnumerable < Image > image = db.Images.Select(i => i).Where(i => i.ImageId == id).Take(1).ToList();
                image.Url = image.ImageId + "_sm.jpg";
                string[] keywords = image.KeyWords.Split(' ');
                ViewBag.keywords = keywords;
                ViewBag.authorLogo = RenderUserLogo(image.ApplicationUserId, "small");
                if (image.Price > 0)
                    ViewBag.NameController = "ImageBuy";
                else ViewBag.NameController = "ImageDownload";
                ViewBag.BigUrl = image.ImageId + "_normal.jpg";
                if (image.Description == "" || image.Description==null) ViewBag.Header = "Image #" + image.ImageId.ToString();
                else if (image.Description.Length > 45) ViewBag.Header = image.Description.Substring(0, 45) + "...";
                else ViewBag.Header = image.Description;

                if (User.Identity.IsAuthenticated)
                {
                    string s = User.Identity.Name;
                    var idAndEmail = db.Users.Where(i => i.UserName == s).Select(i => new
                    {
                        email = i.Email,
                        id = i.Id
                    }).FirstOrDefault();
                    ViewBag.Email = idAndEmail.email;
                    if (image.Price > 0)
                    {
                        ViewBag.hasPlan = db.PlanBuyings.Where(p => p.Id_user == idAndEmail.id && (p.ImageBalance > 0 || p.MoneyBalance > image.Price)).Select(p => p.Id).Count();
                    }
                    else ViewBag.hasPlan = 1;
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
            IEnumerable<Image> images = db.Images.Select(i => i).Where(i => i.Category.Contains(imneed)).OrderByDescending(i => i.ImageId).Except(db.Images.Select(i=>i).Where(i=>i.ImageId==idi)).Take(12).ToList();
            foreach (Image i in images)
            {
                i.Url = "/Content/Content-image/" + i.ImageId + "_sm.jpg";
            }
            ViewBag.CountPages = images.Count();
            return PartialView(images);

        }


        [HttpPost]
        public ActionResult ImageSearch(string keywords, int page = 1, string iscategory = "", string Colors = null, /*string IsBlack = "false",*/ string Orient = null, string OrderBy="", int Count=12)
        {
            IEnumerable<Image> allimages;
            int pageSize = Count;
            //if (keywords == "" ||keywords==" ")
            //{
            //    return RedirectToRoute(new
            //    {
            //        controller = "Home",
            //        action = "NotFound",
            //        message = "Пустая строка запроса"
            //    });
            //}
            if (iscategory == "category")
            {
                if (Colors == "IsBlack")
                {
                    allimages = db.Images.Where(i => i.Category.Contains(keywords)  && i.Orient.Contains(Orient)&&i.IsBlack==true).OrderByDescending(i => i.ImageId).ToList();
                }
                else
                allimages = db.Images.Where(i=>i.Category.Contains(keywords) && i.Colors.Contains(Colors) && i.Orient.Contains(Orient)).OrderByDescending(i => i.ImageId).ToList();
            }
            else
            {
                if (Colors == "IsBlack")
                {
                    allimages = db.Images.Where(i => (i.KeyWords.Contains(keywords)
                                                || i.Category.Contains(keywords)) && i.Orient.Contains(Orient)&&i.IsBlack==true).OrderByDescending(i => i.ImageId).ToList();
                }
                else
                    allimages = db.Images.Where(i => (i.KeyWords.Contains(keywords)
                            || i.Category.Contains(keywords))&&i.Colors.Contains(Colors)&&i.Orient.Contains(Orient)).OrderByDescending(i => i.ImageId).ToList();
            }
            IEnumerable<Image> allImagesForPag;
            switch (OrderBy)
            {
                case "o2":
                    allImagesForPag = allimages.OrderBy(i => i.ImageId).Skip((page - 1) * pageSize).Take(pageSize);
                    break;
                case "o3":
                    allImagesForPag = allimages.OrderBy(i => i.Price).Skip((page - 1) * pageSize).Take(pageSize);
                    break;
                case "o4":
                    allImagesForPag = allimages.OrderByDescending(i => i.Price).Skip((page - 1) * pageSize).Take(pageSize);
                    break;
                default:
                    allImagesForPag = allimages.OrderByDescending(i => i.ImageId).Skip((page - 1) * pageSize).Take(pageSize);
                    break;
            }

            if (allimages.Count() <= 0)
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action="NotFound",
                    message = "Изображения по данному запросу не найдены"
                });
            }

            if (keywords.Length > 2)
            {
                QueryHistory query = new QueryHistory();
                query.QueryDate = DateTime.Now;
                query.QuerySting = keywords;
                db.QueryHistories.Add(query);
                db.SaveChanges();
            }
            foreach (Image i in allImagesForPag)
            {
                i.Url = i.ImageId + "_sm.jpg";
            }
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = allimages.Count() };
            PaginationClass pagclass = new PaginationClass { PageInfoPag = pageInfo, ImagesPag = allImagesForPag };

            return PartialView(pagclass);
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
                var userId = db.Users.Where(u => u.UserName == login).Select(u => u.Id).FirstOrDefault();
                if (userId !=null)
                {
                    /*платежная система*/
                    PlanBuying buying;
                    if (planId.Contains('s'))
                    {
                        buying = db.PlanBuyings.Where(pb => pb.Id_plan.Contains("s") && pb.Id_user == userId).Select(p => p).FirstOrDefault();
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
                            db.PlanBuyings.Remove(buying);
                            db.SaveChanges();

                        }
                        buying.MoneyBalance += db.Plans.Where(p => p.Id == planId).Select(p => p.PlanPrice).FirstOrDefault();
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
                        buying.ImageBalance = db.Plans.Where(p => p.Id == planId).Select(p => p.ImgCount).FirstOrDefault();
                    }
                    db.PlanBuyings.Add(buying);
                    db.SaveChanges();
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
            var j = db.ImgDownloads.Where(imd => imd.Id == Id).Select(imd => imd).FirstOrDefault();
            try
            {
                if (j.DateLast > DateTime.Now)
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