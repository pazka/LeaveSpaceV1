using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
    private List<float[]> vals = new List<float[]>();
    private List<GameObject> objs = new List<GameObject>();
    private List<GameObject> objs2 = new List<GameObject>();
    [SerializeField] private GameObject vis;
    [SerializeField] private float centerX = 0.5f;
    [SerializeField] private float centerY = 0.5f;
    [SerializeField] private float centerZ = 0.5f;

    private GameObject centerVis;
    private float PixelRatio = 1000;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                vals.Add(new[] {Random.value, Random.value});
            }
        }

        for (int i = 0; i < 100; i++)
        {
            objs.Add(Instantiate(vis));
            objs2.Add(Instantiate(vis));
            objs2[i].transform.position = new Vector3(
                vals[i][0] * PixelRatio,
                vals[i][1] * PixelRatio,
                0
            );
        }

        centerVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    }

    // Update is called once per frame
    void Update()
    {
        float[] centerPosX = new float[] {this.centerX, centerZ};
        float[] centerPosY = new float[] {this.centerY, centerZ};

        for (int i = 0; i < 100; i++)
        {
            float posX = FlattenCurve.GetFlattenedOneDimensionPoint(vals[i][0], centerPosX);
            float posY = FlattenCurve.GetFlattenedOneDimensionPoint(vals[i][1], centerPosY);
            objs[i].transform.position = new Vector3(
                posX * PixelRatio,
                posY * PixelRatio,
                0
            );
            //
            // var initPos = new Vector3(vals[i][0]* PixelRatio, vals[i][1]* PixelRatio, 0);
            // var startPos = new Vector3(centerX * PixelRatio, centerY * PixelRatio, centerZ * PixelRatio);
            // var p2prime = new Vector3(vals[i][0] * PixelRatio, vals[i][1] * PixelRatio, posX[1] * PixelRatio);
            // var p1 = new Vector3(posX[0] * PixelRatio, posY[0] * PixelRatio, 0);
            //
            // Debug.DrawLine(startPos, p2prime, Color.red);
            // Debug.DrawLine(p2prime, initPos, Color.magenta);
            // Debug.DrawLine(p2prime, p1, Color.green);
            
            Debug.DrawLine(objs[i].transform.position,objs2[i].transform.position,Color.green);
        }

        centerVis.transform.position = new Vector3(
            centerX * PixelRatio,
            centerY * PixelRatio,
            centerZ * PixelRatio
        );

        centerVis.transform.localScale = new Vector3(20, 20, 20);
        
    }
}