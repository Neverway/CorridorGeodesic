using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WB_HackIntersection : MonoBehaviour
{
    public RectTransform rect;
    public Image dot;
    public Button button;

    public Color normalColor = new Color(1f, 1f, 1f, 0.15f);
    public Color selectedColor = Color.green;

    private int rowLine;
    private int colLine;
    private Action<int, int> onClick;

    public void Bind(int rowLine, int colLine, Action<int, int> onClick)
    {
        this.rowLine = rowLine;
        this.colLine = colLine;
        this.onClick = onClick;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClick?.Invoke(this.rowLine, this.colLine));
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (dot != null) dot.color = selected ? selectedColor : normalColor;
    }
}
