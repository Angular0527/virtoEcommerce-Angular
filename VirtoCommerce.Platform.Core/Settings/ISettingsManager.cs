﻿using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.Platform.Core.Settings
{
    public interface ISettingsManager
    {
        ManifestModuleInfo[] GetModules();
        /// <summary>
        /// Deep load and populate settings values for entity and all nested objects 
        /// </summary>
        /// <param name="entity"></param>
		void LoadEntitySettingsValues(Entity entity);
        /// <summary>
        /// Deep save entity and all nested objects settings values
        /// </summary>
        /// <param name="entity"></param>
        void SaveEntitySettingsValues(Entity entity);
        /// <summary>
        /// Deep remove entity and all nested objects settings values
        /// </summary>
        /// <param name="entity"></param>
		void RemoveEntitySettings(Entity entity);
        /// <summary>
        /// Load and populate settings values for user
        /// </summary>
        /// <param name="user"></param>
		void LoadUserSettingsValues(ApplicationUserExtended user);
        /// <summary>
        /// Ssave user settings values
        /// </summary>
        /// <param name="user"></param>
        void SaveUserSettingsValues(ApplicationUserExtended user);
        /// <summary>
        /// Remove user settings values
        /// </summary>
        /// <param name="user"></param>
		void RemoveUserSettings(ApplicationUserExtended user);
        SettingEntry GetSettingByName(string name);
		SettingEntry[] GetModuleSettings(string moduleId);
		void SaveSettings(SettingEntry[] settings);
        /// <summary>
        /// Used to runtime settings registration
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="settings"></param>
        void RegisterModuleSettings(string moduleId, params SettingEntry[] settings);

        T GetValue<T>(string name, T defaultValue);
        T[] GetArray<T>(string name, T[] defaultValue);
        void SetValue<T>(string name, T value);
    }
}
