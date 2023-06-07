using UnityEngine;

namespace Tools
{
    public class DebugTexture : MonoBehaviour
    {
        private void Start()
        {
            return;
            var texture = new Texture2D(128, 128);
            GetComponent<Renderer>().material.mainTexture = texture;

            for (var y = 0; y < texture.height; y++)
            for (var x = 0; x < texture.width; x++)
            {
                var color = (x & y) != 0 ? Color.white : Color.gray;
                texture.SetPixel(x, y, color);
            }

            texture.Apply();
        }

        private void Update()
        {
        }
    }
}