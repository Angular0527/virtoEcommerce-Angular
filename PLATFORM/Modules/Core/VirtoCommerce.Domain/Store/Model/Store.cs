﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Domain.Store.Model
{
	public class Store : AuditableEntity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		public StoreState StoreState { get; set; }

		public string TimeZone { get; set; }
		public string Country { get; set; }
		public string Region { get; set; }
		public string DefaultLanguage { get; set; }

		public CurrencyCodes? DefaultCurrency { get; set; }
		public string Catalog { get; set; }
		public bool CreditCardSavePolicy { get; set; }
		public string SecureUrl { get; set; }
		public string Email { get; set; }
		public string AdminEmail { get; set; }
		public bool DisplayOutOfStock { get; set; }
	
		public FulfillmentCenter FulfillmentCenter { get; set; }
		public FulfillmentCenter ReturnsFulfillmentCenter { get; set; }
		public ICollection<string> Languages { get; set; }
		public ICollection<CurrencyCodes> Currencies { get; set; }
		public ICollection<StoreSetting> Settings { get; set; }
		public ICollection<string> PaymentGateways { get; set; }
		public ICollection<string> ShipmentGateways { get; set; }
	}
}
