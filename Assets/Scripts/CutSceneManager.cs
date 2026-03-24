using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;

public class CutsceneManager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public GameObject videoPanel;
    public Button nextButton;

    [Header("Text Cutscene")]
    public GameObject textCutscenePanel;
    public TextMeshProUGUI cutsceneText;
    public Button skipButton;

    [Header("Text Lines")]
    [TextArea(2, 4)]
    public string[] textLines;
    public float typewriterSpeed = 0.03f;
    public float pauseAfterLine = 2f;

    private string[] videos = {
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
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        Time.timeScale = 0f;

        textCutscenePanel.SetActive(false);

        // Start in VIDEO MODE
        nextButton.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(false);

        PlayVideo();
    }

    // ── VIDEO SECTION ───────────────────────────────

    void PlayVideo()
    {
        StartCoroutine(PlayVideoRoutine());
    }

    IEnumerator PlayVideoRoutine()
    {
        string path = Path.Combine(Application.streamingAssetsPath, videos[currentIndex]);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = path;

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
    }

    public void NextVideo()
    {
        videoPlayer.Stop(); // 🔥 important

        currentIndex++;

        if (currentIndex < videos.Length)
        {
            PlayVideo();
        }
        else
        {
            videoPanel.SetActive(false);

            nextButton.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(true);

            StartCoroutine(PlayTextCutscene());
        }
    }

    // ── TEXT CUTSCENE ───────────────────────────────

    IEnumerator PlayTextCutscene()
    {
        textCutscenePanel.SetActive(true);
        skipped = false;

        foreach (string line in textLines)
        {
            if (skipped) break;

            cutsceneText.text = "";
            yield return StartCoroutine(TypeLine(line));

            if (skipped) break;

            float elapsed = 0f;
            while (elapsed < pauseAfterLine && !skipped)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        StartGame();
    }

    IEnumerator TypeLine(string line)
    {
        foreach (char c in line)
        {
            if (skipped) yield break;
            cutsceneText.text += c;
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }
    }

    // ── SKIP TEXT ───────────────────────────────────

    public void OnSkipPressed()
    {
        skipped = true;
        StopAllCoroutines();
        StartGame();
    }

    // ── START GAME ──────────────────────────────────

    void StartGame()
    {
        textCutscenePanel.SetActive(false);
        skipButton.gameObject.SetActive(false);

        Time.timeScale = 1f;

        Debug.Log("Game Started");
        Destroy(gameObject);
    }
}