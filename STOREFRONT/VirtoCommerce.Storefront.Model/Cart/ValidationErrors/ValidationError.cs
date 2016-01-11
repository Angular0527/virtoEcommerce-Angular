﻿using System;

namespace VirtoCommerce.Storefront.Model.Cart.ValidationErrors
{
    public abstract class ValidationError
    {
        public ValidationError(Type errorType)
        {
            ErrorCode = errorType.Name;
        }

        public string ErrorCode { get; private set; }
    }
}