﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Payment.Services;

namespace MeS.PaymentGatewaysModule.Web.Managers
{
	public class MeSPaymentGatewayImpl : IPaymentGateway
	{
		private string _gatewayCode;
		private string _description;
		private string _logoUrl;


		public MeSPaymentGatewayImpl(string appKey, string gatewayCode, string description, string logoUrl)
		{
			if (string.IsNullOrEmpty(appKey))
				throw new ArgumentNullException("appKey");

			_gatewayCode = gatewayCode;
			_description = description;
			_logoUrl = logoUrl;
		}

		public string GatewayCode
		{
			get { return _gatewayCode; }
		}

		public string Description
		{
			get { return _description; }
		}

		public string LogoUrl
		{
			get { return _logoUrl; }
		}

		public void CreatePayment(PaymentIn paymentInfo, CustomerOrder order)
		{
			//return paymentInfo;
		}


		public PaymentGatewayType GatewayType
		{
			get { throw new NotImplementedException(); }
		}

		public PaymentInfo GetPayment(string paymentId, string orderId)
		{
			throw new NotImplementedException();
		}
	}
}