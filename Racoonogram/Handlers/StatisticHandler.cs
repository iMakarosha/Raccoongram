using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Racoonogram.Models;
using Racoonogram.Handlers;
using Racoonogram.Services;


namespace Racoonogram.Handlers
{
    public class StatisticHandler
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public StatisticHandler() { }

        public ForStatistics GetStatistic(string userId)
        {
            List<BestBuyers> bests;
            ForStatisticsMy statisticsMy = new ForStatisticsMy();
            try
            {
                statisticsMy.CountDownloads = new UserService().GetOrdersCount(userId);
            }
            catch (Exception ex)
            {
                statisticsMy.CountDownloads = 0;
            }

            try
            {
                statisticsMy.CountLikes = new ImageService().GetUserLikesCount(userId);
            }
            catch (Exception ex)
            {
                statisticsMy.CountLikes = 0;
            }

            try
            {
                statisticsMy.SumTotal = new UserService().GetOrderSum(userId).ToString();
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
                List<string> buyersEmail = new UserService().GetBuyerEmail(userId);
                List<string> buyersSum = new List<string>();
                int q = 0;
                double sum = 0;
                foreach (var item in buyersEmail)
                {
                    q++;
                    if (q < 4)
                    {
                        BestBuyers b = new BestBuyers();
                        b.Email = item;
                        b.Sum = new UserService().GetOrderSum(item, "email").ToString().Replace(',', '.');
                        bests.Add(b);
                    }
                    else
                    {
                        sum += new UserService().GetOrderSum(item, "email");
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

            ForStatisticsCommon statisticsCommon = new ForStatisticsCommon();
            statisticsCommon.I = 3947;
            DateTime date = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));

            try
            {
                var it = new UserService().GetOrderSum(type: "email", date: date);
                statisticsCommon.CommonSum = it.ToString().Replace(',', '.');
            }
            catch (Exception) { statisticsCommon.CommonSum = "13.39"; }
            IEnumerable<QuerysPopular> querys1 = new UserService().GetQuerysPopularsAll();
            statisticsCommon.SearchWords = querys1.GroupBy(j => j.QueryStr).Take(16).ToList();

            return new ForStatistics()
            {
                forStatisticsMy = statisticsMy,
                forStatisticsCommon = statisticsCommon
            };
        }

        public BestAuthors GetStatisticAutors(DateTime date)
        {
            IEnumerable<BestUser> bestUsersLike = new UserService().GetBestUsers(date);
            foreach (var item in bestUsersLike)
            {
                if (new ImageHandler().IsImgExists(item.Id + "_small.jpg"))
                {
                    item.UrlLogo = "/Content/User-logo/" + item.Id + "_small.jpg";
                }
                else item.UrlLogo = "/Content/User-logo/reserve-logo(1).jpg";
            }

            IEnumerable<BestUser> bestUsersDownload = new UserService().GetBestUsers(date);
            foreach (var item in bestUsersDownload)
            {
                if (new ImageHandler().IsImgExists(item.Id + "_small.jpg"))
                {
                    item.UrlLogo = "/Content/User-logo/" + item.Id + "_small.jpg";
                }
                else item.UrlLogo = "/Content/User-logo/reserve-logo(1).jpg";
            }

            return new BestAuthors()
            {
                bestUsersLike = bestUsersLike,
                bestUsersDownload = bestUsersDownload
            };
        }

        public StatisticsDownloaders GetStatisticDownloaders(string userId)
        {
            StatisticsDownloaders statisticsDownloaders = new Models.StatisticsDownloaders();

            List<BestBuyers> bests = new List<BestBuyers>();
            try
            {
                List<string> buyersEmail = new UserService().GetBuyerEmail(userId);
                List<string> buyersSum = new List<string>();
                List<string> buyersCount = new List<string>();
                foreach (var item in buyersEmail)
                {
                    BestBuyers b = new BestBuyers();
                    b.Email = item;
                    b.Sum = new UserService().GetOrderSum(item, "email").ToString().Replace(',', '.');
                    b.CountD = new UserService().GetOrdersCount(item, "email");

                    bests.Add(b);
                }
                statisticsDownloaders.bestBuyers = bests;

            }
            catch (Exception ex)
            {
                statisticsDownloaders.bestBuyers = null;
            }
            return statisticsDownloaders;
        }

        public IEnumerable<StatisticsBuyLike> GetStatisticBuyLikes(string userid, string orderBy1 = "", string orderBy2 = "", string orderBy3 = "", string tab = "f1", string count = "10")
        {
            IEnumerable<StatisticsBuyLike> buyLikes;
            switch (tab)
            {
                case "f2":
                    switch (orderBy1)
                    {
                        case "По дате":
                            var s1 = db.Orders.Where(i => i.ApplicationUserId == userid).GroupBy(i => i.BuyingDate).Select(i => i.Key);
                            List<DateTime> dates = new List<DateTime>();
                            foreach (var item in s1)
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
                            break;

                        case "По e-mail":
                            buyLikes = db.Orders.Where(i => i.ApplicationUserId == userid).GroupBy(i => i.BuyerEmail).Select(j => new StatisticsBuyLike
                            {
                                Var1 = j.Key.ToString(),
                                Id = db.Orders.Where(o => o.ApplicationUserId == userid && o.BuyerEmail == j.Key).Select(o => o.ImageId).FirstOrDefault(),
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
                            if (orderBy2 == "По убыванию")
                            {
                                buyLikes = buyLikes.OrderByDescending(i => i.Id);
                            }
                            else
                            {
                                buyLikes = buyLikes.OrderBy(i => i.Id);
                            }
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
                                     select l.ImageId).GroupBy(j => j).ToList();
                            foreach (var item in s)
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
                            break;
                        case "По категориям":
                            statistics = new List<StatisticsBuyLike>();
                            var d = db.Likes.Select(j => db.Images.Where(i => i.ImageId == j.ImageId && i.ApplicationUserId == userid).Select(i => i.Category).FirstOrDefault()).GroupBy(j => j).ToList();
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
                    break;
                default:
                    {
                        buyLikes = db.Orders.Where(o => o.ApplicationUserId == userid).Select(j => new StatisticsBuyLike
                        {
                            Id = j.ImageId,
                            Url = "/Content/Content-image/" + j.ImageId + "_xs.jpg",
                            Var1 = j.Size.ToString(),
                            Var2 = j.BuyerEmail,
                            Var3 = j.Price.ToString(),
                            Date = j.BuyingDate
                        }).OrderByDescending(o => o.Date);
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
                    }
                    break;
            }
            if (count != "Все")
            {
                buyLikes = buyLikes.Take(Convert.ToInt32(count));
            }
            return buyLikes;
        }

    }
}