using UnityEngine;
using UnityEngine.Events;

public class Valve : MonoBehaviour
{
    public Transform wheel;
    public Vector3 spinAxis = Vector3.forward;
    public float turnTime = 3f;
    public float totalRotation = 720f;

    [Header("Noise")]
    public float noiseRadius = 15f;
    public float noiseInterval = 1f;

    [Header("Audio")]
    public AudioClip creakClip;
    public AudioSource creakAudio;

    public UnityEvent onCompleted;

    public bool Completed { get; private set; }
    float progress, noiseTimer, lastTurnTime = -10f;
    bool warned;

    void Awake()
    {
        if (creakAudio == null) creakAudio = gameObject.AddComponent<AudioSource>();
        creakAudio.playOnAwake = false;
        creakAudio.loop = true;
        creakAudio.spatialBlend = 1f;
        creakAudio.minDistance = 2f;
        creakAudio.maxDistance = 25f;
        creakAudio.volume = 1f;
        if (creakClip != null) creakAudio.clip = creakClip;
    }

    public void Turn(float dt)
    {
        if (Completed) return;
        lastTurnTime = Time.time;

        progress += dt / turnTime;
        if (wheel != null) wheel.Rotate(spinAxis, (totalRotation / turnTime) * dt, Space.Self);

        if (creakAudio.clip == null)
        {
            if (!warned) { Debug.LogWarning("Valve '" + name + "' has no Creak Clip assigned.", this); warned = true; }
        }
        else if (!creakAudio.isPlaying)
        {
            creakAudio.Play();
        }

        noiseTimer += dt;
        if (noiseTimer >= noiseInterval)
        {
            noiseTimer = 0f;
            NoiseUtil.Emit(transform.position, noiseRadius);
        }

        if (progress >= 1f)
        {
            Completed = true;
            creakAudio.Stop();
            onCompleted.Invoke();
        }
    }

    void Update()
    {
        if (creakAudio != null && creakAudio.isPlaying && Time.time - lastTurnTime > 0.15f)
            creakAudio.Stop();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
}
