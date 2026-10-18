using UnityEngine;
using UnityEngine.Events;


public class ValveProgress : MonoBehaviour
{
    [System.Serializable]
    public class Unlock
    {
        public string label = "Gate";
        public int valvesNeeded = 1;
        public UnityEvent onReached;
        [HideInInspector] public bool fired;
    }

    public Unlock[] unlocks;
    public int totalValves = 6;
    public AudioSource unlockSound;

    public int Completed { get; private set; }

    public void RegisterValve()
    {
        Completed++;

        foreach (Unlock u in unlocks)
        {
            if (!u.fired && Completed >= u.valvesNeeded)
            {
                u.fired = true;
                if (unlockSound != null) unlockSound.Play();
                u.onReached.Invoke();
            }
        }
    }

    void OnGUI()
    {
        float s = Screen.height / 1080f;
        GUIStyle style = new GUIStyle(GUI.skin.label)
            { fontSize = Mathf.RoundToInt(36f * s), alignment = TextAnchor.UpperRight };
        style.normal.textColor = new Color(1f, 0.6f, 0.1f);
        GUI.Label(new Rect(Screen.width - 620f * s, 10f * s, 600f * s, 60f * s),
                $"Valves: {Completed} / {totalValves}", style);
    }
}