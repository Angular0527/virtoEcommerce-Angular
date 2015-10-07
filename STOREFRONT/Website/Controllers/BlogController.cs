﻿#region

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Web.Models.Cms;

#endregion

namespace VirtoCommerce.Web.Controllers
{
    public class BlogController : StoreControllerBase
    {
        #region Public Methods and Operators
        [Route("blogs/{blog}")]
        public async Task<ActionResult> DisplayBlogAsync(string blog, int page = 1)
        {
            var model = await Task.FromResult(SiteContext.Current.Blogs[blog]);
            if (model == null)
                throw new HttpException(404, "NotFound");

            Context.Set("blog", model);
            this.Context.Set("current_page", page);

            return View("blog");
        }

        [Route("drafts/blogs/{blog}/{handle}")]
        [Route("blogs/{blog}/{handle}")]
        public async Task<ActionResult> DisplayBlogArticleAsync(string blog, string handle)
        {
            var blogModel = await Task.FromResult(SiteContext.Current.Blogs[blog] as Blog);
            if (blogModel == null)
                throw new HttpException(404, "NotFound");

            var searchHandle = String.Format("{0}/{1}", blog, handle);
            var articleModel = blogModel.AllArticles.SingleOrDefault(x => x.Handle.Equals(searchHandle, StringComparison.OrdinalIgnoreCase));
            Context.Set("blog", blogModel);
            Context.Set("article", articleModel);

            return View("article");
        }
        #endregion
    }
}