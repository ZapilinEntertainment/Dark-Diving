using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BiomeType {empty, hills};

public class Biome {
	

	public BiomeType type;

	public Biome () {type = BiomeType.empty;}

	public Biome (BiomeType biomeType) {
		type = biomeType;
	}
}
