using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour {
		public Shader m_Shader = null;
	public float pc; // Процент погружения, 0 - на поверхности, 1 - там. куда не достаёт свет
	const float MAX_DEEP = -2000, SUNLIGHT_LIMIT = 0.1f;
	float prevHeight, colouringCooldown = 1, t;
		private Material m_Material;
	public Light dayLight;
	public Color normalSeaColor, mySeaColor;
	public Material skyboxMaterial;
	Color prevColor; 

		void Start()
		{  
			if (m_Shader)
			{
				m_Material = new Material(m_Shader);
				m_Material.name = "ImageEffectMaterial";
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.SetColor("_MyColor", mySeaColor);
			}

			else
			{
				Debug.LogWarning(gameObject.name + ": Shader is not assigned. Disabling image effect.", this.gameObject);
				enabled = false;
			}
		SetColors();
		}

	void Update () {
		if (t > 0 ) {t -= Time.deltaTime;}
	}

		void OnRenderImage(RenderTexture src, RenderTexture dst)
		{		
		if (transform.position.y != prevHeight) {
			SetColors();
		}

			if (m_Shader && m_Material)
			{
				Graphics.Blit(src, dst, m_Material);
			}
			else
			{
				Graphics.Blit(src, dst);
				Debug.LogWarning(gameObject.name + ": Shader is not assigned. Disabling image effect.", this.gameObject);
				enabled = false;
			}
		}

		void OnDisable()
		{
			if (m_Material)
			{
				DestroyImmediate(m_Material);
			}
		}

	void SetColors() {
		float waterline = GameMaster.GetWaterlevel();
		prevHeight = transform.position.y;
		if (prevHeight >= waterline) pc = 0;
		else {
			if (prevHeight <= MAX_DEEP) pc = 1;
			else pc = prevHeight / MAX_DEEP;
		}
		//Общее затенение
		m_Material.SetFloat("_DeepPercent", pc);
		// Global Illumination
		Color environmentColor;
		if (transform.position.y < waterline ) environmentColor = Color.Lerp(normalSeaColor, Color.black, pc);
		else environmentColor = new Color (0.9f,0.9f,0.8f,0.1f);
		if (environmentColor != prevColor) {
			RenderSettings.ambientSkyColor= environmentColor;
			skyboxMaterial.color = environmentColor;
			prevColor = environmentColor;
		}
		RenderSettings.ambientIntensity=Mathf.Sin(pc * Mathf.PI);
		RenderSettings.reflectionIntensity=(1-pc)/2;
		//Солнечный свет
		if (pc < SUNLIGHT_LIMIT) {
			if (!dayLight.enabled) dayLight.enabled = true;
			dayLight.intensity = (1 - pc/SUNLIGHT_LIMIT) *0.5f;
		}
		else {if (dayLight.enabled) {dayLight.intensity = 0;dayLight.enabled = false;}}

	}
}
