using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerminalProgramRegistry", menuName = "Terminal/Terminal Program Registry")]
public class TerminalProgramRegistry : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string programId;
        public TerminalProgramBase prefab;
    }

    public List<Entry> programs = new List<Entry>();

    private Dictionary<string, TerminalProgramBase> lookup;

    public TerminalProgramBase GetPrefab(string programId)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, TerminalProgramBase>();
            foreach (var entry in programs)
            {
                if (entry.prefab == null || string.IsNullOrEmpty(entry.programId)) continue;
                lookup[entry.programId] = entry.prefab;
            }
        }

        lookup.TryGetValue(programId, out var prefab);
        return prefab;
    }
}
