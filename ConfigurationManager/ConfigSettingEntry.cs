using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;

namespace ConfigurationManager
{
    internal sealed class ConfigSettingEntry : SettingEntryBase
    {
        public ConfigEntryBase Entry { get; }

        public ConfigSettingEntry(ConfigEntryBase entry, BaseUnityPlugin owner)
        {
            Entry = entry;

            DispName = entry.Definition.Key;
            Category = entry.Definition.Section;
            Description = entry.Description?.Description;

            var converter = TomlTypeConverter.GetConverter(entry.SettingType);
            if (converter != null)
            {
                ObjToStr = o => converter.ConvertToString(o, entry.SettingType);
                StrToObj = s => converter.ConvertToObject(s, entry.SettingType);
            }

            var values = entry.Description?.AcceptableValues;
            if (values != null)
                GetAcceptableValues(values);

            DefaultValue = entry.DefaultValue;

            SetFromAttributes(entry.Description?.Tags, owner);
        }

        private void GetAcceptableValues(AcceptableValueBase values)
        {
            var t = values.GetType();
            var listProp = t.GetProperty(nameof(AcceptableValu