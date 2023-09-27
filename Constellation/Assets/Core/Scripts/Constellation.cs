using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Segment
{
    public Star _start;
    public Star _end;

    public Segment(Star start, Star end)
    {
        _start = start;
        _end = end;
    }

    public override bool Equals(object obj)
    {
        return obj is Segment segment &&
            (
                (
                    EqualityComparer<Star>.Default.Equals(_start, segment._start) &&
                    EqualityComparer<Star>.Default.Equals(_end, segment._end)
                )
                ||
                (
                    EqualityComparer<Star>.Default.Equals(_start, segment._end) &&
                    EqualityComparer<Star>.Default.Equals(_end, segment._start)
                )
            );
    }
}

public class Constellation : MonoBehaviour
{
    public List<Segment> Segments = new List<Segment>();

    private Segment _previewSegment;

    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    private LineRenderer _previewLineRenderer;

    public GameObject StarsParent;
    public float SegmentLineWidth=0.6f;
    public float PreviewLineWidth = 0.3f;
    public Color SegmentColor=Color.green;
    public Color PreviewSegmentColor=Color.gray;

    [Header("Saved constellations attributes")]
    public Color SavedSegmentColor=Color.white;
    public float SavedSegmentLineWidth=0.5f;

    private void Start()
    {
        _previewLineRenderer = CreateLineRenderer(new Segment(null,null), null, PreviewSegmentColor, PreviewLineWidth);
        HidePreviewSegment();
    }

    public void AddSegment(Star start, Star end)
    {
        AddSegment(new Segment(start, end));
    }

    public void AddSegment(Segment segment)
    {
        if( Segments.Find(x=>x.Equals(segment))!=null ) return;
        Segments.Add(segment);
        RefreshRender();
    }

    //Returns the start point of the last segment
    public Star RemoveLastSegment()
    {
        if (Segments.Count <= 0) return null;
        Star startPointOfLastSegment = Segments.Last()._start;
        Segments.RemoveAt(Segments.Count - 1);
        RefreshRender();
        return startPointOfLastSegment;
    }

    private void RefreshRender()
    {
        foreach (var lineRenderer in _lineRenderers)
        {
            Destroy(lineRenderer.gameObject);
        }
        _lineRenderers.Clear();
        foreach (var segment in Segments)
        {
            AddLineRenderer(segment,null,SegmentColor,SegmentLineWidth);
        }
    }

    public void PreviewSegment(Vector3 start, Vector3 end)
    {
        _previewLineRenderer.enabled = true;
        _previewLineRenderer.SetPosition(0, start);
        _previewLineRenderer.SetPosition(1, end);
    }

    public void HidePreviewSegment()
    {
        _previewLineRenderer.enabled = false;
    }

    private void AddLineRenderer(Segment segment, Material material, Color color,float width)
    {
        var lineRenderer = CreateLineRenderer(segment,material,color,width);
        _lineRenderers.Add(lineRenderer);
    }

    private LineRenderer CreateLineRenderer(Segment segment, Material material, Color color, float width)
    {
        var lineRenderer = new GameObject().AddComponent<LineRenderer>();
        lineRenderer.gameObject.name = "LineRenderer";
        lineRenderer.transform.SetParent(transform);
        lineRenderer.positionCount = 2;
        
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = color;

        if(segment._start!=null && segment._end!=null)
        {
            lineRenderer.SetPosition(0, segment._start.transform.position);
            lineRenderer.SetPosition(1, segment._end.transform.position);
        }

        return lineRenderer;
    }

    public void SaveConstellation()
    {
        var consCopy = new GameObject().AddComponent<Constellation>();
        consCopy.gameObject.name = "Constellation";
        consCopy.Segments = Segments;
        consCopy.SegmentColor = SavedSegmentColor;
        consCopy.SegmentLineWidth = SavedSegmentLineWidth;
        consCopy.StarsParent = StarsParent;

        consCopy.RefreshRender();
        ClearConstellation();
    }

    public void ClearConstellation()
    {
        Segments.Clear();
        HidePreviewSegment();
        RefreshRender();
    }
}
