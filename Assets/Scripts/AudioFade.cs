using UnityEngine;
using System.Collections;

public static class AudioFade
{
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Pause();
        audioSource.volume = startVolume;
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        float targetVolume = audioSource.volume;
        audioSource.Play();
        audioSource.volume = 0;

        while (audioSource.volume > 0)
        {
            audioSource.volume += targetVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}