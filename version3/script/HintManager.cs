using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance { get; private set; }

    [Header("Opening messages (shown one after another)")]
    [TextArea] public string[] introMessages;
    public float introDelay = 1.5f;

    [Header("Message timing")]
    public float messageDuration = 5f;

    [Header("Always-on HUD")]
    public string goalText = "Goal: turn all 6 valves, then find the exit.";
    public string controlsText = "Move: WASD   Look: Mouse   Interact: E   Throw: Click   Pause: Esc";

    [Header("Text size (pixels on a 1080-pixel-tall screen; scales with the window)")]
    public float messageFontSize = 40f;
    public float hudFontSize = 28f;
    public float maxBoxWidth = 1500f;

    struct Message { public string text; public float duration; }
    readonly Queue<Message> queue = new Queue<Message>();
    string current = "";
    float hideAt = -1f;
    float startTime;
    bool introQueued;

    void Awake() { Instance = this; }

    void Start() { startTime = Time.time; }

    public void Show(string message)
    {
        queue.Enqueue(new Message { text = message, duration = messageDuration });
    }

    public void SetGoal(string text) { goalText = text; }

    void Update()
    {
        if (!introQueued && Time.time - startTime >= introDelay)
        {
            introQueued = true;
            if (introMessages != null)
                foreach (string m in introMessages) Show(m);
        }

        if (Time.time >= hideAt)
        {
            current = "";
            if (queue.Count > 0)
            {
                Message m = queue.Dequeue();
                current = m.text;
                hideAt = Time.time + m.duration;
            }
        }
    }

    void OnGUI()
    {
        float s = Screen.height / 1080f;

        if (!string.IsNullOrEmpty(current))
        {
            GUIStyle msg = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(messageFontSize * s),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            msg.normal.textColor = Color.white;

            float pad = 20f * s;
            float w = Mathf.Min(maxBoxWidth * s, Screen.width - 40f);
            float textH = msg.CalcHeight(new GUIContent(current), w - pad * 2f);
            Rect box = new Rect((Screen.width - w) / 2f, 70f * s, w, textH + pad * 2f);

            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(box.x + pad, box.y + pad, w - pad * 2f, textH), current, msg);
        }

        GUIStyle hud = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(hudFontSize * s) };

        if (!string.IsNullOrEmpty(goalText))
        {
            hud.normal.textColor = new Color(1f, 0.6f, 0.1f);
            GUI.Label(new Rect(15f * s, 10f * s, Screen.width * 0.7f, 50f * s), goalText, hud);
        }

        if (!string.IsNullOrEmpty(controlsText))
        {
            hud.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
            GUI.Label(new Rect(15f * s, Screen.height - 55f * s, Screen.width - 30f * s, 50f * s), controlsText, hud);
        }
    }
}