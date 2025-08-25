using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Vector3 scale = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 position = new Vector3(0.0f, -0.05f, 0.1f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLoadLevel(Transform sceneTransform)
    {
        sceneTransform.localScale = scale;
        sceneTransform.localPosition = position;
    }
}
