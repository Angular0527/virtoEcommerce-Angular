﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtoCommerce.Domain.Common.Expressions;
using VirtoCommerce.Foundation.Frameworks;
using VirtoCommerce.MarketingModule.Data.Services;
using linq = System.Linq.Expressions;

namespace VirtoCommerce.MarketingModule.Web.Model.TypedExpression.Conditions
{
	public class ConditionAtNumItemsInCart : ConditionBase, IConditionExpression
	{
		public decimal NumItem { get; set; }

		public bool Exactly { get; set; }

		
		#region IConditionExpression Members

		linq.Expression<Func<IEvaluationContext, bool>> IConditionExpression.GetConditionExpression()
		{
			var paramX = linq.Expression.Parameter(typeof(IEvaluationContext), "x");
			var castOp = linq.Expression.MakeUnary(linq.ExpressionType.Convert, paramX, typeof(DefaultPromotionEvaluationContext));
			var methodInfo = typeof(DefaultPromotionEvaluationContext).GetMethod("GetItemsQuantity");
			var methodCall = linq.Expression.Call(castOp, methodInfo, GetNewArrayExpression(ExcludingCategoryIds),
																	 GetNewArrayExpression(ExcludingProductIds));
			var numItem = linq.Expression.Constant(NumItem);
			var binaryOp = Exactly ? linq.Expression.Equal(methodCall, numItem) : linq.Expression.GreaterThanOrEqual(methodCall, numItem);

			var retVal = linq.Expression.Lambda<Func<IEvaluationContext, bool>>(binaryOp, paramX);
			return retVal;
		}

		#endregion
	}
}