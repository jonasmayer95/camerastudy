using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FullSerializer;

/// <summary>
/// Plain old data types that directly map to the JSON data
/// we get from the server.
/// </summary>
/// 


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
    public InseilJoint(double px, double py, double pz, double rx, double ry, double rz, double rw)
    {
        position = new InseilPosition();
        position.x = px;
        position.y = py;
        position.z = pz;

        rotation = new InseilRotation();
        rotation.x = rx;
        rotation.y = ry;
        rotation.z = rz;
        rotation.w = rw;
    }

    public InseilPosition position;
    public InseilRotation rotation;

    public override string ToString()
    {
        return string.Concat(position.ToString(), rotation.ToString());
    }
}

class InseilJointConverter : fsDirectConverter/*<InseilJoint>*/
{
    Dictionary<string, fsData> tmpDict = new Dictionary<string, fsData>(4);

    public override Type ModelType
    {
        get { return typeof(InseilJoint); }
    }

    public override object CreateInstance(fsData data, Type storageType)
    {
        return new InseilJoint();
    }

    //protected override fsResult DoSerialize(InseilJoint model, Dictionary<string, fsData> serialized)
    //{
    //    Dictionary<string, fsData> tmpDict = new Dictionary<string, fsData>(3);
    //    tmpDict["x"] = new fsData(model.position.x);
    //    tmpDict["y"] = new fsData(model.position.y);
    //    tmpDict["z"] = new fsData(model.position.z);
    //    serialized["p"] = new fsData(tmpDict);
    //    tmpDict.Clear();

    //    tmpDict["x"] = new fsData(model.rotation.x);
    //    tmpDict["y"] = new fsData(model.rotation.y);
    //    tmpDict["z"] = new fsData(model.rotation.z);
    //    tmpDict["w"] = new fsData(model.rotation.w);
    //    serialized["r"] = new fsData(tmpDict);

    //    return fsResult.Success;
    //}

    public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
    {
        throw new NotImplementedException();
    }

    public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
    {
        var result = fsResult.Success;

        fsData posData;
        if ((result += CheckKey(data, "p", out posData)).Failed) return result;
        if ((result += CheckType(posData, fsDataType.Object)).Failed) return result;

        fsData rotData;
        if ((result += CheckKey(data, "r", out rotData)).Failed) return result;
        if ((result += CheckType(rotData, fsDataType.Object)).Failed) return result;

        var dict = posData.AsDictionary;
        double px = dict["x"].AsDouble;
        double py = dict["y"].AsDouble;
        double pz = dict["z"].AsDouble;

        dict = rotData.AsDictionary;
        double rx = dict["x"].AsDouble;
        double ry = dict["y"].AsDouble;
        double rz = dict["z"].AsDouble;
        double rw = dict["w"].AsDouble;

        instance = new InseilJoint(px, py, pz, rx, ry, rz, rw);

        return result;
    }

    //protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref InseilJoint model)
    //{
    //    //get p and r, stuff them into a new InseilJoint
    //    var result = fsResult.Success;

    //    fsData posData;
    //    if ((result += CheckKey(data, "p", out posData)).Failed) return result;
    //    if ((result += CheckType(posData, fsDataType.Object)).Failed) return result;

    //    fsData rotData;
    //    if ((result += CheckKey(data, "r", out rotData)).Failed) return result;
    //    if ((result += CheckType(rotData, fsDataType.Object)).Failed) return result;

    //    var dict = posData.AsDictionary;
    //    model.position.x = dict["x"].AsDouble;
    //    model.position.y = dict["y"].AsDouble;
    //    model.position.z = dict["z"].AsDouble;

    //    dict = rotData.AsDictionary;
    //    model.rotation.x = dict["x"].AsDouble;
    //    model.rotation.y = dict["y"].AsDouble;
    //    model.rotation.z = dict["z"].AsDouble;
    //    model.rotation.w = dict["w"].AsDouble;

    //    return result;
    //}
}

//the problem seems to be the deserializer attempts to fill the dictionary with the same keys
//TODO: verify this and if that's the case, clear the dict before deserializing
//TODO: try this one for parsing/deserializing: http://wiki.unity3d.com/index.php/JSONObject
public class InseilMeasurement
{
    public Dictionary<string, InseilJoint> data;
    public UInt64 timestamp;
    private const int JointCount = 28;

    public InseilMeasurement()
    {
        this.data = new Dictionary<string, InseilJoint>(JointCount);
        this.timestamp = 0;
    }

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

    public InseilMessage()
    {
        measurement = new InseilMeasurement();
    }

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
