﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.Quote.Services
{
    public interface IQuoteService
    {
        Task<ICollection<QuoteRequest>> GetQuoteRequestsAsync(string storeId, string customerId, int skip, int take);

        Task<QuoteRequest> GetQuoteRequestAsync(string customerId, string quoteRequestNumber);

        Task UpdateQuoteRequestAsync(QuoteRequest quoteRequest);

        Task RemoveQuoteRequestAsync(string customerId, string quoteRequestId);
    }
}