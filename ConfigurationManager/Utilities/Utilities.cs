
﻿// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using BepInEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ConfigurationManager.Utilities
{
    internal static class Utils
    {
        public static string ToProperCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (str.Length < 2) return str;

            // Start with the first character.
            string result = str.Substring(0, 1).ToUpper();

            // Add the remaining characters.
            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i])) result += " ";
                result += str[i];
            }

            return result;
        }

        // Search for instances of BaseUnityPlugin to also find dynamically loaded plugins. Doing this makes checking Chainloader.PluginInfos redundant.
        // Have to use FindObjectsOfType(Type) instead of FindObjectsOfType<T> because the latter is not available in some older unity versions.
        public static BaseUnityPlugin[] FindPlugins() => Array.ConvertAll(Object.FindObjectsOfType(typeof(BaseUnityPlugin)), input => (BaseUnityPlugin)input);

        public static string AppendZero(this string s)
        {