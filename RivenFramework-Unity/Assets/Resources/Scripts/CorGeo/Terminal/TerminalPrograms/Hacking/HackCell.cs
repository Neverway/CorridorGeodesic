using System.Collections.Generic;

public enum HackOpKind
{
    Nop,
    Adc,
    Jmp,
    Set,
    CmpGoal,
    CmpSecurity,
}

[System.Serializable]
public class HackCell
{
    public HackOpKind kind = HackOpKind.Nop;

    public string opcodeHex = "00";
    public string mnemonic = "NOP";

    public int amount = 1; // used for adc
    public int threshold; // used for cmp goal and cmp security
    public int targetRow; // used for jmp and set
    public int targetCol; // used for jmp and set

    public bool completed;

    public static HackCell Nop() => new HackCell { kind = HackOpKind.Nop, opcodeHex = "00", mnemonic = "NOP" };

    public static HackCell Adc(int amount = 1) => new HackCell
    {
        kind = HackOpKind.Adc, opcodeHex = "69", mnemonic = "ADC", amount = amount
    };

    public static HackCell Jmp(int targetRow, int targetCol) => new HackCell
    {
        kind = HackOpKind.Jmp, opcodeHex = "4C", mnemonic = "JMP", targetRow = targetRow, targetCol = targetCol
    };

    public static HackCell Set(int targetRow, int targetCol) => new HackCell
    {
        kind = HackOpKind.Set, opcodeHex = "85", mnemonic = "SET", targetRow = targetRow, targetCol = targetCol
    };

    public static HackCell CmpGoal(int threshold) => new HackCell
    {
        kind = HackOpKind.CmpGoal, opcodeHex = "C9", mnemonic = "CMP", threshold = threshold
    };

    public static HackCell CmpSecurity(int threshold) => new HackCell
    {
        kind = HackOpKind.CmpSecurity, opcodeHex = "C9", mnemonic = "SEC", threshold = threshold
    };
}
