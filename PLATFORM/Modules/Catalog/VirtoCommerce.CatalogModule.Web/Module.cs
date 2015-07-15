﻿using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Microsoft.Practices.Unity;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.CatalogModule.Web.ExportImport;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;

namespace VirtoCommerce.CatalogModule.Web
{
	public class Module : ModuleBase, ISupportExportModule, ISupportImportModule
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IDatabaseModule Members

        public override void SetupDatabase(SampleDataLevel sampleDataLevel)
        {
            base.SetupDatabase(sampleDataLevel);

            using (var db = new CatalogRepositoryImpl(_connectionStringName))
            {
                IDatabaseInitializer<CatalogRepositoryImpl> initializer;

                switch (sampleDataLevel)
                {
                    case SampleDataLevel.Full:
                        initializer = new SqlCatalogSampleDatabaseInitializer();
                        break;
                    case SampleDataLevel.Reduced:
                        initializer = new SqlCatalogReducedSampleDatabaseInitializer();
                        break;
                    default:
                        initializer = new SetupDatabaseInitializer<CatalogRepositoryImpl, Data.Migrations.Configuration>();
                        break;
                }

                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            #region Catalog dependencies

            Func<ICatalogRepository> catalogRepFactory = () =>
                new CatalogRepositoryImpl(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor(),
                    new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { typeof(Product).Name }));

            _container.RegisterInstance(catalogRepFactory);

            _container.RegisterType<IItemService, ItemServiceImpl>();
            _container.RegisterType<ICategoryService, CategoryServiceImpl>();
            _container.RegisterType<ICatalogService, CatalogServiceImpl>();
            _container.RegisterType<IPropertyService, PropertyServiceImpl>();
            _container.RegisterType<ICatalogSearchService, CatalogSearchServiceImpl>();
			_container.RegisterType<ISkuGenerator, DefaultSkuGenerator>();

            #endregion

      
        }

        #endregion

		#region ISupportExport Members

		public void DoExport(System.IO.Stream outStream, Action<ExportImportProgressInfo> progressCallback)
		{
			var settingManager = _container.Resolve<ISettingsManager>();
			var commerceService = _container.Resolve<ICommerceService>();
			var curencySetting = settingManager.GetSettingByName("VirtoCommerce.Core.General.Currencies");
			var defaultCurrency = EnumUtility.SafeParse<CurrencyCodes>(curencySetting.DefaultValue, CurrencyCodes.USD);

			var exportJob = _container.Resolve<CsvCatalogExporter>();
			var catalogService = _container.Resolve<ICatalogService>();
			var configuration = CsvProductMappingConfiguration.GetDefaultConfiguration();
			configuration.Mode = CsvExportImportMode.Catalog;

			foreach(var catalog in catalogService.GetCatalogsList())
			{
				var exportInfo = new CsvExportInfo
				{
					CatalogId = catalog.Id,
					Currency = defaultCurrency,
					FulfilmentCenterId = commerceService.GetAllFulfillmentCenters().First().Id
				};
				exportJob.DoExport(outStream, configuration, exportInfo, progressCallback);
			}
		
		}

		#endregion

		#region ISupportImportModule Members

		public void DoImport(System.IO.Stream inputStream, Action<ExportImportProgressInfo> progressCallback)
		{
			var configuration = CsvProductMappingConfiguration.GetDefaultConfiguration();
			configuration.Mode = CsvExportImportMode.Catalog;
			var importer = _container.Resolve<CsvCatalogImporter>();
			//Read csv headers and try to auto map fields by name
			using (var reader = new CsvReader(new StreamReader(inputStream, Encoding.UTF8, true, 1024, true)))
			{
				reader.Configuration.Delimiter = configuration.Delimiter;
				if (reader.Read())
				{
					configuration.AutoMap(reader.FieldHeaders);
				}
			}

			importer.DoImport(inputStream, configuration, progressCallback);

		}

		#endregion
	}
}
