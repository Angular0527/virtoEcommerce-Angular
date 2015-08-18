﻿using System.Collections.Generic;

namespace VirtoCommerce.ApiClient.DataContracts.Contents
{
    public class DynamicContentItem
    {
        public string ContentType { get; set; }

        public string Description { get; set; }

        public string Id { get; set; }

        public bool IsMultilingual { get; set; }

        public string Name { get; set; }

        public IDictionary<string, object> Properties { get; set; }


        //#region Fields

        //private IDictionary<string, string> _properties = new Dictionary<string, string>();

        //#endregion

        //#region Public Properties

        //public string ContentType { get; set; }

        //public string Description { get; set; }

        //public string Id { get; set; }

        //public bool IsMultilingual { get; set; }

        //public string Name { get; set; }

        //public IDictionary<string, string> Properties
        //{
        //    get { return _properties; }
        //    set { _properties = value; }
        //}

        //#endregion

        //#region Public Indexers

        //[JsonIgnore]
        //public string this[string name]
        //{
        //    get { return _properties[name]; }
        //    set
        //    {
        //        if (_properties.ContainsKey(name))
        //        {
        //            _properties[name] = value;
        //        }
        //        else
        //        {
        //            _properties.Add(name, value);
        //        }
        //    }
        //}

        //#endregion
    }
}
