using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesBox : Destructible {

	List<Item> content;
	public int[] counts;
	int contentBitmask = 0;
	public int extractionBitmask = 0;
	public bool lootUntilExhausted = false;
	public bool scanned = false;
	public bool explored = false, generated = false;
	public int generatorSeed = 0;
	public float extractionSpeed = 1, speedDegDeclineSpeed = 0.1f;
	public int workingDrones = 1;

	void Awake () {
		content = new List<Item>();
		counts = new int[0];
		if (generatorSeed != 0) GenerateContent();
	}

	public Item Extract() {
		if (!generated) GenerateContent();
		explored = true;
		if ((extractionBitmask == 0) || ((contentBitmask & extractionBitmask) == 0)) return null;
		List <int> acceptableResources = new List <int>();
		int mask1 = contentBitmask, mask2 = extractionBitmask;
		for (int i = 0; i< 32; i++) {
			int idCode = (int)Mathf.Pow(2,i);
			if (((contentBitmask & idCode) != 0) && ((extractionBitmask & idCode) != 0)) acceptableResources.Add(i);
		}
		int n = (int)( Random.value * acceptableResources.Count); // выбрать подходящий тип из тех, что собрался забрать
		for (int i =0; i < content.Count; i++) {
			if (content[i] == null) {content.RemoveAt(i);continue;}
			if (content[i].itemId == acceptableResources[n]) {
				Item searchingItem = content[i]; 
				content[i] = null;
				content.RemoveAt(i);
				RecalculateContentBitmask();
				return searchingItem;
			}
		}
		return null;
	}

	void GenerateContent() {
		counts= new int[32]; // 1 - metal, 2- rareMetal, 3 - plastic, 4 - chemicals,5 - electronic, 6 - human
		switch (generatorSeed) {
		default: 
			counts[1] = 2;
			counts[3] = 3;
			counts[5] = 2;
			break;
		}
		for (int i = 0; i< 32; i++) {
			if (counts[i] > 0) {
				for (int j =0; j < counts[i]; j++) {
					content.Add(Item.GetItemById(i));
				}
			}
		}
		RecalculateContentBitmask();
		generated = true;
	}

	void RecalculateContentBitmask() {
		if (content.Count == 0) {contentBitmask =0; return;}
		bool[] mask = new bool[32];
		counts = new int[32];
		foreach (Item i in content) {
			if (i != null && i.itemId < 32) {
				mask[i.itemId] = true;
				counts[i.itemId]++;
			}
			else content.Remove(i);
		}
		contentBitmask = 0;
		for (int i =0; i< 32; i++) {
			if (mask[i] == true) {contentBitmask += (int)(Mathf.Pow(2,i));}
		}
	}

	public int BitmasksConjuction () { //показывает, можно ли еще что-то добыть по текущей маске добычи
		if (!generated) GenerateContent();
		return (contentBitmask & extractionBitmask);
	}

	public List<Item> GetAvailableResourcesList (bool showCustomItems) {
		if (!generated) GenerateContent();
		List<Item> catalog = new List<Item>();
		for (int i = 0; i< 32;i++) {
			if ((contentBitmask & ((int)Mathf.Pow(2,i))) != 0) catalog.Add(Item.GetItemById(i));
		}
		if (showCustomItems) {
			//something
		}
		return catalog;
	}

	public int GetContentBitmask() {
		if (!generated) GenerateContent();
		return contentBitmask;
	}
}
