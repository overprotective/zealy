
﻿// Popup list created by Eric Haines
// ComboBox Extended by Hyungseok Seo.(Jerry) sdragoon@nate.com
// this oop version of ComboBox is refactored by zhujiangbo jumbozhu@gmail.com
// Modified by MarC0 / ManlyMarco

using System;
using UnityEngine;

namespace ConfigurationManager.Utilities
{
    internal class ComboBox
    {
        private static bool forceToUnShow;
        private static int useControlID = -1;
        private readonly string buttonStyle;
        private bool isClickedComboButton;
        private readonly GUIContent[] listContent;
        private readonly GUIStyle listStyle;
        private readonly int _windowYmax;