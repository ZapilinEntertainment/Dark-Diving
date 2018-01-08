using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class GameMaster {
	static bool paused;
	public static float WATERLEVEL = 0, LIGHT_DEPTH_LIMIT = -2000, SURFACE_EFFECT_DEPTH = 50;
	public static float seaStrength = 1;
	public static Camera cam;
	public static LevelDesigner designer;
	static int guiPiece = 16;
	public static Vector2 cursorPosition;


	public static bool isPaused() {
		return paused;
	}

	public static int GetGUIPiece () {return guiPiece;}
	public static void SetGUIPiece (int k) {if (k<0) k = 16; guiPiece = k;}
}
