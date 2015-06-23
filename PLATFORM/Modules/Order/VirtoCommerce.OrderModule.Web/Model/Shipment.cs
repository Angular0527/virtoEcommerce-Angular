﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VirtoCommerce.Domain.Commerce.Model;

namespace VirtoCommerce.OrderModule.Web.Model
{
	public class Shipment : Operation, IHaveTaxDetalization
	{
		public string Organization { get; set; }
		public string OrganizationId { get; set; }
		public string FulfillmentCenter { get; set; }
		public string FulfillmentCenterId { get; set; }
		public string ShipmentMethodCode { get; set; }
		public string Employee { get; set; }
		public string EmployeeId { get; set; }
		public decimal DiscountAmount { get; set; }


		public string WeightUnit { get; set; }
		public decimal? Weight { get; set; }

		public string MeasureUnit { get; set; }
		public decimal? Height { get; set; }
		public decimal? Length { get; set; }
		public decimal? Width { get; set; }

		public ICollection<LineItem> Items { get; set; }
		public ICollection<PaymentIn> InPayments { get; set; }
		public Address DeliveryAddress { get; set; }
		public Discount Discount { get; set; }

		
		#region IHaveTaxDetalization Members
		public ICollection<TaxDetail> TaxDetails { get; set; }
		#endregion
	}
}