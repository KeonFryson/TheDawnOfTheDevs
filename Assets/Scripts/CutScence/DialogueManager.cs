using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 5)]
        public string text;
        public float displayTime = 3f; // how long this line stays before next

        [Header("Optional Audio")]
        public AudioClip clip; // optional clip to play when this line starts
    }

    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public CanvasGroup dialogueGroup;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.03f;
    public int nextScene = 0;

    [Header("Dialogue Lines")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("Optional Timeline Reference")]
    public PlayableDirector timelineDirector;

    [Header("Optional Audio")]
    [Tooltip("Optional AudioSource used to play per-line clips. If null, clips will be played with PlayClipAtPoint at the main camera position.")]
    public AudioSource audioSource;

    private bool dialoguePlaying = false;

    void Start()
    {
        if (dialogueGroup != null)
            dialogueGroup.alpha = 1;
    }

    void Update()
    {
        // Press space to skip the cutscene / dialogue
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SkipCutscene();
        }
    }

    public IEnumerator PlayDialogue()
    {
        if (dialoguePlaying) yield break;
        dialoguePlaying = true;

        for (int i = 0; i < dialogueLines.Count; i++)
        {
            // play audio for this line (if assigned) then type the line
            PlayAudioForLine(dialogueLines[i]);
            yield return StartCoroutine(TypeLine(dialogueLines[i]));
            yield return new WaitForSeconds(dialogueLines[i].displayTime);
        }

        // Wait a bit then fade out / end
        yield return new WaitForSeconds(1f);
        StartCoroutine(FadeOutAndSwitch());
    }

    IEnumerator TypeLine(DialogueLine line)
    {
        nameText.text = line.speakerName;
        dialogueText.text = "";

        foreach (char c in line.text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator FadeOutAndSwitch()
    {
        if (dialogueGroup != null)
        {
            for (float t = 1; t > 0; t -= Time.deltaTime)
            {
                dialogueGroup.alpha = t;
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextScene);
    }

    // Optional: Call this from Timeline event
    public void TriggerDialogue()
    {
        StartCoroutine(PlayDialogue());
    }

    // Plays the clip assigned to the line. Uses audioSource if provided, otherwise falls back to PlayClipAtPoint.
    private void PlayAudioForLine(DialogueLine line)
    {
        if (line == null || line.clip == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(line.clip);
        }
        else
        {
            Vector3 pos = Vector3.zero;
            if (Camera.main != null) pos = Camera.main.transform.position;
            AudioSource.PlayClipAtPoint(line.clip, pos);
        }
    }

    // Stops timeline & dialogue and immediately loads the next scene
    public void SkipCutscene()
    {
        // Stop timeline if it's playing
        if (timelineDirector != null && timelineDirector.state == PlayState.Playing)
        {
            timelineDirector.Stop();
        }

        // Stop dialogue coroutine(s)
        StopAllCoroutines();
        dialoguePlaying = false;

        // Stop any playing audio on the assigned audio source
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Optionally hide/fade UI immediately
        if (dialogueGroup != null)
        {
            dialogueGroup.alpha = 0f;
        }

        // Load the next scene immediately
        SceneManager.LoadScene(nextScene);
    }
}