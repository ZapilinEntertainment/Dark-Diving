using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	public float lowBorder = 45, upBorder = 0;
	const float ROTATION_SMOOTH_CF = 0.2f, SINK_SPEED = 9,SINKING_SMOOTH_CF = 1.1f, NATURAL_GRAVITY = 9.8f, NATURAL_WATER_MITIGATION = 3,
	UNDERWATER_GRAVITY = 15, CRITICAL_X_ANGLE = 30;
	//mitigation - смягчить (уменьшение гравитации при падении в воду)
	public float sinkSmooth = 0, rotationSmooth = 0;

	public float maxSpeed = 30, acceleration = 5, rotationSpeed = 5;
	float speed, gravity = 0, height, prevHeight, waterlevel = 0;

	float energy;
	public float energyCapacity = 500, lifeModuleConsumption = 0.001f, engineConsumption = 0.02f, maneuverConsumption = 0.01f;

	public GUISkin mainSkin; bool mainSkinSet = false;
	public Texture partsFrame_tx;
	public RaycastHit echoSounderRaycast, forwardRaycast;

	float hullPoints;
	public float maxHullPoints = 100;
	float bottomDistance;

	public Module[] modules;
	public CapsuleCollider mainCollider; float length;
	public GameObject marker;

	public Material myMaterial;
	float prevDepth;
	public Color underwaterColor;

	public Light projector1, projector2;

	int modulesCount = 6, modulesCapacity = 32;
	bool showInventory = false;
	int sh,sw;

	void Awake() {
		height = transform.position.y;
		prevHeight = height;
		hullPoints = maxHullPoints;
		energy = energyCapacity;
		length = mainCollider.height ;
		marker.transform.position = transform.TransformPoint(Vector3.forward * length / modules.Length );
		GameMaster.scenarist.SetPlayer (gameObject);
		modules = new Module[modulesCount];
		sw = Screen.width; sh= Screen.height; int k = GameMaster.GetGUIPiece();
		for (int i =0; i< modulesCount; i++)
		{
			modules[i] = gameObject.AddComponent<Module>();
			modules[i].number = i;
			modules[i].capacity = modulesCapacity;
			modules[i].SetRects(new Rect (0 + i * k, sh - k, k, k), new Rect(sw - 16 * k, 2*k + 4*i*k, 16*k, 4 *k));
		}
	}

	void Update () {
		if (GameMaster.isPaused()) return;
		if (Input.GetKeyDown("p")) {projector1.enabled = !projector1.enabled;projector2.enabled = projector1.enabled;}
		if (Input.GetKeyDown("i")) {
			showInventory = !showInventory;
			foreach (Module m in modules) {
				m.showOnGUI = showInventory;
			}
		}

		float t = Time.deltaTime;
		height = transform.position.y;
		waterlevel = GameMaster.WATERLEVEL;
		//наклон
		float a = Vector3.Angle (transform.forward, Vector3.up);
	
		var layerMask = 1 << 9;
		if ( Physics.Raycast(transform.position, Vector3.down, out echoSounderRaycast, Mathf.Infinity, layerMask )) { bottomDistance = height - echoSounderRaycast.point.y;}
		else bottomDistance = 1000;

		if (height <= waterlevel) {
			if (energy > 0) {
				float sinkStep = SINK_SPEED * sinkSmooth * t;
				if (Input.GetKey("q")) { 
					sinkSmooth += SINKING_SMOOTH_CF * t; 
					if (sinkSmooth > 1) sinkSmooth = 1;
					energy -= maneuverConsumption * t;
					if (Input.GetKey ("e")) {
						energy -= maneuverConsumption * t;
						if (bottomDistance > 0) transform.Translate (Vector3.down * SINK_SPEED * t, Space.World); 
						Differenting (t);
					}
				}
				else {
					if (Input.GetKey ("e")) { 
						energy -= maneuverConsumption * t;
						sinkSmooth -= SINKING_SMOOTH_CF * t; 
						if (sinkSmooth < -1) sinkSmooth = -1;
					}
					else {	if (transform.localRotation.x != 0) Differenting( t ); }
				}

				if (a > 179 && sinkSmooth > 0) sinkSmooth=0;
				else { if (a < 45 && sinkSmooth < 0) sinkSmooth = 0;}
				if (sinkSmooth != 0) transform.Rotate (Vector3.right * sinkStep, Space.Self);


			//Движение и поворот
			if (Input.GetKey( "w")) {if (speed + acceleration * t < maxSpeed) speed += acceleration * t;}
				else {
					if (Input.GetKey( "s") ) {if (speed - acceleration * t > -10) speed -= acceleration * t;}
					else {
						//if (!speedIsFixed) speed = Mathf.SmoothDamp(speed, 0, ref speed, speed/acceleration);
					}
				}	

			if (Input.GetKey ("d")) {
					if (rotationSmooth < 1) {
						rotationSmooth += ROTATION_SMOOTH_CF * t;
						energy -= maneuverConsumption * t;
					}
				}
			else {
				if (Input.GetKey ("a")) {
						if (rotationSmooth > -1) {
							rotationSmooth -= ROTATION_SMOOTH_CF * t;
							energy -= maneuverConsumption * t;
						}
					}
				else rotationSmooth = Mathf.SmoothDamp(rotationSmooth, 0,  ref rotationSmooth, t * 3);
			}
			if (rotationSmooth != 0) transform.Rotate (Vector3.up * rotationSmooth * t * rotationSpeed * speed / maxSpeed, Space.World);

				gravity = Mathf.SmoothDamp(gravity, 0, ref gravity, t);
				if (height > upBorder) transform.Translate (Vector3.down * 10 * t, Space.World);
				else { if (height < lowBorder) transform.Translate (Vector3.up * 5 * t, Space.World);}

				float trz = transform.localRotation.eulerAngles.z;
				if (trz!= 0) {
					if (trz > 180) transform.Rotate (Vector3.forward * rotationSpeed * t , Space.Self);
					else transform.Rotate (Vector3.back * rotationSpeed * t, Space.Self);
					if (trz < rotationSpeed * t || 360 - trz < rotationSpeed * t) {
						Vector3 nrot = transform.rotation.eulerAngles;
						nrot.z = 0;
						transform.rotation = Quaternion.Euler(nrot);
					}
				}

				if (Input.GetKey(KeyCode.Space) && height < upBorder) {
					transform.Translate(Vector3.up * 3 *t , Space.World);
					energy -= 2 * maneuverConsumption * t;
				}
				if (gravity > 0) {gravity -= 3 *t; if (gravity < 0) gravity = 0;}
			}
			else { // OUT OF ENERGY
				if (gravity != 0) {
					if (gravity < UNDERWATER_GRAVITY) {gravity += NATURAL_GRAVITY * t; if (gravity > UNDERWATER_GRAVITY) gravity = UNDERWATER_GRAVITY;}
					else { 
						if (gravity > UNDERWATER_GRAVITY) { gravity -= NATURAL_WATER_MITIGATION * t ; if (gravity < UNDERWATER_GRAVITY) gravity = UNDERWATER_GRAVITY;}
					}
				}
				float trx = transform.localRotation.eulerAngles.x;
				if (bottomDistance > 0 && (trx > CRITICAL_X_ANGLE && trx < 360 - CRITICAL_X_ANGLE)) {
					if (trx < 45) {transform.Translate(Vector3.forward * 5 * t, Space.Self); gravity += 0.01f;}
					else {
						if (trx > 270) {transform.Translate(Vector3.back * 5 * t, Space.Self); gravity += 0.01f;}
							}
				}
				if (speed > 0) {speed -= NATURAL_WATER_MITIGATION * t; if (speed < 0) speed = 0;}
			}
		}
		else {
			if (bottomDistance > 0) {
				gravity += NATURAL_GRAVITY* t;
				if (speed > 0) {speed -= 10 * t; }
				if (speed < 0) speed = 0;
			}
		}
			
			if (speed != 0 ) {
			bool blocked = false;
			if (Physics.Raycast(transform.position, transform.forward, out forwardRaycast, speed * t)) {
				if (forwardRaycast.collider.gameObject.layer == 9) blocked = true;
			}
			if (blocked ==false) transform.Translate(Vector3.forward * speed * t, Space.Self);
			if (bottomDistance < 1.2f) GameMaster.pool.Dustsplash2At(transform.position);
		}

		if (gravity != 0) {
			transform.rotation = Quaternion.RotateTowards( transform.rotation, Quaternion.LookRotation(Vector3.down), NATURAL_GRAVITY * t / 2);
			if (bottomDistance >= gravity * t) transform.Translate (Vector3.down * gravity * t, Space.World);
			else {
				gravity = 0;
				transform.position = echoSounderRaycast.point;
				GameMaster.pool.Dustsplash2At(transform.position);
			}
		}

		waterlevel = GameMaster.WATERLEVEL;
		height = transform.position.y;
		if ((prevHeight > waterlevel && height < waterlevel || prevHeight < waterlevel && height > waterlevel) && Mathf.Abs (height - prevHeight) > 0.02f) GameMaster.pool.Watersplash2At(transform.position);
		prevHeight = height;

		if (energy > 0) {energy -= (lifeModuleConsumption + engineConsumption * Mathf.Abs(speed)) * t; if (energy < 0) energy = 0;}
	}

	void Differenting (float t) {
		Vector3 nrot = transform.rotation.eulerAngles;
		if (Mathf.Abs (nrot.x) <= t || Mathf.Abs(nrot.x - 360) <= t) {
			nrot.x = 0;
			transform.rotation = Quaternion.Euler(nrot);
			sinkSmooth = 0;
		}
		else {
			float difSpeed = SINK_SPEED * t /2;
			if (Mathf.Abs(height - upBorder) > 1) difSpeed/=4;
			if (transform.localRotation.eulerAngles.x < 90) {sinkSmooth -= difSpeed; if (sinkSmooth < 0) sinkSmooth += difSpeed* 0.9f;}
			else {sinkSmooth += difSpeed; if (sinkSmooth >0) sinkSmooth-= difSpeed * 0.9f;}
		}
	}

	void ApplyDamage(Vector4 dmg) {
		hullPoints -= dmg.w;
		if (hullPoints < 0) Destruction();
		float compartmentLength = length / modules.Length; //длина отсека
		Vector3 inpoint = transform.InverseTransformPoint(new Vector3(dmg.x, dmg.y, dmg.z)) - mainCollider.center;
		int compartmentNumber = (int) (inpoint.z / compartmentLength);
		//print (compartmentNumber);
		if (inpoint.z > 0) {compartmentNumber = modules.Length/2 - compartmentNumber - 1;}
		else {compartmentNumber*= -1; compartmentNumber += modules.Length/2;}
		//print ("Compartment "+compartmentNumber.ToString()+ " hit!");
	}

	void Destruction () {
		
	}

	void OnGUI () {
		GUILayout.Label((Mathf.Floor(speed * 100) / 100).ToString());
		GUILayout.Label(bottomDistance.ToString()+ "u");
		GUILayout.Label(GameMaster.cam.transform.rotation.eulerAngles.x.ToString());

		if (!mainSkinSet) {GUI.skin.font = mainSkin.font;}
		int k = GameMaster.GetGUIPiece();
		Rect rightPanelRect = new Rect (0, sh - k, 6*k, k);
		GUI.DrawTexture (rightPanelRect, partsFrame_tx, ScaleMode.StretchToFill); rightPanelRect.y -= k/2; rightPanelRect.height = k/2;
		GUI.Label (rightPanelRect, "Прочность корпуса: " + Mathf.FloorToInt(hullPoints/maxHullPoints * 100).ToString() + '%');
		rightPanelRect.y -= k/2;
		GUI.Label (rightPanelRect, "Энергия : " + (Mathf.FloorToInt(energy/energyCapacity * 10000) / 100.00f ).ToString() + '%');
	}


}
