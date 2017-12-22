using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex  {
	public int x, y;
	public float h;
	GameObject chunk;
	public Biome biome;

	public Hex() {
		x = -1;
		y= -1;
		h = 0;
		chunk = null;
	}

	public Hex(int a, int b) {
		if (a < 0) a = 0;
		if (b < 0) b = 0;
		x = a;
		y= b;
		h=0;
		chunk = null;
	}
	public Hex(int a, int b, GameObject g) {
		if (a < 0) a = 0;
		if (b < 0) b = 0;
		x = a;
		y= b;
		h=0;
		chunk = g;
	}
	public Hex(int xpos, int ypos, float height,GameObject g, Biome b) {
		if (xpos < 0) xpos = 0;
		if (ypos < 0) ypos = 0;
		x = xpos;
		y= ypos;
		h = height;
		chunk = g;
		biome = b;
	}

	public void SetChunk (GameObject g) {chunk = g;}
	public GameObject GetChunk() {return chunk;}
	public Vector3 GetWorldPosition() {return chunk.transform.position;}
	public void SetWorldPosition(Vector3 pos) {chunk.transform.position = pos;}

	public void ApplyHeight() {
		if (chunk == null) return;
		chunk.transform.position = new Vector3(chunk.transform.position.x, h, chunk.transform.position.z);
	}
}

