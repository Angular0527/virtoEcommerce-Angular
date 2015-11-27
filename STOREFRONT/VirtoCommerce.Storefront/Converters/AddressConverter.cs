﻿using Omu.ValueInjecter;
using System;
using VirtoCommerce.Client.Model;
using VirtoCommerce.LiquidThemeEngine.Converters.Injections;
using VirtoCommerce.Storefront.Model;
using shopifyModel = VirtoCommerce.LiquidThemeEngine.Objects;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AddressConverter
    {
        public static Address ToWebModel(this VirtoCommerceCustomerModuleWebModelAddress address)
        {
            var customerAddress = new Address();

            customerAddress.InjectFrom(address);

            return customerAddress;
        }

        public static VirtoCommerceCustomerModuleWebModelAddress ToServiceModel(this shopifyModel.Address address)
        {
            var result = new VirtoCommerceCustomerModuleWebModelAddress();
            result.CopyFrom(address);
            return result;
        }

        public static VirtoCommerceCustomerModuleWebModelAddress CopyFrom(this VirtoCommerceCustomerModuleWebModelAddress result, shopifyModel.Address address)
        {
            result.InjectFrom<NullableAndEnumValueInjection>(address);

            result.Organization = address.Company;
            result.CountryName = address.Country;
            result.PostalCode = address.Zip;
            result.Line1 = address.Address1;
            result.Line2 = address.Address2;
            result.RegionId = address.ProvinceCode;
            result.RegionName = address.Province;

            return result;
        }

        public static Address ToWebModel(this VirtoCommerceCartModuleWebModelAddress address)
        {
            var addressWebModel = new Address();

            addressWebModel.InjectFrom(address);
            addressWebModel.Type = (AddressType)Enum.Parse(typeof(AddressType), address.Type, true);

            return addressWebModel;
        }

        public static VirtoCommerceCartModuleWebModelAddress ToServiceModel(this Address address)
        {
            var addressServiceModel = new VirtoCommerceCartModuleWebModelAddress();

            addressServiceModel.InjectFrom(address);
            addressServiceModel.Type = address.Type.ToString();

            return addressServiceModel;
        }
    }
}