using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPointLightGenerator : MonoBehaviour
{
    public int lightCount = 100;

    private List<LightMovement> lightMovements = new List<LightMovement>();

    void Start(){
        lightMovements.Clear();
        var root = transform.Find("Root");
        if(root){
            foreach(var c in root){
                var child = c as Transform;
                var v = Random.value * 5;
                var rad = Random.value * Mathf.PI * 2;
                var velocity = new Vector3(Mathf.Cos(rad) * v,0,Mathf.Sin(rad) * v);
                lightMovements.Add(new LightMovement(){
                    light = child.GetComponent<Light>(),
                    velocity = velocity
                });
            }
            
        }
    }

    private void Generate(Transform parent){
        
        for(var i = 0; i < lightCount; i ++){
            var go = new GameObject("Light_" + i);
            var light = go.AddComponent<Light>();
            light.transform.SetParent(parent);
            light.type = LightType.Point;
            light.intensity = 5;
            light.color = Random.ColorHSV(0,1,1,1,1,1);
            light.range = Random.Range(2.0f,4.0f);
            light.transform.localPosition = new Vector3(Random.Range(-20f,20f),Random.Range(1f,2f),Random.Range(-20f,20f));
        
        }
    }

    [ContextMenu("Regenerate")]
    private void Generate(){
        var root = transform.Find("Root");
        if(root){
            if(Application.isPlaying){
                Object.Destroy(root.gameObject);
            }else{
                Object.DestroyImmediate(root.gameObject);
            }
        }
        root = new GameObject("Root").transform;
        root.SetParent(this.transform,false);
        root.localPosition = Vector3.zero;
        this.Generate(root);
    }
    
    void Update(){
        foreach(var m in lightMovements){
            var pos = m.light.transform.localPosition;
            pos += m.velocity * Time.deltaTime;
            if(pos.x < -20 || pos.x > 20 || pos.z < - 20 || pos.z > 20){
                m.velocity = - m.velocity;
            }else{
                m.light.transform.localPosition = pos;
            }
        }
    }


    public class  LightMovement
    {
        public Light light;
        public Vector3 velocity;


    }
}
