using UnityEngine;
using UnityEditor;

namespace CpvrLab.VirtualTable {

    [CustomEditor(typeof(BuildOptions))]
    public class BuildOptionsEditor : Editor {

        BuildOptions script { get { return (BuildOptions)target; } }

        private SerializedProperty buildTarget;
        private SerializedProperty advancedSettings;
        private SerializedProperty targetOptions;

        public void OnEnable()
        {
            buildTarget = serializedObject.FindProperty("buildTarget");
            advancedSettings = serializedObject.FindProperty("advancedSettings");
            targetOptions = serializedObject.FindProperty("targetOptions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(buildTarget);

            if(Foldout(advancedSettings)) {
                var prop = advancedSettings.FindPropertyRelative("networkManager");
                EditorGUILayout.PropertyField(prop);

                int numOptions = System.Enum.GetNames(typeof(BuildOptions.Target)).Length;
                if(script.targetOptions.Length != numOptions) {

                    Debug.LogWarning("BuildOptions: Looks like the number of build options changed, updating internal build options list.");
                    // don't lose previous options
                    var newOptions = new BuildOptions.TargetOptions[numOptions];

                    if(script.targetOptions.Length > 0) {
                        for(int i = 0; i < script.targetOptions.Length; i++) {
                            if(i >= numOptions)
                                break;

                            newOptions[i] = script.targetOptions[i];
                        }
                    }

                    script.targetOptions = newOptions;
                }


                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                EditorGUI.indentLevel += 1;

                for(int i = 0; i < numOptions; i++) {
                    EditorGUILayout.PropertyField(targetOptions.GetArrayElementAtIndex(i), new GUIContent(((BuildOptions.Target)i).ToString() + " Target Options"), true);
                    
                }

                EditorGUI.indentLevel -=1;
            }

            serializedObject.ApplyModifiedProperties();

            if(GUI.changed) {
                script.UpdateBuildTarget();
                //var test = new SerializedObject(script.advancedSettings.networkManager);
                //test.ApplyModifiedProperties();
            }

        }

        private bool Foldout(SerializedProperty prop)
        {
            prop.isExpanded = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), prop.isExpanded, prop.displayName, true);
            return prop.isExpanded;
        }

    }

}