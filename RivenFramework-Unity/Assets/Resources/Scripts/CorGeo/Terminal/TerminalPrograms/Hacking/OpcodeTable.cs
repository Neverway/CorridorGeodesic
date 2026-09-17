using System.Collections.Generic;

public static class OpcodeTable
{
    private static readonly Dictionary<int, (HackOpKind kind, string mnemonic)> lookup = new Dictionary<int, (HackOpKind, string)>
    {
        { 0x69, (HackOpKind.Adc, "ADC") },
        { 0x4C, (HackOpKind.Jmp, "JMP") },
        { 0x85, (HackOpKind.Set, "SET") },
        { 0xC9, (HackOpKind.CmpGoal, "CMP") },
        { 0xEA, (HackOpKind.Nop, "NOP") },
    };

    public static void Apply(HackCell target, int byteValue)
    {
        if (lookup.TryGetValue(byteValue, out var entry))
        {
            target.kind = entry.kind;
            target.mnemonic = entry.mnemonic;
            target.opcodeHex = byteValue.ToString("X2");
        }
        else
        {
            target.kind = HackOpKind.Nop;
            target.mnemonic = "NOP";
            target.opcodeHex = byteValue.ToString("X2");
        }
    }
}