using System;
using UnityEngine;

[Serializable]
public class ModelSettings
{
    [Serializable]
    public struct Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3Serializable(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }

        public Vector3 GetValue()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public struct QuaternionSerializable
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionSerializable(Quaternion value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }

        public Quaternion GetValue()
		{
            return new Quaternion(x, y, z, w);
		}
	}

    public string Name;

    public string Key;

	private Vector3Serializable position;
	public Vector3 Position
    {
        get
        {
            return position.GetValue();

        }
        set
        {
            position = new Vector3Serializable(value);
        }
    }

	public QuaternionSerializable rotation;
	public Quaternion Rotation
    {
        get
        {
            return rotation.GetValue();
        }
        set
        {
            rotation = new QuaternionSerializable(value);
        }
    }

    public ModelSettings(string name, string key)
    {
        Name = name;
        Key = key;
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
    }
}
