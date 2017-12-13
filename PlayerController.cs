using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	public float length = 20;
	public float lowBorder = 45, upBorder = 0;
	const float ROTATION_SMOOTH_CF = 0.2f, SINK_SPEED = 9,SINKING_SMOOTH_CF = 1.1f;
	const float NOSE_MAX_ANGLE = 110;
	public float sinkSmooth = 0, rotationSmooth = 0;

	public float maxSpeed = 30, acceleration = 5, rotationSpeed = 5;
	float speed, gravity = 0, height;
	bool speedIsFixed = false;

	void Awake() {
		if (length == 0) length = 10;
	}

	void Update () {
		if (GameMaster.isPaused()) return;
		float t = Time.deltaTime;
		height = transform.position.y;

		//наклон
		float a = Vector3.Angle (transform.forward, Vector3.up);
		float sinkStep = SINK_SPEED * sinkSmooth * t;

		if (height <= GameMaster.GetWaterlevel()) {
			if (Input.GetKey("q")) { 
				sinkSmooth += SINKING_SMOOTH_CF * t; 
				if (sinkSmooth > 1) sinkSmooth = 1;
				if (Input.GetKey ("e")) {
					transform.Translate (Vector3.down * SINK_SPEED * t, Space.World); 
					Differenting (t);
				}
			}
			else {
				if (Input.GetKey ("e")) { 
					sinkSmooth -= SINKING_SMOOTH_CF * t; 
					if (sinkSmooth < -1) sinkSmooth = -1;
				}
				else {	if (transform.localRotation.x != 0) Differenting( t ); }
			}
			if (a > 179 && sinkSmooth > 0) sinkSmooth=0;
			else { if (a < 45 && sinkSmooth < 0) sinkSmooth = 0;}
			if (sinkSmooth != 0) transform.Rotate (Vector3.right * sinkStep, Space.Self);


		//Движение и поворот
		if (Input.GetKey( "w")) {if (speed + acceleration * t < maxSpeed) speed += acceleration * t;}
			else {
				if (Input.GetKey( "s") ) {if (speed - acceleration * t > -10) speed -= acceleration * t;}
				else {
					//if (!speedIsFixed) speed = Mathf.SmoothDamp(speed, 0, ref speed, speed/acceleration);
				}
			}	

		if (Input.GetKey ("d")) {if (rotationSmooth < 1) rotationSmooth += ROTATION_SMOOTH_CF * t;}
		else {
			if (Input.GetKey ("a")) { if (rotationSmooth > -1) rotationSmooth -= ROTATION_SMOOTH_CF * t;}
			else rotationSmooth = Mathf.SmoothDamp(rotationSmooth, 0,  ref rotationSmooth, t * 3);
		}
		if (rotationSmooth != 0) transform.Rotate (Vector3.up * rotationSmooth * t * rotationSpeed * speed / maxSpeed, Space.World);

			gravity = Mathf.SmoothDamp(gravity, 0, ref gravity, t);
			if (height > upBorder) transform.Translate (Vector3.down * 10 * t, Space.World);
			else { if (height < lowBorder) transform.Translate (Vector3.up * 5 * t, Space.World);}

			float trz = transform.localRotation.eulerAngles.z;
			if (trz!= 0) {
				if (trz > 180) transform.Rotate (Vector3.forward * rotationSpeed * t , Space.Self);
				else transform.Rotate (Vector3.back * rotationSpeed * t, Space.Self);
				if (trz < rotationSpeed * t || 360 - trz < rotationSpeed * t) {
					Vector3 nrot = transform.rotation.eulerAngles;
					nrot.z = 0;
					transform.rotation = Quaternion.Euler(nrot);
				}
			}

			if (Input.GetKeyDown (KeyCode.Space) && height < upBorder) {transform.Translate(Vector3.up * 3 *t , Space.World);}
		}
		else {
			gravity += 9.8f * t;
			transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation(Vector3.down), 10 * t);
			if (speed > 0) {speed -= 10 * t; if (speed < 0) speed = 0;}
		}
		if (speed != 0) transform.Translate(Vector3.forward * speed * t, Space.Self);
		if (gravity != 0) transform.Translate (Vector3.down * gravity * t);



	}

	void OnGUI () {
		GUILayout.Label(speed.ToString());
		if (speedIsFixed) GUILayout.Label ("fixed speed");
		GUILayout.Label(transform.localRotation.eulerAngles.z.ToString());
		GUILayout.Label(Vector3.Angle (transform.forward, Vector3.up).ToString());
	}

	void Differenting (float t) {
		Vector3 nrot = transform.rotation.eulerAngles;
		if (Mathf.Abs (nrot.x) <= t || Mathf.Abs(nrot.x - 360) <= t) {
			nrot.x = 0;
			transform.rotation = Quaternion.Euler(nrot);
			sinkSmooth = 0;
		}
		else {
			float difSpeed = SINK_SPEED * t /2;
			if (height < upBorder) difSpeed/=4;
			if (transform.localRotation.eulerAngles.x < 90) {sinkSmooth -= difSpeed; if (sinkSmooth < 0) sinkSmooth += difSpeed* 0.9f;}
			else {sinkSmooth += difSpeed; if (sinkSmooth >0) sinkSmooth-= difSpeed * 0.9f;}
			}
	}
}
