using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tools
{
    public class JsonMenuItems : MonoBehaviour
    {
        private readonly Dictionary<string, string> allValues = new Dictionary<string, string>();
        private string _content = string.Empty;

        private void Start()
        {
            var i = 0;
            foreach (var value in allValues)
            {
                var textInput = gameObject.AddComponent<TextMeshPro>();
                textInput.transform.position = new Vector3(10, 100 * i, 10);
                textInput.SetText(value.Key);

                i++;
            }

            RuntimeConfig.Get();
            _content = string.Join("\n", Config.GetAllKeys());
        }

        private void OnGUI()
        {
            // Make a text field that modifies stringToEdit.
            var content = _content;

            var i = 0;
            foreach (var value in allValues)
            {
                GUI.TextField(new Rect(50, 100 * i, 400, 100), value.Value, 200);

                i++;
            }
        }
    }
}