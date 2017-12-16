using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo : MonoBehaviour {
	public float speed = 10, damage = 10, explosionRange = 10;
	bool activated = true;


	void Update () {
		if (GameMaster.isPaused()) return;
		transform.Translate (Vector3.forward * speed * Time.deltaTime, Space.Self);
	}

	void OnCollisionEnter (Collision c) {Explosion();}

	void Explosion() {
		Collider[] cols = Physics.OverlapSphere(transform.position, explosionRange);
		foreach (Collider col in cols) {
			Vector3 pos = col.ClosestPoint(transform.position);
			float distance_cf = 1 - Vector3.Distance(transform.position, pos) / explosionRange;
			col.transform.root.SendMessage ("ApplyDamage", new Vector4 (pos.x, pos.y, pos.z, damage * distance_cf),SendMessageOptions.DontRequireReceiver);
		}
		Deactivate();
	}

	void Deactivate() {
		activated = false;
		gameObject.SetActive(false);
	}
}
