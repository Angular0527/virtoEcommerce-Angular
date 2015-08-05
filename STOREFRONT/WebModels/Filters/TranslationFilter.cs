﻿#region
using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using DotLiquid;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Web.Models.Services;
using VirtoCommerce.Web.Views.Engines.Liquid.Extensions;

#endregion

namespace VirtoCommerce.Web.Models.Filters
{
    public class TranslationFilter
    {
        #region Public Methods and Operators
        public static string T(string input, params object[] variables)
        {
            var service = CommerceService.Create();
            var context = SiteContext.Current;
            var locs = service.GetLocale(context);
            var defaultLocs = service.GetLocale(context, true);

            if (locs == null && defaultLocs == null)
            {
                return input;
            }

            string retVal;

            // first get a template string
            if (variables != null && variables.Length > 0)
            {
                var dictionary =
                    variables.Where(x => (x is Tuple<string, object>))
                        .Select(x => x as Tuple<string, object>)
                        .ToDictionary(x => x.Item1, x => x.Item2);

                string template;
                if (dictionary.ContainsKey("count") && dictionary["count"] != null) // execute special count routing
                {
                    var count = dictionary["count"].ToInt();
                    JToken templateToken;
                    switch (count)
                    {
                        case 1:
                            templateToken = locs.GetValue(defaultLocs, input + ".one");
                            break;
                        case 0:
                            templateToken = locs.GetValue(defaultLocs, input + ".zero", null);
                            break;
                        case 2:
                            templateToken = locs.GetValue(defaultLocs, input + ".two", null);
                            break;
                        default:
                            templateToken = locs.GetValue(defaultLocs, input + ".other");
                            break;
                    }

                    if (templateToken == null)
                    {
                        templateToken = locs.GetValue(defaultLocs, input + ".other");
                        template = templateToken != null ? templateToken.ToString() : String.Empty;
                    }
                    else
                    {
                        template = templateToken.ToString();
                    }
                }
                else
                {
                    template = locs.GetValue(defaultLocs, input);
                }

                var templateEngine = Template.Parse(template);
                retVal = templateEngine.Render(Hash.FromDictionary(dictionary));
            }
            else
            {
                retVal = locs.GetValue(defaultLocs, input);
            }

            return retVal;
        }
        #endregion

        private static Template ParseCached(string contents)
        {
            if (contents == null)
                return null;

            var contextKey = "vc-cms-template-" + contents.GetHashCode();
            var value = HttpRuntime.Cache.Get(contextKey);

            if (value != null)
            {
                return value as Template;
            }

            var t = Template.Parse(contents);

            HttpRuntime.Cache.Insert(contextKey, t, null, DateTime.UtcNow.AddHours(1), Cache.NoSlidingExpiration);

            return t;
        }
    }

    public static class LocaleExtensions
    {
        #region Public Methods and Operators
        public static string GetValue(this JObject source, JObject defaultSource, string key, string defaultValue = "")
        {
            JToken token = null;

            if (source != null)
            {
                token = source.SelectToken(key);
            }

            if (token != null)
            {
                return token.ToString();
            }

            token = defaultSource.SelectToken(key);
            if (token != null)
            {
                return token.ToString();
            }

            return defaultValue == "" ? key : defaultValue;
        }
        #endregion
    }
}