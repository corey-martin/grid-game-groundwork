#if UNITY_EDITOR

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
	string levelPath = "Assets/Resources/Levels/";
	bool overwriteLevel = false;
                        
    public GameObject[] prefabs;

    bool isHoldingAlt; 
    bool mouseButtonDown;
	bool in2DMode;
    Vector3 drawPos;
	static GameObject newGameObject;
	static bool playModeActive;
	Event e;
	bool titleIsSet;
    static string textFilePath { get { return Application.dataPath + "/leveleditorprefabs.txt"; } }
    public List<string> savedLevels = new List<string>();
    int savedLevelIndex = 0;
	int sceneLevelIndex;
	bool snapToGrid = true;
	bool isLoading;
	bool isDirty;
	Vector3 prevPosition;
	Vector2 scrollPos;
	Color gizmoColor = Color.white;
	Vector2 mousePosOnClick = new Vector2();

    GameObject level {
		get {
			GameObject level = GameObject.Find(currentLevel);
			if (level == null) {
				level = new GameObject();
				level.transform.name = currentLevel;
				level.tag = "Level";
				level.transform.SetParent(GameObject.Find("Levels").transform);
        		Undo.RegisterCreatedObjectUndo (level, "Create object");
			}
			return level;
		}
    }

	List<string> sceneLevels = new List<string>();

	[MenuItem("Window/Level Editor")] 
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(LevelEditor));
	}

	void PopulateList() {
        if (prefabs.Length == 0 && File.Exists(textFilePath)) {
			List<GameObject> newPrefabs = new List<GameObject>();
            string[] prefabNames = File.ReadAllLines(textFilePath);
			foreach (string prefabName in prefabNames) {
				GameObject go = Resources.Load<GameObject>(prefabName);
				if (go != null) {
					newPrefabs.Add(go);
				}
			}
			prefabs = newPrefabs.ToArray();
		}
	}

	void RefreshSavedLevels() {
		savedLevels.Clear();
		DirectoryInfo d = new DirectoryInfo(levelPath);
		foreach (var file in d.GetFiles("*.txt")) {
			savedLevels.Add(file.Name.Replace(".txt", ""));
		}
	}

	void RefreshSceneLevels() {
		sceneLevels.Clear();
		GameObject[] levels = GameObject.FindGameObjectsWithTag("Level");
		foreach (GameObject l in levels) {
			sceneLevels.Add(l.name);
		}
	}
	
	void OnGUI() {

		string previousLevel = currentLevel;

		if (!titleIsSet) {
			titleIsSet = true;
			var texture = Resources.Load<Texture2D>("ggg");
			titleContent = new GUIContent("Level Editor", texture);
		}
		GUI.backgroundColor = Color.grey;

        BeginWindows();
 		Rect windowRect = new Rect(20, 20, 420, 650); 

		GUIStyle myStyle = new GUIStyle (GUI.skin.window); 
		myStyle.padding = new RectOffset(15,15,15,15);

		windowRect = GUILayout.Window(1, windowRect, GetWindows, "", myStyle);
        EndWindows();

		if (previousLevel != currentLevel) {
			Selection.activeGameObject = level;
		}

	}

	void GetWindows(int unusedWindowID) {
		scrollPos = GUILayout.BeginScrollView(scrollPos); 
		
		RefreshSceneLevels();
		if (sceneLevels.Count > 0) {
			DrawingWindow();
		}
		RefreshSavedLevels();
		SaveLoadWindow();
        EditorGUILayout.EndScrollView();
	}

	void DrawingWindow() {

		GUILayout.Label ("DRAWING", EditorStyles.centeredGreyMiniLabel);

		BigSpace();

		if (string.IsNullOrEmpty(currentLevel)) {
			GameObject level = GameObject.FindGameObjectWithTag("Level");
			if (level != null) {
				currentLevel = level.name;
			}
		}

		GUILayout.Label ("Currently Editing: ", EditorStyles.boldLabel);

		sceneLevelIndex = 0;
		for (int i = 0; i < sceneLevels.Count; i++) {
			if (sceneLevels[i] == currentLevel) {
				sceneLevelIndex = i;
			}
		}
        sceneLevelIndex = EditorGUILayout.Popup(sceneLevelIndex, sceneLevels.ToArray());
		currentLevel = sceneLevels[sceneLevelIndex];

		if (currentLevel == null) {
			return;
		}
		
		BigSpace();

        if (prefabs != null && prefabs.Length > 0) {
			List<string> selectStringsTmp = new List<string>();
			selectStringsTmp.Add("None");
			selectStringsTmp.Add("Erase");
			foreach (GameObject prefab in prefabs) {
				if (prefab != null) {
					selectStringsTmp.Add(prefab.transform.name);
				}
			}
			selectStrings = selectStringsTmp.ToArray();
        } else {
			PopulateList();
			return;
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

		if (savedLevels.Count > 0) {
			GUILayout.Label ("SAVING AND LOADING", EditorStyles.centeredGreyMiniLabel);
		}

		BigSpace();

		
		EditorGUILayout.BeginHorizontal();

		if (string.IsNullOrEmpty(newLevelName)) {
			GUILayout.Label ("To create a new level, give it a name: ");
		}
		
        if (!string.IsNullOrEmpty(newLevelName) && GUILayout.Button("Save Level As", GUILayout.Width(150))) {

			level.transform.name = currentLevel = newLevelName;
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
						GameObject[] levels = GameObject.FindGameObjectsWithTag("Level");
						foreach (GameObject l in levels) {
							Undo.DestroyObjectImmediate(l);
						}
					}
					currentLevel = savedLevels[savedLevelIndex];
					LoadFromDisk(currentLevel);
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

        string path = levelPath + levelName + ".txt";
		StreamWriter writer = new StreamWriter(path, false);

        foreach (Transform child in level.transform) {
			writer.WriteLine(child.name);
			writer.WriteLine(child.localPosition.x + "|" + child.localPosition.y + "|" + child.localPosition.z);
			writer.WriteLine(child.localEulerAngles.x + "|" + child.localEulerAngles.y + "|" + child.localEulerAngles.z);
        }

		writer.Close();
		AssetDatabase.ImportAsset(path); 
		RefreshSavedLevels();

		isDirty = false;
	}

	void LoadFromDisk(string levelName) {

		if (isLoading) {
			return;
		}

		Vector3 levelPosition = Vector3.zero;
		GameObject existingLevel = GameObject.Find(levelName);
		if (existingLevel != null && existingLevel.CompareTag("Level")) {
			levelPosition = existingLevel.transform.position;
			Undo.DestroyObjectImmediate(existingLevel);
		}

        string path = levelPath + levelName + ".txt";
		
		if (!File.Exists(path)) {
			Debug.LogError("No level data found at " + path);
			return;
		}

		isLoading = true;

		string line;
		int counter = 0;
		GameObject go = null;

        StreamReader reader = new StreamReader(path); 

		while ((line = reader.ReadLine()) != null) {  
			switch (counter) {
				case 0:
					GameObject prefab = Resources.Load(line) as GameObject;
					go = PrefabUtility.InstantiatePrefab(prefab as GameObject) as GameObject;
					go.transform.parent = level.transform;
        			Undo.RegisterCreatedObjectUndo (go, "Create object");
					counter++;
					break;
				case 1:
        			string[] p = line.Split('|'); 
					go.transform.localPosition = new Vector3(Utils.StringToInt(p[0]), Utils.StringToInt(p[1]), Utils.StringToInt(p[2]));
					counter++;
					break;
				case 2:
        			string[] r = line.Split('|'); 
					go.transform.localEulerAngles = new Vector3(Utils.StringToInt(r[0]), Utils.StringToInt(r[1]), Utils.StringToInt(r[2]));
					counter = 0;
					break;
			}
		}  
        reader.Close();

		RefreshSceneLevels();
		level.transform.position = levelPosition;
		newLevelName = levelName;
		isLoading = false;
		isDirty = false;
	}

	void OnEnable() {
		SceneView.duringSceneGui += SceneGUI;
        EditorApplication.playModeStateChanged += ChangedPlayModeState;
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
 		Reset();
		Refresh();
	}

	void Reset() {
		mouseButtonDown = false;
		CreateGizmoObject();
	}
	
	void Refresh() {
		Game game = FindObjectOfType<Game>();
		if (game != null) {
        	game.EditorRefresh();
		}
		RefreshSceneLevels();
		RefreshSavedLevels();
	}

	void CreateGizmoObject() {
		LevelGizmo levelGizmo = FindObjectOfType<LevelGizmo>();
		if (levelGizmo == null) {
  			new GameObject("LevelGizmo").AddComponent<LevelGizmo>();
		}
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
					selGridInt = 0;
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
			if (Physics.Raycast(ray, out hit, 1000.0f)) {
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

			newGameObject.transform.parent = level.transform;

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
    	level.transform.eulerAngles += new Vector3 (0,0,degrees);
		isDirty = true;
    }
            
    void InvertLevel(string axis) {
    	foreach (Transform child in level.transform) {
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
			foreach (Transform child in level.transform) {
				foreach (Transform tile in child) {
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
}      

#endif