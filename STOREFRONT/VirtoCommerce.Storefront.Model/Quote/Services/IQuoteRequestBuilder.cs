﻿using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer;

namespace VirtoCommerce.Storefront.Model.Quote.Services
{
    public interface IQuoteRequestBuilder
    {
        Task<IQuoteRequestBuilder> GetOrCreateNewTransientQuoteRequestAsync(Store store, CustomerInfo customer, Language language, Currency currency);

        IQuoteRequestBuilder AddItem(Product product, long quantity);

        IQuoteRequestBuilder RemoveItem(string quoteItemId);

        Task SaveAsync();

        QuoteRequest QuoteRequest { get; }
    }
}