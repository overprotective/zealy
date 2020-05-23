﻿using System;
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