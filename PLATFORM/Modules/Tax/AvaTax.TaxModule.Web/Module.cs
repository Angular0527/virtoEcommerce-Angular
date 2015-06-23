﻿using System;
using AvaTax.TaxModule.Web.Controller;
using AvaTax.TaxModule.Web.Managers;
using AvaTax.TaxModule.Web.Observers;
using AvaTax.TaxModule.Web.Services;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Cart.Events;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;

namespace AvaTax.TaxModule.Web
{
    public class Module: ModuleBase
    {
        private const string _usernamePropertyName = "Avalara.Tax.Credentials.AccountNumber";
        private const string _passwordPropertyName = "Avalara.Tax.Credentials.LicenseKey";
        private const string _serviceUrlPropertyName = "Avalara.Tax.Credentials.ServiceUrl";
        private const string _companyCodePropertyName = "Avalara.Tax.Credentials.CompanyCode";
        private const string _isEnabledPropertyName = "Avalara.Tax.IsEnabled";

        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members
        
        public override void Initialize()
        {
            var settingsManager = _container.Resolve<ISettingsManager>();
            
            var avalaraCode = settingsManager.GetValue("Avalara.Tax.Code", string.Empty);
            var avalaraDescription = settingsManager.GetValue("Avalara.Tax.Description", string.Empty);
            var avalaraLogoUrl = settingsManager.GetValue("Avalara.Tax.LogoUrl", string.Empty);

            var settings = _container.Resolve<ISettingsManager>().GetModuleSettings("Avalara.Tax");


            var avalaraTax = new AvaTaxImpl(_usernamePropertyName, _passwordPropertyName, _serviceUrlPropertyName, _companyCodePropertyName, _isEnabledPropertyName, avalaraCode, avalaraDescription, avalaraLogoUrl, settings);

            #region Avalara manager
            _container.RegisterInstance<ITaxManager>(new InMemoryTaxManagerImpl());
            #endregion

            var avalaraManager = _container.Resolve<ITaxManager>();
            avalaraManager.RegisterTax(avalaraTax);

            _container.RegisterType<AvaTaxController>
                (new InjectionConstructor(
                    avalaraTax));

            //Subscribe to cart changes. Calculate taxes   
            _container.RegisterType<IObserver<CartChangeEvent>, CalculateCartTaxesObserver>("CalculateCartTaxesObserver", new InjectionConstructor(avalaraTax));

            //Subscribe to order changes. Calculate taxes   
            _container.RegisterType<IObserver<OrderChangeEvent>, CalculateOrderTaxesObserver>("CalculateOrderTaxesObserver", new InjectionConstructor(avalaraTax));
        }
        
        #endregion
    }
}