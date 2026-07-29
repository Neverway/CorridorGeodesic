using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HackCellDef
{
    public int row;
    public int col;
    public HackOpKind kind;
    public int amount = 1;
    public int threshold;
    public int targetRow;
    public int targetCol;
}

[CreateAssetMenu(fileName = "HackPuzzleDefinition", menuName = "Terminal/Hack Puzzle Definition")]
public class HackPuzzleDefinition : ScriptableObject
{
    public int rows = 4;
    public int cols = 4;
    public int startingAttempts = 4;
    public List<HackCellDef> cells = new List<HackCellDef>();

    public HackCell[][] BuildGrid()
    {
        var grid = new HackCell[rows][];
        for (int row = 0; row < rows; row++)
        {
            grid[row] = new HackCell[cols];
            for (int col = 0; col < cols; col++)
            {
                grid[row][col] = HackCell.Nop();
            }
        }

        foreach (var def in cells)
        {
            if (def.row < 0 || def.row >= rows || def.col < 0 || def.col >= cols) continue;

            HackCell cell = def.kind switch
            {
                HackOpKind.Adc => HackCell.Adc(def.amount),
                HackOpKind.Jmp => HackCell.Jmp(def.targetRow, def.targetCol),
                HackOpKind.Set => HackCell.Set(def.targetRow, def.targetCol),
                HackOpKind.CmpGoal => HackCell.CmpGoal(def.threshold),
                HackOpKind.CmpSecurity => HackCell.CmpSecurity(def.threshold),
                _ => HackCell.Nop(),
            };
            grid[def.row][def.col] = cell;
        }

        return grid;
    }
}