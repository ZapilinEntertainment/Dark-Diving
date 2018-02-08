using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModuleType {Empty};

public class Module : MonoBehaviour {
	public static readonly int SMALL_MODULE_CAPACITY = 16;
	public static float HUMAN_WALKING_ACTIVITY = 0.2f;
	public static Rect MODULE_INFO_RECT;
	public int f_number, f_maxCapacity, f_realCapacity,hoveredItemIndex = -1, focusedItemIndex = -1;
	public bool showOnGUI = false;
	Item[] storage;
	Rect iconRect, storageRect;
	Texture moduleTexture;
	ModuleType f_type = ModuleType.Empty;
	bool havePeopleOnboard = false;
	PlayerController pc;
	float humanActivityTimer = 10, humanActivityDelay = 60,k;



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
		print (storage[index].f_name + " removed");
		storage[index] = null;
		havePeopleOnboard = false;
		foreach (Item it in storage) {
			if (it.type == ItemType.Human) {havePeopleOnboard = true; break;}
		}
	}

	public void Update() {
		if (Input.GetMouseButtonDown(0)) focusedItemIndex = GetFocusedCellNumber();
		k = GameMaster.GetGUIPiece();

		if (GameMaster.isPaused()) return;

		hoveredItemIndex = GetFocusedCellNumber();
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

	int GetFocusedCellNumber () {
		Vector2 curpos = GameMaster.cursorPosition;
		if (curpos.x > storageRect.x && curpos.x < storageRect.x + storageRect.width && curpos.y > storageRect.y && curpos.y < storageRect.y + storageRect.height) {
			int i = (int) ((curpos.x - storageRect.x) / (storageRect.height/2));
			if (curpos.y > storageRect.y + storageRect.height / 2) i += (int)(storage.Length / 2);
			return i;
		}
		else return -1;
	}

	void OnGUI() {
		int sw = Screen.width, sh = Screen.height;
		if (GameMaster.isPaused()) return;
		if (f_type != ModuleType.Empty && moduleTexture!= null) GUI.DrawTexture(iconRect, moduleTexture,ScaleMode.StretchToFill);
		if (showOnGUI) {
			int realPosX = (int) storageRect.x; if (f_type != ModuleType.Empty) realPosX += (int)(storageRect.width)/2;
			GUI.DrawTexture(storageRect, PoolMaster.mainPool.Inventory16cells_tx,ScaleMode.StretchToFill);

			float itemCell = storageRect.height / 2;
			if (focusedItemIndex != -1) GUI.DrawTexture(new Rect(storageRect.x + focusedItemIndex%(storage.Length/2.0f) * itemCell, storageRect.y + focusedItemIndex/(storage.Length/2) * itemCell, itemCell,itemCell), PoolMaster.mainPool.choosingFrame_tx, ScaleMode.StretchToFill);
			Rect itemRect = new Rect(realPosX, storageRect.y, itemCell, itemCell);
			for (int j = 0; j< f_realCapacity; j++) {
				if (storage[j] != null) {
					GUI.DrawTexture(itemRect, storage[j].item_tx, ScaleMode.StretchToFill);
				}
				itemRect.x += itemCell; if (itemRect.x >= sw) {itemRect.x = realPosX; itemRect.y += itemCell;}
			}
					
			int i  = -1; bool focused = false;
			if (focusedItemIndex == -1) i =hoveredItemIndex; else {i = focusedItemIndex; focused = true;}
			GUI.skin.GetStyle("Button").fontSize = (int)(k/6.0f);
			if (i != -1 && i < storage.Length && storage[i] != null) {
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
				if (focused) {
					if (GUI.Button (new Rect(MODULE_INFO_RECT.x , sh - k ,k *2, k), Localization.item_button_drop)) RemoveItem(i);
					if (storage[i].type == ItemType.SingleUse) {
						Rect br = new Rect(MODULE_INFO_RECT.x + 2* k , sh - k ,k * 2, k);
						switch (storage[i].itemId) {
						case Item.CAPACITOR_FULL_ID: if (GUI.Button(br, Localization.general_use)) storage[i].Discharge();	break;
						case Item.CAPACITOR_EMPTY_ID: if (GUI.Button(br, Localization.general_use)) storage[i].Recharge();	break;
						}
					}
				}
					}
		}
	}
}
