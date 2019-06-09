using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public GameObject[] spawners;
    public GameObject playerPrefab;
    public Vector3 playerSpawnPos = Vector3.zero;

    public GameObject[] buttons;

    public string GreenTag;
    public string RedTag;

    public int currentLevel;
    public int[] timeToLevel;
    public int[] numberOfGreensToSurviveOnLevel;
   
    private float currentTimer = 0f;

    private bool isPlayerGreen;

    public GameObject WinPanel;
    public GameObject LosePanel;

    public Text timerText;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        foreach (GameObject spawner in spawners)
        {
            spawner.SetActive(false);
        }

        currentLevel = -1;
        currentTimer = timeToLevel[0];
    }

    public GameObject StartGame()
    {
        Destroy(GameObject.FindObjectOfType<Camera>());
        foreach (GameObject button in buttons)
        {
            button.SetActive(false);
        }

        foreach (GameObject spawner in spawners)
        {
            spawner.SetActive(true);
        }
        GameObject go = Instantiate(playerPrefab);
        go.transform.position = playerSpawnPos;

        ChangeLevel();

        return go;
    }

    public void StartAsGreen()
    {
        GameObject player = StartGame();
        isPlayerGreen = player.GetComponentInChildren<RayShooter>().isGreen = true;
        player.tag = GreenTag;
    }

    public void StartAsRed()
    {
        GameObject player = StartGame();
        player.GetComponentInChildren<RayShooter>().isGreen = false;
        player.tag = RedTag;
    }

    private void Update()
    {
        if (currentLevel < 0)
            return;
        currentTimer -= Time.deltaTime;
        timerText.text = currentTimer.ToString();        
        isWinOnLevel();        
    }

  

    private void isWinOnLevel()
    {
        if (isPlayerGreen)
        {
            if (GameObject.FindGameObjectsWithTag(GreenTag).Length - 1 >= numberOfGreensToSurviveOnLevel[currentLevel] && currentTimer <= 0)
            {
                ChangeLevel();
            }
            else if (currentTimer <= 0)
            {                
                LoseGame();
            }
        }
        else
        {
            if (GameObject.FindGameObjectsWithTag(GreenTag).Length <= 0)
            {
                ChangeLevel();
            }
            else if (currentTimer <= 0)
            {
                LoseGame();
            }
        }
    }

    public void ChangeLevel()
    {
        if (currentLevel + 1 < timeToLevel.Length)
        {
            currentLevel++;
            currentTimer = timeToLevel[currentLevel];

            foreach (Boid boid in GameObject.FindObjectsOfType<Boid>())
            {
                GameObject go = boid.gameObject;
                if (go.layer != LayerMask.NameToLayer("Player"))
                    Destroy(go);
            }
            foreach (Spawner _spawner in GameObject.FindObjectsOfType<Spawner>())
            {
                _spawner.Spawn();
            }
        }
        else
        {
            CompleteGame();
        }
    }

    public void CompleteGame()
    {
        currentLevel = -1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameObject.FindObjectOfType<RayShooter>().canIshowAim = false;
        WinPanel.SetActive(true);
    }

    public void LoseGame()
    {
        currentLevel = -1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameObject.FindObjectOfType<RayShooter>().canIshowAim = false;
        LosePanel.SetActive(true);
    }

    public void RestartGame()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
}
