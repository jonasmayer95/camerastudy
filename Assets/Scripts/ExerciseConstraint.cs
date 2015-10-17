using UnityEngine;
using System.Collections;

public class ExerciseConstraint{
    
    public Vector3 position;
    public string joint;
    public string relativeBone;
    public string type;
    public float tolerance;
    
    public ExerciseConstraint(double x, double y, double z, string joint, string relativeBone, string type, double tolerance)
    {
        this.position = new Vector3((float)x, (float)y, (float)z);
        this.joint = joint;
        this.relativeBone = relativeBone;
        this.type = type;
        this.tolerance = (float)tolerance;
    }

    public override string ToString()
    {
        return position.ToString() + " " + joint + " " + relativeBone + " " + type + " " + tolerance.ToString() + "\n";
    }
}
