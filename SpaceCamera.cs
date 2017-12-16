using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceCamera : MonoBehaviour {

	public float koefficient = 60;

	void Update () {
		if (koefficient != 0) 	transform.position = Camera.main.transform.position / koefficient;
		transform.rotation = Camera.main.transform.rotation;
	}
}
