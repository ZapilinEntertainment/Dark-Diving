using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour {
	float k = 16;
	int sw = 512,sh = 512;
	ResourcesBox chosenResBox; List<Item> resBoxContentTypes;
	Vector2 mousePos;
	static UI playerUI;
	GUISkin submarineGUISkin;

	void Awake () {
		if (playerUI != null) Destroy(playerUI);
		playerUI = this;

		submarineGUISkin = Resources.Load<GUISkin>("submarineGUIskin");
	}

	void Update () {
		sw = Screen.width; sh = Screen.height;
		k = sh / 12;
		mousePos = Input.mousePosition;
		mousePos.y = sh - mousePos.y;

		if (GameMaster.isPaused()) return;
		if (Input.GetMouseButtonDown(0)) {
			Ray r = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
			RaycastHit rh;
			var waterLayerMask = 1<<4 ; //water layer 
			waterLayerMask = ~waterLayerMask;
			if (Physics.Raycast(r, out rh, PlayerController.player.shortRangeScanner,waterLayerMask)) {
				GameObject target = rh.collider.transform.root.gameObject;
				chosenResBox = target.GetComponent<ResourcesBox>();
			} 
		}
	}

	void OnGUI () {
		if (GameMaster.isPaused()) return;

		GUI.skin = submarineGUISkin;
		if (chosenResBox != null) {
			Vector2 rbox_pos = Camera.main.WorldToScreenPoint(chosenResBox.transform.position);
			if ( !chosenResBox.scanned ) {
				Rect r = new Rect(rbox_pos.x - 2*k, sh - rbox_pos.y - k, 4*k, 2*k);
				GUI.Box(r, GUIContent.none);
				if (GUI.Button(r, "Scan")) {
					if (PlayerController.player.ConsumeEnergy(PlayerController.player.shortScannerCost)) {
						chosenResBox.scanned = true;
						resBoxContentTypes = chosenResBox.GetAvailableResourcesList(false);
						print (resBoxContentTypes.Count);
					}
				}				
			}
			else { // уже просканирован
				int n = resBoxContentTypes.Count;
				if (n > 0) {
					
					Rect box_r;
					if (n >= 4) box_r= new Rect(rbox_pos.x - n / 2.0f * k, sh - rbox_pos.y - k, n * k, 1.5f*k);
					else box_r= new Rect(rbox_pos.x -2 * k, sh - rbox_pos.y - k, 4 * k, 1.5f*k);
					GUI.Box(box_r, GUIContent.none);
					Rect info_r = new Rect(box_r.x, box_r.y, k,k);
					for (int i = 0; i < n; i++) {
						GUI.DrawTexture(info_r,resBoxContentTypes[i].item_tx, ScaleMode.StretchToFill);
						info_r.x += k;
					}
					if (GUI.Button(new Rect(box_r.x + box_r.width - 4 *k, box_r.y + k, 2 *k, k/2), "Send drone")) {}
					if (GUI.Button(new Rect(box_r.x + box_r.width - 2 *k, box_r.y + k, 2*k, k/2), "Loot all")) {}
				}
			}
		}
	}
}
