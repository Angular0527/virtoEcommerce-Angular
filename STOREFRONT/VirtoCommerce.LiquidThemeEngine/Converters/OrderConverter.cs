﻿using System;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.LiquidThemeEngine.Converters.Injections;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class OrderConverter
    {
        public static Order ToShopifyModel(this CustomerOrder order, IStorefrontUrlBuilder urlBuilder)
        {
            var result = new Order();
            result.InjectFrom<NullableAndEnumValueInjection>(order);

            result.Cancelled = order.IsCancelled == true;
            result.CancelledAt = order.CancelledDate;
            result.CancelReason = order.CancelReason;
            result.CancelReasonLabel = order.CancelReason;
            result.CreatedAt = order.CreatedDate ?? DateTime.MinValue;
            result.Name = order.Number;
            result.OrderNumber = order.Number;
            result.CustomerUrl = urlBuilder.ToAppAbsolute("/account/order/" + order.Id);
            result.TotalPrice = order.Sum.Amount;

            if (order.Addresses != null)
            {
                var shippingAddress = order.Addresses
                    .FirstOrDefault(a => (a.Type & AddressType.Shipping) == AddressType.Shipping);

                if (shippingAddress != null)
                {
                    result.ShippingAddress = shippingAddress.ToShopifyModel();
                }

                var billingAddress = order.Addresses
                    .FirstOrDefault(a => (a.Type & AddressType.Billing) == AddressType.Billing);

                if (billingAddress != null)
                {
                    result.BillingAddress = billingAddress.ToShopifyModel();
                }
                else if (shippingAddress != null)
                {
                    result.BillingAddress = shippingAddress.ToShopifyModel();
                }

                result.Email = order.Addresses
                    .Where(a => !string.IsNullOrEmpty(a.Email))
                    .Select(a => a.Email)
                    .FirstOrDefault();
            }

            if (order.Discount != null)
            {
                result.Discounts = new[] { order.Discount.ToShopifyModel() };
            }

            var taxLines = new List<TaxLine>();

            if (order.InPayments != null)
            {
                var inPayment = order.InPayments
                    .OrderByDescending(p => p.CreatedDate)
                    .FirstOrDefault();

                if (inPayment != null)
                {
                    if (string.IsNullOrEmpty(inPayment.Status))
                    {
                        result.FinancialStatus = inPayment.IsApproved == true ? "Paid" : "Pending";
                        result.FinancialStatusLabel = inPayment.IsApproved == true ? "Paid" : "Pending";
                    }
                    else
                    {
                        result.FinancialStatus = inPayment.Status;
                        result.FinancialStatusLabel = inPayment.Status;
                    }

                    if (inPayment.TaxIncluded == true)
                    {
                        taxLines.Add(new TaxLine { Title = "Payments tax", Price = inPayment.Tax.Amount });
                    }
                }
            }

            if (order.Shipments != null)
            {
                result.ShippingMethods = order.Shipments.Select(s => s.ToShopifyModel()).ToArray();

                var orderShipment = order.Shipments.FirstOrDefault();

                if (orderShipment != null)
                {
                    if (string.IsNullOrEmpty(orderShipment.Status))
                    {
                        result.FulfillmentStatus = orderShipment.IsApproved == true ? "Sent" : "Not sent";
                        result.FulfillmentStatusLabel = orderShipment.IsApproved == true ? "Sent" : "Not sent";
                    }
                    else
                    {
                        result.FulfillmentStatus = orderShipment.Status;
                        result.FulfillmentStatusLabel = orderShipment.Status;
                    }

                    if (orderShipment.TaxIncluded == true)
                    {
                        taxLines.Add(new TaxLine { Title = "Shipping tax", Price = orderShipment.Tax.Amount });
                    }
                }

                var taxableShipments = order.Shipments
                    .Where(s => s.Tax.Amount > 0)
                    .ToList();

                if (taxableShipments.Count > 0)
                {
                    taxLines.Add(new TaxLine
                    {
                        Title = "Shipping",
                        Price = taxableShipments.Sum(s => s.Tax.Amount),
                        Rate = taxableShipments.Where(s => s.TaxDetails != null).Sum(i => i.TaxDetails.Sum(td => td.Rate)),
                    });
                }
            }

            if (order.Items != null)
            {
                result.LineItems = order.Items.Select(i => i.ToShopifyModel()).ToArray();

                var taxableLineItems = order.Items
                    .Where(i => i.Tax.Amount > 0m)
                    .ToList();

                if (taxableLineItems.Any())
                {
                    taxLines.Add(new TaxLine
                    {
                        Title = "Line items",
                        Price = taxableLineItems.Sum(i => i.Tax.Amount),
                        Rate = taxableLineItems.Where(i => i.TaxDetails != null).Sum(i => i.TaxDetails.Sum(td => td.Rate)),
                    });
                }

                result.SubtotalPrice = result.LineItems.Sum(i => i.LinePrice);
            }

            result.TaxLines = taxLines.ToArray();

            return result;
        }
    }
}
