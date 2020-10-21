using System.Linq;
using System.Web.Mvc;
using Racoonogram.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;

namespace Racoonogram.Services
{
    public class UserService
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public UserService() { }

        #region User

        public void SaveUser(UserAccountEditor userEdit, string id)
        {
            ApplicationUser user = GetUser(id);
            user.UserName = userEdit.UserName;
            user.Site = userEdit.Site;
            user.Email = userEdit.Email;
            user.PhoneNumber = userEdit.PhoneNumber;
            user.Status = userEdit.Status;
            user.Additionally = userEdit.Additionally;

            db.SaveChanges();
        }

        public ApplicationUser GetUser(string id)
        {
            return db.Users.Where(u => u.Id == id).Select(u => u).FirstOrDefault();
        }

        public string GetUserID(string identity, string by = "name")
        {
            switch (by)
            {
                case "name":
                    return db.Users.Where(u => u.UserName == identity).Select(u => u.Id).FirstOrDefault();
                //email
                default:
                    return db.Users.Where(u => u.Email == identity).Select(u => u.Id).FirstOrDefault();
            }
        }

        public string GetUserEmail(string id)
        {
            return db.Users.Where(u => u.Id == id).Select(u => u.Email).FirstOrDefault();
        }

        public string GetUserName(string id)
        {
            return db.Users.Where(us => us.Id == id).Select(us => us.UserName).FirstOrDefault();
        }

        public SelectList GetUsers()
        {
            return new SelectList(db.Users, "Id", "Email");
        }

        public SelectList GetUsers(string userId)
        {
            return new SelectList(db.Users, "Id", "Email", userId);
        }

        public int GetUsersCount()
        {
            return db.Users.Select(i => i.UserName).Count();
        }

        public int GetUsersCount(string searchstring)
        {
            return db.Users.Where(i => i.UserName.Contains(searchstring)).Select(i => i.UserName).Count();
        }

        public UserIdAndEmail GetUserIdAndEmail(string userName)
        {
            return db.Users.Where(i => i.UserName == userName).Select(i => new UserIdAndEmail
            {
                Id = i.Id,
                Email = i.Email

            }).FirstOrDefault();
        }

        public UserAccountEditor GetUserAccountInfo(string userId)
        {
            return db.Users.Select(us => new UserAccountEditor
            {
                Id = us.Id,
                UserName = us.UserName,
                Site = us.Site,
                Email = us.Email,
                PhoneNumber = us.PhoneNumber,
                Additionally = us.Additionally,
                Status = us.Status
            }).Where(u => u.Id == userId).FirstOrDefault();
        }

        public List<string> GetBuyerEmail(string userId)
        {
            return db.Orders.Where(o => o.ApplicationUserId == userId).GroupBy(o => o.BuyerEmail).OrderBy(o => o.Count()).Select(j => j.Key).ToList();
        }

        public IEnumerable<BestUser> GetBestUsers(DateTime date)
        {
            return db.Users.Select(bestu => new BestUser
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
        }

        public IEnumerable<UserSearch> GetUsersSearch(int pageSize)
        {
            return db.Users.Select(q => new UserSearch
            {
                Id = q.Id,
                UserName = q.UserName,
                Site = q.Site,
                CountPubl = q.Images.Select(i => i.ImageId).Count(),
                CountFollow = (from l in db.Likes
                               join i in db.Images on l.ImageId equals i.ImageId
                               where i.ApplicationUserId == q.Id
                               select l.LikeId).Count()
            }).Where(q => q.CountPubl > 0).OrderByDescending(i => i.UserName).Take(pageSize).ToList();
        }

        public IEnumerable<UserSearch> GetUsersSearch(string searchstring, string order, int page, int pageSize)
        {
            switch (order)
            {
                case "follow":
                    return db.Users.Select(q => new UserSearch
                    {
                        Id = q.Id,
                        UserName = q.UserName,
                        Site = q.Site,
                        CountPubl = q.Images.Select(i => i.ImageId).Count(),
                        CountFollow = (from l in db.Likes
                                       join i in db.Images on l.ImageId equals i.ImageId
                                       where i.ApplicationUserId == q.Id
                                       select l.LikeId).Count()
                    }).OrderByDescending(i => i.CountFollow).Where(i => i.UserName.Contains(searchstring)).Take(pageSize).ToList();
                case "images":
                    return db.Users.Select(q => new UserSearch
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
                default:
                    return db.Users.Select(q => new UserSearch
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
            }
        }
        #endregion

        #region Orders

        public void AddOrder(Order order)
        {
            db.Orders.Add(order);
            db.SaveChanges();
        }

        public Order GetOrder(int orderid)
        {
            return db.Orders.Where(o => o.OrderId == orderid).Select(o => o).FirstOrDefault();
        }

        public Order GetOrder(string email, int imageId, int size)
        {
            return db.Orders.Where(o => o.BuyerEmail == email && o.ImageId == imageId && o.Size == size).Select(o => o).FirstOrDefault(); ;
        }

        public int GetOrdersCount(string item, string type = "id")
        {
            if (type == "id")
            {
                return db.Orders.Where(o => o.ApplicationUserId == item).Select(o => o.OrderId).Count();
            }
            else
            {
                return db.Orders.Where(o => o.BuyerEmail == item).Count();
            }
        }

            public double GetOrderSum(string ident = "", string type = "id", DateTime date = default(DateTime))
        {
            switch (type)
            {
                case "id":
                    return db.Orders.Where(o => o.ApplicationUserId == ident).Select(o => o.Price).Sum();
                case "email":
                    return db.Orders.Where(o => o.BuyerEmail == ident).Sum(o => o.Price);
                default:
                    return db.Orders.Where(o => o.BuyingDate > date).Select(o => o.Price).Sum();
            }
        }

        #endregion

        #region QueryHistory

        public IQueryable<Querys> GetPopularQuerys()
        {
            return (from f in db.QueryHistories
                    group f by f.QuerySting into j
                    select new Querys() { QueryStr = j.Key }).Take(30);
        }

        public void QueryHistoryAdd(QueryHistory query)
        {
            db.QueryHistories.Add(query);
            db.SaveChanges();
        }

        public IQueryable<QuerysPopular> GetQuerysPopulars(int takeCount)
        {
            return db.QueryHistories.GroupBy(q => q.QuerySting).Select(j => new QuerysPopular
            {
                QueryStr = j.Key,
                CountThisSearch = db.QueryHistories.Where(q => q.QuerySting == j.Key).Select(q => q.QuerySting).Count()
            }).OrderByDescending(j => j.CountThisSearch).Take(takeCount);
        }

        public IQueryable<QuerysPopular> GetQuerysPopularsAll()
        {
            return db.QueryHistories.Select(z => new QuerysPopular
            {
                QueryStr = z.QuerySting,
                CountThisSearch = db.QueryHistories.Where(o => o.QuerySting == z.QuerySting).Select(o => o.Id).Count()

            }).OrderByDescending(f => f.CountThisSearch);
        }
        #endregion

        #region UserAndImagesAndPlans

        public IQueryable<Buying> GetBuyings(string userId)
        {
            string buyerEmail = GetUserEmail(userId);
            return (from order in db.Orders
                    join use in db.Users on order.ApplicationUserId equals use.Id
                    where order.BuyerEmail == buyerEmail && order.IsHide != 1
                    select new Buying
                    {
                        OrderId = order.OrderId,
                        AuthorId = order.ApplicationUserId,
                        AuthorName = use.UserName,
                        ImageId = order.ImageId,
                        OrdDate = order.BuyingDate,
                        Price = order.Price,
                        Size = order.Size + "px",
                        href = order.ImageId + "_xs.jpg"
                    }).OrderByDescending(o => o.OrdDate).Take(12);
        }

        public IQueryable<Buying> GetBuyingsAll(string userId)
        {
            string buyerEmail = GetUserEmail(userId);

            return (from order in db.Orders
                    join use in db.Users on order.ApplicationUserId equals use.Id
                    where order.BuyerEmail == buyerEmail && order.IsHide != 1
                    select new Buying
                    {
                        OrderId = order.OrderId,
                        AuthorId = order.ApplicationUserId,
                        AuthorName = use.UserName,
                        ImageId = order.ImageId,
                        OrdDate = order.BuyingDate,
                        Price = order.Price,
                        Size = order.Size + "px",
                        href = order.ImageId + "_xs.jpg"
                    }).OrderByDescending(o => o.OrdDate).Skip(12);
        }
        #endregion

        public void DbSave()
        {
            db.SaveChanges();
        }
    }
}