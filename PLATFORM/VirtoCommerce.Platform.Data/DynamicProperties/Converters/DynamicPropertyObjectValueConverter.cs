﻿using System;
using System.Globalization;
using System.Linq;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Data.Model;

namespace VirtoCommerce.Platform.Data.DynamicProperties.Converters
{
    public static class DynamicPropertyObjectValueConverter
    {
        public static object ToModel(this DynamicPropertyObjectValueEntity entity)
        {
            return entity.DictionaryItem != null ? entity.DictionaryItem.ToModel() : entity.RawValue();
        }

        public static DynamicPropertyObjectValueEntity[] ToEntity(this DynamicPropertyObjectValue model, DynamicProperty property)
        {
            var result = (model.Values ?? new object[0]).Select(v => v.ToEntity(property, model.ObjectId, model.Locale)).ToArray();
            return result;
        }

        public static DynamicPropertyObjectValueEntity ToEntity(this object value, DynamicProperty property, string objectId, string locale)
        {
            var result = new DynamicPropertyObjectValueEntity
            {
                PropertyId = property.Id,
                ObjectType = property.ObjectType,
                ObjectId = objectId,
                ValueType = property.ValueType.ToString(),
                Locale = property.IsMultilingual ? locale : null,
            };

            if (property.IsDictionary)
            {
                result.DictionaryItemId = ((DynamicPropertyDictionaryItem)value).Id;
            }
            else
            {
                switch (property.ValueType)
                {
                    case DynamicPropertyValueType.Boolean:
                        result.BooleanValue = Convert.ToBoolean(value);
                        break;
                    case DynamicPropertyValueType.DateTime:
                        result.DateTimeValue = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                        break;
                    case DynamicPropertyValueType.Decimal:
                        result.DecimalValue = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                        break;
                    case DynamicPropertyValueType.Integer:
                        result.IntegerValue = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                        break;
                    case DynamicPropertyValueType.LongText:
                        result.LongTextValue = (string)value;
                        break;
                    default:
                        result.ShortTextValue = (string)value;
                        break;
                }
            }

            return result;
        }
    }
}
