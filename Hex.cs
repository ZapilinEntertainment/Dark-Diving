using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex  {
	public int x, y;
	public GameObject chunk;

	public Hex() {
		x = -1;
		y= -1;
		chunk = null;
	}

	public Hex(int a, int b) {
		if (a < 0) a = 0;
		if (b < 0) b = 0;
		x = a;
		y= b;
		chunk = null;
	}
	public Hex(int a, int b, GameObject g) {
		if (a < 0) a = 0;
		if (b < 0) b = 0;
		x = a;
		y= b;
		chunk = g;
	}

	public void SetChunk (GameObject g) {chunk = g;}
	public GameObject GetChunk() {return chunk;}
	public Vector3 GetWorldPosition() {return chunk.transform.position;}
	public void SetWorldPosition(Vector3 pos) {chunk.transform.position = pos;}
}

