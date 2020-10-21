using System;
using System.Collections.Generic;
using System.Linq;
using Racoonogram.Models;
using Racoonogram.Services;

namespace Racoonogram.Handlers
{
    public class HomeHandler
    {
        public HomeHandler() { }

        public IEnumerable<Image> GetMainpageImages()
        {
            IEnumerable<Image> l = new ImageService().GetImages(12).ToList();
            foreach (Image i in l)
            {
                i.Url = i.ImageId + "_sm.jpg";
            }
            return l;
        }
    }
}