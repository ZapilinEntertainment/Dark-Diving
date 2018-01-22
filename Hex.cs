using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex  {
	const float MEDIUM_BLOCK_GEN_CHANCE = 0.3f;

	public int x, y;
	public float h;
	GameObject chunk;
	public BiomeType biomeType;

	public Hex() {
		x = -1;
		y= -1;
		h = 0;
		chunk = null;
		biomeType = BiomeType.empty;
	}

	public Hex(int a, int b) {
		if (a < 0) a = 0;
		if (b < 0) b = 0;
		x = a;
		y= b;
		h=0;
		chunk = null;
		biomeType = BiomeType.empty;
	}
	public Hex(int a, int b, GameObject g) {
		if (a < 0) a = 0;
		if (b < 0) b = 0;
		x = a;
		y= b;
		h=0;
		chunk = g;
		biomeType = BiomeType.empty;
	}
	public Hex(int xpos, int ypos, float height,GameObject g, BiomeType btype) {
		if (xpos < 0) xpos = 0;
		if (ypos < 0) ypos = 0;
		x = xpos;
		y= ypos;
		h = height;
		chunk = g;
		biomeType = btype;
		if (biomeType != BiomeType.empty) BiomeStructuresGeneration();
	}

	public void SetChunk (GameObject g) {chunk = g;}
	public GameObject GetChunk() {return chunk;}
	public Vector3 GetWorldPosition() {return chunk.transform.position;}
	public void SetWorldPosition(Vector3 pos) {chunk.transform.position = pos;}

	public void ApplyHeight() {
		if (chunk == null) return;
		chunk.transform.position = new Vector3(chunk.transform.position.x, h, chunk.transform.position.z);
	}

	public void BiomeStructuresGeneration() {
		switch (biomeType) {
		case BiomeType.city: 
			float ht = Mathf.Sqrt(3 * 62.5f * 62.5f);

			for (int k =0; k < 6; k++) {
				GameObject sector = new GameObject("sector"+k.ToString());
				sector.transform.parent = chunk.transform;
				sector.transform.localPosition = Vector3.zero;

				int[] buildingsIds = new int[64];
				GameObject[] buildings = new GameObject[64];
				int c = 0;
				for (int i =0; i< 8; i++) {
					int tcount = 3 + 2 * (i - 1);
					for (int j =0; j < tcount; j++) {
						Vector3 pos = sector.transform.TransformPoint(new Vector3((j - tcount/2) * 62.5f, 0.1f, -ht * 2.0f/3.0f - i * ht ) );
						GameObject building = null;
						Vector3 dir = sector.transform.TransformDirection(Vector3.forward);
						Quaternion rotation = Quaternion.LookRotation(-dir, Vector3.up);
						if (j % 2 ==1) {
							rotation = Quaternion.LookRotation(dir, Vector3.up);
							pos.z += 1/3.0f  * ht;
						}
						if (Random.value < 0.75f) {
							if (Random.value < 0.75f) {building = GameObject.Instantiate(PoolMaster.mainPool.city_housing_pref, pos, rotation, sector.transform);buildingsIds[c] = 1;}
							else {building = GameObject.Instantiate(PoolMaster.mainPool.city_offices_pref, pos, rotation, sector.transform);buildingsIds[c] = 2;}
						}
						else {
							if (Random.value > 0.5f) {building = GameObject.Instantiate(PoolMaster.mainPool.city_mall_pref, pos, rotation, sector.transform);buildingsIds[c] = 3;}
							else {building = GameObject.Instantiate(PoolMaster.mainPool.city_industrial_pref, pos, rotation, sector.transform);buildingsIds[c] = 4;}
						}
						buildings[c] = building;
						buildings[c].name = i.ToString() + ';' + j.ToString();
						if (buildings[c] != null) {
							if (Random.value < 1/3.0f) buildings[c].transform.Rotate(Vector3.up * 120);
							else if (Random.value > 0.5f) buildings[c].transform.Rotate(Vector3.up * 240);
							Vector3 originalScale = buildings[c].transform.localScale;
							originalScale.y = 8 - i;
							buildings[c].transform.localScale = originalScale;
						}
						c++;
					}
				}
				int a = 0, b = 4;
				int[] a_indexes = new int[] {0,1,2,3,  4,9,10,11,  8,13,14,15,  16,25,26,27,  20,29,30,31,  24,33,34,35,  
					36,49,50,51,  40,53,54,55,  44,57,58,59,  48,61,62,63};
				int[] b_indexes = new int[] {5,6,7,12,  17,18,19,28,  21,22,23,32,  37,38,39,52,  41,42,43,56,  
					45,46,47,60};
				while (a < a_indexes.Length) {
					if ((buildingsIds[a_indexes[a]] != 0) &&(buildingsIds[a_indexes[a]] == buildingsIds[a_indexes[a
						+1]])&&(buildingsIds[a_indexes[a]]  == buildingsIds[a_indexes[a+2]]) && (buildingsIds[a_indexes[a]]  == buildingsIds[a_indexes[a
							+3]])) 
					{
						GameObject.Destroy(buildings[a_indexes[a+1]]);buildings[a_indexes[a+1]] = null;
						GameObject.Destroy(buildings[a_indexes[a+2]]);buildings[a_indexes[a+2]] = null;
						GameObject.Destroy(buildings[a_indexes[a+3]]);buildings[a_indexes[a+3]] = null;
						buildings[a_indexes[a]].transform.localPosition += Vector3.back * 2.0f/3.0f * ht;
						buildings[a_indexes[a]].transform.localScale = Vector3.one * 2;
					}
					a += 4;
				}
				while (b < b_indexes.Length) {
					if ((buildingsIds[b_indexes[b]] != 0) &&(buildingsIds[b_indexes[b]] == buildingsIds[b_indexes[b+1]]) 
						&&(buildingsIds[b_indexes[b]]  == buildingsIds[b_indexes[b+2]]) && (buildingsIds[b_indexes[b]]  == buildingsIds[b_indexes[b+3]])) 
					{
						GameObject.Destroy(buildings[b_indexes[b+1]]);GameObject.Destroy(buildings[b_indexes[b	+2]]);GameObject.Destroy(buildings[b_indexes[b]]);
						buildings[b_indexes[b+3]].transform.localPosition += Vector3.forward * 2.0f/3.0f * ht;
						buildings[b_indexes[b+3]].transform.localScale = Vector3.one * 2;
					}
					b += 4;
				}
				sector.transform.localRotation = Quaternion.Euler(0, k* 60, 0);
			}
			break;
		}
	}
}

