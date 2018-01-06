using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour {

	public float mass = 1, extent_y = 1 ;
	float massEffect = 0.0001f;
	float force = 0;

	// Update is called once per frame

	void Awake() {
		massEffect = mass / GameMaster.MASS_CONST;
	}

	void Update () {
		if (GameMaster.isPaused()) return;
		RaycastHit rh;
		var layerMask = 1<<4;
		if (Physics.Raycast(transform.position + Vector3.up * 1000, Vector3.down, out rh, Mathf.Infinity, layerMask)) {
			float waterlevel = rh.point.y;
			Vector3 pos = transform.position;

			if (pos.y > waterlevel) {
					force -= 9.8f * Time.deltaTime;
			}
			else {
				float pressure = (waterlevel - pos.y) /2.0f;
					if (force < pressure ) force += pressure * Time.deltaTime * Mathf.Pow(1 - massEffect,2);
					else {force -= 9.8f * Time.deltaTime; if (force < pressure) force = pressure + (1 - massEffect) * pressure;}
				}
			float realForce = force;

			if (pos.y < waterlevel + extent_y) {
				if (force > 0 ) {
					realForce = force * Mathf.Pow(1 - massEffect, 4);
				}
				else {
					realForce = force * Mathf.Pow(1 + massEffect, 4);
				}
			}

			pos.y += realForce * Time.deltaTime;
			transform.position = pos;
		}
	}
}
