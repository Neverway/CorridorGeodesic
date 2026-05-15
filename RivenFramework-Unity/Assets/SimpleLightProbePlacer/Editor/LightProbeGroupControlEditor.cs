using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimpleLightProbePlacer.Editor
{
    [CustomEditor(typeof(LightProbeGroupControl))]
    public class LightProbeGroupControlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var control = (LightProbeGroupControl)target;

            if (GUILayout.Button("Delete All Light Probes"))
            {
                Undo.RecordObject(control.LightProbeGroup, "Light Probe Group - delete all");
                control.DeleteAll();
            }

            if (control.LightProbeGroup != null)
            {
                string message = "Light Probes count: {0}\nMerged Probes: {1}\nRemoved (inside geometry): {2}";
                message = string.Format(message,
                    control.LightProbeGroup.probePositions.Length,
                    control.MergedProbes,
                    control.RemovedInsideGeometry);

                EditorGUILayout.HelpBox(message, MessageType.Info);
            }

            if (GUILayout.Button("Create Light Probes"))
            {
                Undo.RecordObject(control.LightProbeGroup, "Light Probe Group - create");
                control.Create();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Merge Closest Light Probes"))
            {
                Undo.RecordObject(control.LightProbeGroup, "Light Probe Group - merge");
                control.Merge();
            }

            EditorGUI.BeginChangeCheck();

            var mergeDist = EditorGUILayout.Slider("Merge distance", control.MergeDistance, 0, 10);

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Geometry Filtering", EditorStyles.boldLabel);

            var removeInside = EditorGUILayout.Toggle(
                new GUIContent("Remove Inside Geometry",
                    "Removes probes that are inside solid colliders. Casts a ray in the +X " +
                    "direction — if the first surface hit is a back face, the probe is inside."),
                control.RemoveInsideGeometry);

            GUI.enabled = control.RemoveInsideGeometry;

            var invertCheck = EditorGUILayout.Toggle(
                new GUIContent("Invert Check",
                    "Reverses the result — keeps probes inside geometry and removes those outside. " +
                    "Useful if the filter seems to be working backwards for your setup."),
                control.InvertGeometryCheck);

            var geometryLayers = LayerMaskField("Geometry Layers", control.GeometryLayers);

            if (GUILayout.Button("Remove Probes Inside Geometry"))
            {
                Undo.RecordObject(control.LightProbeGroup, "Light Probe Group - remove inside geometry");
                control.RemoveProbesInsideGeometry();
            }

            GUI.enabled = true;

            EditorGUILayout.HelpBox(
                "Requires colliders on geometry. For best results with concave shapes, " +
                "use non-convex MeshColliders. Trigger colliders are ignored.",
                MessageType.None);

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Point Light Settings", EditorStyles.boldLabel);

            var useLights = EditorGUILayout.Toggle("Use Point Lights", control.UsePointLights);

            GUI.enabled = control.UsePointLights;
            var lightRange = EditorGUILayout.FloatField("Range", control.PointLightRange);
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(control, "Light Probe Group Control changes");

                control.MergeDistance = mergeDist;
                control.RemoveInsideGeometry = removeInside;
                control.InvertGeometryCheck = invertCheck;
                control.GeometryLayers = geometryLayers;
                control.UsePointLights = useLights;
                control.PointLightRange = lightRange;

                EditorUtility.SetDirty(target);
            }
        }

        private static LayerMask LayerMaskField(string label, LayerMask selected)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((selected & (1 << layerNumbers[i])) != 0)
                    maskWithoutEmpty |= 1 << i;
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) != 0)
                    mask |= 1 << layerNumbers[i];
            }

            return mask;
        }

        [MenuItem("GameObject/Light/Light Probe Group Control")]
        private static void CreateLightProbeGroupControl(MenuCommand menuCommand)
        {
            var go = new GameObject("Light Probe Group Control");

            go.AddComponent<LightProbeGroupControl>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Light Probe Group Control");

            Selection.activeGameObject = go;
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy | GizmoType.Active)]
        private static void DrawGizmoPointLight(Light light, GizmoType gizmoType)
        {
            var control = FindObjectOfType<LightProbeGroupControl>();

            if (control == null || !control.UsePointLights || light.type != LightType.Point) return;

            List<Vector3> probes = LightProbeGroupControl.CreatePositionsAround(light.transform, control.PointLightRange);

            for (int i = 0; i < probes.Count; i++)
            {
                Gizmos.DrawIcon(probes[i], "NONE", false);
            }
        }
    }
}