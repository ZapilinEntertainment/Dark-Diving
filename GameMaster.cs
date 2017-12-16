using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class GameMaster {
	static bool paused;
	static float WATERLEVEL = 0;
	public static Camera cam;
	public static PoolMaster pool;

	public static bool isPaused() {
		return paused;
	}

	public static float GetWaterlevel () {
		return WATERLEVEL;
	}
}
