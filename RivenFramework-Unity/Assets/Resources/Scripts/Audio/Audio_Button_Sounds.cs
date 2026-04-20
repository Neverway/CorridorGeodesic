//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Plays audio clips for button events like hovering or selecting
// Notes:
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Audio_Button_Sounds : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private Button button;
    [SerializeField] private UnityEvent hover;
    [SerializeField] private UnityEvent select;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData _pointerEventData)
    {
        Audio_FMODAudioManager.PlayOneShot(Audio_FMODEvents.Instance.hover);
    }

    public void OnPointerDown(PointerEventData _pointerEventData)
    {
        Audio_FMODAudioManager.PlayOneShot(Audio_FMODEvents.Instance.select);
    }


    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}