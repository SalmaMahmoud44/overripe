using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class VulnerableOutline : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] float pulseSpeed = 4f;
    [SerializeField] float minAlpha = 0.35f;
    [SerializeField] float maxAlpha = 1f;

    [Header("Width Pulse")]
    [SerializeField] float minWidth = 0.03f;
    [SerializeField] float maxWidth = 0.07f;

    private PolygonCollider2D polygon;
    private LineRenderer line;

    private void Awake()
    {
        polygon = GetComponent<PolygonCollider2D>();
        line = GetComponent<LineRenderer>();

        SetupOutline();
    }

    private void SetupOutline()
    {
        if (polygon == null || line == null)
            return;

        Vector2[] points = polygon.points;

        if (points.Length < 2)
            return;

        line.positionCount = points.Length;
        line.loop = true;
        line.useWorldSpace = false;

        for (int i = 0; i < points.Length; i++)
        {
            line.SetPosition(i,new Vector3(points[i].x, points[i].y, 0f));
        }

        line.startWidth = minWidth;
        line.endWidth = minWidth;
    }

    private void Update()
    {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);

        float width = Mathf.Lerp(minWidth, maxWidth, pulse);

        line.startWidth = width;
        line.endWidth = width;

        Color color = line.startColor;
        color.a = alpha;

        line.startColor = color;
        line.endColor = color;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

