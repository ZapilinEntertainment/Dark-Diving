using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {Empty, Material, Human, SingleUse}

public class Item {
	public string f_name, f_description;
	public int f_count = 1, f_maxCount = 1;
	public int itemId = -1; // -1 is unidentified, -2 is custom item
	public Texture item_tx;
	public float f_drownChance = 0;
	public ItemType type = ItemType.Empty;
	public static readonly Item item_metal, item_dragmetal, item_plastic, item_chemicals, item_electronic, item_people, item_accumulator_full, item_accumulator_empty;

	public const int CAPACITOR_FULL_ID = 7, CAPACITOR_EMPTY_ID = 8;

	static Item () {
		item_metal = new Item ("Metal", "Metallic parts available for recycling.", ItemType.Material,1, 1, 1, PoolMaster.mainPool.item_metal_tx, 0);
		item_dragmetal = new Item ("Rare metals", "Different metals using for making alloys and composites.", ItemType.Material,2, 1,1, PoolMaster.mainPool.item_dragmetal_tx, 0.01f);
		item_plastic = new Item ("Plastic", "Plastic mass available for recycling", ItemType.Material,3, 1,1, PoolMaster.mainPool.item_plastic_tx, 0.05f);
		item_chemicals = new Item ("Chemicals", "Different chemical substances for making explosives and more", ItemType.Material,4,1,1, PoolMaster.mainPool.item_chemicals_tx, 0.9f);
		item_electronic = new Item ("Electronics", "Inner compound of high-technology devices", ItemType.Material,5,1,1, PoolMaster.mainPool.item_electronic_tx, 0.5f);
		item_people = new Item ("People", "Survivors. Poor things.", ItemType.Human,6, 1, 10, PoolMaster.mainPool.item_person_tx, 0.99f);
		item_accumulator_full = new Item("Accumulator", "Storage reserved energy", ItemType.SingleUse, CAPACITOR_FULL_ID,1,1, PoolMaster.mainPool.item_accumulator_full_tx, 0.5f);
		item_accumulator_empty = new Item("Empty accumulator", "Need to be restored", ItemType.SingleUse, CAPACITOR_EMPTY_ID,1,1, PoolMaster.mainPool.item_accumulator_empty_tx, 0.01f);
	}

	public void Flooding () {
		if (Random.value < f_drownChance) {type = ItemType.Empty;itemId = -1;}
	}

	public Item() {
		f_name = "new item";
		f_description = "no description";
		f_count =1;
		f_maxCount = 1;
		f_drownChance = 0;
		type = ItemType.Empty;
		itemId = -1;
		item_tx = PoolMaster.mainPool.item_default_tx;
	}

	public Item (string name, string desc, ItemType itemType, int itemID, int count, int maxCount,Texture icon, float chanceToBeDrown ) {
		f_name = name;
		f_description = desc;
		if (count < 0) f_count = 0;		f_count = count;
		if (maxCount < 0) f_maxCount = 0; f_maxCount = maxCount;
		if (icon !=null) item_tx = icon; else item_tx = PoolMaster.mainPool.item_default_tx;
		if (chanceToBeDrown < 0) chanceToBeDrown = 0; else	chanceToBeDrown = Mathf.Repeat(chanceToBeDrown,1);
		f_drownChance = chanceToBeDrown;
		type = itemType;
		itemId = itemID;
	}

	public Item Clone () {
		Item item2 = new Item();
		Item.CopyItemData(this, item2);	
		return(item2);
	}

	public void Convert (int itemId) {
		Item targetItem = GetItemById(itemId);
		Item.CopyItemData(targetItem, this);
	}

	static void CopyItemData(Item donor, Item acceptor) {
		acceptor.f_name = donor.f_name;
		acceptor.f_description =  donor.f_description;
		acceptor.f_drownChance =  donor.f_drownChance;
		acceptor.f_maxCount =  donor.f_maxCount;
		acceptor.type =  donor.type;
		acceptor.item_tx =  donor.item_tx;
		acceptor.itemId =  donor.itemId;
	}

	public static Item GetItemById(int id) {
		switch (id) {
		case 1: return item_metal.Clone();break;
		case 2: return item_dragmetal.Clone();break;
		case 3: return item_plastic.Clone();break;
		case 4: return item_chemicals.Clone();break;
		case 5: return item_electronic.Clone();break;
		case 6: return item_people.Clone();break;
		default: return null; break;
		}
	}

	public void Discharge() {
		if (itemId != CAPACITOR_FULL_ID) return;
		PlayerController.player.RestoreEnergy();
		this.Convert(CAPACITOR_EMPTY_ID);
	}
	public void Recharge() {
		if (itemId != CAPACITOR_EMPTY_ID) return;
		this.Convert(CAPACITOR_FULL_ID);
	}
}
