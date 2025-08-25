using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private MagicHand magicHand;
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private float initialForceMin = 0.1f;
    [SerializeField] private float initialForceMax = 0.2f;
    [SerializeField] private float initialTorqueMin = 0.001f;
    [SerializeField] private float initialTorqueMax = 0.003f;

    float _lastSpawnTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - _lastSpawnTime > spawnInterval)
        {
            SpawnAsteroid();
            _lastSpawnTime = Time.time;
        }
    }

    private void SpawnAsteroid()
    {
        Vector3 spawnPosition = spawnAreaCenter + new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        GameObject asteroid = Instantiate(asteroidPrefab, gameObject.transform);
        asteroid.transform.position = spawnPosition;
        Rigidbody rb = asteroid.GetComponent<Rigidbody>();
        Vector3 direction = (magicHand.GetCenter() - spawnPosition).normalized;
        if (rb != null)
        {
            rb.AddForce(direction * Random.Range(initialForceMin, initialForceMax), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(initialTorqueMin, initialTorqueMax), ForceMode.Impulse);
        }
    }

 }
