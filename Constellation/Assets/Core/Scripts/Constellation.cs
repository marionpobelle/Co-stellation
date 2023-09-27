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
    public GameObject StarsParent;
    public float SegmentLineWidth=0.6f;
    public float PreviewLineWidth = 0.3f;
    public Color SegmentColor=Color.green;
    public Color PreviewSegmentColor=Color.gray;

    [Header("Saved constellations attributes")]
    public Color SavedSegmentColor=Color.white;
    public float SavedSegmentLineWidth=0.5f;

    public Segment PreviewSegment { 
        get => _previewSegment; 
        set
        {
            _previewSegment = value;
            RefreshRender();
        }
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
        if (PreviewSegment == null) return;
        if(PreviewSegment._start == null || PreviewSegment._end == null) return;

        AddLineRenderer(PreviewSegment, null, PreviewSegmentColor, PreviewLineWidth);
    }

    private void AddLineRenderer(Segment segment, Material material, Color color,float width)
    {
        var lineRenderer = new GameObject().AddComponent<LineRenderer>();
        lineRenderer.gameObject.name = "LineRenderer";
        lineRenderer.transform.SetParent(transform);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, segment._start.transform.position);
        lineRenderer.SetPosition(1, segment._end.transform.position);
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = color;
        _lineRenderers.Add(lineRenderer);
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
        PreviewSegment = null;
        RefreshRender();
    }
}
