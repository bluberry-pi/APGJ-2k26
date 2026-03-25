using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;

public class CutsceneManager : MonoBehaviour
{
    private bool isTyping = false;
    private bool skipRequested = false;
    private int currentLineIndex = 0;
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public GameObject videoPanel;
    public Button nextButton;

    [Header("Fade")]
    public Image fadeOverlay; // FULLSCREEN BLACK IMAGE
    public float fadeDuration = 1f;

    [Header("Text Cutscene")]
    public GameObject textCutscenePanel;
    public TextMeshProUGUI cutsceneText;
    public Button skipButton;

    [Header("Text Lines")]
    [TextArea(2, 4)]
    public string[] textLines;
    public float typewriterSpeed = 0.03f;
    public float pauseAfterLine = 2f;

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
    private bool skipped = false;

    void Start()
    {
        Time.timeScale = 0f;

        textCutscenePanel.SetActive(false);

        nextButton.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(false);

        fadeOverlay.color = new Color(0, 0, 0, 0); // start transparent

        PlayVideo();
    }

    // ── VIDEO ───────────────────────────────

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

        // 🎵 START MID MUSIC AFTER INTRO (index 1)
        if (currentIndex == 1)
        {
            StartMidMusic();
        }

        if (currentIndex < videos.Length)
        {
            PlayVideo();
        }
        else
        {
            StartCoroutine(EndCutsceneFlow());
        }
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
        midMusicSource.volume = startVolume; // reset for reuse
    }
    // ── FINAL FLOW ───────────────────────────────

    IEnumerator EndCutsceneFlow()
    {
        nextButton.gameObject.SetActive(false);

        // 🔻 FADE OUT MID MUSIC FIRST
        if (midMusicSource != null && midMusicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMidMusic());
        }

        // 🔻 FADE SCREEN
        yield return StartCoroutine(Fade(0f, 1f));

        videoPlayer.Stop();
        videoPanel.SetActive(false);

        // 🔔 RING
        SoundFXManager.instance.PlaySoundFXClip(ringring, transform, 1f);

        if (ringring != null)
            yield return new WaitForSecondsRealtime(ringring.length);

        // 💀 DESTROY FADE
        if (fadeOverlay != null)
            Destroy(fadeOverlay.gameObject);

        // 🎵 MAIN BGM STARTS
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StartBackgroundMusic();
        }

        skipButton.gameObject.SetActive(true);
        StartCoroutine(PlayTextCutscene());
    }

    // ── TEXT CUTSCENE ───────────────────────────────

    IEnumerator PlayTextCutscene()
    {
        textCutscenePanel.SetActive(true);

        currentLineIndex = 0;

        while (currentLineIndex < textLines.Length)
        {
            yield return StartCoroutine(TypeLine(textLines[currentLineIndex]));

            // Wait for player to press skip to go next
            skipRequested = false;
            yield return new WaitUntil(() => skipRequested);

            currentLineIndex++;
        }

        StartGame();
    }

    IEnumerator TypeLine(string line)
    {
        cutsceneText.text = "";
        isTyping = true;

        foreach (char c in line)
        {
            if (!isTyping) break;

            cutsceneText.text += c;
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        // ensure full line is shown
        cutsceneText.text = line;
        isTyping = false;
    }

    public void OnSkipPressed()
    {
        if (isTyping)
        {
            isTyping = false;
        }
        else
        {
            skipRequested = true;
        }
    }

    // ── FADE FUNCTION ───────────────────────────────

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

    // ── START GAME ───────────────────────────────

    void StartGame()
    {
        textCutscenePanel.SetActive(false);
        skipButton.gameObject.SetActive(false);

        Time.timeScale = 1f;

        Debug.Log("Game Started");
        Destroy(gameObject);
    }
}