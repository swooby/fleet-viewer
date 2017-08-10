using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CSVReader
{
    private CSVReader()
    {
    }

    /// <summary>
    /// Return null to ignore this entry
    /// </summary>
    public delegate T OnKeyValue<T>(Dictionary<string, string> dictionary);

	private static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
	private static readonly string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    private static readonly char[] TRIM_CHARS = { '\"' };

    public static List<T> Read<T>(string file, OnKeyValue<T> callback) where T : class
	{
		//Debug.Log("CSVReader.Read(\"" + file + "\", ...");

		var list = new List<T>();

		TextAsset data = Resources.Load(file) as TextAsset;
        //Debug.Log("CSVReader.Read: data == " + data);

		if (data == null) return list;

		var lines = Regex.Split(data.text, LINE_SPLIT_RE);

		if (lines.Length <= 1) return list;

		var header = Regex.Split(lines[0], SPLIT_RE);
		//Debug.Log("CSVReader.Read: header == " + header);

		for (var i = 1; i < lines.Length; i++)
		{
			var values = Regex.Split(lines[i], SPLIT_RE);
			//Debug.Log("CSVReader.Read: values == " + values);

			if (values.Length == 0 || values[0] == "") continue;

			var dictionary = new Dictionary<string, string>();

			for (var j = 0; j < header.Length && j < values.Length; j++)
			{
				string stringValue = values[j].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
				//Debug.Log("CSVReader.Read: stringValue == " + stringValue);

				dictionary[header[j]] = stringValue;
			}
			//Debug.Log("CSVReader.Read: dictionary == " + dictionary);

			T genericValue = callback(dictionary);
			//Debug.Log("CSVReader.Read: genericValue == " + genericValue);

			if (genericValue != null)
            {
                list.Add(genericValue);
            }
		}

		return list;
	}
}