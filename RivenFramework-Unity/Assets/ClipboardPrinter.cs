using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipboardPrinter : MonoBehaviour
{
    public bool clearCopiedResults;
    [TextArea(1, 1000)] public string copiedResults = "";


    private HashSet<string> uniqueClipboardStrings = new();
    public string Clipboard => GUIUtility.systemCopyBuffer;

    private void Start()
    {
        uniqueClipboardStrings = new();
        Application.runInBackground = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (clearCopiedResults)
        {
            clearCopiedResults = false;
            uniqueClipboardStrings.Clear();
            copiedResults = "";
        }

        if (uniqueClipboardStrings.Add(Clipboard))
        {
            copiedResults += Clipboard + "\n";
        }
    }
}
