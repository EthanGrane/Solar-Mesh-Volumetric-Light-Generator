using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;
namespace SolarMesh
{
    [CustomEditor(typeof(SolarMesh))]
    public class SolarMeshEditor : Editor
    {
        public Texture2D iconSpot;
        public Texture2D iconRectangle;
        public Texture2D iconAdaptative;
        public Texture2D buttonBackground;

        int selectedOption = 0;

        public override void OnInspectorGUI()
        {
            Header();
            Nav();

            SolarMesh solarMesh = (SolarMesh)target;

            switch (solarMesh.editorMode)
            {
                case EditorMode.Rectangle:
                    RectangleMode();
                    break;

                case EditorMode.Adaptative:
                    AdaptativeMode();
                    break;
            }

            GenerateButton();

            Footer();
            serializedObject.ApplyModifiedProperties();
        }

        void Header()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 40);
            rect.height = 40;

            EditorGUI.DrawRect(rect, new Color(0.85f , 0.67f, 0.01f, 1.0f));

            GUIStyle titleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            EditorGUI.LabelField(rect, "Solar Mesh", titleStyle);
        }

        void Nav()
        {
            const float margin = 0.9f;
            float inspectorWidth = EditorGUIUtility.currentViewWidth * margin;
            float buttonWidth = inspectorWidth * 0.33f;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = buttonWidth,
                fixedHeight = buttonWidth,
            };

            GUIStyle buttonSelectedStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = buttonWidth,
                fixedHeight = buttonWidth,
                normal = { background = buttonBackground }
            };

            SolarMesh solarMesh = (SolarMesh)target;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("", iconRectangle), solarMesh.editorMode == EditorMode.Rectangle ? buttonSelectedStyle : buttonStyle))
            {
                solarMesh.editorMode = EditorMode.Rectangle;
            }

            if (GUILayout.Button(new GUIContent("", iconAdaptative), solarMesh.editorMode == EditorMode.Adaptative ? buttonSelectedStyle : buttonStyle))
            {
                solarMesh.editorMode = EditorMode.Adaptative;
            }

            if (GUILayout.Button(new GUIContent("", iconSpot), solarMesh.editorMode == EditorMode.Spot ? buttonSelectedStyle : buttonStyle))
            {
                solarMesh.editorMode = EditorMode.Spot;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void RectangleMode()
        {
            SolarMesh solarMesh = (SolarMesh)target;
            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            //
            SerializedProperty windowsProperty = serializedObject.FindProperty("windows");
            EditorGUILayout.PropertyField(windowsProperty, true);

            //
            solarMesh.lightPointTransform = (Transform)EditorGUILayout.ObjectField("Light Source Direction", solarMesh.lightPointTransform, typeof(Transform), true);

            //
            const float rayLenght_min = 0.1f;
            const float rayLenght_max = 15f;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Light Ray Distance", GUILayout.Width(inspectorWidth * 0.35f));
            solarMesh.lightRayLenght = (float)EditorGUILayout.Slider(solarMesh.lightRayLenght, rayLenght_min, rayLenght_max);
            GUILayout.EndHorizontal();
        }

        void AdaptativeMode()
        {
            SolarMesh solarMesh = (SolarMesh)target;
            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            //
            SerializedProperty windowsProperty = serializedObject.FindProperty("windows");
            EditorGUILayout.PropertyField(windowsProperty, true);

            //
            solarMesh.lightPointTransform = (Transform)EditorGUILayout.ObjectField("Light Source Direction", solarMesh.lightPointTransform, typeof(Transform), true);

            //
            const float rayLenght_min = 0.1f;
            const float rayLenght_max = 15f;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Light Ray Distance", GUILayout.Width(inspectorWidth * 0.35f));
            solarMesh.lightRayLenght = (float)EditorGUILayout.Slider(solarMesh.lightRayLenght, rayLenght_min, rayLenght_max);
            GUILayout.EndHorizontal();

            //
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ray count", GUILayout.Width(inspectorWidth * 0.35f));

            string[] options = { "Custom", "Low ", "Medium", "High", "Very High", "Ultra" };
            selectedOption = EditorGUILayout.Popup(selectedOption, options);

            if(selectedOption != 0)
                solarMesh.rayCount = (selectedOption) * 26;

            solarMesh.rayCount = Mathf.Clamp(EditorGUILayout.IntField(solarMesh.rayCount, GUILayout.Width(inspectorWidth * 0.14f)), 4, 128);

            if (solarMesh.rayCount % 4 != 0)
                solarMesh.rayCount = Mathf.RoundToInt(solarMesh.rayCount / 4f) * 4;
            GUILayout.EndHorizontal();

        }

        void Footer()
        {
            GUILayout.Space(10);

            const int height = 3;
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.85f, 0.67f, 0.01f, 1.0f));
        }

        void GenerateButton()
        {
            SolarMesh solarMesh = (SolarMesh)target;

            if (solarMesh == null)
            {
                Debug.LogError("solarMesh es null");
                return;
            }

            GUILayout.Space(10);

            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = inspectorWidth * 0.6f,
                fixedHeight = 50,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Generate", buttonStyle))
            {
                Debug.Log("Generate button clicked");
                solarMesh.Generate();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        public enum EditorMode
        {
            Rectangle,
            Adaptative,
            Spot
        }
    }
}