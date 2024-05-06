using UnityEngine;

namespace Tools
{
    public static class KeyBindings
    {
        public static readonly KeyCode ToggleDebugText = KeyCode.F1;
        public static readonly KeyCode Quit = KeyCode.Escape;
        public static readonly KeyCode TogglePureData = KeyCode.F6;

        public static string GetBindingStrings()
        {
            var res = "";
            // Convert this class properties to a string
            foreach (var property in typeof(KeyBindings).GetFields())
            {
                res += property.Name + " : " + property.GetValue(null) + "\n";
            }
            
            return res;
        }
    }
}