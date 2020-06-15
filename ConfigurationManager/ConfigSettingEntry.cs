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
  