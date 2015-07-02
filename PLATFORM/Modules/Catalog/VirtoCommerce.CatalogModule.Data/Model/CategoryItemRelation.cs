﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Model
{
	public class CategoryItemRelation : Entity
	{
		public CategoryItemRelation()
		{
			Id = Guid.NewGuid().ToString("N");
		}
		public int Priority { get; set; }

		#region Navigation Properties

		[StringLength(128)]
		public string ItemId { get; set; }

		[Parent]
		[ForeignKey("ItemId")]
		public virtual Item CatalogItem { get; set; }

		[StringLength(128)]
		[Required]
		public string CategoryId { get; set; }

		public virtual Category Category { get; set; }

		[Required]
		public string CatalogId { get; set; }

		public virtual CatalogBase Catalog { get; set; }

		#endregion
	}
}
