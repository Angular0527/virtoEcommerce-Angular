﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Content.Pages.Data.Models;
using VirtoCommerce.Content.Pages.Data.Repositories;

namespace VirtoCommerce.Content.Pages.Data.Services
{
	public class PagesServiceImpl : IPagesService
	{
		private readonly IPagesRepository _pagesRepository;

		public PagesServiceImpl(IPagesRepository pagesRepository)
		{
			if (pagesRepository == null)
				throw new ArgumentNullException("pagesRepository");

			_pagesRepository = pagesRepository;
		}

		public IEnumerable<Models.ShortPageInfo> GetPages(string storeId, GetPagesCriteria criteria)
		{
			var path = string.Format("{0}/", storeId);
			var pages = _pagesRepository.GetPages(path);
			if(criteria.LastUpdateDate.HasValue)
			{
				return pages.Where(p => p.LastModified > criteria.LastUpdateDate.Value);
			}
			return pages;
		}

		public Models.Page GetPage(string storeId, string pageName, string language)
		{
			var fullPath = GetFullName(storeId, pageName, language);

			return _pagesRepository.GetPage(fullPath);
		}

		public void SavePage(string storeId, Models.Page page)
		{
			var fullPath = GetFullName(storeId, page.Name, page.Language);

			page.Path = fullPath;

			_pagesRepository.SavePage(fullPath, page);
		}

		public void DeletePage(string storeId, ShortPageInfo[] pages)
		{
			foreach (var page in pages)
			{
				var fullPath = GetFullName(storeId, page.Name, page.Language);

				_pagesRepository.DeletePage(fullPath);
			}
		}

		public bool CheckList(string storeId, string name, string language)
		{
			var page = GetPage(storeId, name, language);
			if (page != null)
			{
				return false;
			}

			return true;
		}

		private string GetFullName(string storeId, string pageName, string language)
		{
			return string.Format("{0}/{1}/{2}.liquid", storeId, language, pageName);
		}
	}
}
