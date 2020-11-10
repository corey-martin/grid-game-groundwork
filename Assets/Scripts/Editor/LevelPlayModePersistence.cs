using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class LevelPlayModePersistence
{
    static string textFilePath { get { return Application.persistentDataPath + "/playmodechanges.txt"; } }

    public struct Job {
        public string name;
        public Vector3 position;
        public Vector3 eulerAngles; 
    }

    public static Job[] GetJobs() {
        List<Job> jobs = new List<Job>();
        if (File.Exists(textFilePath)) {
            string[] lines = File.ReadAllLines(textFilePath);
            foreach (string line in lines) {
                string[] split = line.Split('|');
                Job newJob = new Job();
                if (line.Contains("clear")) {
                    newJob.name = split[0];
                    newJob.position = Vec3FromStrings(split[1], split[2], split[3]);
                } else if (line.Contains("newobject")) {
                    newJob.name = split[1];
                    newJob.position = Vec3FromStrings(split[2], split[3], split[4]);
                    newJob.eulerAngles = Vec3FromStrings(split[5], split[6], split[7]);
                }
                jobs.Add(newJob);
            }
            File.Delete(textFilePath);
        }
        return jobs.ToArray();
    }

    static Vector3 Vec3FromStrings(string stringX, string stringY, string stringZ) {
        int x, y, z;
        if (Int32.TryParse(stringX, out x)) {
            if (Int32.TryParse(stringY, out y)) { 
                if (Int32.TryParse(stringZ, out z)) {
                    return new Vector3(x, y, z);
                }
            }
        }
        return Vector3.zero;
    }

    public static void SaveNewObject(GameObject go) {
        Vector3Int p = Utils.Vec3ToInt(go.transform.position);
        Vector3Int r = Utils.Vec3ToInt(go.transform.eulerAngles);
        string s = "newobject|" + go.transform.name + "|" + p.x + "|" + p.y + "|" + p.z + "|" + r.x + "|" + r.y + "|" + r.z;
        WriteText(s);
    }

    public static void ClearAtPosition(Vector3 vector3) {
        Vector3Int v = Utils.Vec3ToInt(vector3);
        string s = "clear|" + v.x + "|" + v.y + "|" + v.z;
        WriteText(s);
    }

    static void WriteText(string newText) {
        List<string> lines = new List<string>();
        lines.Add(newText);
        File.AppendAllLines(textFilePath, lines);
    }
}
