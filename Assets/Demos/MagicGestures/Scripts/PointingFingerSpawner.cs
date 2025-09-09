
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class PointingFingerSpawner : MonoBehaviour
{
    [SerializeField] private MagicHandGestures magicHandGestures;
    //[SerializeField] private AudioClip magicSound;

    public GameObject effectPrefab;   // Prefab of the particle system
    public ParticleSystem effect;
    //private AudioSource audioSource;


    public void Start()
    {
        GameObject go = Instantiate(effectPrefab, transform);
        effect.Stop();

    }

    public void Update()
    {
        if (magicHandGestures.magicHand.IsAvailable() &&  magicHandGestures.IndexPointing)
        {
            if (!effect.isPlaying)
            {
                effect.Play();
            }
            effect.transform.position = magicHandGestures.magicHand.GetCurrentKeyPoints()[8];
        }
        else
        {
            if (effect.isPlaying)
            {
                effect.Stop();
            }
        }
    }


}