using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BiomeType {empty, hills, city};

public class Biome {
	

	public BiomeType type;

	Biome () {type = BiomeType.empty;}

	Biome (BiomeType biomeType) {
		type = biomeType;
	}
}
