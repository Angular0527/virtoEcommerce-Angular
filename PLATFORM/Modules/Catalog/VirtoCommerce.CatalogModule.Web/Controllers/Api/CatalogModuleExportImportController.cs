﻿using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Hangfire;
using VirtoCommerce.CatalogModule.Web.BackgroundJobs;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.CatalogModule.Web.Model.EventNotifications;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Asset;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Notification;
using System.Linq;
using CsvHelper;
using System.IO;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using System.Collections.Generic;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Platform.Core.Settings;
namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
	[RoutePrefix("api/catalog")]
	public class CatalogModuleExportImportController : ApiController
	{
		private readonly ICatalogService _catalogService;
		private readonly INotifier _notifier;
		private readonly ISettingsManager _settingsManager;
		private readonly IBlobStorageProvider _blobStorageProvider;

		public CatalogModuleExportImportController(ICatalogService catalogService, INotifier notifier, ISettingsManager settingsManager, IBlobStorageProvider blobStorageProvider)
		{
			_catalogService = catalogService;
			_notifier = notifier;
			_settingsManager = settingsManager;
			_blobStorageProvider = blobStorageProvider;

		}

		/// <summary>
		/// GET api/catalog/export/sony
		/// </summary>
		/// <param name="id"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		[ResponseType(typeof(ExportNotification))]
		[HttpPost]
		[Route("export")]
		public IHttpActionResult DoExport(CsvExportInfo exportInfo)
		{
			var notification = new ExportNotification(CurrentPrincipal.GetCurrentUserName())
			{
				Title = "Catalog export task",
				Description = "starting export...."
			};
			_notifier.Upsert(notification);

			var catalog = _catalogService.GetById(exportInfo.CatalogId);
			if (catalog == null)
			{
				throw new NullReferenceException("catalog");
			}
			var curencySetting = _settingsManager.GetSettingByName("VirtoCommerce.Core.General.Currencies");
			var defaultCurrency = EnumUtility.SafeParse<CurrencyCodes>(curencySetting.DefaultValue, CurrencyCodes.USD);

			var exportJob = new CsvCatalogExportJob();
			BackgroundJob.Enqueue(() => exportJob.DoExport(exportInfo.CatalogId, exportInfo.CategoryIds, exportInfo.ProductIds,
														   exportInfo.PriceListId, exportInfo.FulfilmentCenterId, exportInfo.Currency ?? defaultCurrency,
														   catalog.DefaultLanguage.LanguageCode, notification));

			return Ok(notification);

		}


		/// <summary>
		/// GET api/catalog/import/mapping?path='c:\\sss.csv'&importType=product&delimiter=,
		/// </summary>
		/// <param name="templatePath"></param>
		/// <param name="importerType"></param>
		/// <param name="delimiter"></param>
		/// <returns></returns>
		[ResponseType(typeof(CsvImportConfiguration))]
		[HttpGet]
		[Route("import/mappingconfiguration")]
		public IHttpActionResult GetMappingConfiguration([FromUri]string fileUrl, [FromUri]string delimiter = ";")
		{
			var retVal = new CsvImportConfiguration
				{
					Delimiter = delimiter,
					FileUrl = fileUrl
				};
			var mappingItems = new List<CsvImportMappingItem>();

			mappingItems.AddRange(ReflectionUtility.GetPropertyNames<coreModel.CatalogProduct>(x => x.Name, x => x.Category).Select(x => new CsvImportMappingItem { EntityColumnName = x, IsRequired = true }));

			mappingItems.AddRange(new string[] {"Sku", "ParentSku", "Review", "PrimaryImage", "AltImage", "SeoUrl", "SeoDescription", "SeoTitle", 
												"PriceId", "Price", "SalePrice", "Currency", "AllowBackorder", "Quantity", "FulfilmentCenterId" }
								   .Select(x => new CsvImportMappingItem { EntityColumnName = x, IsRequired = false }));

			mappingItems.AddRange(ReflectionUtility.GetPropertyNames<coreModel.CatalogProduct>(x => x.Id, x => x.MainProductId, x => x.CategoryId, x => x.IsActive, x => x.IsBuyable, x => x.TrackInventory,
																							  x => x.ManufacturerPartNumber, x => x.Gtin, x => x.MeasureUnit, x => x.WeightUnit, x => x.Weight,
																							  x => x.Height, x => x.Length, x => x.Width, x => x.TaxType, x => x.ProductType, x => x.ShippingType,
																							  x => x.Vendor, x => x.DownloadType, x => x.DownloadExpiration, x => x.HasUserAgreement).Select(x => new CsvImportMappingItem { EntityColumnName = x, IsRequired = false }));



			retVal.MappingItems = mappingItems.ToArray();


			//Read csv headers and try to auto map fields by name
			using (var reader = new CsvReader(new StreamReader(_blobStorageProvider.OpenReadOnly(fileUrl))))
			{
				reader.Configuration.Delimiter = delimiter;
				while (reader.Read())
				{
					var csvColumns = reader.FieldHeaders;
					retVal.CsvColumns = csvColumns;
					//default columns mapping
					if (csvColumns.Any())
					{
						foreach (var mappingItem in retVal.MappingItems)
						{
							var entityColumnName = mappingItem.EntityColumnName;
							var betterMatchCsvColumn = csvColumns.Select(x => new { csvColumn = x, distance = x.ComputeLevenshteinDistance(entityColumnName) })
																 .Where(x => x.distance < 2)
																 .OrderBy(x => x.distance)
																 .Select(x => x.csvColumn)
																 .FirstOrDefault();
							if (betterMatchCsvColumn != null)
							{
								mappingItem.CsvColumnName = betterMatchCsvColumn;
								mappingItem.CustomValue = null;
							}
						}
					}
				}
			}
			//All not mapped properties may be a product property
			retVal.PropertyCsvColumns = retVal.CsvColumns.Except(retVal.MappingItems.Where(x => x.CsvColumnName != null).Select(x => x.CsvColumnName)).ToArray();
			//Generate ETag for identifying csv format
			retVal.ETag = string.Join(";", retVal.CsvColumns).GetMD5Hash();
			return Ok(retVal);
		}


		/// <summary>
		/// GET api/catalog/import/sony
		/// </summary>
		/// <param name="id"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		[ResponseType(typeof(ExportNotification))]
		[HttpPost]
		[Route("import")]
		public IHttpActionResult DoImport(CsvImportConfiguration importConfiguration)
		{
			var notification = new ImportNotification(CurrentPrincipal.GetCurrentUserName())
			{
				Title = "Import catalog from CSV",
				Description = "starting import...."
			};
			_notifier.Upsert(notification);

			var importJob = new CsvCatalogImportJob();
			BackgroundJob.Enqueue(() => importJob.DoImport(importConfiguration, notification));

			return Ok(notification);

		}


		/// <summary>
		///  GET api/catalog/importjobs/123/cancel
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("{id}/cancel")]
		[ResponseType(typeof(void))]
		public IHttpActionResult Cancel(string id)
		{
			return StatusCode(HttpStatusCode.NoContent);
			//var job = _jobList.FirstOrDefault(x => x.Id == id);
			//if (job != null && job.CanBeCanceled)
			//{
			//	job.CancellationToken.Cancel();
			//}

			//return StatusCode(HttpStatusCode.NoContent);
		}


	}
}