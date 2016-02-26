﻿using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;
using VirtoCommerce.Storefront.Model.Quote;

namespace VirtoCommerce.Storefront.Model.Customer
{
    public class CustomerInfo : Entity
    {
        public CustomerInfo()
        {
            Addresses = new List<Address>();
            DynamicProperties = new List<DynamicProperty>();
        }

        public string UserName { get; set; }
        /// <summary>
        /// Returns the email address of the customer.
        /// </summary>
        public string Email { get; set; }

        public string FullName { get; set; }
        /// <summary>
        /// Returns the first name of the customer.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Returns the last name of the customer.
        /// </summary>
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public string TimeZone { get; set; }
        public string DefaultLanguage { get; set; }

        public Address DefaultBillingAddress { get; set; }
        public Address DefaultShippingAddress { get; set; }

        /// <summary>
        /// Returns an array of all addresses associated with a customer.
        /// </summary>
        public ICollection<Address> Addresses { get; set; }
        public ICollection<DynamicProperty> DynamicProperties { get; set; }

        /// <summary>
        /// Returns true if the customer accepts marketing, returns false if the customer does not.
        /// </summary>
        public bool AcceptsMarketing { get; set; }


        /// <summary>
        /// Returns the default customer_address.
        /// </summary>
        public Address DefaultAddress { get; set; }

        /// <summary>
        /// Returns true if user registered  returns false if it anonynous. 
        /// </summary>
        public bool IsRegisteredUser { get; set; }

        /// <summary>
        /// Returns the list of tags associated with the customer.
        /// </summary>
        public ICollection<string> Tags { get; set; }

        [IgnoreDataMember]
        public IStorefrontPagedList<CustomerOrder> Orders { get; set; }
        [IgnoreDataMember]
        public IStorefrontPagedList<QuoteRequest> QuoteRequests { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "user#{0} {1} {2}", Id ?? "undef", UserName ?? "undef", IsRegisteredUser ? "registered" : "anonymous");
        }
    }
}
