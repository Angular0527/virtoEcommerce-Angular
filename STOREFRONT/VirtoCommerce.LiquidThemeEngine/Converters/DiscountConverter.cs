﻿using VirtoCommerce.Storefront.Model;
using shopifyModel = VirtoCommerce.LiquidThemeEngine.Objects;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class DiscountConverter
    {
        public static shopifyModel.Discount ToShopifyModel(this Discount discount)
        {
            var ret = new shopifyModel.Discount
            {
                Amount = discount.DiscountAmount.Amount,
                Code = discount.PromotionId,
                Id = discount.PromotionId,
                Savings = -discount.DiscountAmount.Amount
            };

            return ret;
        }
    }
}
