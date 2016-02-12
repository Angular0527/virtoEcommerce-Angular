﻿using DotLiquid;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Catalog;

namespace VirtoCommerce.LiquidThemeEngine.Objects
{
    public class Search : Drop
    {
        CatalogSearchResult _proxyResults = null;
        //private bool _productsLoaded = false;
        #region Constructors and Destructors
        public Search(CatalogSearchResult results)
        {
            _proxyResults = results;
            this.Performed = true;
        }
        #endregion

        #region Public Properties
        public bool Performed { get; set; }

        private ItemCollection<object> _Results = null;

        public ItemCollection<object> Results
        {
            get { this.LoadSearchResults(); return _Results; }
            set { _Results = value; }
        }

        public int ResultsCount
        {
            get
            {
                var response = Results;
                if (response != null)
                {
                    return response.Count;
                }

                return 0;
            }
        }

        public string Terms { get; set; }
        #endregion

        #region Methods
        private void LoadSearchResults()
        {
            if (!this.Performed)
            {
                return;
            }

            var response = Task.Run(() => _proxyResults.Products).Result;
            //var products = _proxyResults.Products;
            //var pageSize = this.Context == null ? 20 : this.Context["paginate.page_size"].ToInt(20);
            //var skip = this.Context == null ? 0 : this.Context["paginate.current_offset"].ToInt();
            //var terms = this.Terms; //this.Context["current_query"] as string;
            //var type = this.Context == null ? "product" : this.Context["current_type"] as string;

            //var response = Task.Run(() => service.SearchAsync<object>(siteContext, searchQuery)).Result;
            //this.Results = response;

            this.Performed = false;
        }
        #endregion
    }
}
