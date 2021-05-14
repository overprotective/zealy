using System;
using System.Reflection;
using BepInEx;

namespace ConfigurationManager
{
    internal class PropertySettingEntry : SettingEntryBase
    {
        private Type _settingType;

        public PropertySettingEntry(object instance, PropertyInfo settingProp, BaseUnityPlugin pluginInstance)
        {
          