using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Plain old data types that directly map to the JSON data
/// we get from the server.
/// </summary>

struct InseilPosition
{
    public double x;
    public double y;
    public double z;
}

struct InseilRotation
{
    public double x;
    public double y;
    public double z;
    public double w;
}

struct InseilJoint
{
    public InseilPosition position;
    public InseilRotation rotation;
}

class InseilMeasurement
{
    //I used a dictionary because typing in all the joints is
    //not very flexible w.r.t. protocol changes, not to mention
    //it is way too much work for nothing in return.
    public Dictionary<string, InseilJoint> data;
    public UInt64 timestamp;

}
class InseilMessage
{
    public UInt32 version;
    public string id;
    public InseilMeasurement measurement;
    public UInt64 sendtime;
}

