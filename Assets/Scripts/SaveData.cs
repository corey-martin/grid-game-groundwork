using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class SaveData {

	public static bool initialized { get; private set; } = false;
	private static int gameNumber = 1;
	private static BinaryFormatter bf = new BinaryFormatter();
	private static PlayerData playerData = new PlayerData();

	private static string Path(int i) {
		return Application.persistentDataPath + "/" + i + ".dat";
	}

	public static bool GameExists(int i) {
		return File.Exists(Path(i));
	}

	static PlayerData FileData(int i) {
		FileStream file = File.Open(Path(i), FileMode.Open);
		PlayerData data = (PlayerData)bf.Deserialize(file);
		file.Close();
		return data;
	}

	public static void LoadGame(int i) {
		initialized = true;
		gameNumber = i;
		
		PlayerData data = new PlayerData();
		if (GameExists(gameNumber)) {
			data = FileData(gameNumber);
        }
		playerData = data;
	}

	public static void DeleteGame(int i) {
		if (File.Exists(Path(i))) {
			File.Delete(Path(i));
		}
	}

	public static void SaveGame() {		
		FileStream file;
		if (File.Exists(Path(gameNumber))) {
			file = File.Open(Path(gameNumber), FileMode.Open);
		} else {
			file = File.Create(Path(gameNumber));
		}
        bf.Serialize(file, playerData);
		file.Close();
	}

	public static void BeatLevel(string level) {
		if (!playerData.levelsBeaten.Contains(level)) {
			playerData.levelsBeaten.Add(level);
		}
	}
}

[Serializable]
public class PlayerData {
	public List<string> levelsBeaten = new List<string>();
}
