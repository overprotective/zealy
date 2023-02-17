
﻿/// <summary>
/// Class that specifies how a setting should be displayed inside the ConfigurationManager settings window.
/// 
/// Usage:
/// This class template has to be copied inside the plugin's project and referenced by its code directly.
/// make a new instance, assign any fields that you want to override, and pass it as a tag for your setting.
/// 
/// If a field is null (default), it will be ignored and won't change how the setting is displayed.
/// If a field is non-null (you assigned a value to it), it will override default behavior.
/// </summary>
/// 
/// <example> 
/// Here's an example of overriding order of settings and marking one of the settings as advanced:
/// <code>
/// // Override IsAdvanced and Order
/// Config.Bind("X", "1", 1, new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
/// // Override only Order, IsAdvanced stays as the default value assigned by ConfigManager
/// Config.Bind("X", "2", 2, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
/// Config.Bind("X", "3", 3, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
/// </code>
/// </example>
/// 
/// <remarks> 
/// You can read more and see examples in the readme at https://github.com/BepInEx/BepInEx.ConfigurationManager
/// You can optionally remove fields that you won't use from this class, it's the same as leaving them null.
/// </remarks>
#pragma warning disable 0169, 0414, 0649
internal sealed class ConfigurationManagerAttributes
{
    /// <summary>
    /// Should the setting be shown as a percentage (only use with value range settings).
    /// </summary>
    public bool? ShowRangeAsPercent;

    /// <summary>
    /// Custom setting editor (OnGUI code that replaces the default editor provided by ConfigurationManager).
    /// See below for a deeper explanation. Using a custom drawer will cause many of the other fields to do nothing.
    /// </summary>
    public System.Action<BepInEx.Configuration.ConfigEntryBase> CustomDrawer;

    /// <summary>
    /// Custom setting editor that allows polling keyboard input with the Input (or UnityInput) class.
    /// Use either CustomDrawer or CustomHotkeyDrawer, using both at the same time leads to undefined behaviour.
    /// </summary>
    public CustomHotkeyDrawerFunc CustomHotkeyDrawer;

    /// <summary>
    /// Custom setting draw action that allows polling keyboard input with the Input class.
    /// Note: Make sure to focus on your UI control when you are accepting input so user doesn't type in the search box or in another setting (best to do this on every frame).
    /// If you don't draw any selectable UI controls You can use `GUIUtility.keyboardControl = -1;` on every frame to make sure that nothing is selected.
    /// </summary>
    /// <example>
    /// CustomHotkeyDrawer = (ConfigEntryBase setting, ref bool isEditing) =>
    /// {
    ///     if (isEditing)
    ///     {
    ///         // Make sure nothing else is selected since we aren't focusing on a text box with GUI.FocusControl.
    ///         GUIUtility.keyboardControl = -1;
    ///                     
    ///         // Use Input.GetKeyDown and others here, remember to set isEditing to false after you're done!
    ///         // It's best to check Input.anyKeyDown and set isEditing to false immediately if it's true,
    ///         // so that the input doesn't have a chance to propagate to the game itself.
    /// 
    ///         if (GUILayout.Button("Stop"))
    ///             isEditing = false;
    ///     }
    ///     else
    ///     {
    ///         if (GUILayout.Button("Start"))
    ///             isEditing = true;
    ///     }
    /// 
    ///     // This will only be true when isEditing is true and you hold any key
    ///     GUILayout.Label("Any key pressed: " + Input.anyKey);
    /// }
    /// </example>
    /// <param name="setting">
    /// Setting currently being set (if available).
    /// </param>