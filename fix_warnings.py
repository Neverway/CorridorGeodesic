#!/usr/bin/env python3
"""
fix_warnings.py - fixes the CS0114 / CS0108 warnings from the console.

  1. Actor hooks (Start/Update/FixedUpdate/OnDestroy) -> "protected override" + base call
  2. WB_Pause.OnEnable                               -> "protected override" + base call
  3. rigidbody / hingeJoint / light / renderer fields -> adds the "new" keyword

Usage (run from anywhere):
    python3 fix_warnings.py /path/to/Assets            # dry run, changes nothing
    python3 fix_warnings.py /path/to/Assets --apply    # writes the changes

Commit first so git can undo it. Safe to run twice (it skips what is fixed).
"""
import os, re, sys

METHODS = {  # file -> methods to turn into overrides
    "FPPawn_NPCWolf.cs":       ["FixedUpdate"],
    "FPPawn_Player.cs":        ["Update", "FixedUpdate"],
    "Item_Utility_DevGun.cs":  ["Start", "Update"],
    "Item_Utility_Geogun.cs":  ["Start", "Update", "OnDestroy", "FixedUpdate"],
    "WB_Pause.cs":             ["OnEnable"],
}
FIELDS = {  # file -> (type, name) fields that need "new"
    "VisualFramerateLimiter.cs": ("Rigidbody", "rigidbody"),
    "Logic_DoorSwinging.cs":     ("HingeJoint", "hingeJoint"),
    "LightEx.cs":                ("Light", "light"),
    "Graphics_Test_PlaneMask.cs": ("Renderer", "renderer"),
}

apply = "--apply" in sys.argv
args = [a for a in sys.argv[1:] if not a.startswith("--")]
if not args:
    sys.exit(__doc__)
root = args[0]

def find(name):
    hits = []
    for d, _, files in os.walk(root):
        if name in files:
            hits.append(os.path.join(d, name))
    return hits

def fix_method(lines, meth):
    sig = re.compile(r"^(\s*)((?:public|private|protected|internal)\s+)?void\s+" + meth + r"\s*\(\s*\)")
    for i, ln in enumerate(lines):
        m = sig.match(ln)
        if not m:
            continue
        if "override" in ln:
            return f"{meth}: already override, skipped"
        if "=>" in ln:
            return f"{meth}: expression-bodied, edit by hand"
        indent = m.group(1)
        lines[i] = ln.replace(m.group(0), f"{indent}protected override void {meth}()", 1)
        # find the opening brace
        j = i
        while "{" not in lines[j]:
            j += 1
            if j > i + 3:
                return f"{meth}: could not find '{{', edit by hand"
        # already has a base call right after?
        k = j + 1
        while k < len(lines) and not lines[k].strip():
            k += 1
        if k < len(lines) and re.search(r"\bbase\." + meth + r"\(", lines[k]):
            return f"{meth}: made override (base call already there)"
        brace_line = lines[j]
        if brace_line.strip() != "{":
            return f"{meth}: made override, but brace is on the signature line, add base.{meth}(); by hand"
        lines.insert(j + 1, f"{indent}    base.{meth}();\n")
        return f"{meth}: override + base.{meth}();"
    return f"{meth}: not found (already renamed?)"

def fix_field(lines, typ, name):
    pat = re.compile(r"^(\s*(?:\[[^\]]*\]\s*)*(?:(?:public|private|protected|internal|static)\s+)*)" + typ + r"\s+" + name + r"\b")
    for i, ln in enumerate(lines):
        m = pat.match(ln)
        if m:
            if re.search(r"\bnew\s+" + typ, ln[: m.end()]):
                return f"{name}: already has new, skipped"
            lines[i] = ln[: m.end(1)] + "new " + ln[m.end(1):]
            return f"{name}: added 'new'"
    return f"{name}: not found (already renamed?)"

changed = 0
for table, kind in ((METHODS, "m"), (FIELDS, "f")):
    for fname, spec in table.items():
        paths = find(fname)
        if not paths:
            print(f"?? {fname}: not found under {root}")
            continue
        for p in paths:
            with open(p, encoding="utf-8-sig", newline="") as f:
                raw = f.read()
            bom = open(p, "rb").read(3) == b"\xef\xbb\xbf"
            lines = raw.splitlines(keepends=True)
            if kind == "m":
                notes = [fix_method(lines, m) for m in spec]
            else:
                notes = [fix_field(lines, *spec)]
            new = "".join(lines)
            print(("EDIT " if new != raw else "ok   ") + os.path.relpath(p, root))
            for n in notes:
                print("       " + n)
            if new != raw:
                changed += 1
                if apply:
                    with open(p, "w", encoding="utf-8-sig" if bom else "utf-8", newline="") as f:
                        f.write(new)
print(f"\n{changed} file(s) {'changed' if apply else 'would change (dry run, add --apply)'}")
