using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Constants
{
    public static readonly string APP_NAME = "Cubic";
    public static readonly string GAME_VERSION = "0.01";

    public static string TILEMAP_SAVE_FOLDER => Path.Combine(Application.dataPath + "/../", "Maps");

	public static string EDGE_TYPE = "edge";

	public static int NUM_COLLISIONS_TO_SAVE_ON_SERVER = 10;

	// input mapping strings
	public static string AXIS_HORIZONTAL = "Horizontal";
	public static string AXIS_VERTICAL   = "Vertical";
	public static string BUTTON_CHARGE   = "Charge";

	// PhotonPlayer properties keys
	public static int CHARACTER_NAME = 0;
	public static int LEVEL_NAME = 1;
}
