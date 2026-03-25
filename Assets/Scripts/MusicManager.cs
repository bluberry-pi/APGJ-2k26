using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgMusicSource;
    public AudioSource ambientSource;

    [Header("Main Background Music")]
    public AudioClip backgroundMusic;

    [Header("Day-wise Ambient Clips")]
    public AudioClip[] dayAmbientClips;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private int currentDay = -1;
    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

    }

    public void StartBackgroundMusic()
    {
        if (backgroundMusic == null) return;

        bgMusicSource.clip = backgroundMusic;
        bgMusicSource.loop = true;
        bgMusicSource.Play();
    }

    // ─────────────────────────────────────────────
    // 🌆 AMBIENT WITH FADE
    // ─────────────────────────────────────────────
    IEnumerator FadeOutAmbient()
    {
        float startVolume = ambientSource.volume;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        ambientSource.Stop();
        ambientSource.volume = startVolume; // reset for next time
    }
    public void PlayDayAmbient(int dayIndex)
    {
        if (currentDay == dayIndex) return;
        currentDay = dayIndex;

        // Stop any running fade
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        AudioClip newClip = null;

        if (dayIndex < dayAmbientClips.Length)
            newClip = dayAmbientClips[dayIndex];

        // 🚨 If no clip → fade out and stop
        if (newClip == null)
        {
            fadeRoutine = StartCoroutine(FadeOutAmbient());
            return;
        }

        // 🎵 If clip exists → fade to new clip
        fadeRoutine = StartCoroutine(FadeAmbient(newClip));
    }

    IEnumerator FadeAmbient(AudioClip newClip)
    {
        // 🔻 FADE OUT
        float startVolume = ambientSource.volume;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        ambientSource.Stop();

        // 🔄 SWITCH CLIP
        ambientSource.clip = newClip;
        ambientSource.loop = true;
        ambientSource.Play();

        // 🔺 FADE IN
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        ambientSource.volume = startVolume;
    }
}