using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sea : MonoBehaviour {


	void Update () {
		Vector3 pos = Camera.main.transform.position;
		transform.position = new Vector3(pos.x, GameMaster.WATERLEVEL,pos.z);
	}
}
