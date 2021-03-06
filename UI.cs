﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour {
	float k = 16;
	int sw = 512,sh = 512;
	ResourcesBox chosenResBox; List<Item> resBoxContentTypes;
	Vector2 mousePos;
	public static UI playerUI;
	GUISkin submarineGUISkin;
	bool updateResBoxData = false;
	public Transform UI_camera;

	GameObject[] mapCells;
	bool mapIsActive = false;
	GameObject shipMarker;
	const int UI_RENDER_LAYER = 5;
	float distanceCoefficient = 0.1f;

	void Awake () {
		if (playerUI != null) Destroy(playerUI);
		playerUI = this;

		submarineGUISkin = Resources.Load<GUISkin>("submarineGUIskin");
	}

	void Update () {
		if (GameMaster.isPaused()) return;

		sw = Screen.width; sh = Screen.height;
		k = sh / 24; GameMaster.SetGUIPiece(k);
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
				if (chosenResBox != null) updateResBoxData = true;
			} 
		}

		if (Input.GetKeyDown("m")) {
			if (mapIsActive) {
				foreach (GameObject g in mapCells) g.SetActive(false);
				shipMarker.SetActive(false);
				mapIsActive = false;
			}
			else {
				if (mapCells == null) {
					GameMaster.designer.CreateMap(ref mapCells, UI_RENDER_LAYER, out distanceCoefficient);
					shipMarker = Instantiate(Resources.Load<GameObject>("shipMarker_pref")) as GameObject;
					shipMarker.transform.parent = UI_camera;
					//shipMarker.transform.localScale = Vector3.one * radius;
					shipMarker.transform.localRotation = Quaternion.Euler(0,0,PlayerController.player.transform.rotation.eulerAngles.y * (-1));
					shipMarker.layer = UI_RENDER_LAYER;
					shipMarker.SetActive(false);
				}
				foreach (GameObject g in mapCells) g.SetActive(true);
				shipMarker.SetActive(true);
				mapIsActive = true;
			}
		}
		if (mapIsActive) {
			Vector3 delta = ScenarioManager.scenarist.GetPlayerPosition() - GameMaster.designer.GetHex(0,0).GetWorldPosition(); delta.y = delta.z; delta.z = -0.005f;
			shipMarker.transform.localPosition = mapCells[0].transform.localPosition + delta * distanceCoefficient;
			shipMarker.transform.localRotation = Quaternion.Euler(0,0,PlayerController.player.transform.eulerAngles.y * (-1)) ;		
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
					}
				}				
			}
			else { // уже просканирован
				if (updateResBoxData) {
					resBoxContentTypes = chosenResBox.GetAvailableResourcesList(false);
					updateResBoxData = false;
				}
				int n = resBoxContentTypes.Count;
				if (n > 0) {
					
					Rect box_r;
					if (n >= 4) box_r= new Rect(rbox_pos.x - n / 2.0f * k, sh - rbox_pos.y - k, n * k, 1.5f*k);
					else box_r= new Rect(rbox_pos.x -3 * k, sh - rbox_pos.y - k, 6 * k, 1.5f*k);
					GUI.Box(box_r, GUIContent.none);
					Rect info_r = new Rect(box_r.x, box_r.y, k,k);
					if (!chosenResBox.explored) {
						for (int i = 0; i < n; i++) {
							bool alreadyChosen = false;
							if ((chosenResBox.extractionBitmask & ((int)Mathf.Pow(2,resBoxContentTypes[i].itemId))) != 0) {GUI.DrawTexture(info_r,PoolMaster.mainPool.choosingFrame_tx, ScaleMode.StretchToFill);alreadyChosen = true;}
							if (GUI.Button(info_r,resBoxContentTypes[i].item_tx)) {
								if (alreadyChosen) chosenResBox.extractionBitmask -= (int)Mathf.Pow(2,resBoxContentTypes[i].itemId);
								else chosenResBox.extractionBitmask += (int)Mathf.Pow(2,resBoxContentTypes[i].itemId);
								}
							info_r.x += k;
						}
					}
					else {
						for (int i = 0; i < n; i++) {
							bool alreadyChosen = false;
							int iid = resBoxContentTypes[i].itemId;
							if ((chosenResBox.extractionBitmask & ((int)Mathf.Pow(2,iid))) != 0) {GUI.DrawTexture(info_r,PoolMaster.mainPool.choosingFrame_tx, ScaleMode.StretchToFill);alreadyChosen = true;}
							if (GUI.Button(info_r,resBoxContentTypes[i].item_tx)) {
								if (alreadyChosen) chosenResBox.extractionBitmask -= (int)Mathf.Pow(2,iid);
								else chosenResBox.extractionBitmask += (int)Mathf.Pow(2,iid);
							}
							GUI.Label(new Rect(info_r.x, info_r.y, info_r.width/2, info_r.height/2), chosenResBox.counts[iid].ToString());
							info_r.x += k;
						}
					}
					if (GUI.Button(new Rect(box_r.x + box_r.width - 6 *k, box_r.y + k, 2 *k, k/2), "Send drone")) {
						chosenResBox.lootUntilExhausted = false;
						PlayerController.player.SendDroneToResBox(chosenResBox);
					}
					if (GUI.Button(new Rect(box_r.x + box_r.width - 4 *k, box_r.y + k, 2*k, k/2), "Loot")) {
						PlayerController.player.AddLootPoint(chosenResBox);
						chosenResBox.lootUntilExhausted = true;
					}
					if (GUI.Button(new Rect(box_r.x + box_r.width - 2 *k, box_r.y + k, 2*k, k/2), "Loot all")) {
						chosenResBox.extractionBitmask = chosenResBox.GetContentBitmask();
						PlayerController.player.AddLootPoint(chosenResBox);
						chosenResBox.lootUntilExhausted = true;
					}
					if (GUI.Button(new Rect(box_r.x + box_r.width - 4 *k, box_r.y + 1.5f * k, 2*k, k/2), "Stop")) {
						PlayerController.player.RemoveLootPoint(chosenResBox);
					}
					if (GUI.Button(new Rect(box_r.x + box_r.width, box_r.y, k/2, k/2), "+")) {
						if (chosenResBox.workingDrones < PlayerController.player.transportDrones.Length) chosenResBox.workingDrones++;}
					if (GUI.Button(new Rect(box_r.x + box_r.width, box_r.y + k, k/2, k/2), "-")) {
						if (chosenResBox.workingDrones > 0) chosenResBox.workingDrones--;}
					GUI.Label(new Rect(box_r.x + box_r.width, box_r.y + k/2, 2 *k, k/2), chosenResBox.workingDrones.ToString() + " drones");
				}
			}
		}
	}
}
