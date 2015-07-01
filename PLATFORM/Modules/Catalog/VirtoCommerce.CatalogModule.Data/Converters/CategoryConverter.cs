﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using Omu.ValueInjecter;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using VirtoCommerce.Domain.Commerce.Model;

namespace VirtoCommerce.CatalogModule.Data.Converters
{
    public static class CategoryConverter
    {
        /// <summary>
        /// Converting to model type
        /// </summary>
        /// <param name="dbCategoryBase">The database category base.</param>
        /// <param name="catalog">The catalog.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">catalog</exception>
        public static coreModel.Category ToCoreModel(this dataModel.CategoryBase dbCategoryBase, coreModel.Catalog catalog,
                                                    coreModel.Property[] properties = null,  dataModel.Category[] allParents = null)
        {
            if (catalog == null)
                throw new ArgumentNullException("catalog");

			var retVal = new coreModel.Category();
			retVal.InjectFrom(dbCategoryBase);
			retVal.CatalogId = catalog.Id;
			retVal.Catalog = catalog;
			retVal.ParentId = dbCategoryBase.ParentCategoryId;
			retVal.IsActive = dbCategoryBase.IsActive;

            var dbCategory = dbCategoryBase as dataModel.Category;
            if (dbCategory != null)
            {
                retVal.PropertyValues = dbCategory.CategoryPropertyValues.Select(x => x.ToCoreModel(properties)).ToList();
                retVal.Virtual = catalog.Virtual;
				retVal.Links = dbCategory.OutgoingLinks.Select(x => x.ToCoreModel(retVal)).ToList();
            }

            if (allParents != null)
            {
                retVal.Parents = allParents.Select(x => x.ToCoreModel(catalog)).ToArray();
            }

			#region Images
			if (dbCategory.Images != null)
			{
				retVal.Images = dbCategory.Images.OrderBy(x => x.SortOrder).Select(x => x.ToCoreModel()).ToList();
			}
			#endregion

            return retVal;

        }

        /// <summary>
        /// Converting to foundation type
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public static dataModel.CategoryBase ToDataModel(this coreModel.Category category)
        {
			var retVal = new dataModel.Category();

			var id = retVal.Id;
			retVal.InjectFrom(category);
			if(category.Id == null)
			{
				retVal.Id = id;
			}
			retVal.ParentCategoryId = category.ParentId;
			retVal.EndDate = DateTime.UtcNow.AddYears(100);
			retVal.StartDate = DateTime.UtcNow;
			retVal.IsActive = category.IsActive ?? true;
          
            if (category.PropertyValues != null)
            {
                retVal.CategoryPropertyValues = new ObservableCollection<dataModel.CategoryPropertyValue>();
                retVal.CategoryPropertyValues.AddRange(category.PropertyValues.Select(x => x.ToDataModel<dataModel.CategoryPropertyValue>()).OfType<dataModel.CategoryPropertyValue>());
            }

            if (category.Links != null)
            {
				retVal.OutgoingLinks = new ObservableCollection<dataModel.CategoryRelation>();
				retVal.OutgoingLinks.AddRange(category.Links.Select(x => x.ToDataModel(category)));
            }

			#region Images
			if (category.Images != null)
			{
				retVal.Images = new ObservableCollection<dataModel.Image>(category.Images.Select(x=>x.ToDataModel()));
			}
			#endregion

            return retVal;
        }

        /// <summary>
        /// Patch changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
		public static void Patch(this coreModel.Category source, dataModel.Category target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

			//TODO: temporary solution because partial update replaced not nullable properties in db entity
			if (source.IsActive != null)
				target.IsActive = source.IsActive.Value;

			var dbSource = source.ToDataModel() as dataModel.Category;
			var dbTarget = target as dataModel.Category;

            if (dbSource != null && dbTarget != null)
            {
				var patchInjectionPolicy = new PatchInjection<dataModel.Category>(x => x.Code, x=>x.Name);
				target.InjectFrom(patchInjectionPolicy, source);

                if (!dbSource.CategoryPropertyValues.IsNullCollection())
                {
                    dbSource.CategoryPropertyValues.Patch(dbTarget.CategoryPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
                }

				if(!dbSource.OutgoingLinks.IsNullCollection())
				{
					dbSource.OutgoingLinks.Patch(dbTarget.OutgoingLinks, new LinkedCategoryComparer(), (sourceLink, targetLink) => sourceLink.Patch(targetLink));
				}

				if (!dbSource.Images.IsNullCollection())
				{
					dbSource.Images.Patch(dbTarget.Images, (sourceImage, targetImage) => sourceImage.Patch(targetImage));
				}
            }
        }
    }
}
