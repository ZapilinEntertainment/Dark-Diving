using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour {
		public Shader m_Shader = null;
	public float pc; // Процент погружения, 0 - на поверхности, 1 - там. куда не достаёт свет
	const float SUNLIGHT_DEPTH = -200, SUNLIGHT_INTENSITY = 0.3f;
	float prevHeight;
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
		prevHeight = transform.position.y;
		}
		

		void OnRenderImage(RenderTexture src, RenderTexture dst)
		{		
		if (transform.position.y != prevHeight) {
			prevHeight = transform.position.y;

			if (transform.position.y >= 0) pc = 0;
			else {
				pc = transform.position.y / GameMaster.LIGHT_DEPTH_LIMIT;
				if (pc > 1) pc = 1;
			}
			m_Material.SetFloat("_DeepPercent", pc);
			if (prevHeight > SUNLIGHT_DEPTH) { 
				if (dayLight.enabled == false) dayLight.enabled = true;
				float i = prevHeight / SUNLIGHT_DEPTH;
				if (i <= 0) i = SUNLIGHT_INTENSITY;
				else { if (i >= 1) i = 0; else i = 1 - i;}
				if (dayLight.intensity != i) dayLight.intensity = i;
			}
			else {if (dayLight.enabled) dayLight.enabled = false;}
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


}
