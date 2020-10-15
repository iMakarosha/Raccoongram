using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Racoonogram.Models;


namespace Racoonogram.Models
{
    public class I_U_Models:DbContext
    {
        public I_U_Models() : base("DefaultConnection") { }
        public DbSet<GetCategories> getCategories { get; set; }
        public DbSet<Querys> Querys { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<PageInfo> PageInfos { get; set; }
        public DbSet<Photographer> Photographers { get; set; }
        public DbSet<ShowQueryImages> showQueryImages { get; set; }
        public DbSet<ImageUnload> imageUnloads { get; set; }

        public DbSet<ForStatistics> forStatistics { get; set; }
        public DbSet<StatisticsBuyLike> statisticsBuyLikes { get; set; }
        public DbSet<StatisticsDownloaders> statisticsDownloaders { get; set; }
        public DbSet<BestImagesL> bestImagesL { get; set; }
        public DbSet<BestAuthors> bestAuthors { get; set; }



        public DbSet<UserAndImagesAndPlans> userAndImagesAndPlans { get; set; }

    }
    public class UserAccountEditor
    {
        [HiddenInput(DisplayValue = false)]
        public string Id { get; set; }
        [Display(Name = "Логин:")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [StringLength(18, MinimumLength = 5, ErrorMessage = "Необходим логин не менее 5 и не более 18 символов")]
        public string UserName { get; set; }
        [Display(Name = "Email:")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        public string Email { get; set; }
        [Display(Name = "Телефон:")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Статус:")]
        public string Status { get; set; }
        [DataType(DataType.Url)]
        [Display(Name = "Ссылка:")]
        public string Site { get; set; }
        [DataType(DataType.MultilineText)]
        [UIHint("MultilineText")]
        [Display(Name = "О себе:")]
        public string Additionally { get; set; }
        public HttpPostedFileBase logo_file { get; set; }

    }

    public class AuthorAndAllImages
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<Image> ImagesUser { get; set; }
    }
    public class UserSearch
    {
        public string Id { get; set; }
        public string UrlLogo { get; set; }
        public string UserName { get; set; }
        public string Site { get; set; }
        public string SiteShort { get; set; }
        public int CountPubl { get; set; }
        public int CountFollow { get; set; }
    }

    public class UserAndImagesAndPlans
    {
        public string Id { get; set; }
        public ApplicationUser User { get; set; }
        public IEnumerable<Buying> buyings { get; set; }
        public IEnumerable<Buying> buyingsAll { get; set; }
        public IEnumerable<Liking> likings { get; set; }
        public IEnumerable<Plans> plans { get; set; }
        public UserMoney userMoneys { get; set; }

        public IEnumerable<QuerysPopular> querysPopulars { get; set; }
        public IEnumerable<BestImagesL> ImNews1 { get; set; }
        public IEnumerable<BestImagesL> ImNews2 { get; set; }
        public IEnumerable<BestImagesL> ImFree { get; set; }
        //еще должна быть сумма средств на счету, нет?
    }
    public class Buying
    {
        public int OrderId { get; set; }
        public int ImageId { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public double Price { get; set; }
        public string Size { get; set; }
        public DateTime OrdDate { get; set; }
        public string href { get; set; }
    }
    public class Liking
    {
        public int LikeId { get; set; }//Может, удалить
        public int ImageId { get; set; }
        public string Url { get; set; }
        public DateTime LikeDate { get; set; }//Может, удалить
    }
    public class Plans
    {
        public int Id { get; set; }
        public string PlanId { get; set; }
        public int StartPrice { get; set; }
        public int StartImageBalance { get; set; }
        public int ImageBalance { get; set; }
        public int isHide { get; set; }
        public DateTime BuyingDate { get; set; }
    }
    public class UserMoney
    {
        public double MoneyBalance { get; set; }
        public DateTime LastDate { get; set; }
    }

    public class Querys
    {
        public string QueryStr { get; set; }
    }

    public class Categories
    {
        public string CategoryName { get; set; }
    }

    public class ImageUnload
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Display(Name = "Категория")]
        public string Category { get; set; }
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Display(Name = "Ключевые слова")]
        [DataType(DataType.MultilineText)]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Длина строки должна не менее 10 символов")]
        public string KeyWords { get; set; }
        [Display(Name = "Описание")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Display(Name = "Цвета")]
        public string Colors { get; set; }
        [Display(Name = "Стоимость")]
        [DataType(DataType.Currency)]
        [Range(typeof(decimal), "0,00", "5,99", ErrorMessage ="Стоимость загружаемой фотографии не должна превышать 6$")]
        public double Price { get; set; }
        [HiddenInput(DisplayValue = false)]
        public string Url { get; set; }
    }
    /*получить список всех почти категорий доступных*/
    public class GetCategories
    {
        public string CategoryName { get; set; }
        public string Url { get; set; }
    }


    public class PageInfo
    {
        public int PageNumber { get; set; }//tecushaya stranica
        public int PageSize { get; set; }//count of objects
        public int TotalItems { get; set; }//vsego objects
        public int TotalPages//vsego stranic
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / PageSize); }
        }
    }
    public class PaginationClass
    {
        public PageInfo PageInfoPag { get; set; }
        public IEnumerable<Image> ImagesPag{ get; set; }
    }
    public class PaginationClassForUsers
    {
        public PageInfo PageInfoPag { get; set; }
        public IEnumerable<UserSearch>  userSearches{ get; set; }
    }

    public partial class Photographer
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class ShowQueryImages
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public string Category { get; set; }
        public string Url { get; set; }

        public string KeyWords { get; set; }
        public float Price { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }

    /*для статистики*/

    public class ForStatistics
    {
        public ForStatisticsMy forStatisticsMy { get; set; }
        public ForStatisticsCommon forStatisticsCommon { get; set; }
    }
    public class ForStatisticsMy
    {
        public int CountDownloads { get; set; }
        public int CountLikes { get; set; }
        public List<BestBuyers> bestBuyers { get; set; }
        public string SumTotal { get; set; }

    }
    public class BestBuyers
    {
        public string Email { get; set; }
        public int CountD { get; set; }
        public string Sum { get; set; }
    }
    public class ForStatisticsCommon
    {
        public int I { get; set; }
        public string CommonSum { get; set; }
        public List<IGrouping<string, QuerysPopular>> SearchWords { get; set; }

    }
    public class QuerysPopular
    {
        public string QueryStr { get; set; }
        public int CountThisSearch { get; set; }
    }


    public class StatisticsBuyLike
    {
        public int Id { get; set; }
        public double integer { get; set; }
        public string Url { get; set; }
        public string Var1 { get; set; }
        public string Var2 { get; set; }
        public string Var3 { get; set; }
        public DateTime Date { get; set; }
    }

    public class StatisticsDownloaders
    {
        public List<BestBuyers> bestBuyers { get; set; }
    }

    /*для лучших изображений*/
    public class BestImagesL
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Url { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public int LikesCount { get; set; }

    }
    /*для лучших пользователей*/
    public class BestAuthors
    {
        public IEnumerable<BestUser> bestUsersLike { get; set; }
        public IEnumerable<BestUser> bestUsersDownload { get; set; }
    }
    public class BestUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public int Photos { get; set; }
        public int PhotosAtWeek { get; set; }
        public int OrdersAtWeek { get; set; }
        public int LikesAtWeek { get; set; }
        public string UrlLogo { get; set; }
    }
}