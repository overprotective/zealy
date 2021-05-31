
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