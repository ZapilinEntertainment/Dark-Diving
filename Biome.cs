using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BiomeType {empty, city, HighPlains, Mountains, Hills, Plains, CoastalArea, Shelf, ContinentalSlope, 
	ContinentalFoot, AbyssalPlain, OceanRidge, OceanGutter
};

public class Biome {
	

	public BiomeType type;

	Biome () {type = BiomeType.empty;}

	Biome (BiomeType biomeType) {
		type = biomeType;
	}
}
