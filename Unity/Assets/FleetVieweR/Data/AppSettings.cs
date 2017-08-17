using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Don't use BinaryFormatter:
///     https://stackoverflow.com/a/40966346/252308
/// PlayerPrefs is sufficient for our purposes.
/// 
/// References:
///     https://docs.unity3d.com/ScriptReference/PlayerPrefs.html
///     https://stackoverflow.com/questions/40078490/saving-loading-data-in-unity/40097623
///     https://stackoverflow.com/questions/40965645/what-is-the-best-way-to-save-game-state
///     https://www.youtube.com/watch?v=LBs6qOgCDOY
///     https://www.youtube.com/watch?v=kQ5vh4JJFQI Not Android specific; horribly inefficient save to multiple files
///     https://creative.pluralsight.com/tutorial/608-Unity-Mobile-Game-Development-Saving-Data-and-Highscores
///     
/// </summary>
[Serializable]
public class AppSettings
{
    public const string DEFAULT_SYSTEM_NAME = "Star Citizen";
    public const string DEFAULT_MODEL_KEY = "Nox";

	private const string PREF_KEY = "AppSettings";

    public static void Reset()
    {
        PlayerPrefs.DeleteAll();
    }

    public static AppSettings Load()
    {
		string jsonData = PlayerPrefs.GetString(PREF_KEY, null);
        //Debug.LogError("jsonData == " + Utils.Quote(jsonData));

		AppSettings appSettings;

        if (string.IsNullOrEmpty(jsonData))
        {
            appSettings = new AppSettings();
        }
        else
        {
            appSettings = JsonUtility.FromJson<AppSettings>(jsonData);
        }

        //Debug.LogError("appSettings == " + appSettings);

        return appSettings;
	}

    public string SystemName;
    public List<ModelSettings> ModelSettings;

    private AppSettings()
    {
        SystemName = DEFAULT_SYSTEM_NAME;

        ModelSettings = new List<ModelSettings>();

        AddModel(DEFAULT_MODEL_KEY);
    }

    public void Save()
    {
        string jsonData = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(PREF_KEY, jsonData);
        PlayerPrefs.Save();
    }

    public void AddModel(ModelInfo modelInfo)
    {
        AddModel(modelInfo.Name, modelInfo.Name);
    }

	public void AddModel(string modelKey)
	{
		AddModel(modelKey, modelKey);
	}

	public void AddModel(string modelName, string modelKey)
    {
        AddModel(modelName, modelKey, Vector3.zero, Quaternion.identity);
    }

    public void AddModel(string modelName, string modelKey, Vector3 position, Quaternion rotation)
    {
        /*
        if (ModelSettings.ContainsKey(modelName))
        {
            int i = 1;
            while (false)
            {
                //modelKey
            }
        }
        */

        ModelSettings modelSettings = new ModelSettings(modelName, modelKey);
        modelSettings.Position = position;
        modelSettings.Rotation = rotation;

        ModelSettings.Add(modelSettings);
    }
}
