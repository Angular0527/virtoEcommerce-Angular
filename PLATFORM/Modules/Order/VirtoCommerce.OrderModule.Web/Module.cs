﻿using System;
using System.Data.Entity;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Services;
using VirtoCommerce.OrderModule.Data.Observers;
using VirtoCommerce.OrderModule.Data.Repositories;
using VirtoCommerce.OrderModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.OrderModule.Web
{
    public class Module : ModuleBase
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase(SampleDataLevel sampleDataLevel)
        {
            using (var context = new OrderRepositoryImpl(_connectionStringName))
            {
                IDatabaseInitializer<OrderRepositoryImpl> initializer;

                switch (sampleDataLevel)
                {
                    case SampleDataLevel.Full:
                    case SampleDataLevel.Reduced:
                        initializer = new OrderSampleDatabaseInitializer();
                        break;
                    default:
                        initializer = new SetupDatabaseInitializer<OrderRepositoryImpl, Data.Migrations.Configuration>();
                        break;
                }

                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            _container.RegisterType<IEventPublisher<OrderChangeEvent>, EventPublisher<OrderChangeEvent>>();
            //Subscribe to order changes. Calculate totals   
            _container.RegisterType<IObserver<OrderChangeEvent>, CalculateTotalsObserver>("CalculateTotalsObserver");
            //Adjust inventory activity
            _container.RegisterType<IObserver<OrderChangeEvent>, AdjustInventoryObserver>("AdjustInventoryObserver");

            _container.RegisterType<IOrderRepository>(new InjectionFactory(c => new OrderRepositoryImpl("VirtoCommerce", new AuditableInterceptor(), new EntityPrimaryKeyGeneratorInterceptor())));
            //_container.RegisterInstance<IInventoryService>(new Mock<IInventoryService>().Object);
            _container.RegisterType<IOperationNumberGenerator, TimeBasedNumberGeneratorImpl>();

            _container.RegisterType<ICustomerOrderService, CustomerOrderServiceImpl>();
            _container.RegisterType<ICustomerOrderSearchService, CustomerOrderSearchServiceImpl>();
        }

        public override void PostInitialize()
        {
            var cacheManager = _container.Resolve<CacheManager>();
            var cacheSettings = new[] 
			{
				new CacheSettings("Statistic", TimeSpan.FromHours(1))
			};
            cacheManager.AddCacheSettings(cacheSettings);
        }

        #endregion
    }
}
