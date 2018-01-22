using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour {

	const float NATURAL_GRAVITY = 9.8f, FORCE_CONSTANCE_TIMER = 0.04f;
	public float mass = 1, volume = 1, draft= 1 ;
	float yForce = 0, realForce = 0,forceTimer = 0, flyTime = 0, forceChangingSpeed = 10;

	// Update is called once per frame


	void Update () {
		if (GameMaster.isPaused()) return;
		RaycastHit rh;
		var layerMask = 1<<4;
		if (Physics.Raycast(transform.position + Vector3.up * 1000, Vector3.down, out rh, Mathf.Infinity, layerMask)) {
			float waterlevel = rh.point.y;


			float pressure = (waterlevel - transform.position.y ) /10f + 1;
			if (transform.position.y > waterlevel + draft) flyTime += Time.deltaTime;
			else flyTime = 0;


			if (forceTimer > 0) forceTimer-= Time.deltaTime;
			else {
				float prevForce = yForce;
				if (transform.position.y < waterlevel) {
					float k = ( 1 - transform.position.y - (waterlevel - draft)) / (2 * draft);
					if (k < 0) k *= (-1);
					float surface_cf = 1;
					if (waterlevel - transform.position.y < draft) surface_cf= GameMaster.seaStrength;
					yForce = pressure * surface_cf * NATURAL_GRAVITY * volume  - mass  * NATURAL_GRAVITY;
				}
				else {
					yForce = -mass  * NATURAL_GRAVITY * (1 +flyTime);
					if (transform.position.y - waterlevel >draft) yForce /= GameMaster.seaStrength;
				}

				yForce /= mass;
				if (prevForce != realForce)	{
					forceTimer = FORCE_CONSTANCE_TIMER;
				}
			}
			if (realForce< yForce ) {
				if (waterlevel - transform.position.y > draft ) realForce += (forceChangingSpeed+ pressure) * Time.deltaTime; 
				if (realForce > yForce) realForce = yForce;
			}
			else if (realForce > yForce) {
				if (transform.position.y  - waterlevel > draft) {
					realForce -= (forceChangingSpeed+ NATURAL_GRAVITY *flyTime*mass * 0.5f) * Time.deltaTime; 
				}
				else realForce -= forceChangingSpeed * Time.deltaTime; 
				if (realForce < yForce) realForce = yForce;}

			transform.Translate (Vector3.up * realForce * Time.deltaTime,Space.World);
		}
	}
}
