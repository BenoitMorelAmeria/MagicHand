using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleGenerator : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab; // Prefab for the bubble
    [SerializeField] private float bubbleSpawnInterval = 1.0f; // Time interval between bubble spawns
    [SerializeField] private int maxBubbles = 10; // Maximum number of bubbles to spawn
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(0.5f, 0.5f); // Width/Depth size of the plane for bubble spawning
    [SerializeField] private Vector3 spawnCenterPosition = new Vector3(0, -0.5f, 0); // Center position for bubble spawning

    private float timer = 0f;
    private List<GameObject> activeBubbles = new List<GameObject>();

    public void Update()
    {
        timer += Time.deltaTime;

        if (timer >= bubbleSpawnInterval && activeBubbles.Count < maxBubbles)
        {
            timer = 0f;
            SpawnBubble();
        }

        // Clean up any destroyed bubbles from the list
        activeBubbles.RemoveAll(b => b == null);
    }

    private void SpawnBubble()
    {
        // random XZ position around center (since plane is perpendicular to Y)
        float offsetX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float offsetZ = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
        Vector3 spawnPos = spawnCenterPosition + new Vector3(offsetX, 0f, offsetZ);

        GameObject newBubble = Instantiate(bubblePrefab, transform);
        newBubble.transform.localPosition = spawnPos;
        activeBubbles.Add(newBubble);
    }
}

