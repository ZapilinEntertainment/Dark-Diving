using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour {
	public int number = 0, capacity = 16;
	public bool isEmpty = true, showOnGUI = false;
	Item[] storage;
	Rect iconRect, storageRect;
	Texture moduleTexture;

	public void SetRects (Rect icon, Rect inventoryPos) {
		iconRect = icon;
		storageRect = inventoryPos;
	}

	public Module() {
		number = -1;
		capacity = 0;
	}

	public Module(int num, int innerCapacity) {
		number = -num;
		capacity = innerCapacity;
		storage = new Item[innerCapacity];
	}

	void OnGUI() {
		if (GameMaster.isPaused()) return;
		if (!isEmpty && moduleTexture!= null) GUI.DrawTexture(iconRect, moduleTexture,ScaleMode.StretchToFill);
		if (showOnGUI) {
			GUI.DrawTexture(storageRect, GameMaster.pool.Inventory16cells_tx,ScaleMode.StretchToFill);
		}
	}
}
