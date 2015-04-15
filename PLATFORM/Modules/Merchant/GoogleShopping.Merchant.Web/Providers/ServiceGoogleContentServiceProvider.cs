﻿using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.ShoppingContent.v2;

namespace GoogleShopping.MerchantModule.Web.Providers
{
    public class ServiceGoogleContentServiceProvider : IGoogleContentServiceProvider
    {
        private readonly string serviceAccountEmail = "39718569872-p1gucbblanda96o6nr9bbrjdekv8euba@developer.gserviceaccount.com";
        const string keyPath = @"D:\Virtoway\Projects\vc-community\PLATFORM\Modules\Merchant\GoogleShopping.Merchant.Web";
        const string keyName = "key.p12";
        private ShoppingContentService _contentService;

        public ShoppingContentService GetShoppingContentService()
        {
            if (_contentService == null)
            {

                var key = string.Format(@"{0}\{1}", keyPath, keyName);

                var certificate = new X509Certificate2(key, "notasecret", X509KeyStorageFlags.Exportable);

                var credential = new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer(serviceAccountEmail)
                    {
                        Scopes = new[] { ShoppingContentService.Scope.Content }
                    }.FromCertificate(certificate));

                // Create the service.
                _contentService = new ShoppingContentService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "VirtoCommerce shopping integration"
                });
            }

            return _contentService;
        }
    }
}
