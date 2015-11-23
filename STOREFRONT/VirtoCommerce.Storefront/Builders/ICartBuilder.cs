﻿using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Builders
{
    public interface ICartBuilder
    {
        Task<CartBuilder> GetOrCreateNewTransientCartAsync(Store store, Customer customer, Currency currency);

        CartBuilder AddItem(Product product, int quantity);

        CartBuilder UpdateItem(int index, int quantity);

        Task SaveAsync();

        ShoppingCart Cart { get; }
    }
}