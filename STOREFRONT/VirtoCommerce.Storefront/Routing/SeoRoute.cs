﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using CacheManager.Core;
using VirtoCommerce.Client.Api;
using VirtoCommerce.Client.Model;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;

namespace VirtoCommerce.Storefront.Routing
{
    public class SeoRoute : Route
    {
        private readonly Func<WorkContext> _workContextFactory;
        private readonly ICommerceCoreModuleApi _commerceCoreApi;
        private readonly ILocalCacheManager _cacheManager;

        public SeoRoute(string url, IRouteHandler routeHandler, Func<WorkContext> workContextFactory, ICommerceCoreModuleApi commerceCoreApi, ILocalCacheManager cacheManager)
            : base(url, routeHandler)
        {
            _workContextFactory = workContextFactory;
            _commerceCoreApi = commerceCoreApi;
            _cacheManager = cacheManager;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var requestUrl = httpContext.Request.Url.ToString();

            var data = base.GetRouteData(httpContext);

            if (data != null)
            {
                // Get work context
                var workContext = _workContextFactory();

                var path = data.Values["path"] as string;
                var store = data.Values["store"] as string;

                //Special workaround for case when URL contains only slug without store (one store case)
                if (string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(store) && workContext.AllStores != null)
                {
                    //use {store} as {path} if not exist any store with name {store} 
                    path = workContext.AllStores.Any(x => string.Equals(store, x.Id, StringComparison.InvariantCultureIgnoreCase)) ? null : store;
                }

                if (path != null)
                {
                    var tokens = path.Split('/');
                    // TODO: Store path tokens as breadcrumbs to the work context
                    var slug = tokens.LastOrDefault();

                    // Get all seo records for requested slug and also all other seo records with different slug and languages but related to same object
                    // GetSeoRecords('A') returns 
                    // { objectType: 'Product', objectId: '1',  SemanticUrl: 'A', Language: 'en-us', active : false }
                    // { objectType: 'Product', objectId: '1',  SemanticUrl: 'AA', Language: 'en-us', active : true }
                    var seoRecords = GetSeoRecords(slug);
                    if (seoRecords != null)
                    {
                        var seoRecord = seoRecords
                            .Where(x => string.Equals(slug, x.SemanticUrl, StringComparison.OrdinalIgnoreCase))
                            .GetBestMatchedSeoInfo(workContext.CurrentStore, workContext.CurrentLanguage);

                        if (seoRecord != null)
                        {
                            // Ensure the slug is active
                            if (seoRecord.IsActive != true)
                            {
                                // Slug is not active. Try to find the active one for the same entity and language.
                                seoRecord = seoRecords
                                    .Where(
                                        x =>
                                            x.ObjectType == seoRecord.ObjectType && x.ObjectId == seoRecord.ObjectId &&
                                            x.IsActive == true)
                                    .GetBestMatchedSeoInfo(workContext.CurrentStore, workContext.CurrentLanguage);

                                if (seoRecord == null)
                                {
                                    // No active slug found
                                    data.Values["controller"] = "Error";
                                    data.Values["action"] = "Http404";
                                }
                                else
                                {
                                    // The active slug is found
                                    var response = httpContext.Response;
                                    response.Status = "301 Moved Permanently";
                                    response.RedirectLocation = string.Format("{0}{1}", workContext.CurrentStore.Url,
                                        seoRecord.SemanticUrl);
                                    response.End();
                                    data = null;
                                }
                            }
                            else
                            {
                                // Redirect to the slug for the current language if it differs from the requested slug
                                var actualActiveSeoRecord = seoRecords
                                    .Where(
                                        x =>
                                            x.ObjectType == seoRecord.ObjectType && x.ObjectId == seoRecord.ObjectId &&
                                            x.IsActive == true)
                                    .GetBestMatchedSeoInfo(workContext.CurrentStore, workContext.CurrentLanguage);

                                //If actual seo different that requested need redirect 302
                                if (
                                    !string.Equals(actualActiveSeoRecord.SemanticUrl, seoRecord.SemanticUrl,
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    var response = httpContext.Response;
                                    response.Status = "302 Moved Temporarily";
                                    response.RedirectLocation = string.Concat(workContext.CurrentStore.Url,
                                        actualActiveSeoRecord.SemanticUrl);
                                    response.End();
                                    data = null;
                                }
                                else
                                {
                                    // Process the URL
                                    switch (seoRecord.ObjectType)
                                    {
                                        case "CatalogProduct":
                                            data.Values["controller"] = "Product";
                                            data.Values["action"] = "ProductDetails";
                                            data.Values["productId"] = seoRecord.ObjectId;
                                            break;
                                        case "Category":
                                            data.Values["controller"] = "CatalogSearch";
                                            data.Values["action"] = "CategoryBrowsing";
                                            data.Values["categoryId"] = seoRecord.ObjectId;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(path))
                    {
                        var contentPage = TryToFindContentPageWithUrl(workContext, path);
                        if (contentPage != null)
                        {
                            data.Values["controller"] = "Page";
                            data.Values["action"] = "GetContentPage";
                            data.Values["page"] = contentPage;
                        }
                        else
                        {
                            data.Values["controller"] = "Error";
                            data.Values["action"] = "Http404";
                        }
                    }
                }
            }

            return data;
        }


        private static ContentItem TryToFindContentPageWithUrl(WorkContext workContext, string url)
        {
            ContentItem result = null;

            if (workContext.Pages != null)
            {
                url = url.TrimStart('/');
                var pages = workContext.Pages
                    .Where(x =>
                            string.Equals(x.Permalink, url, StringComparison.CurrentCultureIgnoreCase) ||
                            string.Equals(x.Url, url, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

                // Return page with current language or invariant language
                result = pages.FirstOrDefault(x => x.Language == workContext.CurrentLanguage);
                if (result == null)
                {
                    result = pages.FirstOrDefault(x => x.Language.IsInvariant);
                }
            }

            return result;
        }

        private List<VirtoCommerceDomainCommerceModelSeoInfo> GetSeoRecords(string slug)
        {
            var seoRecords = new List<VirtoCommerceDomainCommerceModelSeoInfo>();

            if (!string.IsNullOrEmpty(slug))
            {
                seoRecords = _cacheManager.Get(string.Join(":", "CommerceGetSeoInfoBySlug", slug), "ApiRegion", () => _commerceCoreApi.CommerceGetSeoInfoBySlug(slug));
            }

            return seoRecords;
        }
    }
}
