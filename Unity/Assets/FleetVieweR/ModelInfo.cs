using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelInfo
{
    public const string FIELD_NAME = "Name";
    public const string FIELD_LAST_CHECKED = "Last Checked";
    public const string FIELD_LENGTH = "Length";
    public const string FIELD_BEAM = "Beam";
    public const string FIELD_HEIGHT = "Height";
    public const string FIELD_STORE_URL = "Store URL";
    public const string FIELD_HOLOVIEW_URL = "Holoview URL";
    public const string FIELD_FLEETVIEWER_PATH = "FleetVieweR";

    public string Name { get; private set; }
    public DateTime LastChecked { get; private set; }
    public float LengthMeters { get; private set; }
    public float BeamMeters { get; private set; }
    public float HeightMeters { get; private set; }
    public Uri StoreUrl { get; private set; }
    public Uri HoloviewCtmUrl { get; private set; }
    public string FleetViewerPath { get; private set; }

    public ModelInfo(Dictionary<string, string> dictionary)
    {
        Name = dictionary[FIELD_NAME];

        try
        {
            LastChecked = DateTime.Parse(dictionary[FIELD_LAST_CHECKED]);
        }
        catch (FormatException)
        {
            LastChecked = DateTime.MinValue;
        }

        try
        {
            LengthMeters = float.Parse(dictionary[FIELD_LENGTH]);
        }
        catch (FormatException)
        {
            LengthMeters = float.NaN;
        }

        try
        {
            BeamMeters = float.Parse(dictionary[FIELD_BEAM]);
        }
        catch (FormatException)
        {
            BeamMeters = float.NaN;
        }

        try
        {
            HeightMeters = float.Parse(dictionary[FIELD_HEIGHT]);
        }
        catch (FormatException)
        {
            HeightMeters = float.NaN;
        }

        try
        {
            StoreUrl = new Uri(dictionary[FIELD_STORE_URL]);
        }
        catch (FormatException)
        {
            StoreUrl = null;
        }

        try
        {
            HoloviewCtmUrl = new Uri(dictionary[FIELD_HOLOVIEW_URL]);
        }
        catch (FormatException)
        {
            HoloviewCtmUrl = null;
        }

        FleetViewerPath = dictionary[FIELD_FLEETVIEWER_PATH];
    }

    /*
    public override string ToString()
    {
        return Name;
    }
    */
}
