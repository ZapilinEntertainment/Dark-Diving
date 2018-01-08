using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {Empty, Material, Human}

public class Item {
	public string f_name, f_description;
	public int f_count = 1, f_maxCount = 1;
	public Texture item_tx;
	public float f_drownChance = 0;
	public ItemType type = ItemType.Empty;
	public static readonly Item item_metal, item_dragmetal, item_plastic, item_chemicals, item_electronic, item_people;

	static Item () {
		item_metal = new Item ("Metal", "Metallic parts available for recycling.", ItemType.Material, 1, 1, PoolMaster.mainPool.item_metal_tx, 0);
		item_dragmetal = new Item ("Rare metals", "Different metals using for making alloys and composites.", ItemType.Material, 1,1, PoolMaster.mainPool.item_dragmetal_tx, 0.01f);
		item_plastic = new Item ("Plastic", "Plastic mass available for recycling", ItemType.Material, 1,1, PoolMaster.mainPool.item_plastic_tx, 0.05f);
		item_chemicals = new Item ("Chemicals", "Different chemical substances for making explosives and more", ItemType.Material,1,1, PoolMaster.mainPool.item_chemicals_tx, 0.9f);
		item_electronic = new Item ("Electronics", "Inner compound of high-technology devices", ItemType.Material,1,1, PoolMaster.mainPool.item_electronic_tx, 0.5f);
		item_people = new Item ("People", "Survivors. Poor things.", ItemType.Human, 1, 10, PoolMaster.mainPool.item_person_tx, 0.99f);
	}

	public void Flooding () {
		if (Random.value < f_drownChance) type = ItemType.Empty;
	}

	public Item() {
		f_name = "new item";
		f_description = "no description";
		f_count =1;
		f_maxCount = 1;
		f_drownChance = 0;
		type = ItemType.Empty;
		item_tx = PoolMaster.mainPool.item_default_tx;
	}

	public Item (string name, string desc, ItemType itemType, int count, int maxCount,Texture icon, float chanceToBeDrown ) {
		f_name = name;
		f_description = desc;
		if (count < 0) f_count = 0;		f_count = count;
		if (maxCount < 0) f_maxCount = 0; f_maxCount = maxCount;
		if (icon !=null) item_tx = icon; else item_tx = PoolMaster.mainPool.item_default_tx;
		if (chanceToBeDrown < 0) chanceToBeDrown = 0; else	chanceToBeDrown = Mathf.Repeat(chanceToBeDrown,1);
		f_drownChance = chanceToBeDrown;
		type = itemType;
	}

	public Item Clone () {
		Item item2 = new Item();
		item2.f_name = f_name;
		item2.f_description = f_description;
		item2.f_drownChance = f_drownChance;
		item2.f_maxCount = f_maxCount;
		item2.type = type;
		item2.item_tx = item_tx;
		return(item2);
	}
}
