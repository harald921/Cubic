using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Constants
{
    public static readonly string APP_NAME = "Cubic";

    public static string TILEMAP_SAVE_FOLDER => Path.Combine(Application.dataPath + "/../", "Maps");
}
