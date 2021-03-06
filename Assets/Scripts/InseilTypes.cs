﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/// <summary>
/// Plain old data types that directly map to the JSON data
/// we get from the server.
/// </summary>
/// 


public struct InseilPosition
{
    public InseilPosition(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

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
    public InseilRotation(double x, double y, double z, double w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public double x;
    public double y;
    public double z;
    public double w;

    public override string ToString()
    {
        return string.Format("rot = x:{0}, y:{1}, z:{2}, w:{3}\n", x, y, z, w);
    }
}


public struct InseilJoint
{
    public InseilJoint(double px, double py, double pz, double rx, double ry, double rz, double rw)
    {
        p = new InseilPosition();
        p.x = px;
        p.y = py;
        p.z = pz;

        r = new InseilRotation();
        r.x = rx;
        r.y = ry;
        r.z = rz;
        r.w = rw;
    }

    public InseilPosition p;
    public InseilRotation r;

    public override string ToString()
    {
        return string.Concat(p.ToString(), r.ToString());
    }
}



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


    //actually I only need this for measurement...
    public byte[] ToByteArray()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(data.Count);
                foreach (var joint in data)
                {
                    writer.Write(joint.Key);
                    writer.Write(joint.Value.p.x);
                    writer.Write(joint.Value.p.y);
                    writer.Write(joint.Value.p.z);
                    writer.Write(joint.Value.r.x);
                    writer.Write(joint.Value.r.y);
                    writer.Write(joint.Value.r.z);
                    writer.Write(joint.Value.r.w);
                }
            }

            return stream.ToArray();
        }
    }

    public void FromByteArray(byte[] data)
    {
        //InseilMeasurement retVal = new InseilMeasurement();
        this.data.Clear();

        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int count = reader.ReadInt32();
                string key = "";
                double px, py, pz, rx, ry, rz, rw;

                for (int i = 0; i < count; ++i)
                {
                    key = reader.ReadString();
                    px = reader.ReadDouble();
                    py = reader.ReadDouble();
                    pz = reader.ReadDouble();
                    rx = reader.ReadDouble();
                    ry = reader.ReadDouble();
                    rz = reader.ReadDouble();
                    rw = reader.ReadDouble();

                    var joint = new InseilJoint(px, py, pz, rx, ry, rz, rw);
                    this.data.Add(key, joint);
                }
            }
        }
        //return retVal;
    }
}
public class InseilMessage
{
    public UInt32 protocol_version;
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
        sb.Append(string.Concat(protocol_version.ToString(), "\n"));
        sb.Append(string.Concat(id, "\n"));
        sb.Append(string.Concat(measurement.ToString(), "\n"));
        sb.Append(string.Concat(sendtime, "\n"));
        return sb.ToString();
    }
}
