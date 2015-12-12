﻿using System;
using System.Globalization;
using System.Linq;
using System.Web;
using DotLiquid;
using VirtoCommerce.Storefront.Model.Catalog;
using shopifyModel = VirtoCommerce.LiquidThemeEngine.Objects;
using storefrontModel = VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Filters
{
    /// <summary>
    /// https://docs.shopify.com/themes/liquid-documentation/filters/url-filters
    /// </summary>
    public class UrlFilters
    {
        /// <summary>
        /// Generates a link to the customer login page.
        /// {{ 'Log in' | customer_login_link }}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CustomerLoginLink(string input)
        {
            return BuildHtmlLink("customer_login_link", "~/account/login", input);
        }
        public static string CustomerRegisterLink(string input)
        {
            return BuildHtmlLink("customer_register_link", "~/account/register", input);
        }

        public static string CustomerLogoutLink(string input)
        {
            return BuildHtmlLink("customer_logout_link", "~/account/logout", input);
        }

        public static string EditCustomerAddressLink(string input, string id)
        {
            return BuildOnClickLink(input, "Shopify.CustomerAddress.toggleForm('{0}');return false", id);
        }

        public static string DeleteCustomerAddressLink(string input, string id)
        {
            return BuildOnClickLink(input, "Shopify.CustomerAddress.destroy('{0}');return false", id);
        }

        /// <summary>
        /// Returns the URL of an image. Accepts an image size as a parameter. The img_url filter can be used on the following objects:
        /// product, variant,  line item, collection, image
        /// </summary>
        /// <param name="input"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ImgUrl(object input, string type = null)
        {
            if (input == null)
                return null;

            var retVal = input.ToString();
            var product = input as shopifyModel.Product;
            var image = input as shopifyModel.Image;
            var variant = input as shopifyModel.Variant;
            var collection = input as shopifyModel.Collection;

            if (product != null)
            {
                retVal = product.FeaturedImage?.Src;
            }
            if (image != null)
            {
                retVal = image.Src;
            }
            if (variant != null)
            {
                retVal = variant.FeaturedImage?.Src;
            }
            if (collection != null)
            {
                retVal = collection.Image?.Src;
            }

            return retVal;
        }

        /// <summary>
        /// Generates an HTML link. The first parameter is the URL of the link, and the optional second parameter is the title of the link.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="link"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string LinkTo(object input, string link, string title = "")
        {
            return string.Format("<a href=\"{0}\" title=\"{1}\">{2}</a>", link, title, input);
        }

        /// <summary>
        /// Creates a link to all products in a collection that have a given tag.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string LinkToTag(Context context, object input, object tag)
        {
            return BuildTagLink(TagAction.Replace, tag, input);
        }

        /// <summary>
        /// Creates a link to all products in a collection that have a given tag as well as any tags that have been already selected.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string LinkToAddTag(Context context, object input, object tag)
        {
            return BuildTagLink(TagAction.Add, tag, input);
        }

        /// <summary>
        /// Generates a link to all products in a collection that have all the previous tags that might have been added already, excluding the given tag.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string LinkToRemoveTag(Context context, object input, object tag)
        {
            return BuildTagLink(TagAction.Remove, tag, input);
        }

        /// <summary>
        /// Returns the URL of a file in the "assets" folder of a theme.
        /// {{ 'shop.css' | asset_url }}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string AssetUrl(string input)
        {
            string retVal = null;
            if (input != null)
            {
                var themeAdaptor = (ShopifyLiquidThemeEngine)Template.FileSystem;
                retVal = themeAdaptor.GetAssetAbsoluteUrl(input);
            }
            return retVal;
        }

        /// <summary>
        /// Returns the URL of a global assets that are found on Shopify's servers. 
        /// In virtocommerce is a same asset folder
        /// customer.css
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ShopifyAssetUrl(string input)
        {
            return AssetUrl(input);
        }


        /// <summary>
        /// Returns the URL of a global asset. Global assets are kept in a directory on Shopify's servers. Using global assets can improve the load times of your pages.
        /// In virtocommerce is a same asset folder
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GlobalAssetUrl(string input)
        {
            return AssetUrl(input);
        }

        /// <summary>
        /// Returns the URL of a file.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FileUrl(string input)
        {
            return AssetUrl(input);
        }

        /// <summary>
        /// Get absolute storefront url with specified store and language
        /// </summary>
        /// <param name="input"></param>
        /// <param name="storeId"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        public static string AbsoluteUrl(string input, string storeId = null, string languageCode = null)
        {
            var themeAdaptor = (ShopifyLiquidThemeEngine)Template.FileSystem;
            storefrontModel.Store store = null;
            storefrontModel.Language language = null;
            if (!string.IsNullOrEmpty(storeId))
            {
                store = themeAdaptor.WorkContext.AllStores.FirstOrDefault(x => string.Equals(x.Id, storeId, StringComparison.InvariantCultureIgnoreCase));
            }
            store = store ?? themeAdaptor.WorkContext.CurrentStore;

            if (!string.IsNullOrEmpty(languageCode))
            {
                language = store.Languages.FirstOrDefault(x => string.Equals(x.CultureName, languageCode, StringComparison.InvariantCultureIgnoreCase));
            }
            language = language ?? themeAdaptor.WorkContext.CurrentLanguage;

            var retVal = themeAdaptor.UrlBuilder.ToAppAbsolute(input, store, language);
            return retVal;
        }


        private static string BuildOnClickLink(string title, string onclickFormat, params object[] onclickArgs)
        {
            var onClick = string.Format(CultureInfo.InvariantCulture, onclickFormat, onclickArgs);

            var result = string.Format(CultureInfo.InvariantCulture, "<a href=\"#\" onclick=\"{0}\">{1}</a>",
                HttpUtility.HtmlAttributeEncode(onClick),
                HttpUtility.HtmlEncode(title));

            return result;
        }

        private static string BuildHtmlLink(string id, string virtualUrl, string title)
        {
            var href = BuildAbsoluteUrl(virtualUrl);

            var result = string.Format(CultureInfo.InvariantCulture, "<a href=\"{0}\" id=\"{1}\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(href),
                HttpUtility.HtmlAttributeEncode(id),
                HttpUtility.HtmlEncode(title));

            return result;
        }

        private enum TagAction
        {
            Add,
            Remove,
            Replace
        }

        private static string BuildTagLink(TagAction action, object tagObject, object input)
        {
            var href = string.Empty;
            var title = string.Empty;
            var label = input.ToString();

            if (tagObject == null)
            {
                title = "Remove all tags";
            }
            else
            {
                var tag = tagObject as shopifyModel.Tag;

                if (tag != null)
                {
                    href = GetCurrentUrlWithTags(action, tag.GroupName, tag.Value);
                    title = BuildTagActionTitle(action, tag.Label);

                    label = tag.Label;

                    if (tag.Count > 0)
                    {
                        label += $" ({tag.Count})";
                    }
                }
                else
                {
                    // TODO: Parse tag string
                }
            }

            var result = string.Format(CultureInfo.InvariantCulture, "<a href=\"{0}\" title=\"{1}\">{2}</a>",
                HttpUtility.HtmlAttributeEncode(href),
                HttpUtility.HtmlAttributeEncode(title),
                HttpUtility.HtmlEncode(label));

            return result;
        }

        private static string BuildTagActionTitle(TagAction action, string tagLabel)
        {
            switch (action)
            {
                case TagAction.Remove:
                    return "Remove tag " + tagLabel;
                default:
                    return "Show products matching tag " + tagLabel;
            }
        }

        private static string GetCurrentUrlWithTags(TagAction action, string groupName, string value)
        {
            var term = new Term { Name = groupName, Value = value };
            var themeEngine = (ShopifyLiquidThemeEngine)Template.FileSystem;

            var terms = themeEngine.WorkContext.CurrentCatalogSearchCriteria.Terms
                .Select(t => new Term { Name = t.Name, Value = t.Value })
                .ToList();

            switch (action)
            {
                case TagAction.Add:
                    terms.Add(term);
                    break;
                case TagAction.Remove:
                    terms.RemoveAll(t =>
                        string.Equals(t.Name, groupName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(t.Value, value, StringComparison.OrdinalIgnoreCase));
                    break;
                case TagAction.Replace:
                    terms.Clear();
                    terms.Add(term);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            var request = HttpContext.Current.Request;
            var qs = HttpUtility.ParseQueryString(request.Url.Query);

            if (terms.Any())
            {
                // terms=name1:value1,value2,value3;name2:value1,value2,value3
                qs["terms"] = string.Join(";",
                    terms
                        .OrderBy(t => t.Name)
                        .GroupBy(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase)
                        .Select(
                            g =>
                                string.Join(":", g.Key,
                                    string.Join(",",
                                        g.Select(t => t.Value)
                                            .Distinct(StringComparer.OrdinalIgnoreCase)
                                            .OrderBy(v => v))))
                    ).ToLowerInvariant();
            }
            else
            {
                qs.Remove("terms");
            }

            var virtualUrl = request.AppRelativeCurrentExecutionFilePath;

            if (qs.HasKeys())
            {
                virtualUrl += "?" + qs;
            }

            var result = BuildAbsoluteUrl(virtualUrl);
            return result;
        }


        private static string BuildAbsoluteUrl(string virtualUrl)
        {
            var themeEngine = (ShopifyLiquidThemeEngine)Template.FileSystem;
            var workContext = themeEngine.WorkContext;
            var result = themeEngine.UrlBuilder.ToAppAbsolute(virtualUrl, workContext.CurrentStore, workContext.CurrentLanguage);
            return result;
        }
    }
}
