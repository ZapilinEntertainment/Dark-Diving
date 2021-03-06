﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	const float ROTATION_SMOOTH_CF = 0.2f, SINK_SPEED = 9,SINKING_SMOOTH_CF = 1.1f, NATURAL_GRAVITY = 9.8f, NATURAL_WATER_MITIGATION = 3,
	CRITICAL_X_ANGLE = 30, FORCE_CONSTANCE_TIMER = 0.04f, SCREW_ROTATION_SPEED = 100;
	//mitigation - смягчить (уменьшение гравитации при падении в воду)
	public float sinkSmooth = 0, rotationSmooth = 0;

	public float maxSpeed = 30, acceleration = 5, rotationSpeed = 5;
	float speed, height, prevHeight, waterlevel = 0;

	float energy;
	public float energyCapacity = 500, lifeModuleConsumption = 0.001f, engineConsumption = 0.02f, pumpConsumption = 0.01f;

	public GUISkin mainSkin; bool mainSkinSet = false;
	public Texture partsFrame_tx;
	public RaycastHit echoSounderRaycast, forwardRaycast;

	float hullPoints;
	public float maxHullPoints = 100;
	float bottomDistance;

	Module[] modules;
	public CapsuleCollider mainCollider; 
	public GameObject marker;

	int modulesCount = 6;
	bool showInventory = false;
	int sh,sw;

	public float mass = 100, ballastMass = 0, maxBallastMass = 100, volume = 1, pumpInSpeed= 10, pumpOutSpeed = 10; 
	public float draft = 4, length = 1, width = 8; //осадка, длина, ширина

	Vector3 moveVector;
	public float yForce = 0, realForce = 0, forceChangingSpeed = 10;
	float flyTime = 0;
	public float ballastCompression = 10;
	public float physicDepth = 0;
	public Transform sea;
	public Vector3 seaCorrectionVector = new Vector3(-500,0,-500);
	float forceTimer = 0;

	public float shortRangeScanner = 300, shortScannerCost = 10;

	public CargoDrone[] transportDrones;
	public Vector3 droneHangarPos = new Vector3(0,0, - 15), outerHatch = new Vector3(0,0,40);
	List <ResourcesBox> plannedLootPoints;

	public Transform leftFin, rightFin,fin, leftScrew, rightScrew;
	public ParticleSystem leftScrewPS, rightScrewPS;
	bool speedBlocked = false;
	float prevSpeed;

	Texture batteryFrame_tx, batteryInnerParts_tx, energyWarning_tx,energyCriticalWarning_tx;

	public static PlayerController player;

	void Awake() {
		//singleton pattern
		if (player != null) Destroy(player);
		player = this;

		batteryFrame_tx = Resources.Load<Texture>("Textures/GUI/batteryFrame_tx"); batteryInnerParts_tx = Resources.Load<Texture>("Textures/GUI/batteryInnerParts_tx");
		energyWarning_tx = Resources.Load<Texture>("Textures/GUI/energyWarning_tx"); energyCriticalWarning_tx = Resources.Load<Texture>("Textures/GUI/energyCriticalWarning_tx");

		height = transform.position.y;
		prevHeight = height;
		hullPoints = maxHullPoints;
		energy = energyCapacity ;
		length = mainCollider.height ;
		volume = draft * 2 * length * width;
		//marker.transform.position = transform.TransformPoint(Vector3.forward * length / modules.Length );
		modules = new Module[modulesCount];
		sw = Screen.width; sh= Screen.height; float k = GameMaster.GetGUIPiece();
		Module.MODULE_INFO_RECT = new Rect(sw - 10 *k, sh - 10*k, 10*k, 10*k);
		for (int i =0; i< modulesCount; i++)
		{
			modules[i] = gameObject.AddComponent<Module>();
			modules[i].ModuleSet(i, Module.SMALL_MODULE_CAPACITY, ModuleType.Empty,this);
			modules[i].SetRects(new Rect (0 + i * k, sh - k, k, k), new Rect(sw - 16 * k, 2*k + 4*i*k, 16*k, 4 *k));
		}
			
		modules[0].AddItem(Item.item_metal);
		modules[0].AddItem(Item.item_dragmetal);
		modules[0].AddItem(Item.item_chemicals);
		modules[0].AddItem(Item.item_electronic);
		modules[0].AddItem(Item.item_plastic);
		modules[0].AddItem(Item.item_people);
		modules[0].AddItem(Item.item_accumulator_full);

		transportDrones = new CargoDrone[6];
		GameObject dronePref = Resources.Load<GameObject>("cargoDrone_pref");
		for (int i =0; i< 6; i++) {
			transportDrones[i] = GameObject.Instantiate(dronePref, droneHangarPos, transform.rotation).GetComponent<CargoDrone>();
			transportDrones[i].depot = this;
			transportDrones[i].name = "cargoDrone"+i.ToString();
			transportDrones[i].gameObject.SetActive(false);
		}
		plannedLootPoints = new List<ResourcesBox>();

		UI myUI = gameObject.AddComponent<UI>();
		myUI.UI_camera = GameObject.Find("UI_camera").transform;
	}

	void Update () {


		//testzone
		//if (Input.GetKeyDown("g")) modules[0].AddItem(Item.item_people);
		//end of testzone
		GameMaster.cursorPosition = new Vector2(Input.mousePosition.x, sh - Input.mousePosition.y);
		if (GameMaster.isPaused()) return;

		moveVector =Vector3.zero;
		int directionVectorsCount = 0;

		if (Input.GetKeyDown("i")) {
			showInventory = !showInventory;
			foreach (Module m in modules) {
				m.showOnGUI = showInventory;
			}
		}

		float t = Time.deltaTime;
		height = transform.position.y;
		RaycastHit waterRaycaster;
		var waterLayerMask = 1<<4 ; //water layer
		Physics.Raycast (transform.position + Vector3.up * 1000,Vector3.down, out waterRaycaster, Mathf.Infinity, waterLayerMask );
		waterlevel = waterRaycaster.point.y;
		//наклон
		float a = Vector3.Angle (transform.forward, Vector3.up);
	
		var bottomLayerMask = 1 << 9;
		if ( Physics.Raycast(transform.position, Vector3.down, out echoSounderRaycast, Mathf.Infinity, bottomLayerMask )) { bottomDistance = height - echoSounderRaycast.point.y;}
		else bottomDistance = 1000;

		float pressure = (waterlevel - transform.position.y ) /10f + 1;
		if (height <= waterlevel+draft) {  // Зона, где возможно управление кораблем -поверхность и под водой
			flyTime = 0;
			if (energy > 0) {
				if (Input.GetKey("q")) { 
					sinkSmooth += SINKING_SMOOTH_CF * t; 

					if (Input.GetKey ("e")) {
						if (ballastMass < maxBallastMass) {
							ballastMass += pumpInSpeed *(1 + pressure/10);
							energy -= pumpConsumption * t;
						}
						Differenting (t);
					}
				}
				else {
					if (Input.GetKey ("e")) 	sinkSmooth -= SINKING_SMOOTH_CF * t; 
					else {	if (transform.localRotation.x != 0) Differenting( t ); }
				}
				if (ballastMass > maxBallastMass) ballastMass = maxBallastMass;
				else if (ballastMass < 0) ballastMass = 0;


			//Движение и поворот
				if (!speedBlocked) {
					if (Input.GetKey( "w")) {
							if (speed + acceleration * t < maxSpeed) {
								bool prevSpeedWasNegative = speed < 0;
								speed += acceleration * t;
							if (speed > 0 && prevSpeedWasNegative) {speed = 0;speedBlocked = true;}
							}
						}
						else {
							if (Input.GetKey( "s") ) {
								if (speed - acceleration * t > -10) {
									bool prevSpeedWasPositive = speed > 0;
									speed -= acceleration * t;
								if (speed < 0 && prevSpeedWasPositive) {speed = 0; speedBlocked = true;}
								}
							}
							else {
								//if (!speedIsFixed) speed = Mathf.SmoothDamp(speed, 0, ref speed, speed/acceleration);
							}
						}	
				}
				else {
					if (Input.GetKeyUp ("w") || Input.GetKeyUp("s")) speedBlocked = false;
				}

			if (Input.GetKey ("d")) {
					if (rotationSmooth < 1) {
						rotationSmooth += ROTATION_SMOOTH_CF * t;
					}
				}
			else {
				if (Input.GetKey ("a")) {
						if (rotationSmooth > -1) {
							rotationSmooth -= ROTATION_SMOOTH_CF * t;
						}
					}
				else rotationSmooth = Mathf.SmoothDamp(rotationSmooth, 0,  ref rotationSmooth, t * 3);
			}
			if (rotationSmooth != 0) transform.Rotate (Vector3.up * rotationSmooth * t * rotationSpeed * speed / maxSpeed, Space.World);
				fin.localRotation = Quaternion.Euler(Vector3.down * rotationSmooth * 30);
				//STABILIZE  Z-AXIS
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
			}
			else { // OUT OF ENERGY
				float trx = transform.localRotation.eulerAngles.x;
				if (bottomDistance > 0 && (trx > CRITICAL_X_ANGLE && trx < 360 - CRITICAL_X_ANGLE)) {
					if (trx < 45) {moveVector += transform.TransformDirection(Vector3.forward * 5 * t); directionVectorsCount++; yForce -= 0.01f;}
					else {
						if (trx > 270) {moveVector += transform.TransformDirection(Vector3.back * 5 * t); directionVectorsCount++; yForce -= 0.01f;}
							}
				}
				if (speed > 0) {speed -= NATURAL_WATER_MITIGATION * t; if (speed < 0) speed = 0;}
			}

			float waterlevelDist = Mathf.Abs(transform.position.y - waterlevel);
			if (waterlevelDist < GameMaster.SURFACE_EFFECT_DEPTH) {
				float k = 1 - waterlevelDist /  GameMaster.SURFACE_EFFECT_DEPTH;
				float waterlevel_fwd = waterlevel, waterlevel_aft = waterlevel;
				Vector3 fwd_point = transform.position + transform.forward * length / 2;
				Vector3 aft_point = transform.position - transform.forward * length /2;
				if (Physics.Raycast(fwd_point + Vector3.up * 1000, Vector3.down, out waterRaycaster, Mathf.Infinity, waterLayerMask)) {
					waterlevel_fwd = waterRaycaster.point.y;
				}
				if (Physics.Raycast(aft_point + Vector3.up * 1000, Vector3.down, out waterRaycaster, Mathf.Infinity, waterLayerMask)) {
					waterlevel_aft = waterRaycaster.point.y;
				}
		
				if ((waterlevel_fwd - fwd_point.y) < 0 && (waterlevel_aft - aft_point.y) > 0) {
					sinkSmooth += SINKING_SMOOTH_CF * t * k *k * 2; 
				}
				else {
					if ((waterlevel_fwd - fwd_point.y) > 0 && (waterlevel_aft - aft_point.y) < 0) {
						sinkSmooth -= SINKING_SMOOTH_CF * t * k *k * 2; 
					}
				}

			}
		}
		else { 
			flyTime += t; pressure = 0;
			sinkSmooth += SINKING_SMOOTH_CF * t; 
		}

		if (Input.GetKey(KeyCode.Space)) {
			float k = 1, k2 = 1;
			if (height > waterlevel ) {if (height - waterlevel > draft) {k = 100; k2 = 0;} else k = 10;}
			if (energy > 0 && ballastMass > 0) {
				ballastMass -= 2 * pumpOutSpeed * k* Time.deltaTime;
				energy -= 2 * pumpConsumption * k2 * Time.deltaTime;
			}
		}


		if (forceTimer > 0) forceTimer-= Time.deltaTime;
		else {
			float prevForce = yForce;
			if (transform.position.y < waterlevel) {
				float k = ( 1 - transform.position.y - (waterlevel - draft)) / (2 * draft);
				if (k < 0) k *= (-1);
				float surface_cf = 1;
				if (waterlevel - transform.position.y < draft) {
					surface_cf = GameMaster.seaStrength;
				}
				yForce = pressure * volume *surface_cf   - (mass + ballastMass * ballastCompression);
				yForce *= NATURAL_GRAVITY;
				if (yForce < 0) yForce += yForce * 10 * Mathf.Abs((Mathf.Pow((1 - Vector3.Angle(transform.forward, Vector3.down) / 90.0f), 3)));
			}
			else {
				yForce = -(mass + ballastMass * ballastCompression) * NATURAL_GRAVITY * (1 +flyTime);
				if (transform.position.y - waterlevel >draft) yForce /= GameMaster.seaStrength;
			}

			yForce /= (mass + ballastMass * ballastCompression);
			physicDepth = (mass + ballastMass * ballastCompression) / volume - 1;
			physicDepth *= -10;

			if (prevForce != realForce)	{
				forceTimer = FORCE_CONSTANCE_TIMER;
			}
		}
			if (realForce< yForce ) {
			if (waterlevel - transform.position.y > draft ) realForce += (forceChangingSpeed+ pressure) * Time.deltaTime; 
			if (realForce > yForce) realForce = yForce;
		}
			else if (realForce > yForce) {
			if (transform.position.y  - waterlevel > draft) {
				realForce -= (forceChangingSpeed+ NATURAL_GRAVITY *flyTime*mass * 0.5f) * Time.deltaTime; 
				sinkSmooth += SINKING_SMOOTH_CF * t; 
			}
			else realForce -= forceChangingSpeed * Time.deltaTime; 
			if (realForce < yForce) realForce = yForce;}

			transform.Translate (Vector3.up * realForce * Time.deltaTime,Space.World);
		if (sinkSmooth > 1) sinkSmooth = 1;
		else if (sinkSmooth < -1) sinkSmooth = -1;
			if (a > 179 && sinkSmooth > 0) sinkSmooth=0;
			else { if (a < 45 && sinkSmooth < 0) sinkSmooth = 0;}
			if (sinkSmooth != 0) transform.Rotate (Vector3.right * sinkSmooth *SINK_SPEED * t, Space.Self);
		leftFin.localRotation = Quaternion.Euler(Vector3.back* sinkSmooth* 30);
		rightFin.localRotation = Quaternion.Euler(Vector3.forward* sinkSmooth * 30);
			
		height = transform.position.y;

		bool jumpDown = prevHeight > waterlevel && height < waterlevel ;
		bool jumpUp = prevHeight < waterlevel && height > waterlevel;
		if (jumpUp || jumpDown) {realForce *= 0.5f;}
		prevHeight = height;

		if (energy > 0) {energy -= (lifeModuleConsumption + engineConsumption * Mathf.Abs(speed)) * t; if (energy < 0) energy = 0;}


		if (speed != 0 ) {
			bool blocked = false;
			if (Physics.Raycast(transform.position, transform.forward, out forwardRaycast, speed * t)) {
				if (forwardRaycast.collider.gameObject.layer == 9) blocked = true;
			}
			if (blocked ==false) transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
			if (bottomDistance < 1.2f) PoolMaster.mainPool.Dustsplash2At(transform.position);
			leftScrew.Rotate(Vector3.forward * speed *SCREW_ROTATION_SPEED * t);
			rightScrew.Rotate(Vector3.forward * speed *SCREW_ROTATION_SPEED * t);

			if (leftScrew.position.y > waterlevel) {if (leftScrewPS.isPlaying) leftScrewPS.Stop();} else {if (!leftScrewPS.isPlaying) leftScrewPS.Play();}
			if (rightScrew.position.y > waterlevel) {if (rightScrewPS.isPlaying) rightScrewPS.Stop();} else {if (!rightScrewPS.isPlaying) rightScrewPS.Play();}
			if (prevSpeed != speed) {
				float screw_effects_cf = speed / maxSpeed;
				var main = leftScrewPS.main;
				main.startLifetimeMultiplier = 0.5f + screw_effects_cf;
				//main.startSpeedMultiplier = 0.3f;
				main.startSize = new ParticleSystem.MinMaxCurve(1, 4 + screw_effects_cf);
				main = rightScrewPS.main;
				main.startLifetimeMultiplier = 0.5f + screw_effects_cf;
				//main.startSpeedMultiplier = 0.3f;
				main.startSize = new ParticleSystem.MinMaxCurve(1, 4 + screw_effects_cf);
				var emissionModule = leftScrewPS.emission;
				emissionModule.rateOverTimeMultiplier = 5 + screw_effects_cf * 45;
				emissionModule = rightScrewPS.emission;
				emissionModule.rateOverTimeMultiplier = 5 + screw_effects_cf * 45;
			}
		}
		else {
			if (leftScrewPS.isPlaying) leftScrewPS.Stop();
			if (rightScrewPS.isPlaying) rightScrewPS.Stop();
		}

		if (directionVectorsCount != 0)	transform.position += moveVector / directionVectorsCount;
		sea.position = new Vector3(transform.position.x, 0, transform.position.z) + seaCorrectionVector;
		prevSpeed =speed; 
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
			if (Mathf.Abs(height - waterlevel) > 1) difSpeed/=4;
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

	public Module GetModule(int index) {
		if (index < 0 || index > modules.Length) return null;
		return modules[index];
	}

	public bool LoadItemFromDrone (Item i) {
		foreach (Module m in modules) {
			if (m.AddItem(i) == true) return true;
		}
		return false;
	}
		

	public void AddLootPoint (ResourcesBox source) {
		if (source == null || source.extractionBitmask == 0 || transportDrones.Length == 0) return;
		if (plannedLootPoints.Count > 0) { foreach (ResourcesBox rb in plannedLootPoints) {if (rb == source) return;}}
		plannedLootPoints.Add(source);
		source.isActiveLootPoint = true;
		if (source.workingDrones > 0 ) {
		int activeDronesCount = 0;
		foreach (CargoDrone c in transportDrones) {if (c.gameObject.activeSelf) activeDronesCount++;}
			if (activeDronesCount < transportDrones.Length){
				int newActiveDronesCount = activeDronesCount + source.workingDrones;
				if (newActiveDronesCount > transportDrones.Length) {
					newActiveDronesCount = transportDrones.Length;
					foreach (CargoDrone c in transportDrones) {
						if ( !c.gameObject.activeSelf ) {
							c.transform.position = transform.TransformPoint(droneHangarPos);
							c.gameObject.SetActive(true); 
							c.timer = 0;
							c.target = plannedLootPoints[plannedLootPoints.Count-1];
						}
					}
				}
				else {
					activeDronesCount = source.workingDrones;
					foreach( CargoDrone c in transportDrones) {
						if ( !c.gameObject.activeSelf ) { 
							c.transform.position = transform.TransformPoint(droneHangarPos);
							c.gameObject.SetActive(true);
							c.timer = 0;activeDronesCount --;
							c.target = plannedLootPoints[plannedLootPoints.Count-1];
							if (activeDronesCount == 0) break;}
						}
				}
			}
			}
	}
	public ResourcesBox GetLootPoint () {
		if (plannedLootPoints == null || plannedLootPoints.Count == 0 ) return null;
		ResourcesBox answer = null;
		for (int i =0; i< plannedLootPoints.Count;i++) {
			if (plannedLootPoints[i].BitmasksConjuction() == 0) {plannedLootPoints[i].isActiveLootPoint = false; plannedLootPoints.RemoveAt(i);}
			else {answer = plannedLootPoints[i]; break;}
		}
		return answer;
	}
	public void RemoveLootPoint(ResourcesBox rb) {
		if (rb == null || plannedLootPoints == null || plannedLootPoints.Count == 0) return;
		for (int i =0; i < plannedLootPoints.Count; i++) {
			if (plannedLootPoints[i] == rb) {plannedLootPoints[i].isActiveLootPoint = false; plannedLootPoints.RemoveAt(i);}
		}
	}

	public void SendDroneToResBox (ResourcesBox rbox) {
		if (rbox == null) return;
		CargoDrone drone = null;
		foreach (CargoDrone c in transportDrones) {
			if (c.gameObject.activeSelf ==false || c.target == null) {
				drone = c; break;
			}
		}
		if ( drone == null) {drone = transportDrones[(int)Random.value * (transportDrones.Length - 1)];}
		if (drone != null) {
			drone.target = rbox;
			if (!drone.gameObject.activeSelf) {
				drone.transform.position = transform.TransformPoint(droneHangarPos); 
				drone.gameObject.SetActive(true);}
			drone.changeDestinationAfterHaul = true;
		}
	}

	public bool ConsumeEnergy (float f) {if (energy - f >= 0) {energy-= f; return true;} else return false;}
	public void RestoreEnergy () { energy = energyCapacity;}
		

	void OnGUI () {
		GUILayout.Label((Mathf.Floor(speed * 100) / 100).ToString());
		GUILayout.Label(bottomDistance.ToString()+ "u");
		GUILayout.Label("Рабочая глубина: "+ physicDepth.ToString()+"u");
		GUILayout.Label("Текущая глубина: "+ height.ToString()+"u");

		if (!mainSkinSet) {GUI.skin.font = mainSkin.font;}
		float k = GameMaster.GetGUIPiece() * 2;
		Rect rightPanelRect = new Rect (k, sh - k, 6*k, k);
		GUI.DrawTexture (rightPanelRect, partsFrame_tx, ScaleMode.StretchToFill); rightPanelRect.y -= k/2; rightPanelRect.height = k/2;
		GUI.skin.GetStyle("Label").fontSize = (int)(k/3.0f);
		GUI.Label (rightPanelRect, "Прочность корпуса: " + Mathf.FloorToInt(hullPoints/maxHullPoints * 100).ToString() + '%');
		rightPanelRect.y -= k/2;
		GUI.Label (rightPanelRect, "Энергия : " + (Mathf.FloorToInt(energy/energyCapacity * 10000) / 100.00f ).ToString() + '%');

		float ek = energy/energyCapacity;
		GUI.DrawTexture(new Rect(0, sh - 2*k * ek, k, 2*k*ek), batteryInnerParts_tx, ScaleMode.ScaleAndCrop);
		GUI.DrawTexture(new Rect(0,sh-2*k,k,2*k),batteryFrame_tx,ScaleMode.StretchToFill);
		Rect energyWarningRect = new Rect(sw/2,0,k,k);
		if (ek  < 0.25f) {
			if (ek > 0.1f) GUI.DrawTexture(energyWarningRect, energyWarning_tx,ScaleMode.ScaleToFit);
			else GUI.DrawTexture(energyWarningRect, energyCriticalWarning_tx,ScaleMode.ScaleToFit);
		}
	}


}
