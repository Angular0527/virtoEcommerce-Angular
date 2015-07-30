﻿using System;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.InventoryModule.Web.ExportImport;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.InventoryModule.Web
{
    public class Module : ModuleBase, ISupportExportModule, ISupportImportModule
    {
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase(SampleDataLevel sampleDataLevel)
        {
            using (var context = new InventoryRepositoryImpl())
            {
                var initializer = new SetupDatabaseInitializer<InventoryRepositoryImpl, VirtoCommerce.InventoryModule.Data.Migrations.Configuration>();
                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            _container.RegisterType<IInventoryRepository>(new InjectionFactory(c => new InventoryRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor())));

            _container.RegisterType<IInventoryService, InventoryServiceImpl>();
        }

        #endregion

        #region ISupportExportModule Members

        public void DoExport(System.IO.Stream outStream, PlatformExportImportOptions importOptions, Action<ExportImportProgressInfo> progressCallback)
        {
            var job = _container.Resolve<InventoryExportImport>();
            job.DoExport(outStream, progressCallback);
        }

        #endregion

        #region ISupportImportModule Members

        public void DoImport(System.IO.Stream inputStream, PlatformExportImportOptions importOptions, Action<ExportImportProgressInfo> progressCallback)
        {
            var job = _container.Resolve<InventoryExportImport>();
            job.DoImport(inputStream, progressCallback);
        }

        #endregion


    }
}
