using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Racoonogram.Models;


namespace Racoonogram.Services
{
    public class ImageService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        #region Image

        public void AddImage(Image image)
        {
            db.Images.Add(image);
            db.SaveChanges();
        }

        public Image GetImage(int? imageId)
        {
            return db.Images.Find(imageId);
        }

        public double GetImagePrice(int imageId)
        {
            return db.Images.Where(i => i.ImageId == imageId).Select(i => i.Price).First();
        }

        public ImageUnload GetImageUnload(int imageId)
        {
            return db.Images.Select(u => new ImageUnload
            {
                Id = u.ImageId,
                Category = u.Category,
                KeyWords = u.KeyWords,
                Description = u.Description,
                Colors = u.Colors,
                Price = u.Price
            }).Where(i => i.Id == imageId).FirstOrDefault();
        }

        public IQueryable<Image> GetImages()
        {
            return db.Images.Include(i => i.User);
        }

        public IEnumerable<Image> GetImages(int count)
        {
            return db.Images.Select(i => i).OrderByDescending(i => i.Date).Take(count).ToList();
        }

        public IQueryable<Image> GetImagesByUserId(string userId)
        {
            return db.Images.Where(i => i.ApplicationUserId == userId).OrderByDescending(i => i.ImageId);
        }

        public IQueryable<Image> GetSearchedImages(string keywords, int page = 1, string iscategory = "", string Colors = null, string Orient = null)
        {
            IQueryable<Image> images;
            switch (iscategory)
            {
                case "category":
                    if (Colors == "IsBlack")
                    {
                        images = db.Images.Where(i => i.Category.Contains(keywords) && i.Orient.Contains(Orient) && i.IsBlack == true);
                    }
                    else
                        images = db.Images.Where(i => i.Category.Contains(keywords) && i.Colors.Contains(Colors) && i.Orient.Contains(Orient));
                    break;
                default:
                    if (Colors == "IsBlack")
                    {
                        images = db.Images.Where(i => (i.KeyWords.Contains(keywords)
                                                    || i.Category.Contains(keywords)) && i.Orient.Contains(Orient) && i.IsBlack == true);
                    }
                    else
                        images = db.Images.Where(i => (i.KeyWords.Contains(keywords)
                                || i.Category.Contains(keywords)) && i.Colors.Contains(Colors) && i.Orient.Contains(Orient));
                    break;
            }
            return images.OrderByDescending(i => i.ImageId);
        }

        public IQueryable<Image> GetSimilarImages(string categorySbstr, int imageId)
        {
            return db.Images.Select(i => i).Where(i => i.Category.Contains(categorySbstr)).OrderByDescending(i => i.ImageId).Except(db.Images.Select(i => i).Where(i => i.ImageId == imageId));
        }

        public IQueryable<BestImagesL> GetBestImages(DateTime date, string typeHD, int count)
        {
            if (typeHD == "heart")
            {
                return db.Images.Select(u => new BestImagesL
                {
                    Id = u.ImageId,
                    Category = u.Category,
                    UserId = u.ApplicationUserId,
                    UserName = db.Users.Where(us => us.Id == u.ApplicationUserId).Select(us => us.UserName).FirstOrDefault(),
                    LikesCount = db.Orders.Where(i => i.ImageId == u.ImageId && i.BuyingDate > date).Select(i => i.ImageId).Count()
                }).OrderByDescending(u => u.LikesCount).Take(count);
            }
            else
            {
                return db.Images.Select(u => new BestImagesL
                {
                    Id = u.ImageId,
                    Category = u.Category,
                    UserId = u.ApplicationUserId,
                    UserName = db.Users.Where(us => us.Id == u.ApplicationUserId).Select(us => us.UserName).FirstOrDefault(),
                    LikesCount = db.Likes.Where(i => i.ImageId == u.ImageId && i.BuyingDate > date).Select(i => i.ImageId).Count() 
                }).OrderByDescending(u => u.LikesCount).Take(count);
            }
        }

        public IQueryable<BestImagesL> GetImagesForUserProfile(string typeFor = "news", int takeCount = 8, int skip = 0)
        {
            switch (typeFor)
            {
                case "news":
                    return db.Images.OrderByDescending(i => i.Date).Select(i => new BestImagesL
                    {
                        Id = i.ImageId,
                        Url = i.ImageId + "_sm.jpg",

                    }).Skip(skip).Take(takeCount);
                default:
                    return db.Images.Where(i => i.Price == 0).Select(i => new BestImagesL
                    {
                        Id = i.ImageId,
                        Url = i.ImageId + "_sm.jpg",

                    }).OrderByDescending(i => i.Id).Take(takeCount);
            }
        }

        public IQueryable<BestImagesCount> GetBestImagesCounts()
        {
            return (from f in db.Likes
                    group f by f.ImageId into j
                    select new BestImagesCount
                    {
                        Name = j.Key,
                        Count = db.Likes.Where(i => i.ImageId == j.Key).Select(i => i.LikeId).Count()
                    }).OrderByDescending(j => j.Count).Take(15);
        }

        public void SaveImage(ImageUnload image)
        {
            Racoonogram.Models.Image imEdit = GetImage(image.Id);
            imEdit.Category = image.Category.ToLower();
            imEdit.KeyWords = image.KeyWords.ToLower();
            if (!String.IsNullOrEmpty(image.Description))
            {
                imEdit.Description = image.Description.Substring(0, 1).ToUpper() + image.Description.Substring(1);
            }
            if (!String.IsNullOrEmpty(image.Colors))
            {
                imEdit.Colors = image.Colors.ToLower();
            }
            imEdit.Price = image.Price;
            db.SaveChanges();
        }

        public void ModifyImage(Image image)
        {
            db.Entry(image).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void DeleteImage(int imageId)
        {
            Image image = db.Images.Find(imageId);
            db.Images.Remove(image);
            db.SaveChanges();
        }

        public void HideImage(int id)
        {
            var img = db.Orders.Where(o => o.OrderId == id).Select(o => o).FirstOrDefault();
            img.IsHide = 1;
            db.SaveChanges();
        }

        public void DeleteImage(Image image)
        {
            db.Images.Remove(image);
            db.SaveChanges();
        }

        #endregion

        #region ImageDownload

        public void ImageDownloadAdd(ImgDownload imgDownload)
        {
            db.ImgDownloads.Add(imgDownload);
            db.SaveChanges();
        }

        public ImgDownload GetImageDownload(string imageId)
        {
            return db.ImgDownloads.Where(imd => imd.Id == imageId).Select(imd => imd).FirstOrDefault(); ;
        }

        #endregion

        #region Category

        public IQueryable<GetCategories> GetCategories()
        {
            return db.Images.GroupBy(i => i.Category).Select(j => new GetCategories
            {
                CategoryName = j.Key,
                Url = db.Images.Where(im => im.Category == j.Key).Select(im => im.ImageId).FirstOrDefault() + "_sm.jpg"
            });
        }

        public IQueryable<CategoryListItem> GetCategoryListItems()
        {
            return db.Images.GroupBy(i => i.Category).Select(group => new CategoryListItem { CategoryName = group.Key });
        }

        #endregion

        #region Like

        public void LikeAdd(Like like)
        {
            db.Likes.Add(like);
            db.SaveChanges();
        }

        public int GetLikesCount(string userId)
        {
            return (from l in db.Likes
                    join i in db.Images on l.ImageId equals i.ImageId
                    where i.ApplicationUserId == userId
                    select l.LikeId).Count();
        }

        public IQueryable<Liking> GetUserLikes(string userId)
        {
            return (from like in db.Likes
                    join img in db.Images on like.ImageId equals img.ImageId
                    where like.UserId == userId
                    group like by like.ImageId into k
                    select new Liking
                    {
                        ImageId = k.Key,
                        LikeDate = db.Likes.Where(l => l.ImageId == k.Key).Select(l => l.BuyingDate).OrderByDescending(l => l).FirstOrDefault(),
                        Url = k.Key + "_xs.jpg"
                    }).OrderByDescending(k => k.LikeDate).Take(12);
        }

        public int GetUserLikesCount(string userId)
        {
            return (from l in db.Likes
                    join i in db.Images on l.ImageId equals i.ImageId
                    where i.ApplicationUserId == userId
                    select l.LikeId).Count();
        }

        #endregion

        public void Dispose()
        {
            db.Dispose();
        }
    }
}