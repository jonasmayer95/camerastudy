using UnityEngine;
using System.Collections;

public enum AreaType
{
    linear, angular
}
public enum ColoringType
{
    continious, jumping, interpolate
}

public class AreaFeedback : InseilFeedback {

    // General
    public Vector3 position;
    public Vector3 rotation;
    private Quaternion orientation;

    // Geometry type
    public AreaType areaType;
    public ColoringType colorType;
    public int numTriangles;

    // Angular
    public float angle;
    public float radius;
    private Vector3 pivot;
    public float midColorRange;

    // Linear
    public Vector2 scale;

    private bool forward = true;
    // Geometry
    private Mesh mesh; 
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;
    private Color[] colors;
    public Color startColor;
    public Color midColor;
    public Color endColor;
    public float midPoint;
    public float time;
    public Transform bone;
	// Use this for initialization
	void Start () {
        orientation = Quaternion.Euler(rotation);
        InitializeFeedbackArea();
        time *= colors.Length/2 - 1;
	}
	
	// Update is called once per frame
	void Update () {

        if (forward)
        {
            midPoint += Time.deltaTime * time;
            if (midPoint > colors.Length / 2)
            {
                forward = false;
            }
        }
        else 
        {
            midPoint -= Time.deltaTime * time;
            if (midPoint < 0)
            {
                forward = true;
            }
        }
        UpdateColor();
        transform.position = bone.position;
        transform.rotation = bone.parent.rotation * Quaternion.FromToRotation(Vector3.up, Vector3.right) *Quaternion.FromToRotation(Vector3.right, Vector3.left);
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
            colors = new Color[vertices.Length];

            // Calculate rotation angle and uvScaleFactor
            float angleStep = angle / (numTriangles/2 -1);
            float uvScaleFactor = 0.5f / radius;


            // Init pivot
            vertices[0] = Vector3.zero;
            vertices[vertices.Length / 2] = Vector3.zero;
            uvs[0] = new Vector2(0.5f,0.5f);
            uvs[vertices.Length / 2] = uvs[0];
            Debug.Log(uvs[0]);
            colors[0] = (startColor + endColor)/2;
            colors[vertices.Length/2] = (startColor + endColor) / 2;

            for (int i = 0; i < numTriangles/2; i++)
            {
                // Calculate vertices
                vertices[i + 1] = Quaternion.AngleAxis(i * angleStep, Vector3.back) * Vector3.left;               
                vertices[i + 1 + vertices.Length/2] = Quaternion.AngleAxis(i * angleStep, Vector3.forward) * Quaternion.AngleAxis(angle, Vector3.back) * Vector3.left;
                vertices[i + 1].x = -vertices[i + 1].x;
                vertices[i + 1 + vertices.Length / 2].x = -vertices[i + 1 + vertices.Length / 2].x;

                // Coloring vertices
                if (i <= midPoint)
                {
                    colors[i + 1] = Color.Lerp(startColor, midColor, vertices[i + 1].y);
                    colors[i + colors.Length / 2 + 1] = Color.Lerp(startColor, midColor, vertices[i + 1].y);
                }
                else
                {
                    colors[i + 1] = Color.Lerp(midColor, endColor, vertices[i + 1].y);
                    colors[i + colors.Length / 2 + 1] = Color.Lerp(midColor, endColor, vertices[i + 1].y);
                }

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
        
        CalculateTransform();

    }

    void ApplyGeometry()
    {
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.colors = colors;
    }

    void CalculateTransform()
    {
        //transform.position = position;
        transform.position = bone.position;
        transform.rotation = bone.rotation;
        if (areaType == AreaType.angular)
            transform.localScale = Vector3.one * radius;
        else 
        {
            transform.localScale = new Vector3(scale.x, scale.y, 1);    
        }
    }

    void UpdateColor()
    {
        colors[0] = Color.black;
        colors[colors.Length / 2] = Color.black;

        if (colorType == ColoringType.continious)
        // Continous coloring
        {
            for (int i = 1; i < colors.Length / 2; i++)
            {
                if (i <= midPoint)
                {
                    colors[i] = Color.Lerp(startColor, midColor, (float)i / midPoint);
                    colors[colors.Length -i] = Color.Lerp(startColor, midColor, (float)i / midPoint);
                }
                else
                {
                    colors[i] = Color.Lerp(midColor, endColor, ((float)i - midPoint) / (colors.Length / 2 - 1 - midPoint));
                    colors[colors.Length - i] = Color.Lerp(midColor, endColor, ((float)i - midPoint) / (colors.Length / 2 - 1 - midPoint));
                }
            }
        }
        else if (colorType == ColoringType.jumping)
        {
            // jumping coloring
            for (int i = 1; i < colors.Length / 2; i++)
            {
                colors[i] = startColor;
                colors[colors.Length/2 - 1 -i] = startColor;
            }
            colors[(int)midPoint] = midColor;
            colors[((int)midPoint + 1) % (colors.Length / 2)] = midColor;
            colors[((int)midPoint - 1 + colors.Length / 2) % (colors.Length / 2)] = midColor;
            colors[colors.Length - 1 - (int)midPoint] = midColor;
            colors[colors.Length / 2 - 1 - ((int)midPoint + 1) % (colors.Length / 2) + colors.Length / 2] = midColor;
            colors[colors.Length / 2 - 1 - ((int)midPoint - 1 + colors.Length / 2) % (colors.Length / 2) + colors.Length / 2] = midColor;
            colors[0] = Color.black;
            colors[colors.Length / 2] = Color.black;
        }
        else if (colorType == ColoringType.interpolate)
        {
            int range = (int)((colors.Length / 2 - 1) * midColorRange);
            for(int i = 1; i < colors.Length/2; i++)
            {
                if((int)midPoint - range > i+1)
                {
                    colors[i] = startColor;
                    colors[colors.Length  - i] = startColor;
                }
                else if ((int)midPoint + range < i)
                {
                    colors[i] = endColor;
                    colors[colors.Length  - i] = endColor;
                }
                else
                {
                    if (i < midPoint)
                    {
                        colors[i] = Color.Lerp(startColor, midColor, (i - (midPoint - range)) / (2 * range));
                        colors[colors.Length  - i] = Color.Lerp(startColor, midColor, (i - (midPoint - range)) / (2 * range));
                    }
                    else
                    {
                        colors[i] = Color.Lerp(midColor, endColor, 1 - ((midPoint + range) - i) / (2 * range));
                        colors[colors.Length  - i] = Color.Lerp(midColor, endColor, 1 - ((midPoint + range) - i) / (2 * range));
                        
                    }
                }
            }
        }
        mesh.colors = colors;
    }
}
