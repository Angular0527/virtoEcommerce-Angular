﻿using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Client.Model;
using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.Storefront.Common
{
    /// <summary>
    /// Find best seo match based for passed store and language
    /// </summary>
    public static class SeoExtensions
    {
        /// <summary>
        /// Returns SEO path only if given category and all its parent categories have SEO keywords, otherwise returns default value.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="store"></param>
        /// <param name="language"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetSeoPath(this VirtoCommerceCatalogModuleWebModelCategory category, Store store, Language language, string defaultValue)
        {
            // Path: GrandParentCategory/ParentCategory/Category
            var outline = category.Outlines != null ? category.Outlines.FirstOrDefault() : null;
            var result = GetSeoPath(outline, store, language, defaultValue);
            return result;
        }

        /// <summary>
        /// Returns SEO path only if given product, its category and all parent categories have SEO keywords, otherwise returns default value.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="store"></param>
        /// <param name="language"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetSeoPath(this VirtoCommerceCatalogModuleWebModelProduct product, Store store, Language language, string defaultValue)
        {
            // Path: GrandParentCategory/ParentCategory/ProductCategory/Product
            var outline = product.Outlines != null ? product.Outlines.FirstOrDefault() : null;
            var result = GetSeoPath(outline, store, language, defaultValue);
            return result;
        }

        public static VirtoCommerceDomainCommerceModelSeoInfo GetBestMatchedSeoInfo(this IEnumerable<VirtoCommerceDomainCommerceModelSeoInfo> seoRecords, Store store, Language language)
        {
            VirtoCommerceDomainCommerceModelSeoInfo result = null;

            if (seoRecords != null)
            {
                result = seoRecords
                    .Select(s =>
                    {
                        var score = 0;
                        score += store.Id.Equals(s.StoreId, StringComparison.OrdinalIgnoreCase) ? 4 : 0;
                        score += language.Equals(s.LanguageCode) ? 2 : 0;
                        score += store.DefaultLanguage.Equals(s.LanguageCode) ? 1 : 0;
                        return new { SeoRecord = s, Score = score };
                    })
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.SeoRecord)
                    .FirstOrDefault();
            }

            return result;
        }


        private static string GetSeoPath(VirtoCommerceDomainCatalogModelOutline outline, Store store, Language language, string defaultValue)
        {
            var result = defaultValue;

            if (outline != null)
            {
                var pathSegments = outline.Items
                    .Where(i => i.SeoObjectType != "Catalog")
                    .Select(i => GetBestMatchedSeoKeyword(i.SeoInfos, store, language))
                    .ToList();

                if (pathSegments.All(s => s != null))
                {
                    result = string.Join("/", pathSegments);
                }
            }

            return result;
        }

        private static string GetBestMatchedSeoKeyword(IEnumerable<VirtoCommerceDomainCommerceModelSeoInfo> seoRecords, Store store, Language language)
        {
            string result = null;

            if (seoRecords != null)
            {
                // Select best matched SEO by StoreId and Language
                var bestMatchedSeo = seoRecords.GetBestMatchedSeoInfo(store, language);
                if (bestMatchedSeo != null)
                {
                    result = bestMatchedSeo.SemanticUrl;
                }
            }

            return result;
        }
    }
}
