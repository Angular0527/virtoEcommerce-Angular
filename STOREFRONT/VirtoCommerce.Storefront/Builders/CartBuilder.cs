﻿using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Client.Api;
using VirtoCommerce.Client.Model;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Builders
{
    public class CartBuilder : ICartBuilder
    {
        private readonly IShoppingCartModuleApi _cartApi;
        private readonly IMarketingModuleApi _marketingApi;

        private Store _store;
        private Customer _customer;
        private Currency _currency;
        private ShoppingCart _cart;

        private const string CartSubtotalDiscount = "";

        public CartBuilder(
            IShoppingCartModuleApi cartApi,
            IMarketingModuleApi marketinApi)
        {
            _cartApi = cartApi;
            _marketingApi = marketinApi;
        }

        public async Task<CartBuilder> GetOrCreateNewTransientCartAsync(Store store, Customer customer, Currency currency)
        {
            VirtoCommerceCartModuleWebModelShoppingCart cart = null;

            _store = store;
            _customer = customer;
            _currency = currency;

            cart = await _cartApi.CartModuleGetCurrentCartAsync(_store.Id, _customer.Id);
            if (cart == null)
            {
                _cart = new ShoppingCart(_store.Id, _customer.Id, _customer.Name, "Default", _currency.Code);
            }
            else
            {
                _cart = cart.ToWebModel();
            }

            await EvaluatePromotionsAsync();

            return this;
        }

        public async Task<CartBuilder> AddItemAsync(Product product, int quantity)
        {
            AddLineItem(product.ToLineItem(quantity));

            await EvaluatePromotionsAsync();

            return this;
        }

        public async Task<CartBuilder> ChangeItemQuantityAsync(string id, int quantity)
        {
            var lineItem = _cart.Items.FirstOrDefault(i => i.Id == id);
            if (lineItem != null)
            {
                if (quantity > 0)
                {
                    lineItem.Quantity = quantity;

                    await EvaluatePromotionsAsync();
                }
            }

            return this;
        }

        public async Task<CartBuilder> RemoveItemAsync(string id)
        {
            var lineItem = _cart.Items.FirstOrDefault(i => i.Id == id);
            if (lineItem != null)
            {
                _cart.Items.Remove(lineItem);

                await EvaluatePromotionsAsync();
            }

            return this;
        }

        public async Task<CartBuilder> AddCouponAsync(string couponCode)
        {
            _cart.Coupon = new Coupon
            {
                Code = couponCode
            };

            await EvaluatePromotionsAsync();

            return this;
        }

        public async Task<CartBuilder> RemoveCouponAsync()
        {
            _cart.Coupon = null;

            await EvaluatePromotionsAsync();

            return this;
        }

        public async Task<CartBuilder> AddAddressAsync(Address address)
        {
            var existingAddress = _cart.Addresses.FirstOrDefault(a => a.Type == address.Type);
            if (existingAddress != null)
            {
                _cart.Addresses.Remove(existingAddress);
            }

            _cart.Addresses.Add(address);

            await EvaluatePromotionsAsync();

            return this;
        }

        public async Task<CartBuilder> AddShipmentAsync(ShippingMethod shippingMethod)
        {
            var shipment = shippingMethod.ToShipmentModel(_currency);

            _cart.Shipments.Clear();
            _cart.Shipments.Add(shipment);

            await EvaluatePromotionsAsync();

            return this;
        }

        public async Task<CartBuilder> AddPaymentAsync(PaymentMethod paymentMethod)
        {
            var payment = paymentMethod.ToPaymentModel(_cart.Total, _currency);

            _cart.Payments.Clear();
            _cart.Payments.Add(payment);

            await EvaluatePromotionsAsync();

            return this;
        }

        public async Task<CartBuilder> MergeWithCartAsync(ShoppingCart cart)
        {
            foreach (var lineItem in cart.Items)
            {
                AddLineItem(lineItem);
            }

            _cart.Coupon = cart.Coupon;

            _cart.Shipments.Clear();
            _cart.Shipments = cart.Shipments;

            _cart.Payments.Clear();
            _cart.Payments = cart.Payments;

            await EvaluatePromotionsAsync();

            return this;
        }

        public async Task SaveAsync()
        {
            var cart = _cart.ToServiceModel();

            if (_cart.IsTransient())
            {
                await _cartApi.CartModuleCreateAsync(cart);
            }
            else
            {
                await _cartApi.CartModuleUpdateAsync(cart);
            }
        }

        public ShoppingCart Cart
        {
            get
            {
                return _cart;
            }
        }

        private void AddLineItem(LineItem lineItem)
        {
            var existingLineItem = _cart.Items.FirstOrDefault(li => li.Sku == lineItem.Sku);
            if (existingLineItem != null)
            {
                existingLineItem.Quantity += lineItem.Quantity;
            }
            else
            {
                lineItem.Id = null;
                _cart.Items.Add(lineItem);
            }
        }

        private async Task EvaluatePromotionsAsync()
        {
            var promotionContext = new VirtoCommerceDomainMarketingModelPromotionEvaluationContext
            {
                CartTotal = (double)_cart.Total.Amount,
                Coupon = _cart.Coupon != null ? _cart.Coupon.Code : null,
                CustomerId = _customer.Id,
                StoreId = _store.Id,
            };

            _cart.Discounts.Clear();
            foreach (var lineItem in _cart.Items)
            {
                lineItem.Discounts.Clear();
            }
            foreach (var shipment in _cart.Shipments)
            {
                shipment.Discounts.Clear();
            }

            promotionContext.CartPromoEntries = _cart.Items.Select(i => i.ToPromotionItem()).ToList();
            promotionContext.PromoEntries = promotionContext.CartPromoEntries;

            var rewards = await _marketingApi.MarketingModulePromotionEvaluatePromotionsAsync(promotionContext);
            var validRewards = rewards.Where(pr => pr.IsValid.HasValue && pr.IsValid.Value);

            foreach (var validReward in validRewards)
            {
                if (validReward.RewardType.Equals("CatalogItemAmountReward", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(validReward.ProductId))
                {
                    var lineItem = _cart.Items.FirstOrDefault(i => i.ProductId == validReward.ProductId);
                    if (lineItem != null)
                    {
                        lineItem.Discounts.Add(new Discount
                        {
                            Amount = GetAbsoluteDiscountAmount(lineItem.ExtendedPrice.Amount, validReward),
                            Description = validReward.Promotion.Description,
                            PromotionId = validReward.Promotion.Id,
                            Type = PromotionRewardType.CatalogItemAmountReward
                        });
                    }
                }

                if (validReward.RewardType.Equals("ShipmentReward", StringComparison.OrdinalIgnoreCase))
                {
                    var shipment = _cart.Shipments.FirstOrDefault();
                    if (shipment != null)
                    {
                        shipment.Discounts.Add(new Discount
                        {
                            Amount = GetAbsoluteDiscountAmount(_cart.SubTotal.Amount, validReward),
                            Description = validReward.Promotion.Description,
                            PromotionId = validReward.Promotion.Id,
                            Type = PromotionRewardType.ShipmentReward
                        });
                    }
                }

                if (validReward.RewardType.Equals("CartSubtotalReward", StringComparison.OrdinalIgnoreCase))
                {
                    var coupon = validReward.Promotion.Coupons.FirstOrDefault();
                    if (coupon != null)
                    {
                        var absoluteAmount = GetAbsoluteDiscountAmount(_cart.SubTotal.Amount, validReward);
                        _cart.Discounts.Add(new Discount
                        {
                            Amount = absoluteAmount,
                            Description = validReward.Promotion.Description,
                            PromotionId = validReward.Promotion.Id,
                            Type = PromotionRewardType.CartSubtotalReward
                        });

                        _cart.Coupon = new Coupon
                        {
                            Amount = absoluteAmount,
                            AppliedSuccessfully = true,
                            Code = coupon,
                            Description = validReward.Promotion.Description
                        };
                    }
                }
            }

            var couponDiscount = _cart.Discounts.FirstOrDefault(d => d.Type == PromotionRewardType.CartSubtotalReward);
            if (_cart.Coupon != null && couponDiscount == null)
            {
                _cart.Errors.Add("InvalidCouponCode");
            }
        }

        private Money GetAbsoluteDiscountAmount(decimal originalSum, VirtoCommerceMarketingModuleWebModelPromotionReward reward)
        {
            decimal absoluteDiscountAmount = 0;

            if (reward.AmountType.Equals("Absolute", StringComparison.OrdinalIgnoreCase))
            {
                absoluteDiscountAmount = (decimal)(reward.Amount ?? 0);
            }
            if (reward.AmountType.Equals("Relative", StringComparison.OrdinalIgnoreCase))
            {
                absoluteDiscountAmount = originalSum * (decimal)(reward.Amount ?? 0) / 100;
            }

            return new Money(absoluteDiscountAmount, _currency.Code);
        }
    }
}