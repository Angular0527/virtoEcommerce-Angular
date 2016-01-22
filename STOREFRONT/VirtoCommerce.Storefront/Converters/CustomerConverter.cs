﻿using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Client.Model;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CustomerConverter
    {
        private static readonly char[] _nameSeparator = { ' ' };

   
        public static CustomerInfo ToWebModel(this Register formModel)
        {
            var result = new CustomerInfo
            {
                Email = formModel.Email,
                Name = string.Join(" ", formModel.FirstName, formModel.LastName),
                FirstName = formModel.FirstName,
                LastName = formModel.LastName
            };

            if (string.IsNullOrEmpty(result.Name) || string.IsNullOrWhiteSpace(result.Name))
            {
                result.Name = formModel.Email;
            }
            return result;
        }

        public static CustomerInfo ToWebModel(this VirtoCommerceCustomerModuleWebModelContact contact)
        {
            var retVal = new CustomerInfo();
            retVal.InjectFrom(contact);

            retVal.IsRegisteredUser = true;
            if (contact.Addresses != null)
            {
                retVal.Addresses = contact.Addresses.Select(a => a.ToWebModel()).ToList();
            }

            retVal.DefaultBillingAddress = retVal.Addresses.FirstOrDefault(a => (a.Type & AddressType.Billing) == AddressType.Billing);
            retVal.DefaultShippingAddress = retVal.Addresses.FirstOrDefault(a => (a.Type & AddressType.Shipping) == AddressType.Shipping);

            // TODO: Need separate properties for first, middle and last name
            if (!string.IsNullOrEmpty(contact.FullName))
            {
                var nameParts = contact.FullName.Split(_nameSeparator, 2);

                if (nameParts.Length > 0)
                {
                    retVal.FirstName = nameParts[0];
                }

                if (nameParts.Length > 1)
                {
                    retVal.LastName = nameParts[1];
                }
            }

            if (contact.Emails != null)
            {
                retVal.Email = contact.Emails.FirstOrDefault();
            }

            return retVal;
        }

        public static VirtoCommerceCustomerModuleWebModelContact ToServiceModel(this CustomerInfo customer)
        {
            var retVal = new VirtoCommerceCustomerModuleWebModelContact();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(customer);
            if (customer.Addresses != null)
            {
                retVal.Addresses = customer.Addresses.Select(x => x.ToServiceModel()).ToList();
            }
            if (!string.IsNullOrEmpty(customer.Email))
            {
                retVal.Emails = new[] { customer.Email }.ToList();
            }
            retVal.FullName = customer.Name;
       
            return retVal;
        }
    }
}