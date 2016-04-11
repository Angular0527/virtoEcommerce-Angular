﻿using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.Storefront.Common
{
    public static class LocalizationExtension
    {
        public static IHasLanguage FindWithLanguage(this IEnumerable<IHasLanguage> items, Language language)
        {
            return items.FirstOrDefault(i => i.Language.Equals(language));
        }

        public static TValue FindWithLanguage<T, TValue>(this IEnumerable<T> items, Language language, Func<T, TValue> valueGetter, TValue defaultValue) where T : IHasLanguage
        {
            var retVal = defaultValue;
            var item = items.OfType<IHasLanguage>().FindWithLanguage(language);
            if (item != null)
            {
                retVal = valueGetter((T)item);
            }
            return retVal;
        }
    }
}