using UnityEngine;
using System.Collections;

public enum AreaType
{
    linear, angular
}

public class AreaFeedback : MonoBehaviour {

    // General
    public Vector3 position;
    public Vector3 orientation;

    // Geometry type
    public AreaType areaType;
    public int numTriangles;

    // Angular
    public float angle;
    public float radius;

    // Linear

    // Geometry
    private Mesh mesh; 
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

    // Material and Texture
    public Texture texture;
    public Shader shader;
   

	// Use this for initialization
	void Start () {
        mesh = new Mesh();
        CreateGeometry(areaType);
        GetComponent<MeshFilter>().mesh = mesh;
        ApplyGeometry();
        Renderer rend = GetComponent<Renderer>();
        rend.material = new Material(shader);
        rend.material.mainTexture = texture;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void CreateGeometry(AreaType type)
    {
        if (type == AreaType.angular && numTriangles > 0)
        {
            // Init arrays
            numTriangles = (numTriangles+1)*2;
            vertices = new Vector3[numTriangles + 4];
            uvs = new Vector2[vertices.Length];
            triangles = new int[numTriangles * 3];

            // Calculate rotation angle and uvScaleFactor
            float angleStep = angle / (numTriangles/2 -1);
            float uvScaleFactor = 0.5f / radius;

            // Init pivot
            vertices[0] = Vector3.zero;
            vertices[vertices.Length / 2] = Vector3.zero;
            uvs[0] = new Vector2(0.5f,0.5f);
            uvs[vertices.Length / 2] = uvs[0];

            for (int i = 0; i < numTriangles/2; i++)
            {
                // Calculate vertices
                vertices[i + 1] = Quaternion.AngleAxis(i * angleStep, Vector3.back) * Vector3.left;
                vertices[i + 1 + vertices.Length / 2] = Quaternion.AngleAxis(i * angleStep, Vector3.forward) * Vector3.left; // Fix that

                // Calculate uv coordinates
                uvs[i + 1] = uvs[0] + (Vector2) (uvScaleFactor * vertices[i+1]);
                uvs[i + 1 + vertices.Length/2] = uvs[0] + (Vector2)(uvScaleFactor * vertices[uvs.Length/2 + i + 1]);

                // Calculate triangles
                triangles[i * 3] = i + 1;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = 0;
                triangles[i * 3 + triangles.Length / 2] = i + 1 + vertices.Length/2;
                triangles[i * 3 + 1 + triangles.Length / 2] = i + 2 + vertices.Length/2;
                triangles[i * 3 + 2 + triangles.Length / 2] = vertices.Length/2;

            }
        }

        if (type == AreaType.linear)
        {

        }
    }

    void ApplyGeometry()
    {
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rot)
    {
        Vector3 dir = point - pivot;         // get point direction relative to pivot
        dir = rot * dir;                     // rotate it
        point = dir + pivot;                 // calculate rotated point
        return point;
    }
}
