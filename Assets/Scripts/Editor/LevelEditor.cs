﻿#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LevelEditor : EditorWindow {
     
   	int selGridInt = 0;
    string[] selectStrings;
   
	int rotateInt = 0;       
    string[] rotateStrings = new string[] {
    	"0", "90", "180", "270"    
	};    
	
    int spawnHeight = 0;
	string currentLevel;
    string newLevelName = "";
	string levelPath => Application.dataPath + "/Resources/Levels/";
	bool overwriteLevel = true;
                        
    public GameObject[] prefabs;

    bool isHoldingAlt; 
    bool mouseButtonDown;
	bool in2DMode;
    Vector3 drawPos;
	static GameObject newGameObject;
	static bool playModeActive;
	Event e;
	bool titleIsSet;
    static string textFilePath => Application.dataPath + "/leveleditorprefabs.txt";
    List<string> savedLevels => Utils.allLevels;
    int savedLevelIndex = 0;
	int sceneLevelIndex;
	bool snapToGrid = true;
	bool isLoading;
	bool isDirty;
	Vector3 prevPosition;
	Vector2 scrollPos;
	Color gizmoColor = Color.white;
	Vector2 mousePosOnClick = new Vector2();
	bool refreshPrefabs = true;

	GUIStyle wrapperRef;
	GUIStyle wrapper {
		get {
			if (wrapperRef == null) {
				wrapperRef = new GUIStyle();
				wrapperRef.padding = new RectOffset(20,20,20,20);
				float n = 0.175f;
				wrapperRef.normal.background = Utils.MakeTex(1, 1, new Color(n, n, n, 1f));
			}
			return wrapperRef;
		}
	}

	GameObject levelRef = null;
    GameObject levelManagerGameObject {
		get {
			if (levelRef == null) {
				LevelManager levelManager = FindObjectOfType<LevelManager>();
				if (levelManager != null) {
					levelRef = levelManager.gameObject;
				} else {
					levelRef = new GameObject();
					levelRef.AddComponent<LevelManager>();
					levelRef.transform.name = "LevelManager";
				}
			}
			return levelRef;
		}
    }

	GameObject currentLevelParentRef;
	GameObject currentLevelParent {
		get {
			if (currentLevelParentRef == null) {
				GameObject existingLevelParent = GameObject.Find(currentLevel);
				if (existingLevelParent != null && existingLevelParent.CompareTag("Level")) {
					currentLevelParentRef = existingLevelParent;
				} else {
					currentLevelParentRef = new GameObject();
					currentLevelParentRef.transform.name = currentLevel;
					currentLevelParentRef.transform.parent = levelManagerGameObject.transform;
				}
			}
			return currentLevelParentRef;
		}
	}

	void HorizontalLine() => EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

	List<string> allLevels => Utils.allLevels;

	[MenuItem("Window/Level Editor")] 
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(LevelEditor));
	}

	void OnEnable() {
		SceneView.duringSceneGui += SceneGUI;
        EditorApplication.playModeStateChanged += ChangedPlayModeState;
        Undo.undoRedoPerformed += Refresh;
		PopulateList();
    }

	void OnDisable() {
		SceneView.duringSceneGui -= SceneGUI;
        EditorApplication.playModeStateChanged -= ChangedPlayModeState;
        Undo.undoRedoPerformed -= Refresh;
	}

    void ChangedPlayModeState(PlayModeStateChange state) {
		switch (state) {
			case PlayModeStateChange.EnteredPlayMode:
				playModeActive = true;
				break;
			case PlayModeStateChange.EnteredEditMode:
				playModeActive = false;
				GetPlayModeJobs();
				break;
		}
    }
       
	void OnValidate() {
		if (Utils.isMetaScene) return;
		if (Game.instance == null) {
			return;
			//var prefab = (GameObject)AssetDatabase.LoadAssetAtPath(PathToAsset("GameController"), typeof(GameObject));
	        //var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		} 
		EnsureTagsExist();
 		Reset();
		Refresh();
		refreshPrefabs = true;
	}

	void Reset() {
		mouseButtonDown = false;
		CreateGizmoObject();
	}
	
	void Refresh() {
		if (Utils.isMetaScene) return;
		Game.instance?.EditorRefresh();
		RefreshSavedLevels();
	}

	void CreateGizmoObject() {
		LevelGizmo levelGizmo = FindObjectOfType<LevelGizmo>();
		if (levelGizmo == null) {
  			new GameObject("LevelGizmo").AddComponent<LevelGizmo>();
		}
	}

	void PopulateList() {

        if (File.Exists(textFilePath)) {
			List<GameObject> newPrefabs = new List<GameObject>();
            string[] prefabNames = File.ReadAllLines(textFilePath);
			foreach (string prefabName in prefabNames) {
				GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(PathToAsset(prefabName), typeof(GameObject));
				if (go != null) {
					newPrefabs.Add(go);
				}
			}
			prefabs = newPrefabs.ToArray();
		}
	}

	string PathToAsset(string s) {
        string[] guids = AssetDatabase.FindAssets(s, null);
        foreach (string guid in guids) {
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (path.ToLower().Contains(".prefab")) {
				string fileName = Path.GetFileNameWithoutExtension(path);
				if (fileName == s) {
					return path;
				}
			}
        }
		Debug.LogError("Couldnt find a prefab named " + s);
		return string.Empty;
	}

	void RefreshSavedLevels() {
		Utils.RefreshLevels();
	}

	void EnsureTagsExist() {
		TagHelper.AddTag("Level");
		TagHelper.AddTag("Tile");
	}
	
	void OnGUI() {

		string previousLevel = currentLevel;
		if (!titleIsSet) {
			titleIsSet = true;
			var texture = Resources.Load<Texture2D>("ggg");
			titleContent = new GUIContent("Level Editor", texture);
		}

        GUILayout.BeginVertical(wrapper);

			scrollPos = GUILayout.BeginScrollView(scrollPos); 
				DrawingWindow();
				RefreshSavedLevels();
				SaveLoadWindow();
			EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();

		if (previousLevel != currentLevel) {
			Selection.activeGameObject = currentLevelParent;
		}
	}

	void DrawingWindow() {

		GUILayout.Label ("DRAWING", EditorStyles.centeredGreyMiniLabel);
		HorizontalLine();

		if (string.IsNullOrWhiteSpace(currentLevel)) {
			GameObject level = GameObject.FindGameObjectWithTag("Level");
			if (level != null) {
				currentLevel = level.name;
			}
		}
		
		// FOR MULTIPLE SCENES AT ONCE

		GUILayout.Label ("Currently Editing: ", EditorStyles.boldLabel);

		sceneLevelIndex = 0;
		for (int i = 0; i < allLevels.Count; i++) {
			if (allLevels[i] == currentLevel) {
				sceneLevelIndex = i;
			}
		}
        sceneLevelIndex = EditorGUILayout.Popup(sceneLevelIndex, allLevels.ToArray());
		currentLevel = allLevels[sceneLevelIndex];

		if (currentLevel == null) {
			return;
		}
		
		EditorGUILayout.Space();

		if (prefabs == null || prefabs.Length == 0) {
			PopulateList();
		}

		if (prefabs != null && refreshPrefabs) {
			List<string> selectStringsTmp = new List<string>();
			selectStringsTmp.Add("None");
			selectStringsTmp.Add("Erase");
			foreach (GameObject prefab in prefabs) {
				if (prefab != null) {
					selectStringsTmp.Add(prefab.transform.name);
				}
			}
			selectStrings = selectStringsTmp.ToArray();
			refreshPrefabs = false;
		}
 
		GUILayout.Label ("Selected GameObject:", EditorStyles.boldLabel);
        selGridInt = GUILayout.SelectionGrid(selGridInt, selectStrings, 3, GUILayout.Width(370));

		BigSpace();

		GUILayout.Label ("GameObject Rotation:", EditorStyles.boldLabel);
        rotateInt = GUILayout.SelectionGrid(rotateInt, rotateStrings, 4, GUILayout.Width(330));

		BigSpace();

        gizmoColor = EditorGUILayout.ColorField("Gizmo Color:", gizmoColor);

		///////////////// SPAWN //////////////////

        spawnHeight = EditorGUILayout.IntSlider("Spawn at height:", spawnHeight, 0, 20);

        snapToGrid = EditorGUILayout.Toggle("Snap to grid:", snapToGrid);

		BigSpace();

		///////////////// ROTATION //////////////////

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label ("Rotate Level:", EditorStyles.boldLabel);
        if (GUILayout.Button("90° CW", GUILayout.Width(80))) {
        	RotateLevel(90);
        }
        if (GUILayout.Button("90° CCW", GUILayout.Width(80))) {
        	RotateLevel(-90);
        }
        if (GUILayout.Button("180°", GUILayout.Width(80))) {
        	RotateLevel(180);
        }
        EditorGUILayout.EndHorizontal();

		BigSpace();

		///////////////// INVERSION //////////////////

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label ("Invert Level:", EditorStyles.boldLabel);
        if (GUILayout.Button("X axis", GUILayout.Width(80))) {
        	InvertLevel("x");
        }
        if (GUILayout.Button("Y axis", GUILayout.Width(80))) {
        	InvertLevel("y");
        }
        EditorGUILayout.EndHorizontal();

		BigSpace();
	}
         
	void SaveLoadWindow() {

		GUILayout.Label ("SAVING AND LOADING", EditorStyles.centeredGreyMiniLabel);
		HorizontalLine();
		EditorGUILayout.Space();

		
		EditorGUILayout.BeginHorizontal();

		if (string.IsNullOrWhiteSpace(newLevelName)) {
			if (GameObject.FindGameObjectWithTag("Level") == null) {
				GUILayout.Label ("To create a new level, give it a name: ");
			} else {
				newLevelName = currentLevelParent.name;
			}
		}
		
        if (!string.IsNullOrWhiteSpace(newLevelName) && GUILayout.Button("Save Level As", GUILayout.Width(150))) {

			currentLevelParent.transform.name = currentLevel = newLevelName;
			newLevelName = RemoveInvalidChars(newLevelName);
        	string path = "Assets/Resources/Levels/" + newLevelName + ".txt";

			if (File.Exists(path)) {
				if (EditorUtility.DisplayDialog("Overwrite Level?", "Are you sure you want to overwrite '" + newLevelName + "'?", "Yes", "No")) {
					SaveToDisk(newLevelName);
				}
			} else {
				SaveToDisk(newLevelName);
			}
        }
		newLevelName = EditorGUILayout.TextField(newLevelName);
        EditorGUILayout.EndHorizontal();

		BigSpace();

		
		if (savedLevels.Count > 0) {

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label ("Overwrite level(s) in scene ");
			overwriteLevel = EditorGUILayout.Toggle(overwriteLevel);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Load Level", GUILayout.Width(150))) {
				if (!isDirty || !overwriteLevel || EditorUtility.DisplayDialog("Load " + savedLevels[savedLevelIndex] + "?", "Load " + savedLevels[savedLevelIndex] + "? Any unsaved changes to " + currentLevel + " will be lost.", "Confirm", "Cancel")) {
					if (overwriteLevel) {

						Transform level = FindObjectOfType<LevelManager>().transform;
						for (int i = level.childCount - 1; i >= 0; i--) {
							Undo.DestroyObjectImmediate(level.GetChild(i).gameObject);
						}
					}
					currentLevel = savedLevels[savedLevelIndex];
					LoadFromDisk(currentLevel);
					Refresh();
				}
			}
			savedLevelIndex = EditorGUILayout.Popup(savedLevelIndex, savedLevels.ToArray());
			EditorGUILayout.EndHorizontal();

			BigSpace();
					
			ScriptableObject scriptableObj = this;
			SerializedObject serialObj = new SerializedObject (scriptableObj);
			SerializedProperty serialProp = serialObj.FindProperty ("prefabs");
			EditorGUILayout.PropertyField (serialProp, true);
			serialObj.ApplyModifiedProperties ();

			BigSpace();
		}
	}
                 
	void BigSpace() {
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
	}

	public string RemoveInvalidChars(string filename) {
		return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
	}

	void SaveToDisk(string levelName) {

		if(!System.IO.Directory.Exists(levelPath)) {
			System.IO.Directory.CreateDirectory(levelPath);
		}

        string path = levelPath + levelName + ".json";
		StreamWriter writer = new StreamWriter(path, false);
		writer.WriteLine(JsonUtility.ToJson(new SerializedLevel(currentLevelParent)));
		writer.Close();
		AssetDatabase.ImportAsset(path); 
		RefreshSavedLevels();
		AssetDatabase.Refresh();

		isDirty = false;
	}

	void LoadFromDisk(string levelName) {

		if (isLoading || string.IsNullOrWhiteSpace(levelName)) {
			return;
		}

		Vector3 levelPosition = Vector3.zero;
		GameObject existingLevelParent = GameObject.Find(levelName);
		if (existingLevelParent != null && existingLevelParent.CompareTag("Level")) {
			levelPosition = existingLevelParent.transform.position;
			Undo.DestroyObjectImmediate(existingLevelParent);
		}

		LevelManager.currentLevelName = levelName;
        TextAsset textFile = Resources.Load<TextAsset>("Levels/" + levelName);
		if (textFile == null) {
			Debug.LogError("No level found called " + levelName);
			return;
		}

		isLoading = true;
		
        SerializedLevel serializedLevel = LevelLoader.LoadLevel(levelName);
        
        foreach (var slo in serializedLevel.LevelObjects) {
			GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(PathToAsset(slo.prefab), typeof(GameObject));
	        var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
	        go.transform.parent = currentLevelParent.transform;
	        go.transform.localPosition = slo.pos;
	        go.transform.localEulerAngles = slo.angles;
	        Undo.RegisterCreatedObjectUndo (go, "Create object");
        }

		currentLevelParent.transform.position = levelPosition;
		newLevelName = levelName;
		isLoading = false;
		isDirty = false;
	}

	void Update() {
        if (!EditorApplication.isPlaying && Selection.transforms.Length > 0 && Selection.transforms[0].position != prevPosition) {
			foreach (Transform t in Selection.transforms) {
				if (t.CompareTag("Level")) {
					currentLevel = t.name;
				}
				if (snapToGrid) {
					if (t.CompareTag("Level") || (t.parent != null && t.parent.CompareTag("Level"))) {
						Utils.RoundPosition(t);
						prevPosition = t.position;
					}
				}
			}
		}
	}

	void GetPlayModeJobs() {
		LevelPlayModePersistence.Job[] jobs = LevelPlayModePersistence.GetJobs();
		foreach (LevelPlayModePersistence.Job job in jobs) {
			if (job.name == "clear") {
				ClearObjectsAtPosition(Utils.Vec3ToInt(job.position));
			} else {
				PlayModeCreateObject(job.name, job.position, job.eulerAngles);
			}
		}
	}

	void PlayModeCreateObject(string objName, Vector3 position, Vector3 eulerAngles) {
		for (int i = 0; i < prefabs.Length; i++) {
			if (prefabs[i].transform.name == objName) {
				selGridInt = i + 2;
			}
		}
		CreateObject(position);
		newGameObject.transform.eulerAngles = eulerAngles;
	}
         
	public void SceneGUI(SceneView sceneView) {

		if (Utils.isMetaScene) return;
		if (currentLevelParentRef == null) return;

		e = Event.current;
		in2DMode = sceneView.in2DMode;

		if (e.modifiers != EventModifiers.None) {
			isHoldingAlt = true;
			mouseButtonDown = false;
		} else {
			isHoldingAlt = false;
		}

		Vector3 currentPos = GetPosition(e.mousePosition);
		if (selGridInt != 1) {
			currentPos += (Vector3.back * spawnHeight);
			currentPos = Utils.AvoidIntersect(currentPos);
		}

		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        var controlID = GUIUtility.GetControlID(FocusType.Passive);
		var eventType = e.GetTypeForControl(controlID);

		if (SceneView.mouseOverWindow != sceneView) {
			Reset();
		}
    	if (e.isKey && e.keyCode == KeyCode.P) { 
    		EditorApplication.ExecuteMenuItem("Edit/Play");
    	}
 
    	if (isHoldingAlt) {
			if (eventType == EventType.ScrollWheel) {
				int deltaY = (e.delta.y < 0) ? -1 : 1;
				spawnHeight += deltaY;
				currentPos += (Vector3.back * deltaY);
				e.Use();
			}

		} else {

			if (eventType == EventType.MouseUp) {
				mouseButtonDown = false;
			}
	 
			if (eventType == EventType.MouseDown) {

				if (e.button == 0 && selGridInt != 0) {
					e.Use();
					Refresh();
					drawPos = currentPos;
					CreateObject(Utils.Vec3ToInt(drawPos));
					mouseButtonDown = true;
					mousePosOnClick = e.mousePosition;
					
				} else if (e.button == 1) {
					selGridInt = 1;
					Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
					RaycastHit hit = new RaycastHit();

					if (Physics.Raycast(ray, out hit, 1000.0f)) {
						for (int i = 0; i < prefabs.Length; i++) {
							if (prefabs[i].transform.name == hit.transform.parent.name) {
								selGridInt = i + 2;
							}
						}
					}
				}
				
			} else if (mouseButtonDown) {

				if (Vector2.Distance(mousePosOnClick, e.mousePosition) > 10f) {
					if (!Utils.VectorRoughly2D(drawPos, currentPos, 0.75f)) {
						drawPos = Utils.Vec3ToInt(currentPos);
						CreateObject(drawPos);
						mousePosOnClick = e.mousePosition;
					}
				}
			}
    	}

		LevelGizmo.UpdateGizmo(currentPos, gizmoColor);
		LevelGizmo.Enable(selGridInt != 0);
		sceneView.Repaint();
		Repaint();
    }
 
    Vector3 GetPosition(Vector3 mousePos) {
		if (in2DMode) {
			Vector3 screenPosition = HandleUtility.GUIPointToWorldRay(mousePos).origin;
			return Utils.Vec3ToInt(new Vector3(screenPosition.x, screenPosition.y, 0));
		} else {
			Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

         	RaycastHit hit = new RaycastHit();
			if (Physics.Raycast(ray, out hit, 100.0f)) {
				Vector3 pos = hit.point + (hit.normal * 0.5f);
				if (selGridInt == 1) {
					pos = hit.transform.position;
				}
				return Utils.Vec3ToInt(pos);
			}

			Plane hPlane = new Plane(Vector3.forward, Vector3.zero);
   			float distance = 0; 
			if (hPlane.Raycast(ray, out distance)){
				return Utils.Vec3ToInt(ray.GetPoint(distance));
   			}
		}
		return Vector3.zero;
    }

	GameObject GetPrefabByName(string s) {
		foreach (GameObject prefab in prefabs) {
			if (prefab.transform.name.Contains(s)) {
				return prefab;
			}
		}
		return null;
	}

    void CreateObject(Vector3 pos) {

		if (selGridInt == 1) {
			ClearObjectsAtPosition(Vector3Int.RoundToInt(pos));

		} else {
			GameObject prefab = prefabs[selGridInt - 2];

			newGameObject = PrefabUtility.InstantiatePrefab(prefab as GameObject) as GameObject;
			newGameObject.transform.position = pos;
			newGameObject.transform.parent = currentLevelParent.transform;

			int z = 0;
			switch (rotateInt) {
				case 0:
					z = 0;
					break;
				case 1:
					z = 90;
					break;
				case 2:
					z = 180;
					break;
				case 3:
					z = 270;
					break;
			}

			newGameObject.transform.eulerAngles = new Vector3(0,0,z);

			Vector3 p = newGameObject.transform.position;
			if (spawnHeight < p.z) {
				newGameObject.transform.position = new Vector3(p.x, p.y, -Mathf.Abs(spawnHeight));
			} 
			
			Utils.AvoidIntersect(newGameObject.transform);
			
			if (playModeActive) {
				LevelPlayModePersistence.SaveNewObject(newGameObject);
			}

        	Undo.RegisterCreatedObjectUndo (newGameObject, "Create object");
		}

        Refresh();

		isDirty = true;
    }
                        
    void RotateLevel(int degrees) {
    	currentLevelParent.transform.eulerAngles += new Vector3 (0,0,degrees);
		isDirty = true;
    }
            
    void InvertLevel(string axis) {
    	foreach (Transform child in currentLevelParent.transform) {
			Vector3 p = child.position;
			Vector3 s = child.localScale;
			if (axis == "x") {
				child.position = new Vector3(-p.x, p.y, p.z);
				child.localScale = new Vector3(-s.x, s.y, s.z);
			} else {
				child.position = new Vector3(p.x, -p.y, p.z);
				child.localScale = new Vector3(s.x, -s.y, s.z);
			}
    	}  
		isDirty = true;
    } 
      
    void ClearObjectsAtPosition(Vector3Int pos) {

		bool foundSomething = true;
		while (foundSomething) {
			foundSomething = false;
			foreach (Transform child in currentLevelParent.transform) {
				Transform target = GetTarget(child);
				foreach (Transform tile in target) {
					bool atPosition = (in2DMode) ? Utils.VectorRoughly2D(tile.position, pos) : Utils.VectorRoughly(tile.position, pos);
					if (tile.CompareTag("Tile") && atPosition) {
						foundSomething = true;
						Undo.DestroyObjectImmediate(child.gameObject);
						break;
					}
				}
			}
		}
		isDirty = true;
    }

	Transform GetTarget(Transform t) {
		if (t.name.Contains("Extender")) {
			return t.GetComponentInChildren<Wall>().transform;
		}
		return t;
	}
}      

#endif