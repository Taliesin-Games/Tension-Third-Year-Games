using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CrackGenerator : MonoBehaviour
{

    public string svg;
    public float extrusionDepth = 1f;

    void Start()
    {
        CreateMesh();
    }

    void CreateMesh()
    {
        // 1. Load points from SVG
        Vector2[] shape = LoadPolyline(svg);

        if (shape.Length == 0)
        {
            shape = LoadPathLines(svg);
        }

        // 2. Triangulate
        int[] tris2D = Triangulate(shape);

        // 3. Build 3D vertices
        int count = shape.Length;
        Vector3[] verts = new Vector3[count * 2];

        for (int i = 0; i < count; i++)
        {
            verts[i] = new Vector3(shape[i].x, 0, shape[i].y);                // top
            verts[i + count] = new Vector3(shape[i].x, -extrusionDepth, shape[i].y); // bottom
        }

        // 4. Build triangles (top + bottom + sides)
        List<int> triangles = new();

        // top
        for (int i = 0; i < tris2D.Length; i++)
            triangles.Add(tris2D[i]);

        // bottom (reverse winding)
        for (int i = 0; i < tris2D.Length; i += 3)
        {
            triangles.Add(tris2D[i] + count);
            triangles.Add(tris2D[i + 2] + count);
            triangles.Add(tris2D[i + 1] + count);
        }

        // side walls
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            triangles.Add(i);
            triangles.Add(next);
            triangles.Add(i + count);

            triangles.Add(next);
            triangles.Add(next + count);
            triangles.Add(i + count);
        }

        // 5. Build mesh
        Mesh m = new Mesh();
        m.vertices = verts;
        m.triangles = triangles.ToArray();
        m.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = m;
    }



    public Vector2[] LoadPolyline(string svgPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(svgPath);

        XmlNode polylineNode = doc.GetElementsByTagName("polyline")[0];

        if (polylineNode == null)
        {
            Debug.LogError("No polyline element found in SVG.");
            return new Vector2[0];
        }
        string pointString = polylineNode.Attributes["points"].Value;

        List<Vector2> points = new List<Vector2>();
        string[] pairs = pointString.Split(' ');

        foreach (string p in pairs)
        {
            if (string.IsNullOrWhiteSpace(p)) continue;
            var xy = p.Split(',');
            float x = float.Parse(xy[0], CultureInfo.InvariantCulture);
            float y = float.Parse(xy[1], CultureInfo.InvariantCulture);
            points.Add(new Vector2(x, y));
        }

        return points.ToArray();
    }

    public static Vector2[] LoadPathLines(string svgPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(svgPath);

        var pathNode = doc.GetElementsByTagName("path")[0];
        string d = pathNode.Attributes["d"].Value;

        List<Vector2> points = new List<Vector2>();

        // Tokenize SVG path data, splitting on spaces and commas, but preserving letters
        string[] tokens = System.Text.RegularExpressions.Regex.Split(d, @"[\s,]+");
        int i = 0;
        string lastCmd = "";

        while (i < tokens.Length)
        {
            string token = tokens[i].Trim();
            
            if (string.IsNullOrEmpty(token))
            {
                i++;
                continue;
            }

            // Check if this token starts with a command letter
            if (char.IsLetter(token[0]))
            {
                lastCmd = token[0].ToString();
                
                // Extract coordinates if they're attached to the command (e.g., "M10")
                if (token.Length > 1)
                {
                    if (!float.TryParse(token.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                    {
                        i++;
                        continue;
                    }
                    
                    if (i + 1 >= tokens.Length)
                    {
                        i++;
                        continue;
                    }
                    
                    if (!float.TryParse(tokens[++i], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                    {
                        i++;
                        continue;
                    }
                    
                    if (lastCmd == "M" || lastCmd == "L")
                    {
                        points.Add(new Vector2(x, y));
                    }
                    else if (lastCmd == "Z" || lastCmd == "z")
                    {
                        if (points.Count > 0)
                            points.Add(points[0]);
                    }
                }
                i++;
            }
            else if (char.IsDigit(token[0]) || token[0] == '-' || token[0] == '.')
            {
                // This is a coordinate value; treat as continuation of last command
                if (!float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                {
                    i++;
                    continue;
                }
                
                if (i + 1 >= tokens.Length)
                {
                    i++;
                    continue;
                }
                
                if (!float.TryParse(tokens[++i], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                {
                    i++;
                    continue;
                }
                
                if (lastCmd == "M" || lastCmd == "L")
                {
                    points.Add(new Vector2(x, y));
                }
                else if (lastCmd == "Z" || lastCmd == "z")
                {
                    if (points.Count > 0)
                        points.Add(points[0]);
                }
                i++;
            }
            else
            {
                i++;
            }
        }

        return points.ToArray();
    }


    public int[] Triangulate(Vector2[] points)
    {
        List<int> indices = new List<int>();

        int n = points.Length;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Area(points) > 0)
        {
            for (int i = 0; i < n; i++)
                V[i] = i;
        }
        else
        {
            for (int i = 0; i < n; i++)
                V[i] = (n - 1) - i;
        }

        int nv = n;
        int count = 2 * nv;

        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray(); // bad polygon

            int u = v; if (nv <= u) u = 0;
            v = u + 1; if (nv <= v) v = 0;
            int w = v + 1; if (nv <= w) w = 0;

            if (Snip(points, u, v, w, nv, V))
            {
                int a = V[u];
                int b = V[v];
                int c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);

                for (int s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];

                nv--;
                count = 2 * nv;
            }
        }

        return indices.ToArray();
    }

    static float Area(Vector2[] points)
    {
        int n = points.Length;
        float A = 0;
        for (int p = n - 1, q = 0; q < n; p = q++)
            A += points[p].x * points[q].y - points[q].x * points[p].y;
        return A * 0.5f;
    }

    bool Snip(Vector2[] points, int u, int v, int w, int n, int[] V)
    {
        Vector2 A = points[V[u]];
        Vector2 B = points[V[v]];
        Vector2 C = points[V[w]];

        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) -
                             ((B.y - A.y) * (C.x - A.x))))
            return false;

        for (int p = 0; p < n; p++)
        {
            if (p == u || p == v || p == w) continue;

            Vector2 P = points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }

        return true;
    }

    bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax = C.x - B.x, ay = C.y - B.y;
        float bx = A.x - C.x, by = A.y - C.y;
        float cx = B.x - A.x, cy = B.y - A.y;

        float apx = P.x - A.x, apy = P.y - A.y;
        float bpx = P.x - B.x, bpy = P.y - B.y;
        float cpx = P.x - C.x, cpy = P.y - C.y;

        float aCROSSbp = ax * bpy - ay * bpx;
        float cCROSSap = cx * apy - cy * apx;
        float bCROSScp = bx * cpy - by * cpx;

        return (aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f);
    }
}
