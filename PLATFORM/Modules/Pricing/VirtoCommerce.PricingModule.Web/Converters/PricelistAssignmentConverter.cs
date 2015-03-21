﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Foundation.Money;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using webModel = VirtoCommerce.PricingModule.Web.Model;

namespace VirtoCommerce.PricingModule.Web.Converters
{
	public static class PricelistAssignmentConverter
	{
		public static webModel.PricelistAssignment ToWebModel(this coreModel.PricelistAssignment assignment)
		{
			var retVal = new webModel.PricelistAssignment();
			retVal.InjectFrom(assignment);
		
			return retVal;
		}

		public static coreModel.PricelistAssignment ToCoreModel(this webModel.PricelistAssignment assignment)
		{
			var retVal = new coreModel.PricelistAssignment();
			retVal.InjectFrom(assignment);
			return retVal;
		}


	}
}
