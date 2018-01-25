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
	public MeshRenderer myRenderer;
	public float timer = 0;
	public bool changeDestinationAfterHaul =false;

	void Awake () {
		if (myRenderer == null) myRenderer = gameObject.GetComponent<MeshRenderer>();
	}

	void Update () {
		if (GameMaster.isPaused() || depot == null) return;
		if (timer > 0) {timer -= Time.deltaTime; myRenderer.enabled = true; return;}
		if (content != null) {
			if (Vector3.Distance(transform.position, depot.transform.position) < CONTACT_DISTANCE) {
				if (depot.LoadItemFromDrone(content) == true) {
					content = null; 
					timer = UNLOAD_SPEED;
					myRenderer.enabled = false;
				}
			}
			else {//плыть к носителю
				transform.rotation  = Quaternion.LookRotation(depot.transform.position - transform.position,Vector3.up);
				float dist = Vector3.Distance(transform.position, depot.transform.position);
				if (dist < speed * Time.deltaTime) transform.Translate(Vector3.forward * dist);
				else transform.Translate(Vector3.forward * speed * Time.deltaTime);
			}
		}
		else { //пустой
			if (target != null) {
			// плыть к "руднику"
			if (Vector3.Distance(transform.position, target.transform.position) < CONTACT_DISTANCE) {//грузим на борт
				content = target.Extract();
				if (content != null) {
						timer = target.extractionSpeed;
						myRenderer.enabled = false;
					}
				else target = null;
				if (changeDestinationAfterHaul) 	{
						target = depot.GetLootPoint();
						changeDestinationAfterHaul = false;
					}
			}
			else {
				transform.rotation  = Quaternion.LookRotation(target.transform.position - transform.position,Vector3.up);
				float dist = Vector3.Distance(transform.position, target.transform.position);
				if (dist < speed * Time.deltaTime) transform.Translate(Vector3.forward * dist);
				else transform.Translate(Vector3.forward * speed * Time.deltaTime);
			}
		}
			else { // нет цели
				target = depot.GetLootPoint();
				if (target == null)	{
					if (Vector3.Distance(transform.position, depot.transform.position) < CONTACT_DISTANCE) {
						timer = 0;
						gameObject.SetActive(false);
					}
					else {//плыть к носителю
						transform.rotation  = Quaternion.LookRotation(depot.transform.position - transform.position,Vector3.up);
						float dist = Vector3.Distance(transform.position, depot.transform.position);
						if (dist < speed * Time.deltaTime) transform.Translate(Vector3.forward * dist);
						else transform.Translate(Vector3.forward * speed * Time.deltaTime);
					}
				}
			}
		}
	}
}
