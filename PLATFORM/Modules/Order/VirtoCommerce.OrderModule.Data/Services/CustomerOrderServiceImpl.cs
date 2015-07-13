﻿using System;
using System.Linq;
using VirtoCommerce.Domain.Cart.Services;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Order.Services;
using VirtoCommerce.OrderModule.Data.Converters;
using VirtoCommerce.OrderModule.Data.Model;
using VirtoCommerce.OrderModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrderModule.Data.Services
{
    public class CustomerOrderServiceImpl : ServiceBase, ICustomerOrderService
    {
        private readonly Func<IOrderRepository> _repositoryFactory;
        private readonly IOperationNumberGenerator _operationNumberGenerator;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IItemService _productService;
        private readonly IEventPublisher<OrderChangeEvent> _eventPublisher;
        private readonly IDynamicPropertyService _dynamicPropertyService;

        public CustomerOrderServiceImpl(Func<IOrderRepository> orderRepositoryFactory, IOperationNumberGenerator operationNumberGenerator, IEventPublisher<OrderChangeEvent> eventPublisher, IShoppingCartService shoppingCartService, IItemService productService, IDynamicPropertyService dynamicPropertyService)
        {
            _repositoryFactory = orderRepositoryFactory;
            _shoppingCartService = shoppingCartService;
            _operationNumberGenerator = operationNumberGenerator;
            _eventPublisher = eventPublisher;
            _productService = productService;
            _dynamicPropertyService = dynamicPropertyService;
        }

        #region ICustomerOrderService Members

        public virtual CustomerOrder GetById(string orderId, CustomerOrderResponseGroup respGroup)
        {
            CustomerOrder retVal = null;
            using (var repository = _repositoryFactory())
            {
                var orderEntity = repository.GetCustomerOrderById(orderId, respGroup);
                if (orderEntity != null)
                {
                    retVal = orderEntity.ToCoreModel();
                }

                if (retVal != null && (respGroup & CustomerOrderResponseGroup.WithProducts) == CustomerOrderResponseGroup.WithProducts)
                {
                    var productIds = retVal.Items.Select(x => x.ProductId).ToArray();
                    var products = _productService.GetByIds(productIds, Domain.Catalog.Model.ItemResponseGroup.ItemInfo);
                    foreach (var lineItem in retVal.Items)
                    {
                        var product = products.FirstOrDefault(x => x.Id == lineItem.ProductId);
                        if (product != null)
                        {
                            lineItem.Product = product;
                        }
                    }
                }
            }

            if (retVal != null)
                _dynamicPropertyService.LoadDynamicPropertyValues(retVal);

            _eventPublisher.Publish(new OrderChangeEvent(EntryState.Unchanged, null, retVal));

            return retVal;
        }

        public virtual CustomerOrder Create(CustomerOrder order)
        {
            EnsureThatAllOperationsHaveNumber(order);

            _eventPublisher.Publish(new OrderChangeEvent(EntryState.Added, null, order));

            // TODO: for approved shipments need to decrease inventory

            var orderEntity = order.ToDataModel();

            using (var repository = _repositoryFactory())
            {
                repository.Add(orderEntity);
                CommitChanges(repository);
            }

            _dynamicPropertyService.SaveDynamicPropertyValues(order);

            var retVal = GetById(orderEntity.Id, CustomerOrderResponseGroup.Full);
            return retVal;

        }

        public virtual CustomerOrder CreateByShoppingCart(string cartId)
        {
            var shoppingCart = _shoppingCartService.GetById(cartId);

            if (shoppingCart == null)
            {
                throw new OperationCanceledException("cart not found");
            }
            var customerOrder = shoppingCart.ToCustomerOrder();
            var retVal = Create(customerOrder);

            return retVal;
        }

        public void Update(CustomerOrder[] orders)
        {

            // Need to call business logic for changes and persist changes
            using (var repository = _repositoryFactory())
            {
                foreach (var order in orders)
                {
                    EnsureThatAllOperationsHaveNumber(order);
                    var origOrder = GetById(order.Id, CustomerOrderResponseGroup.Full);

                    // Do business logic on temporary order object
                    _eventPublisher.Publish(new OrderChangeEvent(EntryState.Modified, origOrder, order));

                    var sourceOrderEntity = order.ToDataModel();
                    var targetOrderEntity = repository.GetCustomerOrderById(order.Id, CustomerOrderResponseGroup.Full);

                    if (targetOrderEntity == null)
                    {
                        throw new NullReferenceException("targetOrderEntity");
                    }

                    using (var changeTracker = GetChangeTracker(repository, targetOrderEntity))
                    {
                        changeTracker.Attach(targetOrderEntity);
                        sourceOrderEntity.Patch(targetOrderEntity);
                    }

                    _dynamicPropertyService.SaveDynamicPropertyValues(order);
                }

                CommitChanges(repository);
            }

        }

        public void Delete(string[] oderIds)
        {
            throw new NotImplementedException();
        }
        #endregion


        private ObservableChangeTracker GetChangeTracker(IRepository repository, CustomerOrderEntity customerOrderEntity)
        {
            var retVal = new ObservableChangeTracker
            {
                RemoveAction = x =>
                {
                    repository.Remove(x);
                },
                AddAction = x =>
                {
                    var address = x as AddressEntity;
                    var shipment = x as ShipmentEntity;
                    var lineItem = x as LineItemEntity;

                    if (address != null)
                    {
                        address.CustomerOrder = customerOrderEntity;
                    }
                    if (shipment != null)
                    {
                        foreach (var shipmentItem in shipment.Items)
                        {
                            var orderLineItem = customerOrderEntity.Items.FirstOrDefault(item => item.Id == shipmentItem.Id);
                            if (orderLineItem != null)
                            {
                                orderLineItem.Shipment = shipment;
                            }
                            else
                            {
                                shipmentItem.CustomerOrder = customerOrderEntity;
                            }
                        }
                        shipment.Items = new NullCollection<LineItemEntity>();
                    }
                    if (lineItem != null)
                    {
                        lineItem.CustomerOrder = customerOrderEntity;
                        lineItem.CustomerOrderId = customerOrderEntity.Id;
                    }
                    repository.Add(x);
                }
            };

            return retVal;
        }


        private void EnsureThatAllOperationsHaveNumber(CustomerOrder order)
        {
            foreach (var operation in order.Traverse<Operation>(x => x.ChildrenOperations))
            {
                if (operation.Number == null)
                {
                    operation.Number = _operationNumberGenerator.GenerateNumber(operation);
                }
            }

        }
    }
}
