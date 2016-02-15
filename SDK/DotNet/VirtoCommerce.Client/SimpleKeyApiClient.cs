﻿using System.Collections.Generic;
using RestSharp;
using VirtoCommerce.Client.Client;

namespace VirtoCommerce.Client
{
    public class SimpleKeyApiClient : ApiClient
    {
        private readonly string _simpleApiKey;

        public SimpleKeyApiClient(string basePath, string simpleApiKey)
            : base(basePath)
        {
            _simpleApiKey = simpleApiKey;
        }

        protected override RestRequest PrepareRequest(string path, Method method, Dictionary<string, string> queryParams, string postBody, Dictionary<string, string> headerParams,
            Dictionary<string, string> formParams, Dictionary<string, FileParameter> fileParams, Dictionary<string, string> pathParams)
        {
            var request = base.PrepareRequest(path, method, queryParams, postBody, headerParams, formParams, fileParams, pathParams);

            request.AddHeader("Authorization", "APIKey " + _simpleApiKey);

            return request;
        }
    }
}
