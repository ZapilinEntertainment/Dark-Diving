using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDesigner : MonoBehaviour {
	const float N_RADIUS = 1730, RADIUS = 1000; // перпендикуляр и радиус
	const float MAX_HEIGHT = 200, MAX_DEPTH = - 10000, ACCEPTABLE_DELTA = 150, MAX_GAP = 2000;
	const float MAPCELL_NORMAL = 1.1f, SQRT_THREE = 1.73205f;
	public int circlesCount = 5;
	public GameObject clearHex;
	public Vector3 startPos = new Vector3(0, -2000, 0);
	Hex[,] hex;
	float [,] heights;

	bool mapEnabled = false;
	public Transform cameraTransform;
	public GameObject cellRenderer_pref;
	GameObject[,] hexRenderers;
	GameObject shipMarker;
	public Sprite shipSprite_tx;
	float mapStartXpos = -0.2f, mapStartYpos = 0.168f,GUISpritesZpos = 0.31f;
	float mapCoefficient = 0.1f, distanceCoefficient = 0.1f;
	Color brownColor = new Color (128, 0, 0);

	void Awake () {GameMaster.designer = this;}

	void Start () {
		int a = 10;
		int b = (int) (a * 1.732f * 2);
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
				if (heights[i,j] <= 0) {
					hexRenderers[i,j].GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.blue, heights[i,j]/MAX_DEPTH);
				}
				else {
					hexRenderers[i,j].GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white,brownColor , heights[i,j]/500.0f);
				}
				hexRenderers[i,j].name = "cell "+i.ToString()+','+j.ToString();
				hexRenderers[i,j].gameObject.SetActive(false);
			}
		}
		shipMarker = Instantiate(cellRenderer_pref) as GameObject;
		shipMarker.transform.parent = cameraTransform;
		//shipMarker.transform.localScale = Vector3.one * radius;
		shipMarker.transform.localRotation = Quaternion.Euler(0,0,GameMaster.scenarist.GetPlayerRotation().eulerAngles.y * (-1));
		shipMarker.GetComponent<SpriteRenderer>().sprite = shipSprite_tx;
		shipMarker.SetActive(false);
	}

	void Update () {
		if (GameMaster.isPaused()) return;
		if (Input.GetKeyDown("m")) {
			int a= heights.GetLength(0), b= heights.GetLength(1);
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
			Vector3 delta = GameMaster.scenarist.GetPlayerPosition() - hex[0,0].GetWorldPosition(); delta.y = delta.z; delta.z = -0.005f;
			shipMarker.transform.localPosition = hexRenderers[0,0].transform.localPosition + delta * distanceCoefficient;
			shipMarker.transform.localRotation = Quaternion.Euler(0,0,GameMaster.scenarist.GetPlayerRotation().eulerAngles.y * (-1)) ;		
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
		heights = new float[width, height];
		GameObject g; Hex h;
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
			h = new Hex(i, j, g);
			hex[i,j] = h;
			nm = i.ToString();
			if (nm.Length == 1) nm = '0' + nm;
			nm += j.ToString();
			if (nm.Length == 3) nm = nm.Substring(0,2) + '0' + nm.Substring(2,1);
				g.name = nm;
				pos.x += 3 * RADIUS;

			}
		}
		HeightsGeneration();
	}

	void HeightsGeneration() {
		int a = heights.GetLength(0)/2;
		int b = heights.GetLength(1)/2;

		//first quarter
		int specialPoint1_x = (int)(Random.value * a + a);
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
		if (Random.value > 0.5f) {
			if (Random.value > 0.5f) {
				heights[specialPoint1_x, specialPoint1_y] = MAX_HEIGHT - Random.value * ACCEPTABLE_DELTA;
				x = specialPoint1_x; y= specialPoint1_y;
				heights[specialPoint2_x, specialPoint2_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
				heights[specialPoint3_x, specialPoint3_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
				heights[specialPoint4_x, specialPoint4_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
			}
			else {
				heights[specialPoint2_x, specialPoint2_y] = MAX_HEIGHT - Random.value * ACCEPTABLE_DELTA;
				x = specialPoint2_x; y= specialPoint2_y;
				heights[specialPoint1_x, specialPoint1_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
				heights[specialPoint3_x, specialPoint3_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
				heights[specialPoint4_x, specialPoint4_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
			}
		}
		else 
		{
			if (Random.value > 0.5f) {
				heights[specialPoint4_x, specialPoint4_y] = MAX_HEIGHT - Random.value * ACCEPTABLE_DELTA;
				x = specialPoint4_x; y= specialPoint4_y;
				heights[specialPoint2_x, specialPoint2_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
				heights[specialPoint3_x, specialPoint3_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
				heights[specialPoint1_x, specialPoint1_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
			}
			else
			{
				heights[specialPoint3_x, specialPoint3_y] = MAX_HEIGHT - Random.value * ACCEPTABLE_DELTA;
				x = specialPoint3_x; y= specialPoint3_y;
				heights[specialPoint2_x, specialPoint2_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
				heights[specialPoint1_x, specialPoint1_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
				heights[specialPoint4_x, specialPoint4_y] = MAX_DEPTH + Random.value * ACCEPTABLE_DELTA * 2;
			}
		}

		//округ вершины
		float h = 	heights[x,y];
		a= heights.GetLength(0);
		//print (a);
		b = heights.GetLength(1);
		//print(b);

		Hex[] mountGrid = GetNeighbours(new Hex(x,y));
		for (int mi = 0; mi < 6; mi++) {
			float gpart = 0.7f;
			if (mountGrid[mi] == null) continue;
			float mh= h - (MAX_GAP * (gpart+ (1-gpart)* Random.value));
			heights[mountGrid[mi].x, mountGrid[mi].y] =mh;
			int direction = mi, steps = 10;
			Hex cornerHex = mountGrid[mi];
			for (int s = 0; s < steps; s++) {
				gpart *= 0.7f;
				Hex nh = GetNeighbourCell(cornerHex, direction);
				if (nh == null) break;
				heights[nh.x, nh.y] = mh - (MAX_GAP * (gpart+ (1-gpart)* Random.value));
				mh = heights[nh.x, nh.y] ;
				cornerHex = nh;
			}
		}



		ApplyHeights();
	}

	void ApplyHeights () {
		int width = heights.GetLength(0), height = heights.GetLength(1);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j< height; j++)
			{
				if (heights[i,j] != 0) 	
				{
					Vector3 pos = hex[i,j].GetWorldPosition();
					pos.y = heights[i,j];
					hex[i,j].SetWorldPosition(pos);
				}
			}
		}
	}

	Hex GetNeighbourCell (Hex cell, int n)
	{
		Hex neighbour = new Hex();
		if (n < 0) n= 0;
		else if (n > 5) n = 5;
		if (cell.x%2 == 0) {
			switch (n) {
			case 0: if (cell.x - 1 < 0) return null;  neighbour.x = cell.x -1; neighbour.y = cell.y;break;
			case 1: if (cell.x - 2 < 0) return null; neighbour.x = cell.x; neighbour.y = cell.y; break;
			case 2: if (cell.x - 1 < 0 || cell.y - 1 < 0) return null; neighbour.x = cell.x - 1; neighbour.y = cell.y -1;break;
			case 3: if (cell.x+1 >= heights.GetLength(0) || cell.y -1 < 0) return null; neighbour.x = cell.x+1; neighbour.y = cell.y-1;break;
			case 4: if (cell.x+2 >= heights.GetLength(0)) return null; neighbour.x =cell.x+2; neighbour.y = cell.y;break;
			case 5: if (cell.x+1 >= heights.GetLength(0)) return null; neighbour.x = cell.x+1; neighbour.y = cell.y; break;
			}
		}
		else {
			switch (n) {
			case 0: if (cell.y+1 >= heights.GetLength(1) || cell.x -1 < 0) return null; neighbour.x = cell.x -1; neighbour.y = cell.y+1;break;
			case 1: if (cell.x - 2 < 0) return null; neighbour.x = cell.x -2; neighbour.y = cell.y; break;
			case 2: if (cell.x - 1 < 0) return null; neighbour.x = cell.x-1; neighbour.y = cell.y;break;
			case 3: if (cell.x+1 >= heights.GetLength(0)) return null; neighbour.x = cell.x+1; neighbour.y = cell.y;break;
			case 4: if (cell.x+2 >= heights.GetLength(0)) return null; neighbour.x = cell.x+2; neighbour.y = cell.y;break;
			case 5: if (cell.x+1 >= heights.GetLength(0) || cell.y+1 >= heights.GetLength(1)) return null; neighbour.x = cell.x+1; neighbour.y = cell.y+1; break;
			}
		}
		return neighbour;
	}

	Hex[] GetNeighbours (Hex h) {
		int x = h.x, y = h.y;
		Hex[] nbs = new Hex[6];
		int a = heights.GetLength(0), b= heights.GetLength(1);
		if (x - 1 >= 0) {
			if (x - 2 >= 0) nbs[1] = new Hex(x-2, y);
			if (x%2 == 0) {
				if ( y -1 >= 0) nbs[2] = new Hex(x - 1, y-1 ); 
				nbs[0] = new Hex( x-1, y);
			}
			else {
				nbs[2] = new Hex( x - 1, y); 
				if (y+1 < b) nbs[0] = new Hex(x-1, y+1);
			}
		}
		if (x + 1 < a) {
			if (x + 2 < a) nbs[4] = new Hex(x+2, y);
			if (x%2 == 0) {
				if ( y -1 >= 0) nbs[3] = new Hex (x+1, y-1);
				nbs[5] = new Hex(x+1, y);
			}
			else {
				nbs [3] = new Hex(x+1, y);
				if (y+1 < b) nbs [5] = new Hex(x+1, y+1);
			}
		}
		return nbs;
	}

	public Hex GetHex(int x, int y) {
		if (x<0 || y <0) return null;
		return hex[x,y];
	}
}
