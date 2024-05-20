using UnityEngine;

namespace Tools
{
    public static class KeyBindings
    {
        public static readonly KeyCode ToggleDebug = KeyCode.F1;
        public static readonly KeyCode Quit = KeyCode.Escape;
        public static readonly KeyCode TogglePureData = KeyCode.F6;

        public static string GetBindingStrings()
        {
            //dynamicaly get all fields of this class
            var fields = typeof(KeyBindings).GetFields();
            var result = "";
            foreach (var field in fields)
            {
                result += field.Name + " : " + field.GetValue(null) + "\n";
            }
            
            return result;
        }
    }
}