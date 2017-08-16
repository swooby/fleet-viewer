using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemInfo
{
	public const string FIELD_NAME = "Name";
	public const string FIELD_CONFIG_PATH = "Config Path";

	public string Name { get; private set; }
	public string ConfigPath { get; private set; }

	public SystemInfo(Dictionary<string, string> dictionary)
	{
		Name = dictionary[FIELD_NAME];
		ConfigPath = dictionary[FIELD_CONFIG_PATH];
	}
}
