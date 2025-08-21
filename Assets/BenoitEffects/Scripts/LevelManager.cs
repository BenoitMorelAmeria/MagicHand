using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    
    [SerializeField] List<GameObject> levels = new List<GameObject>();

    int levelIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
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
        foreach (GameObject go in levels)
        {
            if (go.activeSelf)
            {
                go.SetActive(false);
            }
        }
        if (level >= 0 && level < levels.Count)
        {
            levels[level].SetActive(true);
        }
        levelIndex = level;
    }


}
