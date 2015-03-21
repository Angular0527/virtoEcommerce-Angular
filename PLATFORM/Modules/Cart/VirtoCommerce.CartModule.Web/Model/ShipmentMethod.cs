﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VirtoCommerce.Foundation.Frameworks;
using VirtoCommerce.Foundation.Money;

namespace VirtoCommerce.CatalogModule.Web.Model
{
	public class ShipmentMethod : ValueObject<ShipmentMethod>
	{
		public string ShipmentMethodCode { get; set; }
		public string Name { get; set; }
		public string LogoUrl { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public CurrencyCodes Currency { get; set; }
		public decimal Price { get; set; }
		public ICollection<Discount> Discounts { get; set; }
	}
}
