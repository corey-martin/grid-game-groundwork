using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance { get; private set; }
    public GameObject[] prefabs;
    public static string currentLevelName = "";
    static bool isLoading = false;
    [SerializeField] string levelToLoad;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        if (!string.IsNullOrWhiteSpace(levelToLoad)) {
            LoadLevel(levelToLoad);
        }
    }

    public void LoadLevel(string levelName, bool clear = true) {

		if (isLoading || string.IsNullOrWhiteSpace(levelName)) {
			return;
		}
        if (clear) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        currentLevelName = levelName;
        SerializedLevel serializedLevel = LevelLoader.LoadLevel(currentLevelName);
        GameObject newLevelObj = new GameObject();
        newLevelObj.transform.name = currentLevelName;
        newLevelObj.transform.parent = transform;
        LevelLoader.InstantiateLevel(serializedLevel, prefabs, newLevelObj.transform);
        
        EventManager.onLevelStarted?.Invoke(currentLevelName);
    }

    public void QuitLevel() {
        EventManager.onLevelQuit?.Invoke(currentLevelName);
    }
}
