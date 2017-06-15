using UnityEngine;

/*
class Point {
	public Vector3 p;
	public Point next;
} 
*/

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshLineRenderer : MonoBehaviour
{
    
    public Material material;

    private Mesh    _lineMesh;
    private Vector3 _lastPointCenter;
    private Vector3 _lastPointLeft;
    private Vector3 _lastPointRight;
    private float   _lineWidth = .1f;
    private bool    _isFirstQuad = true;

    void Start()
    {
        _lineMesh = GetComponent<MeshFilter>().mesh;
        GetComponent<MeshRenderer>().material = material;
    }

    public float lineWidth
    {
        get { return _lineWidth; }
        set { _lineWidth = lineWidth; }
    }

    public Vector3 lastPointCenter
    {
        get { return _lastPointCenter; }
    }

    public void AddPoint(Vector3 point)
    {
        if (_lastPointCenter != Vector3.zero)
        {
            AddLine(_lineMesh, MakeQuad(_lastPointCenter, point, _lineWidth, _isFirstQuad));
            _isFirstQuad = false;
        }

        _lastPointCenter = point;
    }

    /// <summary>
    /// Adds a quad to the mesh
    /// </summary>
    /// <param name="pointCenter"></param>
    /// <param name="pointLeft"></param>
    /// <param name="pointRight"></param>
    public void AddQuad(Vector3 pointCenter, Vector3 pointLeft, Vector3 pointRight)
    {
        if (_lastPointCenter != Vector3.zero)
        {
            Vector3[] quad;
            if (_isFirstQuad)
            {   quad = new Vector3[4];
                quad[0] = transform.InverseTransformPoint(_lastPointRight);
                quad[1] = transform.InverseTransformPoint(_lastPointLeft);
                quad[2] = transform.InverseTransformPoint(pointRight);
                quad[3] = transform.InverseTransformPoint(pointLeft);
            }
            else
            {   quad = new Vector3[2];
                quad[0] = transform.InverseTransformPoint(_lastPointRight);
                quad[1] = transform.InverseTransformPoint(_lastPointLeft);
            }

            AddLine(_lineMesh, quad);
            _isFirstQuad = false;
        }

        _lastPointCenter = pointCenter;
        _lastPointLeft   = pointLeft;
        _lastPointRight  = pointRight;
    }

    // Original routine for making a quad based only center points
    Vector3[] MakeQuad(Vector3 lastPoint, Vector3 curPoint, float lineWidth, bool all)
    {
        float halfWidth = lineWidth / 2;

        Vector3[] q;
        if (all)
             q = new Vector3[4];
        else q = new Vector3[2];

        Vector3 n = Vector3.Cross(lastPoint, curPoint);
        Vector3 l = Vector3.Cross(n, curPoint - lastPoint);
        l.Normalize();

        if (all)
        {
            q[0] = transform.InverseTransformPoint(lastPoint + l *  halfWidth);
            q[1] = transform.InverseTransformPoint(lastPoint + l * -halfWidth);
            q[2] = transform.InverseTransformPoint(curPoint  + l *  halfWidth);
            q[3] = transform.InverseTransformPoint(curPoint  + l * -halfWidth);
        }
        else
        {
            q[0] = transform.InverseTransformPoint(lastPoint + l *  halfWidth);
            q[1] = transform.InverseTransformPoint(lastPoint + l * -halfWidth);
        }
        return q;
    }

    void AddLine(Mesh mesh, Vector3[] quad)
    {
        int vl = mesh.vertices.Length;

        Vector3[] verts = mesh.vertices;
        verts = resizeVertices(verts, 2 * quad.Length);

        for (int i = 0; i < 2 * quad.Length; i += 2)
        {
            verts[vl + i] = quad[i / 2];
            verts[vl + i + 1] = quad[i / 2];
        }

        Vector2[] uvs = mesh.uv;
        uvs = resizeUVs(uvs, 2 * quad.Length);

        if (quad.Length == 4)
        {
            uvs[vl] = Vector2.zero;
            uvs[vl + 1] = Vector2.zero;
            uvs[vl + 2] = Vector2.right;
            uvs[vl + 3] = Vector2.right;
            uvs[vl + 4] = Vector2.up;
            uvs[vl + 5] = Vector2.up;
            uvs[vl + 6] = Vector2.one;
            uvs[vl + 7] = Vector2.one;
        }
        else
        {
            if (vl % 8 == 0)
            {
                uvs[vl] = Vector2.zero;
                uvs[vl + 1] = Vector2.zero;
                uvs[vl + 2] = Vector2.right;
                uvs[vl + 3] = Vector2.right;

            }
            else
            {
                uvs[vl] = Vector2.up;
                uvs[vl + 1] = Vector2.up;
                uvs[vl + 2] = Vector2.one;
                uvs[vl + 3] = Vector2.one;
            }
        }

        int tl = mesh.triangles.Length;

        int[] ts = mesh.triangles;
        ts = resizeTriangles(ts, 12);

        if (quad.Length == 2)
        {
            vl -= 4;
        }

        // front-facing quad
        ts[tl] = vl;
        ts[tl + 1] = vl + 2;
        ts[tl + 2] = vl + 4;

        ts[tl + 3] = vl + 2;
        ts[tl + 4] = vl + 6;
        ts[tl + 5] = vl + 4;

        // back-facing quad
        ts[tl + 6] = vl + 5;
        ts[tl + 7] = vl + 3;
        ts[tl + 8] = vl + 1;

        ts[tl + 9] = vl + 5;
        ts[tl + 10] = vl + 7;
        ts[tl + 11] = vl + 3;

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = ts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    Vector3[] resizeVertices(Vector3[] ovs, int ns)
    {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++)
        {
            nvs[i] = ovs[i];
        }

        return nvs;
    }

    Vector2[] resizeUVs(Vector2[] uvs, int ns)
    {
        Vector2[] nvs = new Vector2[uvs.Length + ns];
        for (int i = 0; i < uvs.Length; i++)
        {
            nvs[i] = uvs[i];
        }

        return nvs;
    }

    int[] resizeTriangles(int[] ovs, int ns)
    {
        int[] nvs = new int[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++)
        {
            nvs[i] = ovs[i];
        }

        return nvs;
    }
}