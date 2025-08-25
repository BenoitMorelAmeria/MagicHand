using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatHandSpawner : MonoBehaviour
{
    [SerializeField] private MagicHandGestures magicHandGestures;
    [SerializeField] private GameObject spawnObjectPrefab;
    [SerializeField] private float flatnessTimeBeforeSpawn = 0.5f;
    [SerializeField] private float spawnHeightOffset = 0.1f;
    [SerializeField] private float spawnCooldown = 2f;
    [SerializeField] private float verticalityThreshold = 0.5f;
    [SerializeField] private AudioClip magicSound;

    private float flatHandTimer = 0f;
    private float lastSpawnTime = -Mathf.Infinity;
    private bool hasSpawnedForCurrentFlat = false;
    private AudioSource audioSource;



    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = magicSound;
        audioSource.volume = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float verticality = Vector3.Dot(magicHandGestures.palmNormal.normalized, Vector3.up);
        Debug.Log("Verticality: " + verticality);
        // Check if hand is flat
        bool isFlat = magicHandGestures.IsHandFlat;
        if (isFlat)
        {
            flatHandTimer += Time.deltaTime;
            bool spawnCondition = verticality > verticalityThreshold;
            spawnCondition &= flatHandTimer >= flatnessTimeBeforeSpawn;
            spawnCondition &= (Time.time - lastSpawnTime) >= spawnCooldown;
            spawnCondition &= !hasSpawnedForCurrentFlat;
            if (spawnCondition)
            {
                SpawnObject();
                hasSpawnedForCurrentFlat = true;
                lastSpawnTime = Time.time;
            }
        }
        else
        {
            flatHandTimer = 0f;
            hasSpawnedForCurrentFlat = false;
        }
    }

    private void SpawnObject()
    {
        if (audioSource != null && magicSound != null)
        {
            audioSource.PlayOneShot(magicSound);
        }
        Vector3 spawnPosition = magicHandGestures.magicHand.GetCenter() + magicHandGestures.palmNormal * spawnHeightOffset;
        Instantiate(spawnObjectPrefab, spawnPosition, Quaternion.identity);
    }
}
