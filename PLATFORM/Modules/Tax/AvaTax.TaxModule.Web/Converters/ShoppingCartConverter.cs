﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using AvaTaxCalcREST;
using Microsoft.Practices.ObjectBuilder2;
using AddressType = VirtoCommerce.Domain.Cart.Model.AddressType;

namespace AvaTax.TaxModule.Web.Converters
{
    public static class ShoppingCartConverter
    {
        public static GetTaxRequest ToAvaTaxRequest(
            this VirtoCommerce.Domain.Cart.Model.ShoppingCart cart,
            string companyCode,
            bool commit = false)
        {
            if (cart.Addresses != null && cart.Addresses.Any() && cart.Items != null && cart.Items.Any())
            {
                var getTaxRequest = new GetTaxRequest
                {
                    CustomerCode = cart.CustomerId,
                    DocDate = cart.CreatedDate.ToShortDateString(),
                    CompanyCode = companyCode,
                    Client = "VirtoCommerce,2.x,VirtoCommerce",
                    DocCode = cart.Id,
                    DetailLevel = DetailLevel.Tax,
                    Commit = false,
                    DocType = DocType.SalesOrder
                };

                // Document Level Elements
                // Required Request Parameters

                // Best Practice Request Parameters

                // Situational Request Parameters
                // getTaxRequest.CustomerUsageType = "G";
                // getTaxRequest.ExemptionNo = "12345";
                // getTaxRequest.BusinessIdentificationNo = "234243";
                // getTaxRequest.Discount = 50;
                // getTaxRequest.TaxOverride = new TaxOverrideDef();
                // getTaxRequest.TaxOverride.TaxOverrideType = "TaxDate";
                // getTaxRequest.TaxOverride.Reason = "Adjustment for return";
                // getTaxRequest.TaxOverride.TaxDate = "2013-07-01";
                // getTaxRequest.TaxOverride.TaxAmount = "0";

                // Optional Request Parameters
                //getTaxRequest.PurchaseOrderNo = order.Id;
                //getTaxRequest.ReferenceCode = "ref123456";
                //getTaxRequest.PosLaneCode = "09";
                //getTaxRequest.CurrencyCode = order.Currency.ToString();

                // Address Data
                string destinationAddressIndex = "0";

                // Address Data
                var addresses = new List<Address>();

                foreach (var address in cart.Addresses.Select((x, i) => new { Value = x, Index = i }))
                {

                    addresses.Add(
                        new Address
                        {
                            AddressCode = address.Index.ToString(),
                            Line1 = address.Value.Line1,
                            City = address.Value.City,
                            Region = address.Value.RegionName ?? address.Value.RegionId,
                            PostalCode = address.Value.PostalCode,
                            Country = address.Value.CountryName
                        });

                    if (address.Value.AddressType == AddressType.Shipping
                        || address.Value.AddressType == AddressType.Shipping)
                        destinationAddressIndex = address.Index.ToString(CultureInfo.InvariantCulture);
                }

                getTaxRequest.Addresses = addresses;

                // Line Data
                // Required Parameters

                getTaxRequest.Lines = cart.Items.Select((x, i) => new { Value = x, Index = i }).Select(li =>
                    new Line
                    {
                        LineNo = li.Value.Id,
                        ItemCode = li.Value.ProductId,
                        Qty = li.Value.Quantity,
                        Amount = li.Value.PlacedPrice,
                        OriginCode = destinationAddressIndex, //TODO set origin address (fulfillment?)
                        DestinationCode = destinationAddressIndex,
                        Description = li.Value.Name,
                        TaxCode = li.Value.TaxType
                    }
                    ).ToList();

                //Add shipments as lines
                if (cart.Shipments != null && cart.Shipments.Any())
                {
                    cart.Shipments.Select((x, i) => new { Value = x, Index = i }).ForEach(li =>
                    getTaxRequest.Lines.Add(new Line
                    {
                        LineNo = li.Value.Id,
                        ItemCode = li.Value.ShipmentMethodCode,
                        Qty = 1,
                        Amount = li.Value.ShippingPrice,
                        OriginCode = destinationAddressIndex, //TODO set origin address (fulfillment?)
                        DestinationCode = destinationAddressIndex,
                        Description = li.Value.ShipmentMethodCode,
                        TaxCode = li.Value.TaxType
                    })
                    );
                }
                return getTaxRequest;
            }

            return null;
        }
    }
}