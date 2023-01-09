
﻿using System;
using BepInEx;
using BepInEx.Configuration;
using ConfigurationManager.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ConfigurationManager
{
    internal static class SettingSearcher
    {
        private static readonly ICollection<string> _updateMethodNames = new[]
        {
            "Update",
            "FixedUpdate",
            "LateUpdate",
            "OnGUI"
        };

        public static void CollectSettings(out IEnumerable<SettingEntryBase> results, out List<string> modsWithoutSettings, bool showDebug)
        {
            modsWithoutSettings = new List<string>();

            try
            {
                results = GetBepInExCoreConfig();
            }
            catch (Exception ex)
            {
                results = Enumerable.Empty<SettingEntryBase>();
                ConfigurationManager.Logger.LogError(ex);
            }

            foreach (var plugin in Utils.FindPlugins())
            {
                var type = plugin.GetType();

                var pluginInfo = plugin.Info.Metadata;

                if (type.GetCustomAttributes(typeof(BrowsableAttribute), false).Cast<BrowsableAttribute>()
                    .Any(x => !x.Browsable))
                {
                    modsWithoutSettings.Add(pluginInfo.Name);
                    continue;
                }

                var detected = new List<SettingEntryBase>();

                detected.AddRange(GetPluginConfig(plugin).Cast<SettingEntryBase>());

                detected.RemoveAll(x => x.Browsable == false);

                if (detected.Count == 0)
                    modsWithoutSettings.Add(pluginInfo.Name);

                // Allow to enable/disable plugin if it uses any update methods ------
                if (showDebug && type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(x => _updateMethodNames.Contains(x.Name)))
                {
                    var enabledSetting = new PropertySettingEntry(plugin, type.GetProperty("enabled"), plugin);
                    enabledSetting.DispName = "!Allow plugin to run on every frame";
                    enabledSetting.Description = "Disabling this will disable some or all of the plugin's functionality.\nHooks and event-based functionality will not be disabled.\nThis setting will be lost after game restart.";
                    enabledSetting.IsAdvanced = true;
                    detected.Add(enabledSetting);
                }