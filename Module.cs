using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModuleType {Empty};

public class Module : MonoBehaviour {
	public static readonly int SMALL_MODULE_CAPACITY = 16;
	public static float HUMAN_WALKING_ACTIVITY = 0.2f;
	public static Rect MODULE_INFO_RECT;
	public int f_number, f_maxCapacity, f_realCapacity;
	public bool showOnGUI = false;
	Item[] storage;
	Rect iconRect, storageRect;
	Texture moduleTexture;
	ModuleType f_type = ModuleType.Empty;
	bool havePeopleOnboard = false;
	PlayerController pc;
	float humanActivityTimer = 10, humanActivityDelay = 60;



	public void SetRects (Rect icon, Rect inventoryPos) {
		iconRect = icon;
		storageRect = inventoryPos;
	}

	public Module() {
		f_number = -1;
		f_maxCapacity = SMALL_MODULE_CAPACITY; f_realCapacity = f_maxCapacity;
		storage = new Item[f_realCapacity];
		f_type = ModuleType.Empty;
	}
		

	public void ModuleSet (int number, int maxCapacity, ModuleType type, PlayerController controller) {
		if (number < 0) number = -1;
		if (maxCapacity < 0) maxCapacity = 0;
		f_number = number;
		f_maxCapacity = maxCapacity;
		f_type = type;
		if (f_type == ModuleType.Empty) f_realCapacity = f_maxCapacity; else f_realCapacity = f_maxCapacity /2;
		storage = new Item[f_realCapacity];
		pc = controller;
	}

	public bool AddItem (Item itemToAdd) {
		Item item = itemToAdd.Clone();
		if (storage == null ||item== null || storage.Length == 0 || item.f_count <= 0) return false;
		bool found = false;
		for (int i = 0; i < f_realCapacity; i++) {
			if (storage[i] == null) {
				storage[i] = item;
				if (item.type == ItemType.Human) havePeopleOnboard = true;
				found =true;
				break;
			}
			else {
				if (storage[i].type == ItemType.Human && item.type == ItemType.Human ) 
				{
					if (storage[i].f_count <= storage[i].f_maxCount) {
						int c = storage[i].f_count + item.f_count;
						if ( c > storage[i].f_maxCount) {storage[i].f_count = storage[i].f_maxCount; item.f_count = (c - storage[i].f_maxCount); continue;}
						else {
							storage[i].f_count += item.f_count;
							if (storage[i].f_count > 2) storage[i].item_tx = PoolMaster.mainPool.item_manyPersons_tx;
							else storage[i].item_tx = PoolMaster.mainPool.item_twoPersons_tx;
							found = true; break;
						}
					}
				}
				else {if (storage[i].type == ItemType.Empty) RemoveItem(i);}
			}
		}
		return found;
	}
		

	public void RemoveItem (int index) {
		if (index <0 || index > f_realCapacity || storage[index] == null) return;
		storage[index] = null;
		havePeopleOnboard = false;
		foreach (Item it in storage) {
			if (it.type == ItemType.Human) {havePeopleOnboard = true; break;}
		}
	}

	public void Update() {
		if (GameMaster.isPaused()) return;
		if (havePeopleOnboard) {
			if (humanActivityTimer > 0) humanActivityTimer-=Time.deltaTime;
			if (humanActivityTimer <= 0) {
				List<int> freeCells = new List<int>();
				for (int i = 0; i < storage.Length; i++) {
					if (storage[i] == null)  freeCells.Add(i);
					else {
						if (storage[i].type == ItemType.Human && storage[i].f_count > 1) {
							if (Random.value < HUMAN_WALKING_ACTIVITY) {
								if (Random.value > 0.5f) {
									//топают в соседний отсек
									int k = 1; if (Random.value > 0.5f) k = -1;
									//if (pc.GetModule(f_number + k)).AddItem(Item.item_people)
									// придется писать отедльные функции add и remove с количественными значениями
								}
							}
						}
					}
				}
				humanActivityTimer = humanActivityDelay;
			}
		}
	}

	void OnGUI() {
		int sw = Screen.width;
		if (GameMaster.isPaused()) return;
		if (f_type != ModuleType.Empty && moduleTexture!= null) GUI.DrawTexture(iconRect, moduleTexture,ScaleMode.StretchToFill);
		if (showOnGUI) {
			int realPosX = (int) storageRect.x; if (f_type != ModuleType.Empty) realPosX += (int)(storageRect.width)/2;
			GUI.DrawTexture(storageRect, PoolMaster.mainPool.Inventory16cells_tx,ScaleMode.StretchToFill);

			float itemCell = storageRect.height / 2;
			Rect itemRect = new Rect(realPosX, storageRect.y, itemCell, itemCell);
			for (int k = 0; k < f_realCapacity; k++) {
				if (storage[k] != null) {
					GUI.DrawTexture(itemRect, storage[k].item_tx, ScaleMode.StretchToFill);
				}
				itemRect.x += itemCell; if (itemRect.x >= sw) {itemRect.x = realPosX; itemRect.y += itemCell;}
			}
			Vector2 curpos = GameMaster.cursorPosition;
			if (curpos.x > realPosX) {
				if (curpos.y > storageRect.y && curpos.y < storageRect.y + storageRect.height) {
					int i = (int) ((curpos.x - storageRect.x) / itemCell);
					if (curpos.y > storageRect.y + storageRect.height / 2) i += (int)(storage.Length / 2);
					if (i < storage.Length && storage[i] != null) {
						Rect r = MODULE_INFO_RECT;
						r.width = MODULE_INFO_RECT.width / 2; r.height = MODULE_INFO_RECT.height / 2;
						GUI.DrawTexture(r, storage[i].item_tx, ScaleMode.StretchToFill);
						r.x += MODULE_INFO_RECT.width /2;
						GUI.Label (r, storage[i].f_name + " (" + storage[i].f_count.ToString() + ')');
						r.y += MODULE_INFO_RECT.height /2;
						r.x -= MODULE_INFO_RECT.width /2;
						r.width = MODULE_INFO_RECT.width;
						r.height = MODULE_INFO_RECT.height;
						GUI.Label(r, storage[i].f_description);
					}
				}
			}
		}
	}
}
