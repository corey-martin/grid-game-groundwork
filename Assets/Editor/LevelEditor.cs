#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelEditor : EditorWindow {
     
   	int selGridInt = 0;
    string[] selectStrings;
   
	int rotateInt = 0;       
    string[] rotateStrings = new string[] {
    	"0", "90", "180", "270"    
	};    
	
    int spawnHeight = 0;
	string levelName;
	string levelPath = "Assets/Resources/Levels/";
                        
    public GameObject[] prefabs;

    bool isHoldingAlt = false; 
    bool mouseButtonDown = false;
    Vector3 pos;

	[MenuItem("Window/Level Editor")] 
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(LevelEditor));
	}
                 
	void OnGUI() {
                 
        ScriptableObject scriptableObj = this;
        SerializedObject serialObj = new SerializedObject (scriptableObj);
        SerializedProperty serialProp = serialObj.FindProperty ("prefabs");
        EditorGUILayout.PropertyField (serialProp, true);
        serialObj.ApplyModifiedProperties ();

        if (prefabs != null && prefabs.Length > 0) {
			List<string> selectStringsTmp = new List<string>();
			selectStringsTmp.Add("None");
			selectStringsTmp.Add("Empty");
			foreach (GameObject prefab in prefabs) {
				if (prefab != null) {
					selectStringsTmp.Add(prefab.transform.name);
				}
			}
			selectStrings = selectStringsTmp.ToArray();
        } else {
			return;
		}
 
		GUILayout.Label ("Selected GameObject:", EditorStyles.boldLabel);
        selGridInt = GUILayout.SelectionGrid(selGridInt, selectStrings, 3, GUILayout.Width(370));

		GUILayout.Label ("GameObject Rotation:", EditorStyles.boldLabel);
        rotateInt = GUILayout.SelectionGrid(rotateInt, rotateStrings, 4, GUILayout.Width(330));

		EditorGUILayout.Space();

		///////////////// SPAWN //////////////////

        spawnHeight = EditorGUILayout.IntField("Spawn at height:", spawnHeight);

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		///////////////// ROTATION //////////////////

		GUILayout.Label ("Rotate Level:", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
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
		EditorGUILayout.Space();

		///////////////// INVERSION //////////////////

		GUILayout.Label ("Invert Level:", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("X axis", GUILayout.Width(80))) {
        	InvertLevel("x");
        }
        if (GUILayout.Button("Y axis", GUILayout.Width(80))) {
        	InvertLevel("y");
        }
        EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		///////////////// SAVE //////////////////

        levelName = EditorGUILayout.TextField("Level Name:", levelName);

        if (GUILayout.Button("Save To Disk", GUILayout.Width(150))) {
			
        	string path = "Assets/Resources/Levels/" + levelName + ".txt";

			if (File.Exists(path)) {
				if (EditorUtility.DisplayDialog("Overwrite Level?", "Are you sure you want to overwrite `" + levelName + "`?", "Yes", "No")) {
					SaveToDisk();
				}
			} else {
				SaveToDisk();
			}
        }

        if (GUILayout.Button("Load From Disk", GUILayout.Width(150))) {
        	LoadFromDisk();
        }

		EditorGUILayout.Space();
		EditorGUILayout.Space();
	}

	void SaveToDisk() {
		Transform level = GetLevelObject().transform;

		if(!System.IO.Directory.Exists(levelPath)) {
			System.IO.Directory.CreateDirectory(levelPath);
		}

        string path = levelPath + levelName + ".txt";
		StreamWriter writer = new StreamWriter(path, false);

        foreach (Transform child in level) {
			writer.WriteLine(child.name);
			writer.WriteLine(child.position.x + "|" + child.position.y + "|" + child.position.z);
			writer.WriteLine(child.eulerAngles.x + "|" + child.eulerAngles.y + "|" + child.eulerAngles.z);
        }

		writer.Close();
		AssetDatabase.ImportAsset(path); 
	}

	void LoadFromDisk() {
        string path = levelPath + levelName + ".txt";
		string line;
		int counter = 0;
		GameObject go = null;

        StreamReader reader = new StreamReader(path); 

		while ((line = reader.ReadLine()) != null) {  
			switch (counter) {
				case 0:
					GameObject prefab = Resources.Load(line) as GameObject;
					go = PrefabUtility.InstantiatePrefab(prefab as GameObject) as GameObject;
					go.transform.parent = GetLevelObject().transform;
					counter++;
					break;
				case 1:
        			string[] p = line.Split('|'); 
					go.transform.position = new Vector3(Utils.StringToInt(p[0]), Utils.StringToInt(p[1]), Utils.StringToInt(p[2]));
					counter++;
					break;
				case 2:
        			string[] r = line.Split('|'); 
					go.transform.eulerAngles = new Vector3(Utils.StringToInt(r[0]), Utils.StringToInt(r[1]), Utils.StringToInt(r[2]));
					counter = 0;
					break;
			}
		}  

        reader.Close();
	}

	void OnEnable() {
		SceneView.onSceneGUIDelegate += SceneGUI;
        EditorApplication.playModeStateChanged += ChangedPlayModeState;
    }

    private static void ChangedPlayModeState(PlayModeStateChange state) {
        if (state == PlayModeStateChange.EnteredEditMode) {
			AssetDatabase.Refresh();
		}
    }
       
	void OnValidate() {
		Refresh();
	}
	
	void Refresh() {
		isHoldingAlt = false;
		mouseButtonDown = false;
		Game game = FindObjectOfType<Game>();
		if (game != null) {
        	game.EditorRefresh();
		}
	}

	public void SceneGUI(SceneView sceneView) {
		Event e = Event.current;

		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        var controlID = GUIUtility.GetControlID(FocusType.Passive);
		var eventType = e.GetTypeForControl(controlID);

    	if (e.isKey && e.keyCode == KeyCode.P) { 
    		EditorApplication.ExecuteMenuItem("Edit/Play");
    	}

    	if (eventType == EventType.KeyDown) {
			if (e.keyCode == KeyCode.LeftAlt || e.keyCode == KeyCode.LeftControl) {
    			isHoldingAlt = true; 
			}
    	}

    	if (eventType == EventType.KeyUp) {
			if (e.keyCode == KeyCode.LeftAlt || e.keyCode == KeyCode.LeftControl) {
    			isHoldingAlt = false; 
			}
    	}

    	if (!isHoldingAlt) {

			if (eventType == EventType.MouseUp && e.button == 0 && selGridInt != 0 && sceneView.in2DMode) {
				e.Use();
				mouseButtonDown = false;
			}
	 
			if (eventType == EventType.MouseDown && e.button == 0 && selGridInt != 0 && sceneView.in2DMode) {
				e.Use();
				pos = GetPosition(e.mousePosition);
				CreateObject(pos);
				mouseButtonDown = true;
			}

			if (mouseButtonDown) {
				Vector3 currentPos = GetPosition(e.mousePosition);
				if (pos != currentPos) {
					pos = currentPos;
					CreateObject(pos);
				}
			}
    	}
    }
 
    Vector3 GetPosition(Vector3 mousePos) {
		Vector3 screenPosition = HandleUtility.GUIPointToWorldRay(mousePos).origin;
		return new Vector3(Mathf.Round(screenPosition.x), Mathf.Round(screenPosition.y), 0);
    }

    void CreateObject(Vector3 pos) {

		GameObject prefab = prefabs[0];

		if (selGridInt == 1) {
			ClearObjectsAtPosition(Vector3Int.RoundToInt(pos));

		} else {
			prefab = prefabs[selGridInt - 2];

			GameObject go  = PrefabUtility.InstantiatePrefab(prefab as GameObject) as GameObject;
			go.transform.position = pos;

			go.transform.parent = GetLevelObject().transform;

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

			go.transform.eulerAngles = new Vector3(0,0,z);

			Vector3 p = go.transform.position;
			go.transform.position = new Vector3(p.x, p.y, -Mathf.Abs(spawnHeight));
			
			Utils.AvoidIntersect(go.transform);
        	Undo.RegisterCreatedObjectUndo (go, "Create object");
		}

        Refresh();
    }
                        
    void RotateLevel(int degrees) {
    	GetLevelObject().transform.eulerAngles += new Vector3 (0,0,degrees);
    }
            
    void InvertLevel(string axis) {
		Transform level = GameObject.Find("Level").transform;
    	foreach (Transform child in level) {
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
    } 
      
    void ClearObjectsAtPosition(Vector3Int pos) {
		Transform level = GetLevelObject().transform;
        foreach (Transform child in level) {
            Vector3Int childPos = Vector3Int.RoundToInt(child.position);
            if (childPos.x == pos.x && childPos.y == pos.y) {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
    }

    GameObject GetLevelObject() {
        GameObject level = GameObject.Find("Level");
        if (level == null) {
            level = new GameObject();
            level.transform.name = "Level";
        }
        return level;
    }
}      

#endif