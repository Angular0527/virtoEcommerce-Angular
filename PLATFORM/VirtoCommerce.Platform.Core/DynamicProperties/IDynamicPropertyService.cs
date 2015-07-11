﻿namespace VirtoCommerce.Platform.Core.DynamicProperties
{
    public interface IDynamicPropertyService
    {
        string[] GetObjectTypes();

        DynamicProperty[] GetProperties(string objectType);
        void SaveProperties(DynamicProperty[] properties);
        void DeleteProperties(string[] propertyIds);

        DynamicPropertyDictionaryItem[] GetDictionaryItems(string propertyId);
        void SaveDictionaryItems(string propertyId, DynamicPropertyDictionaryItem[] items);
        void DeleteDictionaryItems(string[] itemIds);

        DynamicPropertyObjectValue[] GetObjectValues(string objectType, string objectId);
        void SaveObjectValues(DynamicPropertyObjectValue[] values);
        void DeleteObjectValues(string objectType, string objectId);
    }
}
