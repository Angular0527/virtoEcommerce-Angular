﻿using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model
{
    public class Discount : ValueObject<Discount>
    {
        /// <summary>
        /// Gets or sets the value of promotion id
        /// </summary>
        public string PromotionId { get; set; }

        /// <summary>
        /// Gets or sets discount amount type (absolute or relative)
        /// </summary>
        public AmountType Type { get; set; }

        /// <summary>
        /// Gets or sets the value of absolute discount amount
        /// </summary>
        public Money AbsoluteAmount { get; set; }

        /// <summary>
        /// Gets or sets the value of relative discount amount
        /// </summary>
        public double? RelativeAmount { get; set; }

        /// <summary>
        /// Gets or sets the value of discount description
        /// </summary>
        public string Description { get; set; }
    }
}