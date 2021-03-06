﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour {
		public Shader m_Shader = null;
	public float pc; // Процент погружения, 0 - на поверхности, 1 - там. куда не достаёт свет
	const float SUNLIGHT_DEPTH = -1000, SUNLIGHT_INTENSITY = 1, DELTA_LIMIT = 0.1f;
	float prevHeight;
		private Material m_Material;
	public Light dayLight;
	public Color normalSeaColor, mySeaColor;
	public Material skyboxMaterial, bottomMaterial;
	float surfaceHeight;


	void Awake () {
		surfaceHeight = 0;
	}

		void Start()
		{  
			if (m_Shader)
			{
				m_Material = new Material(m_Shader);
				m_Material.name = "ImageEffectMaterial";
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.SetColor("_MyColor", mySeaColor);
			if (transform.position.y >= 0) pc = 0;
			else {
				pc = transform.position.y / GameMaster.LIGHT_DEPTH_LIMIT;
				if (pc > 1) pc = 1;
				}
				m_Material.SetFloat("_DeepPercent", pc);
			SetBackgroundColor();
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

		float surfaceDist = 0;
		var layerMask = 1<<4;
		RaycastHit rh;
		if (Physics.Raycast (transform.position + Vector3.up * 1000,Vector3.down, out rh, Mathf.Infinity, layerMask )) {
			surfaceHeight = rh.point.y;
		}
		surfaceDist = transform.position.y - surfaceHeight;

		if (surfaceDist > 0) pc = 0;
		else {
			pc = surfaceDist / GameMaster.LIGHT_DEPTH_LIMIT  ;
			if (pc > 1) pc =1;
		}
		m_Material.SetFloat("_DeepPercent", pc);
		//pc = 1 - полная темнота

			


		if (Mathf.Abs(transform.position.y - prevHeight) >= DELTA_LIMIT) {
			prevHeight = transform.position.y;
			if (prevHeight > SUNLIGHT_DEPTH) { 
				if (dayLight.enabled == false) dayLight.enabled = true;
				float i = prevHeight / SUNLIGHT_DEPTH;
				if (i <= 0) i = SUNLIGHT_INTENSITY;
				else { if (i >= 1) i = 0; else i = 1 - i;}
				if (i > SUNLIGHT_INTENSITY) i = SUNLIGHT_INTENSITY;
				if (dayLight.intensity != i) dayLight.intensity = i;
			}
			else {if (dayLight.enabled) dayLight.enabled = false;}

			SetBackgroundColor();
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
		
	void SetBackgroundColor () {
		Color newColor = Color.white;
		if (pc > 0) newColor = Color.Lerp(mySeaColor, Color.black, pc);
		//gameObject.GetComponent<Camera>().backgroundColor = newColor;
		skyboxMaterial.color = Color.Lerp(Color.white, Color.black, pc);
		//bottomMaterial.SetColor("_MyColor",newColor);
	}

		void OnDisable()
		{
			if (m_Material)
			{
				DestroyImmediate(m_Material);
			}
		}
		

		


}
