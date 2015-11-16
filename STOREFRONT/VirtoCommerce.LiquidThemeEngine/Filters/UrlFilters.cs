﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotLiquid;
using DotLiquid.Util;
using System.Threading;
using VirtoCommerce.Storefront.Model;

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
            var path = VirtualPathUtility.ToAbsolute("~/account/login");
            return string.Format("<a href=\"{0}\" id=\"customer_login_link\">{1}</a>", path, input);
        }
        public static string CustomerRegisterLink(string input)
        {
            var path = VirtualPathUtility.ToAbsolute("~/account/register");
            return string.Format("<a href=\"{0}\" id=\"customer_register_link\">{1}</a>", path, input);
        }

        public static string CustomerLogoutLink(string input)
        {
            var path = VirtualPathUtility.ToAbsolute("~/account/logoff");
            return string.Format("<a href=\"{0}\" id=\"customer_logout_link\">{1}</a>", path, input);
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
        // </summary>
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
        /// <returns></returns>
        public static string AbsoluteUrl(string input, string storeId = null, string languageCode = null)
        {
            var themeAdaptor = (ShopifyLiquidThemeEngine)Template.FileSystem;
            Store store = null;
            Language language = null;
            if (!string.IsNullOrEmpty(storeId))
            {
                store = themeAdaptor.WorkContext.AllStores.FirstOrDefault(x => string.Equals(x.Id, storeId, StringComparison.InvariantCultureIgnoreCase));
            }
            store = store ?? themeAdaptor.WorkContext.CurrentStore;

            if (!string.IsNullOrEmpty(languageCode))
            {
                language = store.Languages.FirstOrDefault(x => string.Equals(x.CultureName, languageCode, StringComparison.InvariantCultureIgnoreCase));
            }
            language = language ?? store.DefaultLanguage;

            var retVal = themeAdaptor.UrlBuilder.ToAbsolute(themeAdaptor.WorkContext, input, store, language);
            return retVal;
        }
    }
}