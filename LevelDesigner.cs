﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDesigner : MonoBehaviour {
	const float N_RADIUS = 1730, RADIUS = 1000; // перпендикуляр и радиус
	const float AVERAGE_DEPTH_NOMINAL =-13800, MAX_HEIGHT = 200, MAX_DEPTH_NOMINAL = - 23000, ACCEPTABLE_DELTA = 150, MAX_GAP_NOMINAL = 2000;
	const float MAPCELL_NORMAL = 1.1f, SQRT_THREE = 1.73205f;
	public int circlesCount = 5;
	float maxDepth, averageDepth,maxGap;
	public GameObject clearHex;
	public Vector3 startPos = new Vector3(0, -2000, 0);
	Hex[,] hex;

	bool mapEnabled = false;
	public Transform cameraTransform;
	public GameObject cellRenderer_pref;
	GameObject[,] hexRenderers;
	GameObject shipMarker;
	public Sprite shipSprite_tx;
	float mapStartXpos = -0.2f, mapStartYpos = 0.168f,GUISpritesZpos = 0.31f;
	float mapCoefficient = 0.1f, distanceCoefficient = 0.1f;
	Color brownColor = new Color (128, 0, 0);

	void Awake () {
		GameMaster.designer = this;
		startPos = circlesCount *2 * RADIUS * new Vector3(-1,0,1);
	}

	void Start () {
		int a = circlesCount;
		int b = (int) (a * 1.732f * 2);
		float scaleCoefficient = circlesCount / 36f;
		maxDepth = MAX_DEPTH_NOMINAL * scaleCoefficient; //print (maxDepth);
		averageDepth = AVERAGE_DEPTH_NOMINAL * scaleCoefficient; //print (averageDepth);
		maxGap = MAX_GAP_NOMINAL * scaleCoefficient; if (maxGap < 1000) maxGap = 1000;
		startPos.y = averageDepth;
		Gen2(b,a);
		hexRenderers = new GameObject[b,a];

		float nearClipSize_y = Camera.main.nearClipPlane * 2 * Mathf.Tan(Camera.main.fieldOfView/2 * Mathf.Deg2Rad);
		mapCoefficient =nearClipSize_y / (MAPCELL_NORMAL * 2 * b) ;
		float radius = 2 * MAPCELL_NORMAL / SQRT_THREE * mapCoefficient;
		distanceCoefficient = radius / RADIUS;
		mapStartXpos = a/2.0f * 3* radius * (-1); mapStartYpos = MAPCELL_NORMAL * mapCoefficient * (b-1);

		for (int i = 0; i < b; i++) {
			for (int j = 0; j< a; j++)
			{
				hexRenderers[i,j] = Instantiate(cellRenderer_pref) as GameObject;
				hexRenderers[i,j].transform.parent = cameraTransform;
				hexRenderers[i,j].transform.localScale = Vector3.one * mapCoefficient;
				hexRenderers[i,j].transform.localRotation = Quaternion.Euler(Vector3.zero);
				hexRenderers[i,j].transform.localPosition = new Vector3(mapStartXpos + j * 3 * radius, mapStartYpos - i *  radius * SQRT_THREE * 0.5f, GUISpritesZpos);
				if (i%2 == 0) hexRenderers[i,j].transform.localPosition -=new Vector3(1.5f * radius,0,0);
				if (hex[i,j].h <= 0) {
					hexRenderers[i,j].GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.blue, hex[i,j].h/maxDepth);
				}
				else {
					hexRenderers[i,j].GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white,brownColor , hex[i,j].h/500.0f);
				}
				hexRenderers[i,j].name = "cell "+i.ToString()+','+j.ToString();
				hexRenderers[i,j].gameObject.SetActive(false);
			}
		}
		shipMarker = Instantiate(cellRenderer_pref) as GameObject;
		shipMarker.transform.parent = cameraTransform;
		//shipMarker.transform.localScale = Vector3.one * radius;
		shipMarker.transform.localRotation = Quaternion.Euler(0,0,ScenarioManager.scenarist.GetPlayerRotation().eulerAngles.y * (-1));
		shipMarker.GetComponent<SpriteRenderer>().sprite = shipSprite_tx;
		shipMarker.SetActive(false);
	}

	void Update () {
		if (GameMaster.isPaused()) return;
		if (Input.GetKeyDown("m")) {
			int a= hex.GetLength(0), b= hex.GetLength(1);
			if (mapEnabled)
			{
				for (int i = 0; i < a; i++) {
					for (int j = 0; j< b; j++)
					{hexRenderers[i,j].gameObject.SetActive(false);}
			}
				shipMarker.SetActive(false);
				mapEnabled = false;
		}
			else
			{
				for (int i = 0; i < a; i++) {
					for (int j = 0; j< b; j++)
					{hexRenderers[i,j].gameObject.SetActive(true);}
				} 
				shipMarker.SetActive(true);
				mapEnabled = true;}
	}
	}

	void LateUpdate() {
		if (GameMaster.isPaused()) return;
		if (mapEnabled) {
			Vector3 delta = ScenarioManager.scenarist.GetPlayerPosition() - hex[0,0].GetWorldPosition(); delta.y = delta.z; delta.z = -0.005f;
			shipMarker.transform.localPosition = hexRenderers[0,0].transform.localPosition + delta * distanceCoefficient;
			shipMarker.transform.localRotation = Quaternion.Euler(0,0,ScenarioManager.scenarist.GetPlayerRotation().eulerAngles.y * (-1)) ;		
		}
	}

	void Gen1 () {
		GameObject g = Instantiate(clearHex, startPos, Quaternion.identity) as GameObject;
		g.name = "(0,0)";
		float ang = 0, rad = 0;
		Vector3 pos = startPos;
		for (int n = 0; n < circlesCount; n++) {
			int hexCount = 6;
			for (int i = 0; i < hexCount; i++ ) {
				float fi = i;
				rad = fi/hexCount * Mathf.PI * 2 ;
				pos.x = Mathf.Cos(rad) * N_RADIUS* (1 + n);
				pos.z = Mathf.Sin(rad) * N_RADIUS* (1 + n);
				pos.y += Random.value * N_RADIUS / 10;
				g = Instantiate(clearHex, pos,Quaternion.identity) as GameObject;
				g.name = "("+(n+1).ToString()+','+((int)(rad * Mathf.Rad2Deg)).ToString() +")";
			}
		}
	}

	void Gen2 (int width,int height) {
		hex = new Hex[width, height];
		GameObject g; 
		string nm = "";
		Vector3 pos = startPos;
		for (int i = 0; i< width; i++)
		{
			pos = startPos;
			pos.z -= N_RADIUS/2 * i;
			if (i%2 != 0) pos.x += 1.5f * RADIUS;
		for (int j = 0; j < height; j++) {
			g= Instantiate(clearHex, pos, Quaternion.identity) as GameObject;
				//print (i.ToString()+" "+j.ToString());
			nm = i.ToString();
			if (nm.Length == 1) nm = '0' + nm;
			nm += j.ToString();
			if (nm.Length == 3) nm = nm.Substring(0,2) + '0' + nm.Substring(2,1);
				g.name = nm;
				pos.x += 3 * RADIUS;
				hex[i,j] = new Hex(i, j, pos.y, g, new Biome(BiomeType.empty));
			}
		}

		HeightsGeneration();
	}

	void HeightsGeneration() {
		int step = 0, dirMain = 5, dirUpto = 1, dirUndo = 3, yCount = hex.GetLength(1) * 2;
		Hex mainHex = hex[0,0], underHex, ontoHex;
		float gpart = 0.3f;
		while (mainHex != null) {
			float h = (1 - Mathf.Cos(step/((float)yCount) * Mathf.PI * 2)) * averageDepth;
			mainHex.h = h;
			mainHex.ApplyHeight();

				float dh = h, gpart2 = gpart;
				underHex = GetNeighbourCell(mainHex, dirUndo);
			while (underHex != null) {
					float delta = maxGap * (gpart + (1 - gpart) * Random.value);
					if (Random.value < 0.5f) delta*=-1;
					Hex b = GetNeighbourCell(underHex, 1),c = GetNeighbourCell(underHex, 2);
					float minHeight = dh, maxHeight = dh;
					if (b != null) {
						if (b.h > maxHeight) maxHeight = b.h;
						else {if (b.h < minHeight) minHeight = b.h;}
					}
					if (c != null) {
						if (c.h > maxHeight) maxHeight = c.h;
						else {if (c.h < minHeight) minHeight = c.h;}
					}

				underHex.h = dh + delta;
				if (underHex.h > minHeight + maxGap) underHex.h = minHeight + maxGap;
				else {
					if (underHex.h < maxHeight - maxGap) underHex.h = maxHeight - maxGap;
				}
				underHex.ApplyHeight();
				//print ("min: "+minHeight.ToString() + ", max: "+ maxHeight.ToString() +", maxGap: " +maxGap.ToString() +", end: "+underHex.h.ToString());
				dh = underHex.h;
				underHex = GetNeighbourCell(underHex, dirUndo);
				//underHex = null;
				}
			// НАД ГЛАВНОЙ ДИАГОНАЛЬЮ :
			dh = mainHex.h;
			ontoHex = GetNeighbourCell(mainHex, dirUpto);
			while (ontoHex != null) {
				float delta = maxGap * (gpart + (1 - gpart) * Random.value);
				if (Random.value < 0.5f) delta*=-1;
				Hex b = GetNeighbourCell(ontoHex, 3),c = GetNeighbourCell(ontoHex, 2);
				float minHeight = dh, maxHeight = dh;
				if (b != null) {
					if (b.h > maxHeight) maxHeight = b.h;
					else {if (b.h < minHeight) minHeight = b.h;}
				}
				if (c != null) {
					if (c.h > maxHeight) maxHeight = c.h;
					else {if (c.h < minHeight) minHeight = c.h;}
				}

				ontoHex.h = dh + delta;
				if (ontoHex.h > minHeight + maxGap) ontoHex.h = minHeight + maxGap;
				else {
					if (ontoHex.h < maxHeight - maxGap) ontoHex.h = maxHeight - maxGap;
				}
				//print ("min: "+minHeight.ToString() + ", max: "+ maxHeight.ToString() +", maxGap: " +maxGap.ToString() +", end: "+ontoHex.h.ToString());
				ontoHex.ApplyHeight();
				dh = ontoHex.h;
				ontoHex = GetNeighbourCell(ontoHex, dirUpto);
				//ontoHex = null;
			}
			mainHex = GetNeighbourCell(mainHex, dirMain);
			step++;
		}
	}

	void HeightsGeneration2() {
		int positionBorder = (int)((MAX_HEIGHT - averageDepth) / maxGap); //print (positionBorder);
		int a = hex.GetLength(0);
		int b = hex.GetLength(1);
		//first quarter
		int specialPoint1_x = (int)(a - Random.value * (a/2 - positionBorder));
		int specialPoint1_y = (int)(Random.value * b);
		//second quarter
		int specialPoint2_x = (int)(Random.value * a );
		int specialPoint2_y = (int)(Random.value * b);
		//third quarter
		int specialPoint3_x = (int)(Random.value * a );
		int specialPoint3_y = (int)(Random.value * b + b);
		//fourth quarter
		int specialPoint4_x = (int)(Random.value * a + a);
		int specialPoint4_y = (int)(Random.value * b + b);

		int x,y;
		Hex peak;
		Hex[] deep = new Hex[3];
		if (Random.value > 0.5f) {
			if (Random.value > 0.5f) {
				peak = new Hex(specialPoint1_x, specialPoint1_y);
				deep[0] = new Hex(specialPoint2_x, specialPoint2_y);
				deep[1] = new Hex(specialPoint3_x, specialPoint3_y);
				deep[2] = new Hex(specialPoint4_x, specialPoint4_y);
			}
			else {
				peak = new Hex(specialPoint2_x, specialPoint2_y);
				deep[0] = new Hex(specialPoint1_x, specialPoint1_y);
				deep[1] = new Hex(specialPoint3_x, specialPoint3_y);
				deep[2] = new Hex(specialPoint4_x, specialPoint4_y);
			}
		}
		else 
		{
			if (Random.value > 0.5f) {
				peak = new Hex(specialPoint4_x, specialPoint4_y);
				deep[0] = new Hex(specialPoint2_x, specialPoint2_y);
				deep[1] = new Hex(specialPoint3_x, specialPoint3_y);
				deep[2] = new Hex(specialPoint1_x, specialPoint1_y);
			}
			else
			{
				peak = new Hex(specialPoint3_x, specialPoint3_y);
				deep[0] = new Hex(specialPoint2_x, specialPoint2_y);
				deep[1] = new Hex(specialPoint1_x, specialPoint1_y);
				deep[2] = new Hex(specialPoint4_x, specialPoint4_y);
			}
		}
		peak.h = MAX_HEIGHT - Random.value * ACCEPTABLE_DELTA;
		foreach (Hex d in deep) {d.h = maxDepth + Random.value * ACCEPTABLE_DELTA;}


		//округ вершины
		a= hex.GetLength(0);
		//print (a);
		b = hex.GetLength(1);
		//print(b);

		Hex[] mountGrid = GetNeighbours(peak);
		for (int mi = 0; mi < 6; mi++) {
			float gpart = 0.7f;
			if (mountGrid[mi] == null) continue;
			float mh= peak.h - (maxGap * (gpart+ (1-gpart)* Random.value));
			mountGrid[mi].h=mh;
			Hex hx = GetNeighbourCell(mountGrid[mi],mi);
			float gpart2 = gpart;
			while (mh > averageDepth && hx != null)
			{
				hx.h = mh - maxGap * (gpart2 + (1-gpart2) * Random.value);
				gpart2 *= gpart2;
				mh = hx.h;
				hx = GetNeighbourCell(hx, mi);
			}
		}
		for (int i = 0; i < 3; i++) {
			if (Random.value > 0.5f) {
				//кратер
				float gpart = 0.1f;
				Hex[] nbs = GetNeighbours(deep[i]);
				for (int d =0; d < 6; d ++) {
					if (nbs[d] == null) continue;
					nbs[d].h= deep[i].h + maxGap * (gpart + (1-gpart) * Random.value);
					Hex nb2 = GetNeighbourCell(nbs[d], d);
					if (nb2 != null) {
						float gpart2 = gpart*1.5f;
						nb2.h = nbs[d].h + maxGap* (gpart2 + (1-gpart2) * Random.value);
					}
				}
			}
			else {
				int dir1 = Mathf.RoundToInt(Random.value * 5);
				float delta = 1 + Random.value * 2; if (Random.value > 0.5f) delta *= -1;
				int dir2= dir1 +Mathf.RoundToInt(delta);
				if (dir2 < 0) dir2 *= -1; else {if (dir2 > 5) dir2-=6; }

				float h = deep[i].h;
				float gpart = 0.3f, gpart2;
				Hex hx = GetNeighbourCell(deep[i], dir1);
				gpart2 = gpart;
				while (h <= averageDepth && hx!=null) {
					hx.h = h + maxGap * (gpart + (1-gpart * Random.value));
					gpart2 *= 0.9f;
					h = hx.h;
					hx = GetNeighbourCell(hx, dir1);
				}
				hx = GetNeighbourCell(deep[i], dir2); h = deep[i].h; 
				gpart2 = gpart;
				while (h <= averageDepth && hx!=null) {
					hx.h = h + maxGap * (gpart + (1-gpart * Random.value));
					gpart2 *= 0.9f;
					h = hx.h;
					hx = GetNeighbourCell(hx, dir2);
				}
			}
		}


		foreach (Hex h in hex) {h.ApplyHeight();}
	}
		

	Hex GetNeighbourCell (Hex cell, int n)
	{
		if (cell == null) return null;
		int x = -1, y = -1;
		if (n < 0) n= 0;
		else if (n > 5) n = 5;
		if (cell.x%2 == 0) {
			switch (n) {
			case 0: if (cell.x - 1 < 0) return null;  x = cell.x -1; y = cell.y;break;
			case 1: if (cell.x - 2 < 0) return null; x = cell.x - 2; y = cell.y; break;
			case 2: if (cell.x - 1 < 0 || cell.y - 1 < 0) return null; x = cell.x - 1; y = cell.y -1;break;
			case 3: if (cell.x+1 >= hex.GetLength(0) || cell.y -1 < 0) return null; x = cell.x+1; y = cell.y-1;break;
			case 4: if (cell.x+2 >= hex.GetLength(0)) return null; x =cell.x+2; y = cell.y;break;
			case 5: if (cell.x+1 >= hex.GetLength(0)) return null; x = cell.x+1; y = cell.y; break;
			}
		}
		else {
			switch (n) {
			case 0: if (cell.y+1 >= hex.GetLength(1) || cell.x -1 < 0) return null; x = cell.x -1; y = cell.y+1;break;
			case 1: if (cell.x - 2 < 0) return null; x = cell.x -2; y = cell.y; break;
			case 2: if (cell.x - 1 < 0) return null; x = cell.x-1; y = cell.y;break;
			case 3: if (cell.x+1 >= hex.GetLength(0)) return null; x = cell.x+1; y = cell.y;break;
			case 4: if (cell.x+2 >= hex.GetLength(0)) return null; x = cell.x+2; y = cell.y;break;
			case 5: if (cell.x+1 >= hex.GetLength(0) || cell.y+1 >= hex.GetLength(1)) return null; x = cell.x+1; y = cell.y+1; break;
			}
		}
		if (x == -1 || y== -1) return null;
		else return hex[x,y];
	}

	Hex[] GetNeighbours (Hex h) {
		int x = h.x, y = h.y;
		Hex[] nbs = new Hex[6];
		int a = hex.GetLength(0), b= hex.GetLength(1);
		if (x - 1 >= 0) {
			if (x - 2 >= 0) nbs[1] = hex[x-2, y];
			if (x%2 == 0) {
				if ( y -1 >= 0) nbs[2] = hex[x - 1, y-1 ]; 
				nbs[0] = hex[x-1, y];
			}
			else {
				nbs[2] = hex[x - 1, y]; 
				if (y+1 < b) nbs[0] = hex[x-1, y+1];
			}
		}
		if (x + 1 < a) {
			if (x + 2 < a) nbs[4] = hex[x+2, y];
			if (x%2 == 0) {
				if ( y -1 >= 0) nbs[3] = hex[x+1, y-1];
				nbs[5] = hex[x+1, y];
			}
			else {
				nbs [3] = hex[x+1, y];
				if (y+1 < b) nbs [5] = hex[x+1, y+1];
			}
		}
		return nbs;
	}

	public Hex GetHex(int x, int y) {
		if (x<0 || y <0 || x>= hex.GetLength(0) || y >= hex.GetLength(1)) return null;
		return hex[x,y];
	}
}
