﻿using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerModule.Web.Model
{
    public class Contact : Member
    {
        public Contact()
            : base("Contact")
        {
        }

        public override string DisplayName
        {
            get { return FullName; }
        }

        public string FullName { get; set; }
        public string TimeZone { get; set; }
        public string DefaultLanguage { get; set; }
        public DateTime? BirthDate { get; set; }
        public string TaxpayerId { get; set; }
        public string PreferredDelivery { get; set; }
        public string PreferredCommunication { get; set; }
        public string Salutation { get; set; }

        public ICollection<string> Organizations { get; set; }

	
	}
}
