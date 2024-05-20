using Tools;
using UnityEngine;

public class VisualPool : ObjectPool<GameObject>
{
    public GameObject ressource;

    protected override GameObject CreateOneObject()
    {
        var obj = Instantiate(ressource);
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