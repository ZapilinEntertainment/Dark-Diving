using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class GameMaster {
	static bool paused;
	public static float WATERLEVEL = 0, LIGHT_DEPTH_LIMIT = -2000;
	public static Camera cam;
	public static PoolMaster pool;
	public static ScenarioManager scenarist;
	public static LevelDesigner designer;


	public static bool isPaused() {
		return paused;
	}
}
