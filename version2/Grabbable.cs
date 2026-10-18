using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    public float minImpactSpeed = 2f;
    public float noiseRadius = 20f;
    public AudioClip impactSound;
    [Range(0f, 1f)] public float volume = 1f;
    public float noiseCooldown = 0.5f;

    public Rigidbody Body { get; private set; }
    float lastNoiseTime = -10f;
    bool warned;

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.relativeVelocity.magnitude < minImpactSpeed) return;
        if (Time.time - lastNoiseTime < noiseCooldown) return;
        lastNoiseTime = Time.time;

        if (impactSound != null)
            AudioSource.PlayClipAtPoint(impactSound, transform.position, volume);
        else if (!warned)
        {
            Debug.LogWarning("Grabbable '" + name + "' has no Impact Sound assigned.", this);
            warned = true;
        }

        NoiseUtil.Emit(transform.position, noiseRadius);
    }
}