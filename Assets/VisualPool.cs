using UnityEngine;

namespace Assets
{
    public class VisualPool : ObjectPool<GameObject>
    {
        public GameObject batRessource;

        protected override GameObject CreateOneObject()
        {
            var obj = Instantiate(batRessource);
            obj.SetActive(false);
            
            return obj;
        }

        protected override void DeactivateOneObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        protected override void RemoveOneObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
}