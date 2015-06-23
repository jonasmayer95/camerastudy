using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullSerializer;

/// <summary>
/// Plain old data types that directly map to the JSON data
/// we get from the server.
/// </summary>

public struct InseilPosition
{
    public double x;
    public double y;
    public double z;

    public override string ToString()
    {
        return string.Format("pos = x:{0}, y:{1}, z:{2}\n", x, y, z);
    }
}

public struct InseilRotation
{
    public double x;
    public double y;
    public double z;
    public double w;

    public override string ToString()
    {
        return string.Format("rot = x:{0}, y:{1}, z:{2}, w:{3}\n", x, y, z, w);
    }
}

[fsObject(Converter = typeof(InseilJointConverter))]
public struct InseilJoint
{
    public InseilPosition position;
    public InseilRotation rotation;

    public override string ToString()
    {
        return string.Concat(position.ToString(), rotation.ToString());
    }
}

class InseilJointConverter : fsDirectConverter<InseilJoint>
{
    public override object CreateInstance(fsData data, Type storageType)
    {
        return new InseilJoint();
    }

    protected override fsResult DoSerialize(InseilJoint model, Dictionary<string, fsData> serialized)
    {
        Dictionary<string, fsData> posDict = new Dictionary<string, fsData>(3);
        posDict["x"] = new fsData(model.position.x);
        posDict["y"] = new fsData(model.position.y);
        posDict["z"] = new fsData(model.position.z);
        serialized["p"] = new fsData(posDict);

        Dictionary<string, fsData> rotDict = new Dictionary<string, fsData>(4);
        rotDict["x"] = new fsData(model.rotation.x);
        rotDict["y"] = new fsData(model.rotation.y);
        rotDict["z"] = new fsData(model.rotation.z);
        rotDict["w"] = new fsData(model.rotation.w);
        serialized["r"] = new fsData(rotDict);

        return fsResult.Success;
    }

    protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref InseilJoint model)
    {
        //get p and r, stuff them into a new InseilJoint
        var result = fsResult.Success;

        fsData posData;
        if ((result += CheckKey(data, "p", out posData)).Failed) return result;
        if ((result += CheckType(posData, fsDataType.Object)).Failed) return result;

        fsData rotData;
        if ((result += CheckKey(data, "r", out rotData)).Failed) return result;
        if ((result += CheckType(rotData, fsDataType.Object)).Failed) return result;

        var dict = posData.AsDictionary;
        model.position.x = dict["x"].AsDouble;
        model.position.y = dict["y"].AsDouble;
        model.position.z = dict["z"].AsDouble;

        dict = rotData.AsDictionary;
        model.rotation.x = dict["x"].AsDouble;
        model.rotation.y = dict["y"].AsDouble;
        model.rotation.z = dict["z"].AsDouble;
        model.rotation.w = dict["w"].AsDouble;

        return result;
    }
}

public class InseilMeasurement
{
    //I used a dictionary because typing in all the joints is
    //not very flexible w.r.t. protocol changes, not to mention
    //it is way too much work for nothing in return.
    public Dictionary<string, InseilJoint> data;
    public UInt64 timestamp;

    public override string ToString()
    {
        //TODO: use http://stackoverflow.com/questions/3639094/most-efficient-dictionaryk-v-tostring-with-formatting
        //to get a debug print of the dictionary
        return data.ToString() + "\n" + timestamp.ToString() + "\n";
    }
}
public class InseilMessage
{
    public UInt32 version;
    public string id;
    public InseilMeasurement measurement;
    public UInt64 sendtime;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Concat(version.ToString(), "\n"));
        sb.Append(string.Concat(id, "\n"));
        sb.Append(string.Concat(measurement.ToString(), "\n"));
        sb.Append(string.Concat(sendtime, "\n"));
        return sb.ToString();
    }
}

