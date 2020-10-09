using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Racoonogram.Models;



namespace Racoonogram.Controllers
{
    /**/
    [RequireHttps]
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
            IQueryable<Querys> q = from f in db.QueryHistories
                                   group f by f.QuerySting into j
                                   select new Querys() {  QueryStr = j.Key };

            ViewBag.MinHeight = "min-height: 600px;padding: 15%;";
            return View(q);
        }

        private void GenereteRandomBack()
        {
            Random r = new Random();
            int rand; Image backI;
            do
            {
                rand = r.Next(1, db.Images.Select(i => i.ImageId).Count());
                backI = db.Images.Select(i => i).Where(i => i.ImageId == rand).FirstOrDefault();
            }
            while (backI == null);

            if (System.IO.File.Exists(Server.MapPath("~/Content/Content-image/" + backI.ImageId + ".jpg")))
            {
                ViewBag.BackImage = "/Content/Content-image/" + backI.ImageId + ".jpg";
            }
            else ViewBag.BackImage = "/Content/Images/help-back.jpg";
            if (backI.ApplicationUserId != null)
            {
                ViewBag.BackAuthor = backI.User.UserName;
                ViewBag.BackAuthorHref = backI.ApplicationUserId;
            }
            ViewBag.likes = db.Likes.Where(i => i.ImageId == backI.ImageId).Select(i => i.LikeId).Count();
            
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
            }).OrderByDescending(i => i.UserName).Take(pageSize).ToList();

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
                if (Site.Length <= 40)
                    return Site;
                else return Site.Substring(0, 40) + "...";
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
            Image image = db.Images.Select(i => i).Where(i => i.ImageId == id).FirstOrDefault();
            // IEnumerable < Image > image = db.Images.Select(i => i).Where(i => i.ImageId == id).Take(1).ToList();
            image.Url = image.ImageId + "_sm.jpg";
            string[] keywords = image.KeyWords.Split(' ');
            ViewBag.keywords = keywords;


            if (image.Price > 0)
                ViewBag.NameController = "ImageBuy";
            else ViewBag.NameController = "ImageDownload";

            if (User.Identity.IsAuthenticated) {
                string s = User.Identity.Name;
                ViewBag.Email = db.Users.Where(i => i.UserName == s).Select(i => i.Email).FirstOrDefault();
            }
            return View(image);
        }
        [HttpPost]
        public JsonResult ImageBuy(string email, string optradio, int ImageId, string ApplicationUserId)
        {
            if (email == null || email == "")
                return Json("<b style='color:red'>Поле Email обязательно для заполнения</b>");
            if (optradio == null || optradio == "")
                return Json("<b style='color:red'>Выберите размер загружаемой фотографии</b>");
            int size = Convert.ToInt32(optradio.Substring(0, optradio.Length - 2));
            double price = db.Images.Where(i => i.ImageId == ImageId).Select(i => i.Price).First();
            Order order = new Order
            {
                ImageId = ImageId,
                ApplicationUserId = ApplicationUserId,
                BuyerEmail = email,
                Size = size,
                Price = price,
                BuyingDate = DateTime.Now
            };
            db.Orders.Add(order);
            db.SaveChanges();
            //string path = Server.MapPath("/Content/Content-image/" + ImageId + ".jpg");
            //byte[] mas = System.IO.File.ReadAllBytes(path);
            //string file_type = "application/jpg";
            //string file_name = "Raccoonogram_im.jpg";
            //return File(mas, file_type, file_name);
            //GetFile(ImageId);

            return Json(size);
        }

        public FileResult GetFile(int ImageId, int size)
        {
            string path = Server.MapPath("/Content/Content-image/" + ImageId + ".jpg");
            System.Drawing.Image imNormal = System.Drawing.Image.FromFile(path);




            System.Drawing.ImageConverter _imageConverter = new System.Drawing.ImageConverter();
            byte[] mas;
            if (!(imNormal.Height < size && imNormal.Width < size))
            {
                using (imNormal)
                {
                    Double xRatio = (double)imNormal.Width / size;
                    Double yRatio = (double)imNormal.Height / size;
                    Double ratio = Math.Max(xRatio, yRatio);

                    int nnx = (int)Math.Floor(imNormal.Width / ratio);
                    int nny = (int)Math.Floor(imNormal.Height / ratio);
                    System.Drawing.Bitmap cpy = new System.Drawing.Bitmap(nnx, nny, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(cpy))
                    {
                        gr.Clear(System.Drawing.Color.Transparent);

                        // This is said to give best quality when resizing images
                        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        gr.DrawImage(imNormal,
                            new System.Drawing.Rectangle(0, 0, nnx, nny),
                            new System.Drawing.Rectangle(0, 0, imNormal.Width, imNormal.Height),
                            System.Drawing.GraphicsUnit.Pixel);
                    }
                    mas = (byte[])_imageConverter.ConvertTo(cpy, typeof(byte[]));
                    //return cpy;
                }
            }

            //byte[] xByte = (byte[])_imageConverter.ConvertTo(imNormal, typeof(byte[]));
            //byte[] mas = System.IO.File.ReadAllBytes(path);
            else
            {
                mas = (byte[])_imageConverter.ConvertTo(imNormal, typeof(byte[]));
            }




            string file_type = "application/jpg";
            string file_name = "Raccoonogram_im_"+ImageId+"_"+size+".jpg";
            return File(mas, file_type, file_name);
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
        public ActionResult ImageSearch(string keywords, int page = 1, string iscategory="")
        {
            IEnumerable<Image> allimages;
            int pageSize = 12;
            if (keywords == "" ||keywords==" ")
            {
                return RedirectToRoute(new
                {
                    controller = "Home",
                    action = "NotFound",
                    message = "Пустая строка запроса"
                });
            }
            if (iscategory == "category")
            {
                allimages = db.Images.Where(i=>i.Category.Contains(keywords)).OrderByDescending(i => i.ImageId).ToList();
            }
            else
            {
               allimages = db.Images.Where(i => i.KeyWords.Contains(keywords)
                            || i.Category.Contains(keywords)).OrderByDescending(i => i.ImageId).ToList();
            }
            IEnumerable<Image> allImagesForPag = allimages.OrderByDescending(i => i.ImageId).Skip((page - 1) * pageSize).Take(pageSize);
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
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult Questions()
        {
            ViewBag.Message = "Questions.";

            return View();
        }
    }
}