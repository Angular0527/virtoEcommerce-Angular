﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VirtoCommerce.Platform.Core.Settings
{
    public static class SettingsExtension
    {
        public static T GetSettingValue<T>(this IEnumerable<SettingEntry> settings, string settingName, T defaulValue)
        {
            var retVal = defaulValue;
            var setting = settings.FirstOrDefault(x => x.Name.Equals(settingName, StringComparison.OrdinalIgnoreCase));
            if (setting != null && setting.Value != null)
            {
                retVal = (T)Convert.ChangeType(setting.Value, typeof(T), CultureInfo.InvariantCulture);
            }
            return retVal;
        }
    }
}
