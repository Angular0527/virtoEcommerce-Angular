﻿using System.Linq;
using webModels = VirtoCommerce.MenuModule.Web.Models;
using coreModels = VirtoCommerce.Content.Menu.Data.Models;

namespace VirtoCommerce.MenuModule.Web.Converters
{
	public static class MenuLinkListConverter
	{
		public static coreModels.MenuLinkList ToCoreModel(this webModels.MenuLinkList list)
		{
			var retVal = new coreModels.MenuLinkList
			             {
			                 Id = list.Id,
			                 Name = list.Name,
			                 StoreId = list.StoreId,
			                 Language = list.Language,
			                 MenuLinks = list.MenuLinks.Select(s => s.ToCoreModel()).ToList()
			             };

			return retVal;
		}

		public static webModels.MenuLinkList ToWebModel(this coreModels.MenuLinkList list)
		{
		    if (list == null)
		        return null;

			var retVal = new webModels.MenuLinkList
			             {
			                 Id = list.Id,
			                 Name = list.Name,
			                 StoreId = list.StoreId,
			                 Language = list.Language
			             };

		    if (list.MenuLinks.Any())
		    {
		        retVal.MenuLinks = list.MenuLinks.OrderByDescending(l => l.Priority).Select(s => s.ToWebModel()).ToArray();
		    }

		    return retVal;
		}
	}
}