using UnityEngine;
using System.Collections;

public enum AreaType
{
    linear, angular
}

public class AreaFeedback : MonoBehaviour {

    // General
    public Vector3 position;
    public Vector3 rotation;
    private Quaternion orientation;

    // Geometry type
    public AreaType areaType;
    public int numTriangles;

    // Angular
    public float angle;
    public float radius;
    private Vector3 pivot;

    // Linear
    public Vector2 scale; 


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
        orientation = Quaternion.Euler(rotation);
        InitializeFeedbackArea();
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
                vertices[i + 1 + vertices.Length/2] = Quaternion.AngleAxis(i * angleStep, Vector3.forward) * Quaternion.AngleAxis(angle, Vector3.back) * Vector3.left;
                vertices[i + 1].x = -vertices[i + 1].x;
                vertices[i + 1 + vertices.Length / 2].x = -vertices[i + 1 + vertices.Length / 2].x;


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
            numTriangles = 4;
            vertices = new Vector3[8];
            uvs = new Vector2[8];
            triangles = new int[numTriangles * 3];

                
            vertices[0] = Vector2.zero;
            vertices[1] = Vector2.up;
            vertices[2] = Vector2.right;
            vertices[3] = Vector2.one;
            vertices[4] = Quaternion.AngleAxis(180,Vector2.up) * Vector2.zero;
            vertices[5] = Quaternion.AngleAxis(180, Vector2.up) * Vector2.up;
            vertices[6] = Quaternion.AngleAxis(180, Vector2.up) * Vector2.left;
            vertices[7] = Quaternion.AngleAxis(180, Vector2.up) * new Vector2(-1, 1);
                

            uvs[0] = Vector2.zero;
            uvs[4] = Vector2.zero;
            uvs[1] = Vector2.up;
            uvs[5] = Vector2.up;
            uvs[2] = Vector2.right;
            uvs[6] = Vector2.right;
            uvs[3] = Vector2.one;
            uvs[7] = Vector2.one;

            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 1;
            triangles[4] = 3;
            triangles[5] = 2;
            triangles[6] = 4;
            triangles[7] = 6;
            triangles[8] = 5;
            triangles[9] = 5;
            triangles[10] = 6;
            triangles[11] = 7;
            
        }
    }

    void InitializeFeedbackArea()
    {
        // Build mesh
        mesh = new Mesh();
        CreateGeometry(areaType);
        GetComponent<MeshFilter>().mesh = mesh;
        ApplyGeometry();

        // Create Material
        Renderer rend = GetComponent<Renderer>();
        rend.material = new Material(shader);
        rend.material.mainTexture = texture;
        
        CalculateTransform();

    }

    void ApplyGeometry()
    {
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    void CalculateTransform()
    {
        transform.position = position;
        transform.rotation = orientation;
        if (areaType == AreaType.angular)
            transform.localScale = Vector3.one * radius;
        else 
        {
            transform.localScale = new Vector3(scale.x, scale.y, 1);    
        }
    }
}
