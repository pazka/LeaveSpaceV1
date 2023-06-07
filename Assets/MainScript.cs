using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public VisualPool visualPool;
    
    private List<GameObject> objs = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        visualPool.PreloadNObjects(10);
        
        var converter = FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes.ALLGP);
        converter.Init(1920,1080);
        
    }

    // Update is called once per frame
    void Update()
    {
        int i = 0;
        foreach (var obj in objs)
        {
            //make the object go around in a circle given the time
            obj.transform.position = new Vector3(Mathf.Cos(Time.time+i*10), 0, Mathf.Sin(Time.time+i*10));
            i++;
        }
    }
}
