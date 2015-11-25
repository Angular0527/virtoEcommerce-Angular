﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Client.Model;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
namespace VirtoCommerce.Storefront.Converters
{
    public static class CategoryConverter
    {
        public static Category ToWebModel(this VirtoCommerceCatalogModuleWebModelCategory category, VirtoCommerceCatalogModuleWebModelProduct[] products = null)
        {
            var retVal = new Category();
            retVal.InjectFrom<NullableAndEnumValueInjection>(category);

            if (category.SeoInfos != null)
                retVal.SeoInfo = category.SeoInfos.Select(s => s.ToWebModel()).FirstOrDefault();

            if (category.Images != null)
            {
                retVal.Images = category.Images.Select(i => i.ToWebModel()).ToArray();
                retVal.PrimaryImage = retVal.Images.FirstOrDefault();
            }

       
            return retVal;
        }
    }
}