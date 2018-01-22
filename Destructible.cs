using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour {
	public float hp, maxHp = 1, armor_cf = 1;

	void Awake () {
		if (hp == 0) hp = maxHp;
	}
}
