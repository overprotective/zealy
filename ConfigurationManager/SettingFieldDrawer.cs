// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using ConfigurationManager.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using BepInEx;
using UnityEngine;

namespace ConfigurationManager
{
    internal class SettingFieldDrawer
    {
        private static IEnumerable<KeyCode> _keysToCheck;

        public static Dictionary<Type, Action<SettingEntryBase>> SettingDrawHandlers { get; }

        private static readonly Dictionary<SettingEntryBase, ComboBox> _comboBoxCache = new Dictionary<SettingEntryBase, ComboBox>();
        private static readonly Dictionary<SettingEntryBase, ColorCacheEntry> _colorCache = new Dictionary<SettingEntryBase, ColorCacheEntry>();

        private static ConfigurationManager _instance;

        private static SettingEntryBase _currentKeyboardShortcutToSet;
        public static bool SettingKeyboardShortcut => _currentKeyboardShortcutToSet != null;

        static SettingFieldDrawer()
        {
            SettingDrawHandlers = new Dictionary<Type, Action<SettingEntryBase>>
            {
                {typeof(bool), DrawBoolField},
                {typeof(BepInEx.Configuration.KeyboardShortcut), DrawKeyboardShortcut},
                {typeof(KeyCode), DrawKeyCode },
                {typeof(Color), DrawColor },
                {typeof(Vector2), DrawVector2 },
                {typeof(Vector3), DrawVector3 },
                {typeof(Vector4), DrawVector4 },
                {typeof(Quaternion), DrawQuaternion },
            };
        }

        public SettingFieldDrawer(ConfigurationManager instance)
        {
            _instance = instance;
        }

        public void DrawSettingValue(SettingEntryBase setting)
        {
            if (setting.CustomDrawer != null)
                setting.CustomDrawer(setting is ConfigSettingEntry newSetting ? newSetting.Entry : null);
            else if (setting.CustomHotkeyDrawer != null)
            {
                var isBeingSet = _currentKeyboardShortcutToSet == setting;
                var isBeingSetOriginal = isBeingSet;

                setting.CustomHotkeyDrawer(setting is ConfigSettingEntry newSetting ? newSetting.Entry : null, ref isBeingSet);

                if (isBeingSet != isBeingSetOriginal)
                    _currentKeyboardShortcutToSet = isBeingSet ? setting : null;
            }
            else if (setting.ShowRangeAsPercent != null && setting.AcceptableValueRange.Key != null)
                DrawRangeField(setting);
            else if (setting.AcceptableValues != null)
                DrawListField(setting);
            else if (DrawFieldBasedOnValueType(setting))
                return;
            else if (setting.SettingType.IsEnum)
                DrawEnumField(setting);
            else
                DrawUnknownField(setting, _instance.RightColumnWidth);
        }

        public static void ClearCache()
        {
            _comboBoxCache.Clear();

            foreach (var tex in _colorCache)
                UnityEngine.Object.Destroy(tex.Value.Tex);
            _colorCache.Clear();
        }

        public static void DrawCenteredLabel(string text, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            GUILayout.FlexibleSpace();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static GUIStyle _categoryHeaderSkin;
        public static void DrawCategoryHeader(string text)
        {
            if (_categoryHeaderSkin == null)
            {
                _categoryHeaderSkin = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperCenter,
                    wordWrap = true,
                    stretchWidth = true,
                    fontSize = 14
                };
            }

            GUILayout.Label(text, _categoryHeaderSkin);
        }

        private static GUIStyle _pluginHeaderSkin;
        public static bool DrawPluginHeader(GUIContent content, bool isCollapsed)
        {
            if (_pluginHeaderSkin == null)
            {
                _pluginHeaderSkin = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperCenter,
                    wordWrap = true,
                    stretchWidth = true,
                    fontSize = 15
                };
            }

            if (isCollapsed) content.text += "\n...";
            return GUILayout.Button(content, _pluginHeaderSkin, GUILayout.ExpandWidth(true));
        }

        public static bool DrawCurrentDropdown()
        {
            if (ComboBox.CurrentDropdownDrawer != null)
            {
                ComboBox.CurrentDropdownDrawer.Invoke();
                ComboBox.CurrentDropdownDrawer = null;
                return true;
            }
            return false;
        }

        private static void DrawListField(SettingEntryBase setting)
        {
            var acceptableValues = setting.AcceptableValues;
            if (acceptableValues.Length == 0)
                throw new ArgumentException("AcceptableValueListAttribute returned an empty list of acceptable values. You need to supply at least 1 option.");

            if (!setting.SettingType.IsInstanceOfType(acceptableValues.FirstOrDefault(x => x != null)))
                throw new ArgumentException("AcceptableValueListAttribute returned a list with items of type other than the settng type itself.");

            if (setting.SettingType == typeof(KeyCode))
                DrawKeyCode(setting);
            else
                DrawComboboxField(setting, acceptableValues, _instance.SettingWindowRect.yMax);
        }

        private static bool DrawFieldBasedOnValueType(SettingEntryBase setting)
        {
            if (SettingDrawHandlers.TryGetValue(setting.SettingType, out var drawMethod))
            {
