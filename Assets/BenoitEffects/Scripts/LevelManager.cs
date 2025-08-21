using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    
    [SerializeField] List<GameObject> levels = new List<GameObject>();

    GameObject currentLevel;

    int levelIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject go in levels)
        {
            if (go.activeSelf)
            {
                go.SetActive(false);
            }
        }
        LoadLevel(0);    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        LoadLevel((levelIndex + 1) % levels.Count);
    }

    public void LoadLevel(int level)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }
        if (level < 0 && level >= levels.Count)
        {
            return;
        }
        GameObject toClone = levels[level];
        currentLevel = Instantiate(toClone, toClone.transform.parent);
        levelIndex = level;
        currentLevel.SetActive(true);
    }


}
