﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using VirtoCommerce.Client.Api;
using VirtoCommerce.Client.Model;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Pricing.Services;
using VirtoCommerce.Storefront.Converters;

namespace VirtoCommerce.Storefront.Services
{
    public class PricingServiceImpl : IPricingService
    {
        private readonly IPricingModuleApi _pricingApi;
        private readonly Func<WorkContext> _workContextFactory;
        public PricingServiceImpl(Func<WorkContext> workContextFactory, IPricingModuleApi pricingApi)
        {
            _pricingApi = pricingApi;
            _workContextFactory = workContextFactory;
        }

        #region IPricingService Members
        public async Task EvaluateProductPricesAsync(IEnumerable<Product> products)
        {
            var workContext = _workContextFactory();

            //Evaluate products prices
            var evalContext = new VirtoCommerceDomainPricingModelPriceEvaluationContext
            {
                ProductIds = products.Select(p => p.Id).ToList(),
                PricelistIds = workContext.CurrentPricelists.Select(p => p.Id).ToList(),
                CatalogId = workContext.CurrentStore.Catalog,
                CustomerId = workContext.CurrentCustomer.Id,
                Language = workContext.CurrentLanguage.CultureName,
                CertainDate = workContext.StorefrontUtcNow,
                StoreId = workContext.CurrentStore.Id
            };

            var pricesResponse = await _pricingApi.PricingModuleEvaluatePricesAsync(evalContext);

            var alreadyDefinedProductsPriceGroups = pricesResponse.Select(x => x.ToWebModel(workContext.AllCurrencies, workContext.CurrentLanguage)).GroupBy(x => x.ProductId);
            foreach (var product in products)
            {
                var productPricesGroup = alreadyDefinedProductsPriceGroups.FirstOrDefault(x => x.Key == product.Id);
                if (productPricesGroup != null)
                {
                    //Get first price for each currency
                    product.Prices = productPricesGroup.GroupBy(x => x.Currency).Select(x => x.FirstOrDefault()).Where(x => x != null).ToList();
                }
                //Need add product price for all store currencies (even if not returned from api need make it by currency exchange convertation)
                foreach (var storeCurrency in workContext.CurrentStore.Currencies)
                {
                    var price = product.Prices.FirstOrDefault(x => x.Currency == storeCurrency);
                    if (price == null)
                    {
                        price = new ProductPrice(storeCurrency);
                        if (product.Prices.Any())
                        {
                            price = product.Prices.First().ConvertTo(storeCurrency);
                        }
                        product.Prices.Add(price);
                    }
                }
                product.Currency = workContext.CurrentCurrency;
                product.Price = product.Prices.FirstOrDefault(x => x.Currency.Equals(workContext.CurrentCurrency));
            }

        }

        #endregion
    }
}