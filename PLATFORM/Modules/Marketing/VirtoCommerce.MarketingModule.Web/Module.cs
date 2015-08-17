﻿using System;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Marketing.Model;
using VirtoCommerce.Domain.Marketing.Services;
using VirtoCommerce.MarketingModule.Data.Repositories;
using VirtoCommerce.MarketingModule.Data.Services;
using VirtoCommerce.MarketingModule.Web.ExportImport;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.MarketingModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase(SampleDataLevel sampleDataLevel)
        {
            using (var context = new MarketingRepositoryImpl())
            {
                var initializer = new SetupDatabaseInitializer<MarketingRepositoryImpl, VirtoCommerce.MarketingModule.Data.Migrations.Configuration>();
                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            _container.RegisterType<IMarketingRepository>(new InjectionFactory(c => new MarketingRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor())));

            var promotionExtensionManager = new DefaultMarketingExtensionManagerImpl();

            _container.RegisterInstance<IMarketingExtensionManager>(promotionExtensionManager);
            _container.RegisterType<IPromotionService, PromotionServiceImpl>();
            _container.RegisterType<IMarketingDynamicContentEvaluator, DefaultDynamicContentEvaluatorImpl>();
            _container.RegisterType<IDynamicContentService, DynamicContentServiceImpl>();
            _container.RegisterType<IMarketingSearchService, MarketingSearchServiceImpl>();
            _container.RegisterType<IMarketingPromoEvaluator, DefaultPromotionEvaluatorImpl>();
        }

        public override void PostInitialize()
        {
            var promotionExtensionManager = _container.Resolve<IMarketingExtensionManager>();
            EnsureRootFoldersExist(new[] { VirtoCommerce.MarketingModule.Web.Model.MarketingConstants.ContentPlacesRootFolderId, VirtoCommerce.MarketingModule.Web.Model.MarketingConstants.CotentItemRootFolderId });

			//Create standard dynamic properties for dynamic content item (content type (dic) and html (long text))
			var dynamicPropertyService = _container.Resolve<IDynamicPropertyService>();
			var contentItemTypeProperty = new DynamicProperty
			{
				Id = "Marketing_DynamicContentItem_Type_Property",
				IsDictionary = true,
				Name = "Content type",
				ObjectType = typeof(DynamicContentItem).FullName,
				ValueType = DynamicPropertyValueType.ShortText,
				CreatedBy = "Auto",
			};
			var htmlProperty = new DynamicProperty
			{
				Id = "Marketing_DynamicContentItem_Html_Property",
				Name = "Html",
				ObjectType = typeof(DynamicContentItem).FullName,
				ValueType = DynamicPropertyValueType.LongText,
				CreatedBy = "Auto",
			};
			var dictItemHtml = new DynamicPropertyDictionaryItem
			{
				 Id = "Html",
				 Name = "Html"
			};
			dynamicPropertyService.SaveProperties(new [] { contentItemTypeProperty, htmlProperty });
			dynamicPropertyService.SaveDictionaryItems(contentItemTypeProperty.Id,  new [] { dictItemHtml });
		}


        #endregion

		#region ISupportExportImportModule Members

		public void DoExport(System.IO.Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<MarketingExportImport>();
            exportJob.DoExport(outStream, progressCallback);
        }

		public void DoImport(System.IO.Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<MarketingExportImport>();
            exportJob.DoImport(inputStream, progressCallback);
        }

		public string ExportDescription
		{
			get
			{
				var settingManager = _container.Resolve<ISettingsManager>();
				return settingManager.GetValue("Marketing.ExportImport.Description", String.Empty);
			}
		}
        #endregion


        private void EnsureRootFoldersExist(string[] ids)
        {
            var dynamicContentService = _container.Resolve<IDynamicContentService>();
            foreach (var id in ids)
            {
                var rootFolder = dynamicContentService.GetFolderById(id);
                if (rootFolder == null)
                {
                    rootFolder = new Domain.Marketing.Model.DynamicContentFolder
                    {
                        Id = id,
                        Name = id
                    };
                    dynamicContentService.CreateFolder(rootFolder);
                }
            }

        }
    }
}
