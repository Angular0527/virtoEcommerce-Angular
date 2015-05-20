﻿using System;
using System.Linq;
using System.Web.Http.Results;
using VirtoCommerce.Domain.Marketing.Model;
using VirtoCommerce.Domain.Marketing.Services;
using VirtoCommerce.Platform.Tests.Bases;
using dataModel = VirtoCommerce.MarketingModule.Data.Model;
using VirtoCommerce.MarketingModule.Data.Repositories;
using VirtoCommerce.MarketingModule.Data.Services;
using VirtoCommerce.DynamicExpressionModule.Data;
using VirtoCommerce.DynamicExpressionModule.Data.Promotion;
using VirtoCommerce.MarketingModule.Test.CustomDynamicPromotionExpressions;
using VirtoCommerce.MarketingModule.Web.Controllers.Api;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using Xunit;
using webModel = VirtoCommerce.MarketingModule.Web.Model;

namespace VirtoCommerce.MarketingModule.Test
{
    public class MarketingControllerScenarios : FunctionalTestBase
	{
		[Fact]
		public void Test()
		{
			var repository = new MarketingRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor());
			var contentItem = new dataModel.DynamicContentItem()
			{
				Name = "ss",
			};
			contentItem.PropertyValues = new dataModel.DynamicContentItemProperty[] { 
				new dataModel.DynamicContentItemProperty
				{
					 Name = "ssss",
					  DecimalValue = 33.0m
				}
			};
			repository.Add(contentItem);
			repository.UnitOfWork.Commit();

		}
		
		[Fact]
		public void CreateStandartDynamicPromotionThroughtApi()
		{
			var marketingController = GetMarketingController(GetPromotionExtensionManager());

			webModel.Promotion promotion = null;
			var promoResult = (marketingController.GetPromotionById("CartFiveDiscount") as OkNegotiatedContentResult<webModel.Promotion>);
			if (promoResult != null)
			{
				promotion = promoResult.Content;
			}
			if (promotion == null)
			{
				promotion = (marketingController.GetNewDynamicPromotion() as OkNegotiatedContentResult<webModel.Promotion>).Content;

				promotion.Description = "Buy at $100 and get a 5% discount";
				promotion.Name = "CartFiveDiscount";
				promotion.Id = "CartFiveDiscount";

				var expressionTree = promotion.DynamicExpression as DynamicExpression;
				
				//Curreny is USD
				var currencyExpression = expressionTree.FindAvailableExpression<ConditionCurrencyIs>();
				currencyExpression.Currency = CurrencyCodes.USD.ToString();
				expressionTree.Children.Add(currencyExpression);
				//Condition: Cart subtotal great or equal that 100$
				var subtotalExpression = expressionTree.FindAvailableExpression<ConditionCartSubtotalLeast>();
				subtotalExpression.SubTotal = 100;
				expressionTree.Children.Add(subtotalExpression);
				//Reward: Get 5% whole cart discount
				var rewardExpr = expressionTree.FindAvailableExpression<RewardItemGetOfRel>();
				rewardExpr.Amount = 0.5m;
				expressionTree.Children.Add(rewardExpr);

				promotion = (marketingController.CreatePromotion(promotion) as OkNegotiatedContentResult<webModel.Promotion>).Content;
			}
		
			var marketingEval = new DefaultPromotionEvaluatorImpl(GetMarketingService());
			var context = GetPromotionEvaluationContext();
			var result = marketingEval.EvaluatePromotion(context);
		
		}


		[Fact]
		public void ExtendPromotionExpressionTreeAndCreateNewDynamicPromotionWithCustomExpression()
		{
			var extensionManager = GetPromotionExtensionManager();
			//Register custom dynamic expression in main expression tree now it should be availabe for ui in expression builder
			var blockExpression = extensionManager.PromotionDynamicExpressionTree as DynamicExpression;
		    var blockCatalogCondition = blockExpression.FindChildrenExpression<BlockCatalogCondition>();
			blockCatalogCondition.AvailableChildren.Add(new ConditionItemWithTag());

			var marketingController = GetMarketingController(extensionManager);


			//Create custom promotion
			webModel.Promotion promotion = null;
			var promoResult = (marketingController.GetPromotionById("TaggedProductDiscount") as OkNegotiatedContentResult<webModel.Promotion>);
			if (promoResult != null)
			{
				promotion = promoResult.Content;
			}
			if (promotion == null)
			{
				promotion = (marketingController.GetNewDynamicPromotion() as OkNegotiatedContentResult<webModel.Promotion>).Content;

				promotion.Description = "Buy all product with tag '#FOOTBAL' with 7% discount";
				promotion.Name = "TaggedProductDiscount";
				promotion.Id = "TaggedProductDiscount";

				blockExpression = promotion.DynamicExpression as DynamicExpression;
				blockCatalogCondition = blockExpression.FindChildrenExpression<BlockCatalogCondition>();
				var blockReward = blockExpression.FindChildrenExpression<RewardBlock>();

				var conditionExpr = blockCatalogCondition.FindAvailableExpression<ConditionItemWithTag>();
				conditionExpr.Tags = new string[] { "#FOOTBAL" };
				blockCatalogCondition.Children.Add(conditionExpr);

				var rewardExpr = blockReward.FindAvailableExpression<RewardItemGetOfRel>();
				rewardExpr.Amount = 0.7m;
				blockReward.Children.Add(rewardExpr);
			
				promotion = (marketingController.CreatePromotion(promotion) as OkNegotiatedContentResult<webModel.Promotion>).Content;
			}

			var marketingEval = new DefaultPromotionEvaluatorImpl(GetMarketingService());
			var context = GetPromotionEvaluationContext();
			context.PromoEntries.First().Attributes["tag"] = "#FOOTBAL";
			var result = marketingEval.EvaluatePromotion(context);

		}


		[Fact]
		public void EvaluatePromotionWhenCatalogBrowsing()
		{
		}

		[Fact]
		public void EvaluateCartPromotion()
		{
		}

		private static PromotionEvaluationContext GetPromotionEvaluationContext()
		{
			var context = new PromotionEvaluationContext();
			context.CartPromoEntries = context.PromoEntries = new ProductPromoEntry[]
			{
					new ProductPromoEntry
					{
						CatalogId = "Samsung",
						CategoryId = "100df6d5-8210-4b72-b00a-5003f9dcb79d",
						ProductId = "v-b000bkzs9w",
						Price = 160.44m,
						Quantity = 2,

					},
					new ProductPromoEntry
					{
						CatalogId = "Sony",
						CategoryId = "100df6d5-8210-4b72-b00a-5003f9dcb79d",
						ProductId = "v-a00032ksj",
						Price = 12.00m,
						Quantity = 1,

					},
					new ProductPromoEntry
					{
						CatalogId = "LG",
						CategoryId = "100df6d5-8210-4b72-b00a-5003f9dcb79d",
						ProductId = "v-c00021211",
						Price = 1.00m,
						Quantity = 1,

					}
			};
			return context;
		}

		private static IPromotionService GetMarketingService()
		{
			Func<IMarketingRepository> foundationRepositoryFactory = () => new MarketingRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor());
			var promotionExtensionManager = new InMemoryExtensionManagerImpl();
			var retVal = new PromotionServiceImpl(foundationRepositoryFactory, promotionExtensionManager);
			return retVal;
		}

		private static MarketingModulePromotionController GetMarketingController(IMarketingExtensionManager extensionManager)
		{
			var retVal = new MarketingModulePromotionController(GetMarketingService(), extensionManager);
			return retVal;
		}

		private static IMarketingExtensionManager GetPromotionExtensionManager()
		{
			var retVal = new InMemoryExtensionManagerImpl();
			retVal.PromotionDynamicExpressionTree = GetDynamicExpression();
			return retVal;
		}

		private static PromoDynamicExpressionTree GetDynamicExpression()
		{
			var customerConditionBlock = new BlockCustomerCondition();
			customerConditionBlock.AvailableChildren = new DynamicExpression[] { new ConditionIsEveryone(), new ConditionIsFirstTimeBuyer(), 
																				  new ConditionIsRegisteredUser() }.ToList();

			var catalogConditionBlock = new BlockCatalogCondition();
			catalogConditionBlock.AvailableChildren = new DynamicExpression[] { new ConditionEntryIs(), new ConditionCurrencyIs(), 
																		       new  ConditionCodeContains(), new ConditionCategoryIs(), 
																			    }.ToList();

			var cartConditionBlock = new BlockCartCondition();
			cartConditionBlock.AvailableChildren = new DynamicExpression[] { new ConditionCartSubtotalLeast(), new ConditionAtNumItemsInCart(), 
																			 new ConditionAtNumItemsInCategoryAreInCart(), new ConditionAtNumItemsOfEntryAreInCart() }.ToList();
			var rewardBlock = new RewardBlock();
			rewardBlock.AvailableChildren = new DynamicExpression[] { new RewardCartGetOfAbsSubtotal(),  new RewardItemGetFreeNumItemOfProduct(),  new RewardItemGetOfAbs(),
																	   new RewardItemGetOfAbsForNum(), new RewardItemGetOfRel(), new RewardItemGetOfRelForNum(),
																	   new RewardItemGiftNumItem(), new RewardShippingGetOfAbsShippingMethod(), new RewardShippingGetOfRelShippingMethod ()}.ToList();

			var rootBlockExpressions = new DynamicExpression[] { customerConditionBlock, catalogConditionBlock, cartConditionBlock, rewardBlock }.ToList();
			var retVal = new PromoDynamicExpressionTree()
			{
				Children = rootBlockExpressions,
				AvailableChildren = rootBlockExpressions
			};
			return retVal;

		}
	}
}
