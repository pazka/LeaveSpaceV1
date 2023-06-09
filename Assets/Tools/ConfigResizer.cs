using Tools;
using UnityEngine;

namespace Visuals
{
    public class ConfigResizer : MonoBehaviour
    {
        private void Start()
        {
            var config = Configuration.GetConfig();
            transform.position += new Vector3(config.offsetX, config.offsetY);

            transform.localScale =
                Vector3.Scale(transform.localScale, new Vector3(config.scaleX, config.scaleY));
        }
    }
}