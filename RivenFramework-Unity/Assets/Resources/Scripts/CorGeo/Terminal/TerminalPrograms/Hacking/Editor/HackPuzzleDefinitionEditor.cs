using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HackPuzzleDefinition))]
public class HackPuzzleDefinitionEditor : Editor
{
    private const float cellSize = 34f;

    private static readonly Color colorNop = new Color(0.25f, 0.25f, 0.25f);
    private static readonly Color colorAdc = new Color(0.55f, 0.4f, 0.15f);
    private static readonly Color colorJmp = new Color(0.3f, 0.3f, 0.55f);
    private static readonly Color colorSet = new Color(0.5f, 0.3f, 0.5f);
    private static readonly Color colorGoal = new Color(0.15f, 0.4f, 0.6f);
    private static readonly Color colorSecurity = new Color(0.6f, 0.15f, 0.15f);

    private int selectedRow = -1;
    private int selectedCol = -1;
    private (int r, int c)? pickingTargetFor = null;

    public override void OnInspectorGUI()
    {
        var puzzle = (HackPuzzleDefinition)target;

        DrawPuzzleSettings(puzzle);
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
        DrawLegend();
        EditorGUILayout.Space(4);

        if (pickingTargetFor.HasValue)
        {
            EditorGUILayout.HelpBox("Click a cell below to set as the jump/set target.", MessageType.Info);
        }

        var selectedDef = FindDef(puzzle, selectedRow, selectedCol);
        (int r, int c)? highlightTarget = null;
        if (selectedDef != null && (selectedDef.kind == HackOpKind.Jmp || selectedDef.kind == HackOpKind.Set))
            highlightTarget = (selectedDef.targetRow, selectedDef.targetCol);

        for (int r = 0; r < puzzle.rows; r++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < puzzle.cols; c++)
            {
                bool isTarget = highlightTarget.HasValue && highlightTarget.Value.Item1 == r && highlightTarget.Value.Item2 == c;
                DrawCellButton(puzzle, r, c, isTarget);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);

        if (selectedRow >= 0 && selectedRow < puzzle.rows && selectedCol >= 0 && selectedCol < puzzle.cols)
        {
            DrawCellEditor(puzzle, selectedRow, selectedCol);
        }
        else
        {
            EditorGUILayout.HelpBox("Click a cell above to edit it.", MessageType.Info);
        }
    }

    private void DrawPuzzleSettings(HackPuzzleDefinition puzzle)
    {
        EditorGUILayout.LabelField("Puzzle Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int newRows = Mathf.Max(1, EditorGUILayout.IntField("Rows", puzzle.rows));
        int newCols = Mathf.Max(1, EditorGUILayout.IntField("Cols", puzzle.cols));
        int newAttempts = Mathf.Max(0, EditorGUILayout.IntField("Starting Attempts", puzzle.startingAttempts));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(puzzle, "Edit Hack Puzzle Settings");

            if (newRows < puzzle.rows || newCols < puzzle.cols)
            {
                int removed = puzzle.cells.RemoveAll(c => c.row >= newRows || c.col >= newCols);
                if (removed > 0) Debug.Log($"HackPuzzleDefinition: removed {removed} cell(s) now out of bounds.");
            }

            puzzle.rows = newRows;
            puzzle.cols = newCols;
            puzzle.startingAttempts = newAttempts;
            EditorUtility.SetDirty(puzzle);
        }
    }

    private void DrawLegend()
    {
        EditorGUILayout.BeginHorizontal();
        DrawLegendSwatch(colorNop, "Nop");
        DrawLegendSwatch(colorAdc, "Adc");
        DrawLegendSwatch(colorJmp, "Jmp");
        DrawLegendSwatch(colorSet, "Set");
        DrawLegendSwatch(colorGoal, "Goal");
        DrawLegendSwatch(colorSecurity, "Security");
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLegendSwatch(Color color, string label)
    {
        var prev = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUILayout.Box(GUIContent.none, GUILayout.Width(14), GUILayout.Height(14));
        GUI.backgroundColor = prev;
        GUILayout.Label(label, GUILayout.Width(58));
    }

    private void DrawCellButton(HackPuzzleDefinition puzzle, int r, int c, bool isJumpTarget)
    {
        var def = FindDef(puzzle, r, c);
        Color bg = def == null ? colorNop : ColorFor(def.kind);
        if (isJumpTarget) bg = Color.Lerp(bg, Color.white, 0.45f);

        string label = def == null ? "\u00B7" : ShortLabel(def);
        if (isJumpTarget) label += "\n\u25CE";

        bool isSelected = selectedRow == r && selectedCol == c;
        var style = new GUIStyle(GUI.skin.button) { fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal, fontSize = 9 };

        var prevColor = GUI.backgroundColor;
        GUI.backgroundColor = bg;
        bool clicked = GUILayout.Button(label, style, GUILayout.Width(cellSize), GUILayout.Height(cellSize));
        GUI.backgroundColor = prevColor;

        if (!clicked) return;

        if (pickingTargetFor.HasValue)
        {
            var (pr, pc) = pickingTargetFor.Value;
            var pdef = FindDef(puzzle, pr, pc);
            if (pdef != null && (pdef.kind == HackOpKind.Jmp || pdef.kind == HackOpKind.Set))
            {
                Undo.RecordObject(puzzle, "Set Jump/Set Target");
                pdef.targetRow = r;
                pdef.targetCol = c;
                EditorUtility.SetDirty(puzzle);
            }
            pickingTargetFor = null;
        }
        else
        {
            selectedRow = r;
            selectedCol = c;
        }
    }

    private void DrawCellEditor(HackPuzzleDefinition puzzle, int r, int c)
    {
        EditorGUILayout.LabelField($"Editing cell (R{r}, C{c})", EditorStyles.boldLabel);

        var def = FindDef(puzzle, r, c);
        bool hadDef = def != null;
        HackOpKind kind = def?.kind ?? HackOpKind.Nop;
        int amount = def?.amount ?? 1;
        int threshold = def?.threshold ?? 0;
        int targetRow = def?.targetRow ?? 0;
        int targetCol = def?.targetCol ?? 0;

        EditorGUI.BeginChangeCheck();
        kind = (HackOpKind)EditorGUILayout.EnumPopup("Kind", kind);

        if (kind == HackOpKind.Adc)
            amount = EditorGUILayout.IntField("Amount", amount);

        if (kind == HackOpKind.CmpGoal || kind == HackOpKind.CmpSecurity)
            threshold = EditorGUILayout.IntField("Threshold", threshold);

        if (kind == HackOpKind.Jmp || kind == HackOpKind.Set)
        {
            targetRow = EditorGUILayout.IntSlider("Target Row", targetRow, 0, puzzle.rows - 1);
            targetCol = EditorGUILayout.IntSlider("Target Col", targetCol, 0, puzzle.cols - 1);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(puzzle, "Edit Hack Cell");
            if (kind == HackOpKind.Nop)
            {
                if (hadDef) puzzle.cells.Remove(def);
            }
            else
            {
                if (!hadDef)
                {
                    def = new HackCellDef { row = r, col = c };
                    puzzle.cells.Add(def);
                }
                def.kind = kind;
                def.amount = amount;
                def.threshold = threshold;
                def.targetRow = targetRow;
                def.targetCol = targetCol;
            }
            EditorUtility.SetDirty(puzzle);
        }

        if (kind == HackOpKind.Jmp || kind == HackOpKind.Set)
        {
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Pick Target By Clicking Grid"))
                pickingTargetFor = (r, c);

            if (kind == HackOpKind.Jmp && targetRow == r && targetCol == c)
                EditorGUILayout.HelpBox("This JMP targets itself and will cause an infinite loop! (That's bad >:{ )", MessageType.Warning);
        }

        EditorGUILayout.Space(4);
        if (hadDef && GUILayout.Button("Clear Cell (set to a Nop)"))
        {
            Undo.RecordObject(puzzle, "Clear Hack Cell");
            puzzle.cells.Remove(def);
            EditorUtility.SetDirty(puzzle);
        }
    }

    private HackCellDef FindDef(HackPuzzleDefinition puzzle, int r, int c)
    {
        if (puzzle == null) return null;
        return puzzle.cells.FirstOrDefault(d => d.row == r && d.col == c);
    }

    private string ShortLabel(HackCellDef def)
    {
        switch (def.kind)
        {
            case HackOpKind.Adc: return "+" + def.amount;
            case HackOpKind.Jmp: return "J\u2192";
            case HackOpKind.Set: return "SET";
            case HackOpKind.CmpGoal: return ">=" + def.threshold;
            case HackOpKind.CmpSecurity: return "!" + def.threshold;
            default: return "\u00B7";
        }
    }

    private Color ColorFor(HackOpKind kind)
    {
        switch (kind)
        {
            case HackOpKind.Adc: return colorAdc;
            case HackOpKind.Jmp: return colorJmp;
            case HackOpKind.Set: return colorSet;
            case HackOpKind.CmpGoal: return colorGoal;
            case HackOpKind.CmpSecurity: return colorSecurity;
            default: return colorNop;
        }
    }
}