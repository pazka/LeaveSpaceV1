using UnityEngine;

namespace Tools
{
    public static class KeyBindings
    {
        public static readonly KeyCode ToggleDebugText = KeyCode.F1;
        public static readonly KeyCode Quit = KeyCode.Escape;

        public static string GetBindingStrings()
        {
            var res = "";
            var props = typeof(KeyBindings).GetProperties();
            foreach (var prop in props) res += prop.Name + " = " + prop.GetValue(null, null) + "\n";

            return res;
        }
    }
}