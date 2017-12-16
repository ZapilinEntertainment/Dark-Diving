using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour {
		public Shader m_Shader = null;
	public float pc;
		const float MAX_DEEP = -1000;
		float prevHeight;
		private Material m_Material;


		void Start()
		{  
			if (m_Shader)
			{
				m_Material = new Material(m_Shader);
				m_Material.name = "ImageEffectMaterial";
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			}

			else
			{
				Debug.LogWarning(gameObject.name + ": Shader is not assigned. Disabling image effect.", this.gameObject);
				enabled = false;
			}
		}

		void OnRenderImage(RenderTexture src, RenderTexture dst)
		{
		
		if (transform.position.y != prevHeight) {
			prevHeight = transform.position.y;
			pc = 1 - prevHeight / MAX_DEEP;
			if (pc < 0) pc = 0; 
			else {if (pc > 1) pc = 1;}
			m_Material.SetFloat("_DeepPercent", pc);
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
