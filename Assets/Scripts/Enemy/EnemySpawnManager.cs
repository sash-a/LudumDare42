using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public EnemySpawner spawner;
    public float timeBetweenWaves;
    public float breakTimeElapsed;

    public static EnemySpawnManager singleton;

    private bool isBreakTime;

    public List<EnemyAI> playerEnemies;
    public List<EnemyAI> heaterEnemies;

    public AudioSource mainMusic;
    private int mainCount = 0;
    public AudioSource breakMusic;
    private int breakCount = 0;
    public AudioSource secondBeep;
    public AudioSource finalBeep;


    void Start()
    {
        singleton = this;

        playerEnemies = new List<EnemyAI>();
        heaterEnemies = new List<EnemyAI>();

        spawner = GetComponent<EnemySpawner>();
        singleton = this;
        isBreakTime = false;
        breakMusic.PlayDelayed(2);
    }

    void Update()
    {
        if (!spawner.isWave && spawner.allEnemiesDead && !isBreakTime)
        {
            StartCoroutine(breakTime());
            isBreakTime = true;
            breakTimeElapsed = 0;
            return;
        }

        // Is break time
        breakTimeElapsed += Time.deltaTime;

        //Sound
        if (isBreakTime && breakCount == 0)
        {
//            mainMusic.Pause();
            StartCoroutine(AudioFade.FadeOut(mainMusic, 2));
            mainCount = 0;
            breakCount++;
            //StartCoroutine(PlayMusic(1, breakMusic));
            StartCoroutine(AudioFade.FadeIn(breakMusic, 2));
        }
        else if (!isBreakTime && mainCount == 0)
        {
//            breakMusic.Pause();
            StartCoroutine(AudioFade.FadeOut(breakMusic, 2));
            breakCount = 0;
            mainCount++;
//            StartCoroutine(PlayMusic(1, mainMusic));
            StartCoroutine(AudioFade.FadeIn(mainMusic, 2));
        }
    }

    IEnumerator breakTime()
    {
        StartCoroutine(playSecondBeep());
        yield return new WaitForSeconds(timeBetweenWaves);
        spawner.spawn();
        isBreakTime = false;
    }

    IEnumerator PlayMusic(float seconds, AudioSource music)
    {
        yield return new WaitForSeconds(seconds);
        AudioFade.FadeIn(music, 1);
//        music.PlayDelayed(1);
    }


    IEnumerator playSecondBeep()
    {
        yield return new WaitForSeconds(timeBetweenWaves - 4);
        secondBeep.Play();
        yield return new WaitForSeconds(1);
        secondBeep.Play();
        yield return new WaitForSeconds(1);
        secondBeep.Play();
        yield return new WaitForSeconds(1);
        finalBeep.Play();
    }
}