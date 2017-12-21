using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour {
	GameObject player;
	Vector3 playerPos;
	Hex currentLocation;
	float hexUpdateTime = 1.5f, t = 0;

	void Awake () {
		GameMaster.scenarist = this;
	}
		

	void Update () {
		playerPos = player.transform.position;
		if (t == 0) {
			RaycastHit rh;
			int layerMask = 1 << 9;
			if (Physics.Raycast(playerPos, Vector3.down, out rh, Mathf.Infinity, layerMask)) {
				int x = 0, y =0;
				bool a = int.TryParse(rh.collider.transform.root.name.Substring(0,2), out x);
				bool b = int.TryParse(rh.collider.transform.root.name.Substring(2,2), out y);
				if (a && b) currentLocation = GameMaster.designer.GetHex(x,y);
				else currentLocation = null;
			}
			else currentLocation = null;
			t = hexUpdateTime;
		}
		else {
			t -= Time.deltaTime;
			if (t < 0) t = 0;
		}
	}

	public void SetPlayer (GameObject g) {player = g;}
	public Vector3 GetPlayerPosition() {return playerPos;}
	public Vector3 GetPlayerRotationEuler() {return player.transform.rotation.eulerAngles;}
	public Quaternion GetPlayerRotation() {return player.transform.rotation;}
	public Hex GetCurrentHex() {return currentLocation;}
	public Vector3 GetPlayerForward() {return player.transform.forward;}
}
