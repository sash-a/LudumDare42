using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerUI : MonoBehaviour
{
    public List<TextMeshProUGUI> killCounters;
    public List<TextMeshProUGUI> currencyCounters;

    public List<PlayerController> controllers;
    public List<CurrencyManager> currencyManagers;

    public EnemySpawnManager waveDetails;

    public TextMeshProUGUI waveCompleteText;
    public TextMeshProUGUI NextWaveInText;
    public TextMeshProUGUI waveCounter;
    public TextMeshProUGUI gameOverText;

    public AudioSource gameOverSound;

    private bool gameOver;
    private bool once;

    //Counter
    private int playerDeadCount = 0;

    void Start()
    {
        controllers = new List<PlayerController>();
        currencyManagers = new List<CurrencyManager>();
        once = false;
        // Enables the correct number of UI elements
        for (int i = 0; i < 4; i++)
        {
            try
            {
                killCounters[i].enabled = (WorldManager.singleton.players[i] != null);
                currencyCounters[i].enabled = (WorldManager.singleton.players[i] != null);

                currencyManagers.Add(WorldManager.singleton.players[i].GetComponent<CurrencyManager>());
                controllers.Add(WorldManager.singleton.players[i].GetComponent<PlayerController>());
            }
            catch (ArgumentOutOfRangeException e)
            {
                killCounters[i].enabled = false;
                currencyCounters[i].enabled = false;
            }
        }
    }

    void Update()
    {
        if (gameOver)
            return;
        
        for (int i = 0; i < WorldManager.singleton.players.Count; i++)
        {
            killCounters[i].text = controllers[i].kills + " - kills";
            currencyCounters[i].text = "$" + currencyManagers[i].cash;
        }

        if (waveDetails.spawner.allEnemiesDead && once && waveDetails.spawner.wave > 0)
        {
            once = false;
            waveCompleteText.gameObject.SetActive(true);
            StartCoroutine(removeWaveComplete());
        }

        float breakTimeRemaining = waveDetails.timeBetweenWaves - waveDetails.breakTimeElapsed;
        if (breakTimeRemaining <= 4f && breakTimeRemaining > 0f)
        {
            NextWaveInText.gameObject.SetActive(true);
            NextWaveInText.text =
                "Next wave in\n" + (int) (waveDetails.timeBetweenWaves - waveDetails.breakTimeElapsed);
        }

        if (breakTimeRemaining <= 0f)
        {
            waveCounter.text = "Wave " + waveDetails.spawner.wave;
            NextWaveInText.gameObject.SetActive(false);
            once = true;
        }

        foreach (var player in controllers)
        {
            if (player.isDowned)
            {
                playerDeadCount++;
            }
        }

        if (playerDeadCount >= WorldManager.singleton.players.Count)
        {
            gameOver = true;
            StartCoroutine(GameOver());
        }

        //reset counter
        playerDeadCount = 0;
    }

    IEnumerator removeWaveComplete()
    {
        yield return new WaitForSeconds(5);
        waveCompleteText.gameObject.SetActive(false);
    }

    IEnumerator GameOver()
    {
        gameOverSound.Play();
        gameOverText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        gameOverText.gameObject.SetActive(false);
        SceneManager.LoadScene(0);
    }
}