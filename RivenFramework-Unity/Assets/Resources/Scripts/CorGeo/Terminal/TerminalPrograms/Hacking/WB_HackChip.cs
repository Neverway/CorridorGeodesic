using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WB_HackChip : MonoBehaviour
{
    public TMP_Text label;
    public Button button;
    public Image border;

    public Color normalColor = Color.gray;
    public Color selectedColor = Color.white;

    private int index;
    private Action<int> onClick;

    public void Bind(int index, string text, bool selected, Action<int> onClick)
    {
        this.index = index;
        this.onClick = onClick;

        label.text = text;
        if (border != null) border.color = selected ? selectedColor : normalColor;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClick?.Invoke(this.index));
    }
}