using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;

public class CutsceneManager : MonoBehaviour
{
    [Header("Debug")]
    public bool skipAllCutscenes = false;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public GameObject videoPanel;
    public Button nextButton;

    [Header("Fade")]
    public Image fadeOverlay;
    public float fadeDuration = 1f;

    [Header("Text Cutscene")]
    public GameObject textCutscenePanel;
    public TextMeshProUGUI cutsceneText;
    public Button skipButton; // this is the "Next" button for text lines

    [Header("Text Lines")]
    [TextArea(2, 4)]
    public string[] textLines;
    public float typewriterSpeed = 0.03f;

    [Header("Mid Cutscene Music")]
    public AudioSource midMusicSource;
    public AudioClip midMusicClip;
    public float midMusicFadeDuration = 1.5f;

    [Header("SFX")]
    public AudioClip ringring;

    private string[] videos = {
        "intro_fixed.mp4",
        "fixed_vid1.mp4",
        "fixed_vid2.mp4",
        "fixed_vid3.mp4",
        "fixed_vid4.mp4",
        "fixed_vid5.mp4"
    };

    private int currentIndex = 0;

    // ── Flavour-style next tracking (mirrors DayManager) ──
    private bool nextPressed = false;
    private bool isTyping = false;

    void Start()
    {
        Time.timeScale = 0f;

        textCutscenePanel.SetActive(false);
        nextButton.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(false);

        fadeOverlay.color = new Color(0, 0, 0, 0);

        if (skipAllCutscenes)
        {
            StartGame();
            return;
        }

        PlayVideo();
    }

    // ── Called by the Next button during text cutscene ──────────────────
    public void OnSkipPressed()
    {
        nextPressed = true;
    }

    // ── VIDEO ────────────────────────────────────────────────────────────

    void PlayVideo()
    {
        StartCoroutine(PlayVideoRoutine());
    }

    IEnumerator PlayVideoRoutine()
    {
        string path = Path.Combine(Application.streamingAssetsPath, videos[currentIndex]);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = path;

        if (videos[currentIndex] == "intro_fixed.mp4")
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetDirectAudioVolume(0, 1f);
        }
        else
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
    }

    void StartMidMusic()
    {
        if (midMusicClip == null || midMusicSource == null) return;

        midMusicSource.clip = midMusicClip;
        midMusicSource.loop = true;
        midMusicSource.volume = 1f;
        midMusicSource.Play();
    }

    public void NextVideo()
    {
        videoPlayer.Stop();

        currentIndex++;

        if (currentIndex == 1)
            StartMidMusic();

        if (currentIndex < videos.Length)
            PlayVideo();
        else
            StartCoroutine(EndCutsceneFlow());
    }

    IEnumerator FadeOutMidMusic()
    {
        float startVolume = midMusicSource.volume;
        float t = 0f;

        while (t < midMusicFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            midMusicSource.volume = Mathf.Lerp(startVolume, 0f, t / midMusicFadeDuration);
            yield return null;
        }

        midMusicSource.Stop();
        midMusicSource.volume = startVolume;
    }

    // ── FINAL FLOW ───────────────────────────────────────────────────────

    IEnumerator EndCutsceneFlow()
    {
        nextButton.gameObject.SetActive(false);

        if (midMusicSource != null && midMusicSource.isPlaying)
            yield return StartCoroutine(FadeOutMidMusic());

        yield return StartCoroutine(Fade(0f, 1f));

        videoPlayer.Stop();
        videoPanel.SetActive(false);

        SoundFXManager.instance.PlaySoundFXClip(ringring, transform, 1f);

        if (ringring != null)
            yield return new WaitForSecondsRealtime(ringring.length);

        if (fadeOverlay != null)
            Destroy(fadeOverlay.gameObject);

        if (MusicManager.Instance != null)
            MusicManager.Instance.StartBackgroundMusic();

        skipButton.gameObject.SetActive(true);
        StartCoroutine(PlayTextCutscene());
    }

    // ── TEXT CUTSCENE — mirrors DayManager flavour text exactly ──────────

    IEnumerator PlayTextCutscene()
    {
        textCutscenePanel.SetActive(true);

        for (int i = 0; i < textLines.Length; i++)
        {
            string line = textLines[i];

            // Reset flags for this line
            nextPressed = false;
            isTyping = true;
            cutsceneText.text = "";

            // Typewrite character by character
            foreach (char c in line)
            {
                if (nextPressed) break; // first press skips typing → show full line

                cutsceneText.text += c;
                yield return new WaitForSecondsRealtime(typewriterSpeed);
            }

            isTyping = false;

            // If next was pressed mid-type: snap to full line, wait for ANOTHER press
            if (nextPressed)
            {
                cutsceneText.text = line;
                nextPressed = false;            // consume the skip press
                yield return new WaitUntil(() => nextPressed); // wait for real "next"
            }
            else
            {
                // Typing finished naturally — wait for player to press Next
                yield return new WaitUntil(() => nextPressed);
            }
        }

        StartGame();
    }

    // ── FADE ─────────────────────────────────────────────────────────────

    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeOverlay.color = new Color(0, 0, 0, a);
            yield return null;
        }

        fadeOverlay.color = new Color(0, 0, 0, to);
    }

    // ── START GAME ───────────────────────────────────────────────────────

    void StartGame()
    {
        textCutscenePanel.SetActive(false);
        skipButton.gameObject.SetActive(false);

        Time.timeScale = 1f;

        Debug.Log("Game Started");
        Destroy(gameObject);
    }
}