using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cameradron : MonoBehaviour {

	public float speed = 5, rotation_speed = 15, energyLimit = 100, energy;
	bool activated = true;
	// Use this for initialization
	void Start () {
		Activate();
	}
	
	// Update is called once per frame
	void Update () {
		if (GameMaster.isPaused() || !activated) return;
		float t = Time.deltaTime;
		bool motion = false;

		if (Input.GetKey("w")) {transform.Translate(Vector3.forward * speed * t, Space.Self); motion = true;}
		if (Input.GetKey("s")) {transform.Translate(Vector3.back * speed * t, Space.Self); motion = true;}
		if (Input.GetKey("a")) {transform.Translate(Vector3.left * speed * t, Space.Self); motion = true;}
		if (Input.GetKey("d")) {transform.Translate(Vector3.right * speed * t, Space.Self); motion = true;}
		if (Input.GetKey("q")) {transform.Rotate(Vector3.forward * rotation_speed * t, Space.Self); motion = true;}
		if (Input.GetKey("e")) {transform.Rotate(Vector3.back * rotation_speed * t, Space.Self); motion = true;}
		if (Input.GetKey(KeyCode.Space)) {transform.Translate(Vector3.up * speed * t, Space.Self); motion = true;}
		if (Input.GetKey(KeyCode.LeftControl)) {transform.Translate(Vector3.down * speed * t, Space.Self); motion = true;}
		float delta = Input.GetAxis("Mouse X");
		if (delta != 0 ) {transform.Rotate(0, delta * rotation_speed * t, 0, Space.World); motion = true;}
		delta = Input.GetAxis("Mouse Y");
		if (delta != 0) {transform.Rotate (Vector3.left* delta *rotation_speed * t, Space.Self); motion = true;}

		if (motion) energy -= t; else energy -= t /10.0f;
		if (energy < 0) {Deactivate(); return;}
	}

	public void Deactivate() {
		activated = false;
		Cursor.visible = true;
	}

	public void Activate() {
		activated = true;
		energy = energyLimit;
		Cursor.visible = false;
	}

	void OnGUI() {
		GUILayout.Label(energy.ToString());
	}
}
