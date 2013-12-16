using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Manager))]
public class ManagerEditor : Editor {
	override public void OnInspectorGUI() {
		EditorGUILayout.BeginHorizontal();
		((Manager)target).timeScale = EditorGUILayout.FloatField("Time Scale", ((Manager)target).timeScale);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		((Manager)target).gravity = EditorGUILayout.FloatField("Gravity", ((Manager)target).gravity);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		((Manager)target).drag = EditorGUILayout.Vector2Field("Drag", new Vector2(((Manager)target).drag.x, ((Manager)target).drag.y));
		EditorGUILayout.EndHorizontal();
		//if (GUI.changed) EditorUtility.SetDirty(target);
	}
}
