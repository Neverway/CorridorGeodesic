using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ErryLib.ExtensionMethods
{ 
    public static class ExtentionMethods_VisualElement
    {
        public static VisualElement AddDivider(this VisualElement ve)
        {
            var divider = new VisualElement();
            divider.style.height = 1;
            divider.style.marginBottom = 2;
            divider.style.marginTop = -2;
            divider.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f);

            ve.Add(divider);
            return ve;
        }
        public static bool ToggleActive(this VisualElement ve)
        {
            bool isNowActive = !ve.IsActive();
            ve.SetActive(isNowActive);
            return isNowActive;
        }
        public static void SetActive(this VisualElement ve, bool active)
        {
            if (active)
                ve.style.display = DisplayStyle.Flex;
            else
                ve.style.display = DisplayStyle.None;
        }
        public static bool IsActive(this VisualElement ve) =>
            ve.style.display != DisplayStyle.None;
    }
}
