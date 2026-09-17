using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TerminalBootLoader : MonoBehaviour
{
    public TMP_Text bootText;
    public GameObject bootPanel;
    private readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();
    public int maxVisibleLines = 20;
    private readonly List<string> visibleLines = new List<string>();


    public string[] bootLines =
    {
        // Hardware POST
        "SOLSTICE-5100 SYSTEM INITIALIZATION",
        "Warming filaments: tube bank 0-7... OK",
        "Charging delay-line memory (2048 words)... OK",
        "Relay logic self-check: 4096 relays... OK",
        "Spinning up tape deck /dev/tape0... ready",
        "Core memory self-test: 64K words... OK",

        // Kernal handoff
        "[    0.000000] Linux version 1.4.0-solstice (solstice-kernel@riftdeck) (gcc-12) #1 SMP PREEMPT_DYNAMIC",
        "[    0.000000] Command line: BOOT_IMAGE=/boot/vmlinuz-1.4.0-solstice root=UUID=riftdeck0 ro quiet",
        "[    0.041207] x86/fpu: Supporting XSAVE feature 0x001: 'x87 floating point registers'",
        "[    0.512931] clocksource: tsc: mask: 0xffffffffffffffff max_cycles",
        "Initializing solstice kernal (1.4)...",
        "[  WARN  ] Init interrupted!",
        "[  OK  ] Started Journal Service.",
        "[  OK  ] Reached target Local File Systems.",
        "Mounting /dev/riftdeck...",
        "[  OK  ] Mounted /dev/riftdeck",
        "Starting RiftdeckInjection...",
        
        // Injection
        "Kernel panic - not syncing: Attempted to kill init! exitcode=0x00000100",
        "CPU: 0 PID: 1 Comm: jamiey Not tainted 1.4.0-solstice #1",
        "Call Trace:",
        " <TASK>",
        " dump_stack_lvl+0x44/0x5c",
        " panic+0x119/0x2eb",
        //" Kͬe͐ŕ͊nͦa͗lͮͣͮ c̍òrͬr̍uͩ̎ͫpͯt̒eͪ͐̋̊ͮd̍̃!͛",
        //" Rͮě̐s̓t͋ȁrͨ̂t͐̅ͪiͮn̐gͫ̔ͥ͑͗.ͪ̿̉̍̊.̊.͗",
        " Kernal corrupted!",
        " Restarting...",
        " </TASK>",
        "Kernel Offset: disabled",
        "---[ end Kernel panic - not syncing: Attempted to kill init! exitcode=0x00000100 ]---",
        "",
        ">;3",
        "Jamiey wuz here!",
        "",

        // Recovery
        "Restarting system...",
        "[  OK  ] Reached target Multi-User System.",
        "[  OK  ] Initialized solstice kernal (1.4)",
        "Starting TP_DirectoryBrowser...",
        "[  OK  ] Started TP_DirectoryBrowser.",
        "Rendering...",
    };
    
    public float lineDelay = 0.15f;
    public float charDelay = 0.01f;

    public System.Action OnBootComplete;

    public void Play()
    {
        visibleLines.Clear();
        bootText.text = "";
        gameObject.SetActive(true);
        if (bootPanel != null) bootPanel.SetActive(true);
        StartCoroutine(BootRoutine());
    }
    
    public void SkipToEnd()
    {
        StopAllCoroutines();
        var last = bootLines.Skip(Mathf.Max(0, bootLines.Length - maxVisibleLines));
        bootText.text = string.Join("\n", bootLines);
        if (bootPanel != null) bootPanel.SetActive(false);
        OnBootComplete?.Invoke();
    }

    private IEnumerator BootRoutine()
    {
        visibleLines.Clear();
 
        foreach (var line in bootLines)
        {
            while (visibleLines.Count >= maxVisibleLines) visibleLines.RemoveAt(0);
 
            sb.Clear();
            foreach (char c in line)
            {
                sb.Append(c);
                RenderWithPartial(sb.ToString());
                if (charDelay > 0f) yield return new WaitForSeconds(charDelay);
            }
 
            visibleLines.Add(sb.ToString());
            RenderWithPartial(null);
            yield return new WaitForSeconds(lineDelay);
        }
 
        yield return new WaitForSeconds(0.3f);
        if (bootPanel != null) bootPanel.SetActive(false);
        OnBootComplete?.Invoke();
        bootText.text = "Please Insert Tapedeck...";

    }

    private void RenderWithPartial(string partial)
    {
        if (partial == null)
        {
            bootText.text = string.Join("\n", visibleLines);
            return;
        }
 
        if (visibleLines.Count == 0)
        {
            bootText.text = partial;
        }
        else
        {
            bootText.text = string.Join("\n", visibleLines) + "\n" + partial;
        }
    }

}
