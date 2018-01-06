using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class GameMaster {
	static bool paused;
	public static float WATERLEVEL = 0, LIGHT_DEPTH_LIMIT = -2000;
	public const float MASS_CONST = 10000;
	public static float seaStrength = 20;
	public static Camera cam;
	public static PoolMaster pool;
	public static ScenarioManager scenarist;
	public static LevelDesigner designer;
	static int guiPiece = 16;


	public static bool isPaused() {
		return paused;
	}

	public static int GetGUIPiece () {return guiPiece;}
	public static void SetGUIPiece (int k) {if (k<0) k = 16; guiPiece = k;}
}
