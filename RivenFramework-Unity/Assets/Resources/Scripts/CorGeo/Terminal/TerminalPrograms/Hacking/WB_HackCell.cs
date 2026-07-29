using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WB_HackCell : MonoBehaviour
{
    public RectTransform rect;
    public TMP_Text hexLabel;
    public TMP_Text mnemonicLabel;
    public TMP_Text value;
    public Image background;
    public Image outline;
    public TMP_Text popupPrefab;
    public CanvasGroup canvasGroup;

    public Color colorNormal = new Color(0.2f, 0.15f, 0.05f);
    public Color colorGoal = new Color(0.1f, 0.3f, 0.5f);
    public Color colorGoalComplete = new Color(0.1f, 0.4f, 0.15f);
    public Color colorSecurity = new Color(0.4f, 0.1f, 0.1f);

    public Color textNormal = Color.white;
    public Color textSuccess = Color.green;
    public Color textDanger = Color.red;
    public Color textNeutral = Color.gray;

    public Color stepHighlightColor = Color.white;
    public Color selectionPreviewColor = Color.yellow;

    public float moveAnimationSeconds = 0.25f;
    public float hideAnimationSeconds = 0.2f;

    private Coroutine moveRoutine;
    private Coroutine hideRoutine;
    
    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    public void Render(HackCell cell)
    {
        switch (cell.kind)
        {
            case HackOpKind.CmpGoal:
                hexLabel.text = $"{cell.opcodeHex}";
                mnemonicLabel.text = $"{cell.mnemonic}";
                value.text = $">={cell.threshold}";
                background.color = cell.completed ? colorGoalComplete : colorGoal;
                break;
            case HackOpKind.CmpSecurity:
                hexLabel.text = $"{cell.opcodeHex}";
                mnemonicLabel.text = $"{cell.mnemonic}";
                value.text = $">={cell.threshold}";
                background.color = colorSecurity;
                break;
            case HackOpKind.Adc:
                hexLabel.text = $"{cell.opcodeHex}";
                mnemonicLabel.text = $"{cell.mnemonic}";
                value.text = $"+{cell.amount}";
                background.color = colorNormal;
                break;
            case HackOpKind.Jmp:
            case HackOpKind.Set:
                hexLabel.text = $"{cell.opcodeHex}";
                mnemonicLabel.text = $"{cell.mnemonic}";
                value.text = $"";
                background.color = colorNormal;
                break;
            default:
                hexLabel.text = $"";
                mnemonicLabel.text = $"";
                value.text = $"";
                background.color = colorNormal;
                break;
        }
    }

    public void SetOutline(bool on, Color color)
    {
        if (outline == null) return;
        outline.enabled = on;
        outline.color = color;
    }

    public void SetHighlighted(bool on) => SetOutline(on, stepHighlightColor);
    public void SetPreviewSelected(bool on) => SetOutline(on, selectionPreviewColor);

    
    public void MoveTo(Vector2 targetAnchoredPos, bool animate = true)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        if (!animate)
        {
            rect.anchoredPosition = targetAnchoredPos;
            return;
        }
        moveRoutine = StartCoroutine(AnimateMove(targetAnchoredPos));
    }

    private IEnumerator AnimateMove(Vector2 target)
    {
        Vector2 start = rect.anchoredPosition;
        float t = 0f;
        while (t < moveAnimationSeconds)
        {
            t += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(start, target, t / moveAnimationSeconds);
            yield return null;
        }
        rect.anchoredPosition = target;
    }

    public void SetHidden(bool hidden, bool animate = true)
    {
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        canvasGroup.blocksRaycasts = !hidden;
        float target = hidden ? 0f : 1f;

        if (!animate)
        {
            canvasGroup.alpha = target;
            return;
        }
        hideRoutine = StartCoroutine(AnimateHide(target));
    }

    private IEnumerator AnimateHide(float target)
    {
        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < hideAnimationSeconds)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / hideAnimationSeconds);
            yield return null;
        }
        canvasGroup.alpha = target;
    }

    public void ShowPopup(string text, Color color)
    {
        if (popupPrefab == null) return;
        var popup = Instantiate(popupPrefab, transform);
        popup.text = text;
        popup.color = color;
        StartCoroutine(AnimatePopup(popup.rectTransform));
    }

    private IEnumerator AnimatePopup(RectTransform popupRect)
    {
        var startPos = popupRect.anchoredPosition;
        var endPos = startPos + new Vector2(0f, 24f);
        var canvasGroupPopup = popupRect.GetComponent<CanvasGroup>();
        if (canvasGroupPopup == null) canvasGroupPopup = popupRect.gameObject.AddComponent<CanvasGroup>();

        float duration = 0.6f;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float p = time / duration;
            popupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, p);
            canvasGroupPopup.alpha = 1f - p;
            yield return null;
        }
        Destroy(popupRect.gameObject);
    }
}
