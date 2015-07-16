﻿using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model;

namespace VirtoCommerce.Domain.Order.Model
{
	public class Shipment : Operation, IStockOutOperation, IHaveTaxDetalization, ISupportCancellation
	{
		public string OrganizationId { get; set; }
		public string OrganizationName { get; set; }

		public string FulfillmentCenterId { get; set; }
		public string FulfillmentCenterName { get; set; }

		public string EmployeeId { get; set; }
		public string EmployeeName { get; set; }

		public string ShipmentMethodCode { get; set; }
		public string ShipmentMethodOption { get; set; }

		public string CustomerOrderId { get; set; }
		public CustomerOrder CustomerOrder { get; set; }

		#region IStockOperation members
		public ICollection<ShipmentItem> Items { get; set; } 
		#endregion
		public ICollection<ShipmentPackage> Packages { get; set; }

		public ICollection<PaymentIn> InPayments { get; set; }

		public string WeightUnit { get; set; }
		public decimal? Weight { get; set; }

		public string MeasureUnit { get; set; }
		public decimal? Height { get; set; }
		public decimal? Length { get; set; }
		public decimal? Width { get; set; }

		public string TaxType { get; set; }

		public Address DeliveryAddress { get; set; }
		public decimal DiscountAmount
		{
			get
			{
				return Discount != null ? Discount.DiscountAmount : 0;
			}
		}
		public Discount Discount { get; set; }

		

		public override IEnumerable<Operation> ChildrenOperations
		{
			get
			{
				var retVal = new List<Operation>();

				if (InPayments != null)
				{
					foreach (var inPayment in InPayments)
					{
						inPayment.ParentOperationId = this.Id;
						retVal.Add(inPayment);
					}
				}
				return retVal;
			}
		}

		#region IStockOperation Members

		public IEnumerable<IPosition> Positions
		{
			get { return Items; }
		}

		#endregion

		#region ITaxDetailSupport Members

		public ICollection<TaxDetail> TaxDetails { get; set; }

		#endregion
	}
}
