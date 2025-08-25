using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictorySpawner : MonoBehaviour
{
    [SerializeField] private MagicHandGestures magicHandGestures;
    [SerializeField] private float cooldown = 5.0f;
    [SerializeField] private AudioClip victorySound;
    private AudioSource audioSource;
    private float lastVictoryTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = victorySound;
        audioSource.volume = 1.0f;
        audioSource.loop = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (magicHandGestures.magicHand.IsAvailable() && magicHandGestures.IsVictory && Time.time - lastVictoryTime > cooldown)
        {
            lastVictoryTime = Time.time;
            OnVictory();
        }

    }

    private void OnVictory()
    {
        audioSource.Play();
    }
}
