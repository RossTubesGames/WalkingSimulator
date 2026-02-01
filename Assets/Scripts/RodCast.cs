using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class RodCast : MonoBehaviour
{
    public enum CastState { Idle, CastingOut, Out, ReelingIn }

    [Header("References")]
    public Transform attachPoint;
    public MagnetToFish magnetSystem;

    [Header("Direction (Inspector-Tweakable)")]
    public Vector3 localDirection = new Vector3(0, 0, 1);
    public bool normalizeDirection = true;
    public bool flattenY = true;
    [Range(0f, 1f)] public float cameraBlend = 0f;
    public float endVerticalBias = -0.25f;

    [Header("Throw Feel")]
    public float castDistance = 10f;
    public float arcHeight = 3f;
    public float castOutTime = 0.8f;
    public float reelInTime = 0.8f;

    [Header("Line")]
    public bool drawLine = true;
    public float lineWidth = 0.02f;
    public bool lineSlack = false;
    public float slackAmount = 0.25f;

    // NEW: event fired when the magnet reaches the end (waiting state)
    public event Action CastOutCompleted;
    public event Action ReelInCompleted;
    // Expose current magnet so listeners can attach things to it
    public GameObject CurrentMagnet => magnet;

    // Runtime
    private LineRenderer line;
    private GameObject magnet;
    private Rigidbody magRb;
    private Vector3 endHoldPos;
    private CastState state = CastState.Idle;
    private Coroutine running;

    void Awake()
    {
        if (!magnetSystem) magnetSystem = GetComponent<MagnetToFish>();

        line = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();
        if (!line.material)
        {
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh) line.material = new Material(sh);
        }
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = 2;
        line.enabled = false;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (state)
            {
                case CastState.Idle:
                    magnet = FindMagnetUnderAttachPoint();
                    if (magnet && magnetSystem && magnetSystem.HasMagnetAttached())
                    {
                        StartNewRoutine(CastOut());
                    }
                    break;
                case CastState.Out:
                    StartNewRoutine(ReelIn());
                    break;
            }
        }

        if ((state == CastState.CastingOut || state == CastState.Out || state == CastState.ReelingIn) && drawLine)
            DrawLine();

        if (state == CastState.Out && magnet)
            magnet.transform.position = endHoldPos;
    }

    private void StartNewRoutine(IEnumerator routine)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(routine);
    }

    private GameObject FindMagnetUnderAttachPoint()
    {
        if (!attachPoint) return null;
        for (int i = 0; i < attachPoint.childCount; i++)
        {
            var c = attachPoint.GetChild(i);
            if (c.GetComponent<MagnetMarker>() != null) return c.gameObject;
        }
        return null;
    }

    private Vector3 BuildWorldDirection()
    {
        Vector3 dir = attachPoint.TransformDirection(localDirection);
        if (flattenY) dir = Vector3.ProjectOnPlane(dir, Vector3.up);
        if (normalizeDirection && dir.sqrMagnitude > 0.0001f) dir.Normalize();
        if (cameraBlend > 0f && Camera.main)
        {
            var camDir = Camera.main.transform.forward;
            if (flattenY) camDir = Vector3.ProjectOnPlane(camDir, Vector3.up);
            camDir.Normalize();
            dir = Vector3.Slerp(dir, camDir, cameraBlend);
            if (normalizeDirection) dir.Normalize();
        }
        return dir;
    }

    private IEnumerator CastOut()
    {
        state = CastState.CastingOut;

        if (magnetSystem) magnetSystem.enabled = false;

        magnet.transform.SetParent(null, true);
        magRb = magnet.GetComponent<Rigidbody>();
        if (magRb)
        {
            magRb.isKinematic = true;
            magRb.useGravity = false;
            magRb.linearVelocity = Vector3.zero;
            magRb.angularVelocity = Vector3.zero;
        }

        Vector3 start = attachPoint.position;
        Vector3 end = start + BuildWorldDirection() * castDistance + Vector3.up * endVerticalBias;

        if (drawLine) line.enabled = true;

        float t = 0f;
        float denom = Mathf.Max(0.0001f, castOutTime);
        while (t < 1f)
        {
            t += Time.deltaTime / denom;
            float u = Mathf.Clamp01(t);

            Vector3 pos = Vector3.Lerp(start, end, u);
            pos.y += 4f * arcHeight * u * (1f - u);
            magnet.transform.position = pos;

            if (drawLine) DrawLine();
            yield return null;
        }

        endHoldPos = end;
        magnet.transform.position = endHoldPos;
        state = CastState.Out;

        // NEW: notify listeners that a cast has just completed
        CastOutCompleted?.Invoke();
    }

    private IEnumerator ReelIn()
    {
        state = CastState.ReelingIn;

        if (magRb)
        {
            magRb.isKinematic = true;
            magRb.useGravity = false;
            magRb.linearVelocity = Vector3.zero;
            magRb.angularVelocity = Vector3.zero;
        }

        Vector3 start = magnet.transform.position;
        Vector3 end = attachPoint.position;

        float t = 0f;
        float denom = Mathf.Max(0.0001f, reelInTime);
        while (t < 1f)
        {
            t += Time.deltaTime / denom;
            float u = Mathf.Clamp01(t);

            magnet.transform.position = Vector3.Lerp(start, end, u);
            if (drawLine) DrawLine();
            yield return null;
        }

        // Reattach to rod tip
        magnet.transform.SetParent(attachPoint, true);
        magnet.transform.localPosition = Vector3.zero;
        magnet.transform.localRotation = Quaternion.identity;

        if (drawLine) line.enabled = false;

        if (magnetSystem) magnetSystem.enabled = true;

        magnet = null;
        magRb = null;
        state = CastState.Idle;

        // NEW: notify listeners that reel-in is complete
        ReelInCompleted?.Invoke();
    }

    private void DrawLine()
    {
        if (!line || !magnet) return;

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        Vector3 a = attachPoint.position;
        Vector3 b = magnet.transform.position;

        if (!lineSlack)
        {
            if (line.positionCount != 2) line.positionCount = 2;
            line.SetPosition(0, a);
            line.SetPosition(1, b);
        }
        else
        {
            if (line.positionCount != 3) line.positionCount = 3;
            Vector3 mid = (a + b) * 0.5f + Vector3.down * slackAmount;
            line.SetPosition(0, a);
            line.SetPosition(1, mid);
            line.SetPosition(2, b);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!attachPoint) return;
        Gizmos.color = Color.cyan;
        Vector3 start = attachPoint.position;
        Vector3 end = start + BuildWorldDirection() * castDistance + Vector3.up * endVerticalBias;

        Vector3 p0 = start;
        for (int i = 1; i <= 20; i++)
        {
            float u = i / 20f;
            Vector3 p1 = Vector3.Lerp(start, end, u);
            p1.y += 4f * arcHeight * u * (1f - u);
            Gizmos.DrawLine(p0, p1);
            p0 = p1;
        }
    }
}
