// borrowed from https://answers.unity.com/questions/33597/is-it-possible-to-create-a-tag-programmatically.html
// via users yoyo, Leslie-Young & LiWa
using UnityEditor;
 
public static class TagHelper {

    public static void AddTag(string tagname) {
        UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");

        if (asset != null && asset.Length > 0) {
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; i++) {
                if (tags.GetArrayElementAtIndex(i).stringValue == tagname) return;
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagname;
            so.ApplyModifiedProperties();
            so.Update();
        }
    }
}