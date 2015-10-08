using UnityEngine;
using System.Collections;

public class InseilMainCamera : MonoBehaviour {

    public static InseilMainCamera instance;
    public float speed;
    public float border;
    private Camera cam;
    private Vector2 minPositions;
    private Vector2 maxPositions;
    private float minZ;
    private Transform relTo;
    Vector3 closePos = Vector3.zero;
    private bool following = false;
    private Vector3 startPos;

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {

        cam = GetComponent<Camera>();
        startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {

        UpdateCameraPosition();
	}

    /// <summary>
    /// Updates the dimension of the exercise
    /// </summary>
    /// <param name="minPositions">the left and bottom most positions within an exercise</param>
    /// <param name="maxPositions">the right and top most positions within an exercise</param>
    /// <param name="minZ">the minimal z position</param>
    /// <param name="relTo">the object the positions are relative to</param>
    public void UpdateExerciseDimensions(Vector2 minPositions, Vector2 maxPositions, float minZ, Transform relTo)
    {
        this.minPositions = minPositions;
        this.maxPositions = maxPositions;
        this.minZ = minZ;
        this.relTo = relTo;

       // Debug.Log(minPositions + " MinPos " + maxPositions + " MaxPos " + minZ);
    }

    public void ResetCamera()
    {
        minZ = 0;
        minPositions = Vector2.zero;
        maxPositions = Vector2.zero;
        following = false;
        transform.position = startPos;
    }

    private void UpdateCameraPosition()
    {
        // Find correct camera z position that all objects are visible
        if (cam.WorldToScreenPoint(new Vector3(minPositions.x, 0, minZ)).x > cam.pixelWidth * border && cam.WorldToScreenPoint(new Vector3(maxPositions.x, 0, minZ)).x < cam.pixelWidth * (1 - border) &&
            cam.WorldToScreenPoint(new Vector3(0, minPositions.y, minZ)).y > cam.pixelHeight * border && cam.WorldToScreenPoint(new Vector3(0, maxPositions.y, minZ)).y < cam.pixelHeight * (1 - border) &&
            transform.position.z <= minZ)
        {
            if (following == false)
            {
                transform.position =  new Vector3((minPositions.x + maxPositions.x) / 2.0f, (minPositions.y + maxPositions.y) / 2.0f, transform.position.z);
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                closePos = transform.position - relTo.position;
            }
        }

        // Everything being initialized: now only following the avatar
        else if (transform.position.z >= minZ || Mathf.Abs((minPositions.x + maxPositions.x) / 2.0f - transform.position.x) > 0.1f)
        {
            following = true;
        }

        if (following)
        {
            Vector3 camPosition = new Vector3((minPositions.x + maxPositions.x) / 2.0f, (minPositions.y + maxPositions.y) / 2.0f, relTo.position.z + closePos.z);
            transform.position = Vector3.Lerp(transform.position, camPosition, Time.deltaTime * 10.0f);
        }
    }
}
