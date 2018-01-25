using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMaster : MonoBehaviour {
	
	public ParticleSystem watersplash2_pref, dustsplash2_pref;
	public Texture Inventory16cells_tx, item_metal_tx, item_dragmetal_tx, item_plastic_tx, item_electronic_tx, item_chemicals_tx, item_person_tx,
	item_twoPersons_tx, item_manyPersons_tx, item_VIPperson_tx, item_default_tx;
	public Texture2D wave_tx;
	ParticleSystem watersplash2, dustsplash2;
	const int WATERSPLASH2_PARTICLES_COUNT = 70, DUSTSPLASH2_PARTICLES_COUNT = 70;
	const int WAVE_TX_RES = 512;
	public Material waterMaterial;
	public static PoolMaster mainPool;
	public GameObject city_housing_pref, city_mall_pref, city_offices_pref, city_industrial_pref;

	public Texture choosingFrame_tx;

	void Awake () {
		//singleton pattern
		if (mainPool != null)  Component.Destroy(mainPool);
		mainPool = this;
		//---
		watersplash2 = Instantiate (watersplash2_pref);
		dustsplash2 = Instantiate (dustsplash2_pref);
		GameMaster.SetGUIPiece (Screen.height / 36);

		wave_tx = new Texture2D(WAVE_TX_RES, WAVE_TX_RES);
		Color[] pixels = new Color[WAVE_TX_RES * WAVE_TX_RES];
		for (int i  = 0; i < WAVE_TX_RES; i++)
		{
			float xsin =  i; xsin /= WAVE_TX_RES; 
			float ysin = Mathf.Sin(xsin * 2 *Mathf.PI) * WAVE_TX_RES;
				for (int j = 0; j< WAVE_TX_RES; j++) {
				float k = (float)(j - ysin); k/= WAVE_TX_RES/2; if (k < 0) k*= -1;
				pixels[i * WAVE_TX_RES + j] = Color.Lerp(Color.white, Color.green,Mathf.Abs(Mathf.Sin(k *  Mathf.PI)));
			}
		}

		wave_tx.SetPixels(pixels);
		wave_tx.Apply();
		waterMaterial.SetTexture("_NoiseTex", wave_tx);

		choosingFrame_tx = Resources.Load<Texture>("Textures/GUI/choosingFrame_tx");
	}

	public void Watersplash2At (Vector3 pos) {
		watersplash2.transform.position = pos;
		watersplash2.Emit (WATERSPLASH2_PARTICLES_COUNT);
	}
	public void Dustsplash2At (Vector3 pos) {
		dustsplash2.transform.position = pos;
		dustsplash2.Emit (DUSTSPLASH2_PARTICLES_COUNT);
	}
}
