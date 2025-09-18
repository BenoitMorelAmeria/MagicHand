using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSpheres : MonoBehaviour
{
    private GameObject[] spheres;

    // Start is called before the first frame update
    void Start()
    {
        // Get all children with "Sphere" components (or just their GameObjects)
        int childCount = transform.childCount;
        spheres = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            spheres[i] = transform.GetChild(i).gameObject;
        }
        Debug.Log("HandSpheres: Start " + childCount);

    }

    public GameObject GetSphere(int index) =>
        (index >= 0 && index < spheres.Length) ? spheres[index] : null;

    public int SphereCount => spheres.Length;

}
