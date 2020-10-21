using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using Racoonogram.Models;
using Racoonogram.Services;
using System.Web;

namespace Racoonogram.Handlers
{
    public class ImageHandler
    {
        public ImageHandler() { }

        public string[] GenereteRandomBack()
        {
            Random r = new Random();
            int rand; /*Image backI;*/
            string[] arrayImages = new string[3];
            int[] l = new int[3] { 0, 0, 0 };
            List<BestImagesCount> bestImages = new ImageService().GetBestImagesCounts().ToList();
            for (int j = 0; j < 3; j++)
            {
            gotome: rand = r.Next(0, bestImages.Count() - 1);
                rand = Convert.ToInt32(bestImages[rand].Name);
                if (System.IO.File.Exists(GetServerPath("~/Content/Content-image/" + rand + ".jpg")))
                {
                    if (l[0] != rand && l[1] != rand)
                    {
                        arrayImages[j] = "/Content/Content-image/" + rand + ".jpg";
                        l[j] = rand;
                    }
                    else goto gotome;
                }
                else arrayImages[j] = "/Content/Images/help-back.jpg";

            }
            return arrayImages;
        }

        public void UploadImage(System.Web.HttpFileCollectionBase files, Racoonogram.Models.Image i)
        {
            string fullPath = GetServerPath("~/Content/Content-image/");
            foreach (string file in files)
            {
                var upload = files[file];
                if (upload != null)
                {
                    System.Drawing.Image waterMarkIm;
                    System.Drawing.Bitmap bit;
                    try
                    {
                        //get filename
                        string fileName = System.IO.Path.GetFileName(upload.FileName);
                        //save file
                        string fullPathName = GetServerPath("~/Content/MidTerm/") + fileName;
                        upload.SaveAs(fullPathName);
                        new ImageService().AddImage(i);
                        string newName = fullPath + i.ImageId + ".jpg";
                        if (System.IO.File.Exists(newName))
                        {
                            System.IO.File.Delete(newName);
                        }

                        System.Drawing.Image imNormal = System.Drawing.Image.FromFile(fullPathName);
                        imNormal.Save(newName);

                        imNormal = new ImageHandler().ResizeImage(imNormal, 1920, 1920);

                        /*ДОБАВЛЕНИЕ ВОДЯНОГО ЗНАКА*/
                        if (imNormal.Width > imNormal.Height)
                        {
                            waterMarkIm = System.Drawing.Image.FromFile(GetServerPath("~/Content/Images/") + "Watermark_.png");
                        }
                        else if (imNormal.Width < imNormal.Height)
                        {
                            waterMarkIm = System.Drawing.Image.FromFile(GetServerPath("~/Content/Images/") + "Watermark__.png");
                        }
                        else
                        {
                            waterMarkIm = System.Drawing.Image.FromFile(GetServerPath("~/Content/Images/") + "Watermark___.png");
                        }
                        int h = imNormal.Height;
                        int w = imNormal.Width;
                        bit = new Bitmap(w, h);
                        using (Graphics g = Graphics.FromImage(bit))
                        {
                            g.DrawImage(imNormal, 0, 0, w, h);
                            g.DrawImage(waterMarkIm, 10, ((imNormal.Height - waterMarkIm.Height) / 2), waterMarkIm.Width, waterMarkIm.Height);
                        }

                        bit.Save(fullPath + i.ImageId + "_normal.jpg");

                        System.Drawing.Image imNormalSmall = new ImageHandler().ResizeImage(bit, 400, 240);
                        imNormalSmall.Save(fullPath + i.ImageId + "_sm.jpg");

                        imNormalSmall = new ImageHandler().ResizeImage(imNormalSmall, 200, 40);
                        imNormalSmall.Save(fullPath + i.ImageId + "_xs.jpg");
                        imNormal = null;
                        System.IO.File.Delete(fullPathName);
                    }
                    catch (Exception ex)
                    {
                        string exeption = ex.Message;
                    }
                }
            }
        }

        public void DeleteImage(int imageId)
        {

            Models.Image im = new ImageService().GetImage(imageId);
            if (im != null)
                new ImageService().DeleteImage(imageId);
            if (System.IO.File.Exists(GetServerPath("~/Content/Content-image/" + imageId + ".jpg")))
            {
                System.IO.File.Delete(GetServerPath("~/Content/Content-image/") + imageId + "_normal.jpg");
                System.IO.File.Delete(GetServerPath("~/Content/Content-image/") + imageId + "_sm.jpg");
            }
        }

        public void UpdateUserLogo(string userId, System.Web.HttpPostedFileBase userLogoFile )
        {
            string fileName = System.IO.Path.GetFileName(userLogoFile.FileName);
            //save file

            string fullPathName = GetServerPath("~/Content/MidTerm/") + fileName;
            userLogoFile.SaveAs(fullPathName);

            string newName = GetServerPath("~/Content/User-logo/") + userId + ".jpg";
            if (System.IO.File.Exists(newName))
            {
                System.IO.File.Delete(newName);
            }
            System.Drawing.Image imNormal = System.Drawing.Image.FromFile(fullPathName);
            imNormal = new ImageHandler().ResizeImage(imNormal, 300);
            imNormal.Save(newName);
            imNormal = new ImageHandler().ResizeImage(imNormal, 80);
            imNormal.Save(GetServerPath("~/Content/User-logo/") + userId + "_small.jpg");

            System.IO.File.Delete(fullPathName);
        }

        public System.Drawing.Image ResizeImage(System.Drawing.Image img, int maxWidth, int maxHeight = 0)
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

        public System.Drawing.Bitmap CommonBlock(System.Drawing.Image img, double xRatio, double yRatio)
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
        
        public string GetFile(int ImageId, int size)
        {
            try
            {
                System.Drawing.Image imNormal = System.Drawing.Image.FromFile(GetServerPath("/Content/Content-image/" + ImageId + ".jpg"));
                System.Drawing.Image iii;

                Double xRatio = (double)imNormal.Width / size;
                Double yRatio = (double)imNormal.Height / size;
                Double ratio = Math.Max(xRatio, yRatio);
                int nnx = (int)Math.Floor(imNormal.Width / ratio);
                int nny = (int)Math.Floor(imNormal.Height / ratio);
                var destRect = new System.Drawing.Rectangle(0, 0, nnx, nny);
                var destImage = new System.Drawing.Bitmap(nnx, nny);
                destImage.SetResolution(imNormal.HorizontalResolution, imNormal.VerticalResolution);
                using (var grapgics = System.Drawing.Graphics.FromImage(destImage))
                {
                    grapgics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    grapgics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    grapgics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    grapgics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    grapgics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                    using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                    {
                        wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                        grapgics.DrawImage(imNormal, destRect, 0, 0, imNormal.Width, imNormal.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                    }
                }
                iii = destImage;

                string name = "Raccoonogram_im_" + ImageId + "_" + size + "_" + DateTime.Now;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                using (System.Security.Cryptography.MD5 md5hash = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(name);
                    byte[] hash = md5hash.ComputeHash(inputBytes);
                    for (int i = 0; i < hash.Length; i++)
                    {
                        sb.Append(hash[i].ToString("X2"));
                    }
                }

                string file_name = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/downloads/") + sb.ToString() + ".jpg";
                iii.Save(file_name);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string GetServerPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }

        public bool IsImgExists(string imgName)
        {
            return (System.IO.File.Exists(new ImageHandler().GetServerPath("~/Content/User-logo/") + imgName));
        }

        public string RenderSiteShort(string Site)
        {
            if (Site != null)
            {
                if (Site.Length <= 30)
                    return Site;
                else return Site.Substring(0, 30) + "...";
            }
            return null;
        }

        public string RenderUserLogo(string UserId, string size = "normal")
        {
            string end;
            if (size == "normal")
            {
                end = ".jpg";
            }
            else
            {
                end = "_small.jpg";
            }
            string exPath = GetServerPath("~/Content/User-logo/" + UserId + end);
            if (System.IO.File.Exists(exPath))
            {
                return "/Content/User-logo/" + UserId + end;
            }
            else
            {
                Random r = new Random();
                return "/Content/User-logo/reserve-logo(" + r.Next(1, 4) + ").jpg";
            }
        }
    }
}