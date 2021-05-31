
﻿// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using BepInEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ConfigurationManager
{
    /// <summary>
    /// Class representing all data about a setting collected by ConfigurationManager.
    /// </summary>
    public abstract class SettingEntryBase
    {
        /// <summary>
        /// List of values this setting can take
        /// </summary>
        public object[] AcceptableValues { get; protected set; }

        /// <summary>
        /// Range of the values this setting can take
        /// </summary>
        public KeyValuePair<object, object> AcceptableValueRange { get; protected set; }

        /// <summary>
        /// Should the setting be shown as a percentage (only applies to value range settings)
        /// </summary>
        public bool? ShowRangeAsPercent { get; protected set; }

        /// <summary>
        /// Custom setting draw action.
        /// Use either CustomDrawer or CustomHotkeyDrawer, using both at the same time leads to undefined behaviour.
        /// </summary>
        public Action<BepInEx.Configuration.ConfigEntryBase> CustomDrawer { get; private set; }

        /// <summary>
        /// Custom setting draw action that allows polling keyboard input with the Input class.
        /// Use either CustomDrawer or CustomHotkeyDrawer, using both at the same time leads to undefined behaviour.
        /// </summary>
        public CustomHotkeyDrawerFunc CustomHotkeyDrawer { get; private set; }

        /// <summary>
        /// Custom setting draw action that allows polling keyboard input with the Input class.
        /// </summary>
        /// <param name="setting">Setting currently being set, is available</param>
        /// <param name="isCurrentlyAcceptingInput">Set this ref parameter to true when you want the current setting drawer to receive Input events. Remember to set it to false after you are done!</param>
        public delegate void CustomHotkeyDrawerFunc(BepInEx.Configuration.ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);

        /// <summary>
        /// Show this setting in the settings screen at all? If false, don't show.
        /// </summary>
        public bool? Browsable { get; protected set; }

        /// <summary>
        /// Category the setting is under. Null to be directly under the plugin.
        /// </summary>
        public string Category { get; protected set; }

        /// <summary>
        /// If set, a "Default" button will be shown next to the setting to allow resetting to default.
        /// </summary>
        public object DefaultValue { get; protected set; }

        /// <summary>
        /// Force the "Reset" button to not be displayed, even if a valid DefaultValue is available. 
        /// </summary>
        public bool HideDefaultButton { get; protected set; }

        /// <summary>
        /// Force the setting name to not be displayed. Should only be used with a <see cref="CustomDrawer"/> to get more space.
        /// Can be used together with <see cref="HideDefaultButton"/> to gain even more space.
        /// </summary>
        public bool HideSettingName { get; protected set; }

        /// <summary>
        /// Optional description shown when hovering over the setting
        /// </summary>
        public string Description { get; protected internal set; }

        /// <summary>
        /// Name of the setting
        /// </summary>
        public virtual string DispName { get; protected internal set; }

        /// <summary>
        /// Plugin this setting belongs to
        /// </summary>
        public BepInPlugin PluginInfo { get; protected internal set; }

        /// <summary>
        /// Only allow showing of the value. False whenever possible by default.
        /// </summary>
        public bool? ReadOnly { get; protected set; }

        /// <summary>
        /// Type of the variable
        /// </summary>
        public abstract Type SettingType { get; }
