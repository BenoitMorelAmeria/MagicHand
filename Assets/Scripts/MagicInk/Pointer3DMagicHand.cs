using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer3MagicHand : MonoBehaviour
{
    [SerializeField] private MagicHand magicHand;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void Update()
    {
        if (magicHand.IsAvailable())
        {
            Vector3 position = magicHand.GetKeyPoint(4) ;
            transform.position = position;
        }
    }
}
