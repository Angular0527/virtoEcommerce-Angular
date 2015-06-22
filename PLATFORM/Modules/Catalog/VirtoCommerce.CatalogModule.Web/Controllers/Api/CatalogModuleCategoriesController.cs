﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Linq;
using System.Web.Http.Description;
using VirtoCommerce.CatalogModule.Web.Converters;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Platform.Core.Security;
using webModel = VirtoCommerce.CatalogModule.Web.Model;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Domain.Commerce.Model;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [RoutePrefix("api/catalog/categories")]
    [CheckPermission(Permission = PredefinedPermissions.CategoriesManage)]
    public class CatalogModuleCategoriesController : ApiController
    {
        private readonly ICatalogSearchService _searchService;
        private readonly ICategoryService _categoryService;
        private readonly IPropertyService _propertyService;
        private readonly ICatalogService _catalogService;

        public CatalogModuleCategoriesController(ICatalogSearchService searchService,
                                    ICategoryService categoryService,
                                    IPropertyService propertyService, ICatalogService catalogService)
        {
            _searchService = searchService;
            _categoryService = categoryService;
            _propertyService = propertyService;
            _catalogService = catalogService;
        }

        // GET: api/catalog/categories/5
        [HttpGet]
        [ResponseType(typeof(webModel.Category))]
        [Route("{id}")]
        public IHttpActionResult Get(string id)
        {
            var category = _categoryService.GetById(id);

            if (category == null)
            {
                return NotFound();
            }
            var allCategoryProperties = _propertyService.GetCategoryProperties(id);
            var retVal = category.ToWebModel(allCategoryProperties);
            return Ok(retVal);
        }

        // GET: api/catalog/apple/categories/newcategory&parentCategoryId='ddd'"
        [HttpGet]
        [Route("~/api/catalog/{catalogId}/categories/newcategory")]
        [ResponseType(typeof(webModel.Category))]
        public IHttpActionResult GetNewCategory(string catalogId, [FromUri]string parentCategoryId = null)
        {
            var retVal = new webModel.Category
            {
                ParentId = parentCategoryId,
                CatalogId = catalogId,
                Catalog = _catalogService.GetById(catalogId).ToWebModel(),
                Code = Guid.NewGuid().ToString().Substring(0, 5),
                SeoInfos = new List<SeoInfo>()
            };

            return Ok(retVal);
        }


        // POST:  api/catalog/categories
        [HttpPost]
        [ResponseType(typeof(void))]
        [Route("")]
        public IHttpActionResult Post(webModel.Category category)
        {
            var coreCategory = category.ToModuleModel();
            if (coreCategory.Id == null)
			{
				if (coreCategory.SeoInfos == null || !coreCategory.SeoInfos.Any())
				{
					var slugUrl = category.Name.GenerateSlug();
					if (!String.IsNullOrEmpty(slugUrl))
					{
						var catalog = _catalogService.GetById(category.CatalogId);
						var defaultLanguage = catalog.Languages.First(x => x.IsDefault).LanguageCode;
						coreCategory.SeoInfos = new SeoInfo[] { new SeoInfo { LanguageCode = defaultLanguage, SemanticUrl = slugUrl } };
					}
				}

				var retVal = _categoryService.Create(coreCategory).ToWebModel();
				retVal.Catalog = null;
				return Ok(retVal);
			}
            else
            {
                _categoryService.Update(new[] { coreCategory });
                return StatusCode(HttpStatusCode.NoContent);
            }
        }

        // POST: api/catalog/categories/5
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("")]
        public IHttpActionResult Delete([FromUri]string[] ids)
        {
            _categoryService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
