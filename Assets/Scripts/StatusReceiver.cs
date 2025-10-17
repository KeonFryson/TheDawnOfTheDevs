using System.Collections;
using UnityEngine;

// Example component you can add to enemy prefabs (or add same methods to Blue_Enemy)
public class StatusReceiverExample : MonoBehaviour
{
    private Coroutine stunCoroutine;
    private bool blinded = false;
    private int smokeCount = 0;

    // Called by flash grenade
    public void OnStunned(float seconds)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(seconds));
    }

    private IEnumerator StunRoutine(float seconds)
    {
        // disable movement/AI here (implement per enemy)
        var ai = GetComponent<MonoBehaviour>(); // replace with actual AI component reference
        if (ai != null) ai.enabled = false;
        yield return new WaitForSeconds(seconds);
        if (ai != null) ai.enabled = true;
        stunCoroutine = null;
    }

    // Called by flash grenade for visual impairment
    public void OnBlinded(float seconds)
    {
        if (!blinded)
        {
            blinded = true;
            // e.g. reduce vision, change sprite, stop aiming, etc.
            StartCoroutine(BlindRoutine(seconds));
        }
    }

    private IEnumerator BlindRoutine(float seconds)
    {
        // Implement visual changes here
        yield return new WaitForSeconds(seconds);
        blinded = false;
    }

    // Smoke enter/exit notifications
    public void OnEnterSmoke()
    {
        smokeCount++;
        // e.g. reduce accuracy, slow down, or mark as concealed
    }

    public void OnExitSmoke()
    {
        smokeCount = Mathf.Max(0, smokeCount - 1);
        // restore behavior when smokeCount == 0
    }
}