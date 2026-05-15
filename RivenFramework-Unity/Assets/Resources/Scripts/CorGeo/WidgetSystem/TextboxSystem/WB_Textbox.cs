//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WB_Textbox : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("World Anchoring")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.0f;
    [SerializeField] private float scaleDistanceMin = 2f;
    [SerializeField] private float scaleDistanceMax = 20f;
    [SerializeField] private float floatingSpeechWidth = 600;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [NonSerialized] public Transform speechEmissionPoint;
    [NonSerialized] public SpeechStyle speechStyle;

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private Vector2 defaultAnchorMin;
    private Vector2 defaultAnchorMax;
    private Vector2 defaultAnchoredPosition;
    private Vector3 defaultScale;
    private bool defaultsCaptured;
    private SpeechStyle lastSpeechStyle;
    private bool speechStyleDirty = true;
    private Vector2 defaultSizeDelta;

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Image portrait;
    public new TMP_Text name;
    public TMP_Text chat;
    public TextboxDisplayMode displayMode;
    public bool allowPlayerToAdvanceText = true;
    [SerializeField] private Image textboxFrame;
    [SerializeField] private Animator animator;
    [SerializeField] private RectTransform rectTransform;
    private Canvas rootCanvas;
    private RectTransform canvasRect;

    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Awake()
    {
        //rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        canvasRect = rootCanvas.GetComponent<RectTransform>();

        if (worldCamera == null)
            worldCamera = FindObjectOfType<Camera>();
    }
    
    public void Update()
    {
        HandleDisplayMode();
        HandleWorldAnchor();
        HandleSpeechStyle();
    }

    private void OnDestroy()
    {
        //GameInstance.Get<GI_TextboxManager>().textEventActive = false;
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void HandleDisplayMode()
    {
        switch (displayMode)
        {
            case TextboxDisplayMode.monologue:
                portrait.enabled = false;
                name.enabled = false;
                chat.rectTransform.offsetMin = new Vector2(15, 15);
                chat.rectTransform.offsetMax = new Vector2(-15, -15);
                break;
            case TextboxDisplayMode.dialogueNoPortrait:
                portrait.enabled = false;
                name.enabled = true;
                //chat.rectTransform.offsetMin = new Vector2(15, 15);
                //chat.rectTransform.offsetMax = new Vector2(-15, -15);
                break;
            case TextboxDisplayMode.dialogue:
                portrait.enabled = true;
                name.enabled = true;
                //chat.rectTransform.offsetMin = new Vector2(100, 15);
                //chat.rectTransform.offsetMax = new Vector2(-15, -15);
                break;
            case TextboxDisplayMode.shopMono:
                SetDrawInBack();
                portrait.enabled = false;
                name.enabled = false;
                //chat.rectTransform.offsetMin = new Vector2(15, 15);
                //chat.rectTransform.offsetMax = new Vector2(-200, -15);
                break;
            case TextboxDisplayMode.shopDia:
                SetDrawInBack();
                portrait.enabled = true;
                name.enabled = true;
                //chat.rectTransform.offsetMin = new Vector2(100, 15);
                //chat.rectTransform.offsetMax = new Vector2(-200, -15);
                break;
            case TextboxDisplayMode.centered:
                SetDrawInBack();
                portrait.enabled = false;
                name.enabled = false;
                //chat.rectTransform.offsetMin = new Vector2(200, 15);
                //chat.rectTransform.offsetMax = new Vector2(-200, -15);
                break;
        }
    }

    private void HandleWorldAnchor()
    {
        if (!defaultsCaptured)
        {
            defaultAnchorMin = rectTransform.anchorMin;
            defaultAnchorMax = rectTransform.anchorMax;
            defaultAnchoredPosition = rectTransform.anchoredPosition;
            defaultScale = rectTransform.localScale;
            defaultSizeDelta = rectTransform.sizeDelta;
            defaultsCaptured = true;
        }

        if (speechEmissionPoint == null || worldCamera == null)
        {
            rectTransform.anchorMin = defaultAnchorMin;
            rectTransform.anchorMax = defaultAnchorMax;
            rectTransform.anchoredPosition = defaultAnchoredPosition;
            rectTransform.localScale = defaultScale;
            rectTransform.sizeDelta = defaultSizeDelta;
            return;
        }

        Vector3 worldPos = speechEmissionPoint.position;
        Vector3 viewportPos = worldCamera.WorldToViewportPoint(worldPos);

        float currentScale = Mathf.Lerp(maxScale, minScale,
            Mathf.InverseLerp(scaleDistanceMin, scaleDistanceMax,
                Vector3.Distance(worldCamera.transform.position, worldPos)));

        Vector2 boxSize = rectTransform.rect.size * currentScale;
        float halfW = (boxSize.x * 0.5f) / canvasRect.rect.width;
        float halfH = (boxSize.y * 0.5f) / canvasRect.rect.height;

        float vx = Mathf.Clamp(viewportPos.x, halfW, 1f - halfW);
        float vy = Mathf.Clamp(viewportPos.y, halfH, 1f - halfH);

        if (viewportPos.z < 0f)
        {
            vx = viewportPos.x < 0.5f ? 1f - halfW : halfW;
            vy = viewportPos.y < 0.5f ? 1f - halfH : halfH;
        }

        float anchoredX = (vx - 0.5f) * canvasRect.rect.width;
        float anchoredY = (vy - 0.5f) * canvasRect.rect.height;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);
        rectTransform.localScale = Vector3.one * currentScale;
        rectTransform.sizeDelta = new Vector2(floatingSpeechWidth, defaultSizeDelta.y);
    }

    private void HandleSpeechStyle()
    {
        if (!speechStyleDirty && speechStyle == lastSpeechStyle) return;
        if (animator == null) return;

        switch (speechStyle)
        {
            case SpeechStyle.normal: animator.Play("normal"); break;
            case SpeechStyle.yelling: animator.Play("yelling"); break;
            case SpeechStyle.announcement: animator.Play("announcement"); break;
            case SpeechStyle.whispering: animator.Play("whispering"); break;
            case SpeechStyle.thought: animator.Play("thought"); break;
            case SpeechStyle.radio: animator.Play("radio"); break;
        }

        lastSpeechStyle = speechStyle;
        speechStyleDirty = false;
    }    
    
    private void SetDrawInBack()
    {
        gameObject.transform.SetAsFirstSibling();
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void SetSpeechStyle(SpeechStyle style)
    {
        speechStyle = style;
        speechStyleDirty = true;
    }

    #endregion
}
