using UnityEngine;
public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float range = 2.5f;

    [Header("Grabbing")]
    public float maxGrabMass = 15f;
    public float holdDistance = 1.5f;
    public float holdPower = 15f;
    public float maxHoldSeparation = 3f;
    public float throwSpeed = 12f;

    Transform holdPoint;
    Grabbable held;
    CharacterController cc;
    string prompt = "";

    void Awake()
    {
        if (cam == null) cam = UnityEngine.Camera.main.transform;
        cc = GetComponentInParent<CharacterController>();

        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.SetParent(cam);
        holdPoint.localPosition = Vector3.forward * holdDistance;
        holdPoint.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (held != null)
        {
            prompt = "E - Drop   |   Click - Throw";
            if (Input.GetKeyDown(interactKey)) Drop();
            else if (Input.GetMouseButtonDown(0)) Throw();
            return;
        }

        prompt = "";
        if (!Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
            return;
        Valve valve = hit.collider.GetComponentInParent<Valve>();
        if (valve != null)
        {
            if (!valve.Completed)
            {
                prompt = "Hold E - Turn valve";
                if (Input.GetKey(interactKey)) valve.Turn(Time.deltaTime);
            }
            return;
        }

        Grabbable g = hit.collider.GetComponentInParent<Grabbable>();
        if (g != null)
        {
            if (g.Body.mass > maxGrabMass) { prompt = "Too heavy"; return; }
            prompt = "E - Pick up";
            if (Input.GetKeyDown(interactKey)) Grab(g);
        }
    }

    void FixedUpdate()
    {
        if (held == null) return;

        Vector3 toHold = holdPoint.position - held.Body.position;
        if (toHold.magnitude > maxHoldSeparation) { Drop(); return; }

        held.Body.linearVelocity = toHold * holdPower;
        held.Body.angularVelocity = Vector3.zero;
    }

    void Grab(Grabbable g)
    {
        held = g;
        held.Body.useGravity = false;
        SetIgnoreWithPlayer(held, true);
    }

    void Drop()
    {
        if (held == null) return;
        held.Body.useGravity = true;
        SetIgnoreWithPlayer(held, false);
        held = null;
    }

    void Throw()
    {
        Grabbable g = held;
        Drop();
        g.Body.linearVelocity = cam.forward * throwSpeed;
    }

    void SetIgnoreWithPlayer(Grabbable g, bool ignore)
    {
        if (cc == null) return;
        foreach (Collider c in g.GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(cc, c, ignore);
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(prompt)) return;

        float s = Screen.height / 1080f;
        GUIStyle style = new GUIStyle(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(40f * s) };

        Rect r = new Rect(0, Screen.height * 0.6f, Screen.width, 70f * s);

        style.normal.textColor = Color.black;
        GUI.Label(new Rect(r.x + 2f * s, r.y + 2f * s, r.width, r.height), prompt, style);
        style.normal.textColor = Color.white;
        GUI.Label(r, prompt, style);
    }
}
