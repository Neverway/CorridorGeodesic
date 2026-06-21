using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SimpleLightProbePlacer
{
    [RequireComponent(typeof(LightProbeGroup))]
    [AddComponentMenu("Rendering/Light Probe Group Control")]
    public class LightProbeGroupControl : MonoBehaviour
    {
        [SerializeField] private float m_mergeDistance = 0.5f;
        [SerializeField] private bool m_usePointLights = true;
        [SerializeField] private float m_pointLightRange = 1;
        [SerializeField] private bool m_removeInsideGeometry = true;
        [SerializeField] private bool m_invertGeometryCheck = false;
        [SerializeField] private LayerMask m_geometryLayers = ~0;

        public float MergeDistance { get { return m_mergeDistance; } set { m_mergeDistance = value; } }
        public int MergedProbes { get { return m_mergedProbes; } }
        public int RemovedInsideGeometry { get { return m_removedInsideGeometry; } }
        public bool UsePointLights { get { return m_usePointLights; } set { m_usePointLights = value; } }
        public float PointLightRange { get { return m_pointLightRange; } set { m_pointLightRange = value; } }
        public bool RemoveInsideGeometry { get { return m_removeInsideGeometry; } set { m_removeInsideGeometry = value; } }
        public bool InvertGeometryCheck { get { return m_invertGeometryCheck; } set { m_invertGeometryCheck = value; } }
        public LayerMask GeometryLayers { get { return m_geometryLayers; } set { m_geometryLayers = value; } }

        public LightProbeGroup LightProbeGroup
        {
            get
            {
                if (m_lightProbeGroup != null) return m_lightProbeGroup;
                return m_lightProbeGroup = GetComponent<LightProbeGroup>();
            }
        }

        private int m_mergedProbes;
        private int m_removedInsideGeometry;
        private LightProbeGroup m_lightProbeGroup;

        public void DeleteAll()
        {
            //LightProbeGroup.probePositions = null;
            m_mergedProbes = 0;
            m_removedInsideGeometry = 0;
        }

        public void Create()
        {
            DeleteAll();

            List<Vector3> positions = CreatePositions();
            positions.AddRange(CreateAroundPointLights(m_pointLightRange));
            positions = MergeClosestPositions(positions, m_mergeDistance, out m_mergedProbes);

            if (m_removeInsideGeometry)
                positions = FilterProbesInsideGeometry(positions, m_geometryLayers, m_invertGeometryCheck, out m_removedInsideGeometry);

            ApplyPositions(positions);
        }

        public void Merge()
        {
            if (LightProbeGroup.probePositions == null) return;

            List<Vector3> positions = MergeClosestPositions(LightProbeGroup.probePositions.ToList(), m_mergeDistance, out m_mergedProbes);
            positions = positions.Select(x => transform.TransformPoint(x)).ToList();

            ApplyPositions(positions);
        }

        public void RemoveProbesInsideGeometry()
        {
            if (LightProbeGroup.probePositions == null || LightProbeGroup.probePositions.Length == 0)
                return;

            List<Vector3> worldPositions = LightProbeGroup.probePositions
                .Select(x => transform.TransformPoint(x))
                .ToList();

            worldPositions = FilterProbesInsideGeometry(worldPositions, m_geometryLayers, m_invertGeometryCheck, out m_removedInsideGeometry);

            ApplyPositions(worldPositions);
        }

        private static List<Vector3> FilterProbesInsideGeometry(List<Vector3> positions, LayerMask layers, bool invert, out int removedCount)
        {
            List<Vector3> filtered = new List<Vector3>(positions.Count);

            foreach (var pos in positions)
            {
                bool inside = IsInsideGeometry(pos, layers);
                bool keep = invert ? inside : !inside;
                if (keep)
                    filtered.Add(pos);
            }

            removedCount = positions.Count - filtered.Count;
            return filtered;
        }
        public static bool IsInsideGeometry(Vector3 worldPosition, LayerMask layers)
        {
            Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

            int insideCount = 0;

            foreach (var dir in directions)
            {
                RaycastHit hit;
                if (Physics.Raycast(worldPosition, dir, out hit, Mathf.Infinity, layers, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Dot(dir, hit.normal) > 0f)
                        insideCount++;
                }
            }

            return insideCount > 3;
        }

        private void ApplyPositions(List<Vector3> positions)
        {
            //LightProbeGroup.probePositions = positions.Select(x => transform.InverseTransformPoint(x)).ToArray();
        }

        private static List<Vector3> CreatePositions()
        {
            var lightProbeVolumes = FindObjectsOfType<LightProbeVolume>();

            if (lightProbeVolumes.Length == 0) return new List<Vector3>();

            List<Vector3> probes = new List<Vector3>();

            for (int i = 0; i < lightProbeVolumes.Length; i++)
            {
                probes.AddRange(lightProbeVolumes[i].CreatePositions());
            }

            return probes;
        }

        private static List<Vector3> CreateAroundPointLights(float range)
        {
            var lights = FindObjectsOfType<Light>().Where(x => x.type == LightType.Point).ToList();

            if (lights.Count == 0) return new List<Vector3>();

            List<Vector3> probes = new List<Vector3>();

            for (int i = 0; i < lights.Count; i++)
            {
                probes.AddRange(CreatePositionsAround(lights[i].transform, range));
            }

            return probes;
        }

        private static List<Vector3> MergeClosestPositions(List<Vector3> positions, float distance, out int mergedCount)
        {
            if (positions == null)
            {
                mergedCount = 0;
                return new List<Vector3>();
            }

            int exist = positions.Count;
            bool done = false;

            while (!done)
            {
                Dictionary<Vector3, List<Vector3>> closest = new Dictionary<Vector3, List<Vector3>>();

                for (int i = 0; i < positions.Count; i++)
                {
                    List<Vector3> points = positions.Where(x => (x - positions[i]).magnitude < distance).ToList();
                    if (points.Count > 0 && !closest.ContainsKey(positions[i]))
                    {
                        closest.Add(positions[i], points);
                    }
                }

                positions.Clear();
                List<Vector3> keys = closest.Keys.ToList();

                for (int i = 0; i < keys.Count; i++)
                {
                    var center = closest[keys[i]].Aggregate(Vector3.zero, (result, target) => result + target) / closest[keys[i]].Count;
                    if (!positions.Exists(x => x == center)) positions.Add(center);
                }

                done = positions.Select(x => positions.Where(y => y != x && (y - x).magnitude < distance)).All(x => !x.Any());
            }

            mergedCount = exist - positions.Count;
            return positions;
        }

        public static List<Vector3> CreatePositionsAround(Transform transform, float range)
        {
            Vector3[] corners =
            {
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            };

            return corners.Select(x => transform.TransformPoint(x * range)).ToList();
        }
    }
}