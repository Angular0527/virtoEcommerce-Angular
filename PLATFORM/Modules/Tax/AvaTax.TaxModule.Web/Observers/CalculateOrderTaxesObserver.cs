﻿using System;
using System.Linq;
using AvaTax.TaxModule.Web.Converters;
using AvaTax.TaxModule.Web.Services;
using AvaTaxCalcREST;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Platform.Core.Common;

namespace AvaTax.TaxModule.Web.Observers
{
    public class CalculateOrderTaxesObserver : IObserver<OrderChangeEvent>
	{
        private readonly ITax _taxSettings;

        public CalculateOrderTaxesObserver(ITax taxSettings)
        {
            _taxSettings = taxSettings;
        }

		#region IObserver<CustomerOrder> Members

		public void OnCompleted()
		{
		}

		public void OnError(Exception error)
		{
		}

		public void OnNext(OrderChangeEvent value)
		{
            if (_taxSettings.IsEnabled && value.ChangeState == EntryState.Modified)
			    CalculateCustomerOrderTaxes(value);
		}

		#endregion
		private void CalculateCustomerOrderTaxes(OrderChangeEvent context)
		{
			var order = context.ModifiedOrder;

            if (!string.IsNullOrEmpty(_taxSettings.Username) && !string.IsNullOrEmpty(_taxSettings.Password)
                && !string.IsNullOrEmpty(_taxSettings.ServiceUrl)
                && !string.IsNullOrEmpty(_taxSettings.CompanyCode))
            {
                var taxSvc = new TaxSvc(_taxSettings.Username, _taxSettings.Password, _taxSettings.ServiceUrl);
                var isCommit = order.InPayments != null && order.InPayments.Any() && order.InPayments.All(pi => pi.IsApproved);
                var request = order.ToAvaTaxRequest(_taxSettings.CompanyCode, isCommit);
                if (request != null)
                {
                    var getTaxResult = taxSvc.GetTax(request);
                    if (!getTaxResult.ResultCode.Equals(SeverityLevel.Success))
                    {
                        var error = string.Join(Environment.NewLine, getTaxResult.Messages.Select(m => m.Details));
                        OnError(new Exception(error));
                    }
                    else
                    {
                        foreach (TaxLine taxLine in getTaxResult.TaxLines ?? Enumerable.Empty<TaxLine>())
                        {
                            order.Items.ToArray()[Int32.Parse(taxLine.LineNo)].Tax = taxLine.Tax;
                            //foreach (TaxDetail taxDetail in taxLine.TaxDetails ?? Enumerable.Empty<TaxDetail>())
                            //{
                            //}
                  
                        }
                        order.Tax = getTaxResult.TotalTax;
                    }
                }
            }
            else
            {
                OnError(new Exception("AvaTax credentials not provided"));
            }
		}

		
	}
}