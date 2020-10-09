using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Racoonogram.Models;

namespace Racoonogram.Helpers
{
    public static class PageHelpers
    {
        public static MvcHtmlString PageLinks (this HtmlHelper html,
            PageInfo pageInfo, Func<int, string> pageUrl)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 1; i <= pageInfo.TotalPages; i++)
            {
                TagBuilder tab = new TagBuilder("a");
                tab.MergeAttribute("href", pageUrl(i));
                tab.InnerHtml = i.ToString();
                //если текущая страница, то выделяем ее
                if (i == pageInfo.PageNumber)
                {
                    tab.AddCssClass("selected");
                    tab.AddCssClass("btn-primary");
                }
                tab.AddCssClass("btn btn-default hrefs-query");
                result.Append(tab.ToString());
            }
            return MvcHtmlString.Create(result.ToString());
        }
    }
}