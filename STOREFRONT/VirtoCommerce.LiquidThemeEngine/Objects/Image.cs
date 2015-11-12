﻿using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.LiquidThemeEngine.Objects
{
    /// <summary>
    /// https://docs.shopify.com/themes/liquid-documentation/objects/product#product-image
    /// </summary>
    public class Image : Drop
    {
        private readonly Storefront.Model.Image _image;
        private readonly Storefront.Model.Product _product;

        public Image(Storefront.Model.Image image, Storefront.Model.Product product)
        {
            _image = image;
            _product = product;
        }

        public string Alt
        {
            get
            {
                return _product.Name;
            }
        }

        public bool? AttachedToVariant
        {
            get
            {
                //TODO no info about it
                return true;
            }
        }

        public string ProductId
        {
            get
            {
                return _product.Id;
            }
        }

        public int Position
        {
            get
            {
                //TODO no info about it
                return 0;
            }
        }

        public string Src
        {
            get
            {
                return _image.Url;
            }
        }

        public string Name
        {
            get
            {
                return _image.Name;
            }
        }

        public IEnumerable<Variant> Variants
        {
            get
            {
                //TODO no info
                return null;
            }
        }
    }
}
