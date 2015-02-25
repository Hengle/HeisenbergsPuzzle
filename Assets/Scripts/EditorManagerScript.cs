﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// NOTE: When adding a new IEnvironmentObject to the Editor manager, simply follow these steps
//   1. Add the new environment Objects prefab to the class variables
//   2. Add the ActionType.Choose<YOUR OBJECT> in the Input Systems. As well as a special button for selecting it
//   3. Add a case in the OnInputEvent for your new ActionType, set a new value for the create variable
//   4. Add a case in the "Create function" to instantiate your new object
//   5. Add a case in the "getValidComponent" function for your new object, based on tag

//Basic Environment Object Properties 
// NOTE TEMPORARY CLASS
public class ObjectSet {
	
	public Vector3 postion; //transform.position
	public Quaternion rotation; //transform.rotation
	public Vector3 scale; //transfrom.scale
	public string name; //name of game object
	
	//Initialise Objects properties 
	public ObjectSet (Vector3 pos, Quaternion rot, Vector3 sca, string obName) {
		
		postion = pos;
		rotation = rot;
		scale = sca;
		name = obName;
		
	}
	
}

public class EditorManagerScript : MonoBehaviour {
	public GameObject player; // Prefab for the players
	public GameObject efield; // Electric Field prefab;
	public GameObject mfield; // Magnetic Field prefab;
	public GameObject wall; // Barrier prefab
	public GameObject measurer; // Measurer prefab
	public GameObject antiMatter; // prefab for antiMatter
	public GameObject teleporter; // prefab for Teleporter
	public GameObject spawnPoint; // prefab spawnpoint
	private GameObject currentPlayerSpawnPoint; 
	GameObject selectedEnvironment = null;
	IEnvironmentObject selectedScript = null;
	List<GameObject> environments = new List<GameObject>();
	float ResizeExtraPush = 10f;
	float create; // 0 = Wall, 1 = Electric field, 2 = magnetic Field
	
	// Something ONLY for teleporters (may change later)
	TeleporterScript activatedTeleporter = null;
	
	
	// Use this for initialization
	void Start () {
		bool found = false;
		GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");
		foreach(GameObject spaw in spawns) {
			if(spaw.GetComponent<SpawnScript>().playerSpawn) {
				currentPlayerSpawnPoint = GameObject.Find ("SpawnPoint");
				environments.Add (currentPlayerSpawnPoint); 
				found = true;
				break;
			}
		}
		if (!found) {
			GameObject newSpawnPoint = Instantiate (spawnPoint, Vector3.zero, Quaternion.identity) as GameObject;
			environments.Add (newSpawnPoint);
			newSpawnPoint.GetComponent<SpawnScript>().playerSpawn = true;
			newSpawnPoint.GetComponent<SpawnScript>().spawningObject = player;
			currentPlayerSpawnPoint = newSpawnPoint;
		}
	}
	
	// Listens for appropriate Input
	void OnInputEvent(Vector2 rawValue, ActionType action) {
		if (action == ActionType.Create) {
			Create (create);
		} else if (action == ActionType.Select) {
			Select ();
		} else if (action == ActionType.ChooseWall) {
			create = 0; // Create a wll
		} else if (action == ActionType.ChooseEField) {
			create = 1;
		} else if (action == ActionType.ChooseMField) {
			create = 2;
		} else if (action == ActionType.ChooseMeasurer) {
			create = 3;
		} else if (action == ActionType.ChooseAntiMatter) {
			create = 4;
		} else if (action == ActionType.ChooseTeleporter) {
			create = 5;
		} else if (action == ActionType.ChooseSpawn) {
			create = 6;		
		} else if (action == ActionType.ChooseAltSpawn) {
			create = 7;		
		} else if (selectedEnvironment != null) {
			if (action == ActionType.Destroy) {
				Destroy ();
			} else if (action == ActionType.Snap) {
				Snap ();
			} else if (action == ActionType.Flip) {
				Flip ();
			} else if (action == ActionType.Resize) {
				Resize (rawValue.x*ResizeExtraPush);
			} else if (action == ActionType.ResizeDirection) {
				ResizeDirection ();
			} else if (action == ActionType.DisableEntity) {
				SetEntityField();
			}
		}
	}
	
	// We create a wall in the space where the mouse is currently at
	// We then add the wall to our list of walls
	public void Create(float typeOfObject) {
		Vector3 envPosition = Camera.main.ScreenToWorldPoint (new Vector3(Input.mousePosition.x,Input.mousePosition.y, UniversalHelperScript.Instance.cameraZDistance));
		GameObject tempEnv;
		if (create == 0) {
			tempEnv = Instantiate (wall,envPosition, Quaternion.identity) as GameObject;
		} else if (create == 1) {
			tempEnv = Instantiate (efield,envPosition, Quaternion.identity) as GameObject;
		} else if (create == 2) {
			tempEnv = Instantiate (mfield,envPosition, Quaternion.identity) as GameObject;
		} else if (create == 3) {
			tempEnv = Instantiate (measurer,envPosition,Quaternion.identity) as GameObject;
		} else if (create == 4) {
			tempEnv = Instantiate (antiMatter,envPosition,Quaternion.identity) as GameObject;
		} else if (create == 5) {
			tempEnv = Instantiate (teleporter,envPosition,Quaternion.identity) as GameObject;	
		} else if (create == 6) {
			currentPlayerSpawnPoint.transform.localPosition = envPosition;
			return;
		} else if (create == 7) {
			tempEnv = Instantiate (spawnPoint,envPosition,Quaternion.identity) as GameObject;
		} else {
		   return;
		}
		environments.Add (tempEnv);	
		selectedEnvironment = tempEnv;
		selectedScript = getValidComponent (tempEnv);	
	}
	
	private void Destroy() {
		environments.Remove (selectedEnvironment);
		Destroy (selectedEnvironment);
		selectedScript = null;
		selectedEnvironment = null;
	}
	
	private void Flip() {
		selectedScript.Flip ();
	}
	
	private void Resize(float resize) {
		selectedScript.Resize (resize);
	}
	
	private void ResizeDirection() {
		selectedScript.ChangeResizeDirection();
	}
	
	private void Snap() {
		selectedScript.Snap();
	}
	
	private void SetEntityField() {
		selectedScript.ToggleEntity();
	}
	
	private void Select() {
		RaycastHit2D hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero);
		if (hit.collider != null) {
			selectedEnvironment = hit.collider.gameObject; // Error with antimatter due to their large trigger colliders (FIXED)
			selectedScript = getValidComponent (selectedEnvironment);
		}
	}
	
	private IEnvironmentObject getValidComponent(GameObject selectedEnvironment) {
		IEnvironmentObject selectedScript = null;
		if (selectedEnvironment.tag == "Walls") {
			selectedScript = selectedEnvironment.GetComponent<WallScript>();
		} else if (selectedEnvironment.tag == "EField") {
			selectedScript = selectedEnvironment.GetComponent<ElectricFieldScript>();
		} else if (selectedEnvironment.tag == "MField") {
			selectedScript = selectedEnvironment.GetComponent<MagneticFieldScript>();
		} else if (selectedEnvironment.tag == "Measurer") {
			selectedScript = selectedEnvironment.GetComponent<MeasurerScript>();
		} else if (selectedEnvironment.tag == "AntiMatter") {
			selectedScript = selectedEnvironment.GetComponent<AntiMatterScript>();	
		} else if (selectedEnvironment.tag == "Teleporter") {
			selectedScript = selectedEnvironment.GetComponent<TeleporterScript>();
		} else if (selectedEnvironment.tag == "Spawn") {
			selectedScript = selectedEnvironment.GetComponent<SpawnScript>();	
		}
		return selectedScript;
	}
	
	public void SetTeleporter(TeleporterScript teleporter) {
		if (activatedTeleporter != null) {
			activatedTeleporter.activateTeleporter (false); // Sets the previous active teleporter as inactive
		}
		activatedTeleporter = teleporter; // Sets the new teleporter as active
	}
	
	public TeleporterScript GetTeleport() {
		return activatedTeleporter;
	}
	
	public void EnableInput() {
		InputSystem.Instance.OnInputEditor += OnInputEvent;
	}
	
	public void DisableInput() {
		InputSystem.Instance.OnInputEditor -= OnInputEvent;
	}
	
	public void SetEditor(bool edit) {
		GameObject[] spawns = GameObject.FindGameObjectsWithTag ("Spawn");
		SpawnScript tempScript;
		foreach (GameObject spawn in spawns) {
			tempScript = spawn.GetComponent<SpawnScript>();
			tempScript.SetEditor (edit);
		}
	}
	
	private static EditorManagerScript instance; //Instance of InputSystem gameobject. InputSystem acts as a singleton class, being independent of other game objects 
	private EditorManagerScript() {}
	
	public static EditorManagerScript Instance
	{
		
		get 
		{
			if(instance == null) {
				instance = GameObject.FindObjectOfType(typeof(EditorManagerScript)) as EditorManagerScript; //Instantiate class if instance == null
				
			}
			return instance;
			
		}
	}
			
}

