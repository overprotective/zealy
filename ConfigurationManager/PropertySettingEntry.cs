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
            SetFromAttributes(settingProp.GetCustomAttributes(false), pluginInstance);
            if (Browsable == null) Browsable = settingProp.CanRead && settingProp.CanWrite;
            ReadOnly = settingProp.CanWrite;
            Property = settingProp;
            Instance = instance;
        }

        public object Instance { get; internal set; }
        public P