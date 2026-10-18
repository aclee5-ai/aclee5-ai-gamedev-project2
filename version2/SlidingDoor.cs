using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public bool autoHeight = true;
    public float extraClearance = 0.5f;
    public Vector3 openOffset = new Vector3(0f, 5f, 0f);
    public float speed = 3f;
    public AudioSource openSound;

    Vector3 openPos;
    bool opening;

    public void Open()
    {
        Vector3 offset = openOffset;

        if (autoHeight)
        {
            bool found = false;
            Bounds b = new Bounds(transform.position, Vector3.zero);
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                if (!found) { b = r.bounds; found = true; } else b.Encapsulate(r.bounds);
            }
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                if (!found) { b = c.bounds; found = true; } else b.Encapsulate(c.bounds);
            }
            if (found) offset = new Vector3(0f, b.size.y + extraClearance, 0f);
        }

        openPos = transform.position + offset;
        opening = true;
        if (openSound != null) openSound.Play();
    }

    void Update()
    {
        if (!opening) return;
        transform.position = Vector3.MoveTowards(transform.position, openPos, speed * Time.deltaTime);
    }
}