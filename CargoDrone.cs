using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoDrone : MonoBehaviour {
	public const float CONTACT_DISTANCE = 1.0f, UNLOAD_SPEED = 2;

	public float speed = 15;
	public float maximumWorkingDepth = - 2000;
	public PlayerController depot;
	Item content;
	public ResourcesBox target;
	float timer = 0;

	void Update () {
		if (GameMaster.isPaused()) return;
		if (timer > 0) {timer -= Time.deltaTime; return;}
		if (content != null) {
			if (Vector3.Distance(transform.position, depot.transform.position) < CONTACT_DISTANCE) {
				if (depot.LoadItemFromDrone(content) == true) {
					content = null; 
					if (target.lootUntilExhausted != true || target.BitmasksConjuction() == 0) { // закончить транспортировку
						target = null;
						timer = 0;
						gameObject.SetActive(false);
					}
					else timer = UNLOAD_SPEED;
				}
			}
			else {//плыть к носителю
				transform.rotation  = Quaternion.LookRotation(depot.transform.position - transform.position,Vector3.up);
				float dist = Vector3.Distance(transform.position, depot.transform.position);
				if (dist < speed * Time.deltaTime) transform.Translate(Vector3.forward * dist);
				else transform.Translate(Vector3.forward * speed * Time.deltaTime);
			}
		}
		else { // плыть к "руднику"
			if (Vector3.Distance(transform.position, target.transform.position) < CONTACT_DISTANCE) {//грузим на борт
				content = target.Extract();
				timer = target.extractionSpeed;
			}
			else {
				transform.rotation  = Quaternion.LookRotation(target.transform.position - transform.position,Vector3.up);
				float dist = Vector3.Distance(transform.position, target.transform.position);
				if (dist < speed * Time.deltaTime) transform.Translate(Vector3.forward * dist);
				else transform.Translate(Vector3.forward * speed * Time.deltaTime);
			}
		}
	}
}
