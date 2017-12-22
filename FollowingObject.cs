using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingObject : MonoBehaviour {

	public Transform obj;
	void Update () {
		transform.position = new Vector3(obj.position.x, transform.position.y, obj.position.z);
	}
}
