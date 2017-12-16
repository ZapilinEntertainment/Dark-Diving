using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMaster : MonoBehaviour {
	public ParticleSystem watersplash2_pref, dustsplash2_pref;
	ParticleSystem watersplash2, dustsplash2;
	const int WATERSPLASH2_PARTICLES_COUNT = 70, DUSTSPLASH2_PARTICLES_COUNT = 70;
	void Awake () {
		GameMaster.pool = this;
		watersplash2 = Instantiate (watersplash2_pref);
		dustsplash2 = Instantiate (dustsplash2_pref);
	}

	public void Watersplash2At (Vector3 pos) {
		watersplash2.transform.position = pos;
		watersplash2.Emit (WATERSPLASH2_PARTICLES_COUNT);
	}
	public void Dustsplash2At (Vector3 pos) {
		dustsplash2.transform.position = pos;
		dustsplash2.Emit (DUSTSPLASH2_PARTICLES_COUNT);
	}
}
