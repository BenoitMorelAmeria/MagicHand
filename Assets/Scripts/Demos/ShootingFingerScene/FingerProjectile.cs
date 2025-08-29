using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerProjectile : MonoBehaviour
{
    [SerializeField] float maxDistance = 0.5f;
    Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;    
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
