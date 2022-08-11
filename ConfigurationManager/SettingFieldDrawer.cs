﻿// Made by MarC0 / ManlyMarco
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
                drawMethod(setting);
                return true;
            }
            return false;
        }

        private static void DrawBoolField(SettingEntryBase setting)
        {
            var boolVal = (bool)setting.Get();
            var result = GUILayout.Toggle(boolVal, boolVal ? "Enabled" : "Disabled", GUILayout.ExpandWidth(true));
            if (result != boolVal)
                setting.Set(result);
        }

        private static void DrawEnumField(SettingEntryBase setting)
        {
            if (setting.SettingType.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
                DrawFlagsField(setting, Enum.GetValues(setting.SettingType), _instance.RightColumnWidth);
            else
                DrawComboboxField(setting, Enum.GetValues(setting.SettingType), _instance.SettingWindowRect.yMax);
        }

        private static void DrawFlagsField(SettingEntryBase setting, IList enumValues, int maxWidth)
        {
            var currentValue = Convert.ToInt64(setting.Get());
            var allValues = enumValues.Cast<Enum>().Select(x => new { name = x.ToString(), val = Convert.ToInt64(x) }).ToArray();

            // Vertically stack Horizontal groups of the options to deal with the options taking more width than is available in the window
            GUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));
            {
                for (var index = 0; index < allValues.Length;)
                {
                    GUILayout.BeginHorizontal();
                    {
                        var currentWidth = 0;
                        for (; index < allValues.Length; index++)
                        {
                            var value = allValues[index];

                            // Skip the 0 / none enum value, just uncheck everything to get 0
                            if (value.val != 0)
                            {
                                // Make sure this horizontal group doesn't extend over window width, if it does then start a new horiz group below
                                var textDimension = (int)GUI.skin.toggle.CalcSize(new GUIContent(value.name)).x;
                                currentWidth += textDimension;
                                if (currentWidth > maxWidth)
                                    break;

                                GUI.changed = false;
                                var newVal = GUILayout.Toggle((currentValue & value.val) == value.val, value.name,
                                    GUILayout.ExpandWidth(false));
                                if (GUI.changed)
                                {
                                    var newValue = newVal ? currentValue | value.val : currentValue & ~value.val;
                                    setting.Set(Enum.ToObject(setting.SettingType, newValue));
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUI.changed = false;
            }
            GUILayout.EndVertical();
            // Make sure the reset button is properly spaced
            GUILayout.FlexibleSpace();
        }

        private static void DrawComboboxField(SettingEntryBase setting, IList list, float windowYmax)
        {
            var buttonText = ObjectToGuiContent(setting.Get());
            var dispRect = GUILayoutUtility.GetRect(buttonText, GUI.skin.button, GUILayout.ExpandWidth(true));

            if (!_comboBoxCache.TryGetValue(setting, out var box))
            {
                box = new ComboBox(dispRect, buttonText, list.Cast<object>().Select(ObjectToGuiContent).ToArray(), GUI.skin.button, windowYmax);
                _comboBoxCache[setting] = box;
            }
            else
            {
                box.Rect = dispRect;
                box.ButtonContent = buttonText;
            }

            box.Show(id =>
            {
                if (id >= 0 && id < list.Count)
                    setting.Set(list[id]);
            });
        }

        private static GUIContent ObjectToGuiContent(object x)
        {
            if (x is Enum)
            {
                var enumType = x.GetType();
                var enumMember = enumType.GetMember(x.ToString()).FirstOrDefault();
                var attr = enumMember?.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().FirstOrDefault();
                if (attr != null)
                    return new GUIContent(attr.Description);
                return new GUIContent(x.ToString().ToProperCase());
            }
            return new GUIContent(x.ToString());
        }

        private static void DrawRangeField(SettingEntryBase setting)
        {
            var value = setting.Get();
            var converted = (float)Convert.ToDouble(value, CultureInfo.InvariantCulture);
            var leftValue = (float)Convert.ToDouble(setting.AcceptableValueRange.Key, CultureInfo.InvariantCulture);
            var rightValue = (float)Convert.ToDouble(setting.AcceptableValueRange.Value, CultureInfo.InvariantCulture);

            var result = GUILayout.HorizontalSlider(converted, leftValue, rightValue, GUILayout.ExpandWidth(true));
            if (Math.Abs(result - converted) > Mathf.Abs(rightValue - leftValue) / 1000)
            {
                var newValue = Convert.ChangeType(result, setting.SettingType, CultureInfo.InvariantCulture);
                setting.Set(newValue);
            }

            if (setting.ShowRangeAsPercent == true)
            {
                DrawCenteredLabel(
                    Mathf.Round(100 * Mathf.Abs(result - leftValue) / Mathf.Abs(rightValue - leftValue)) + "%",
                    GUILayout.Width(50));
            }
            else
            {
                var strVal = value.ToString().AppendZeroIfFloat(setting.SettingType);
                var strResult = GUILayout.TextField(strVal, GUILayout.Width(50));
                if (strResult != strVal)
                {
                    try
                    {
                        var resultVal = (float)Convert.ToDouble(strResult, CultureInfo.InvariantCulture);
                        var clampedResultVal = Mathf.Clamp(resultVal, leftValue, rightValue);
                        setting.Set(Convert.ChangeType(clampedResultVal, setting.SettingType, CultureInfo.InvariantCulture));
                    }
                    catch (FormatException)
                    {
                        // Ignore user typing in bad data
                    }
                }
            }
        }

        private void DrawUnknownField(SettingEntryBase setting, int rightColumnWidth)
        {
            // Try to use user-supplied converters
            if (setting.ObjToStr != null && setting.StrToObj != null)
  