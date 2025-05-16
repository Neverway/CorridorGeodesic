//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Item_Dev_KINMachine : Item
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [SerializeField] private int currentToolMode;
    public List<ToolMode> toolModes = new List<ToolMode>();

    //=-----------------=
    // Private Variables
    //=-----------------=
    private GameObject actorClipboard;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private Animator animator;
    [SerializeField] private SpriteRenderer toolModeSprite;
    [SerializeField] private GameObject laser;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        animator = GetComponent<Animator>();
        toolModeSprite.sprite = toolModes[currentToolMode].toolModeIcon;
    }


    //=-----------------=
    // Internal Functions
    //=-----------------=
    public void CycleNext()
    {
        if (currentToolMode + 1 >= toolModes.Count)
        {
            currentToolMode = 0;
        }
        else
        {
            currentToolMode++;
        }

        toolModeSprite.sprite = toolModes[currentToolMode].toolModeIcon;
    }


    //=-----------------=
    // External Functions
    //=-----------------=
    public override void ItemAction1()
    {
        animator.SetBool("AltFiring", false);
        animator.SetBool("Firing", true);
        var hit = new RaycastHit();
        switch (toolModes[currentToolMode].toolModeName)
        {
            case "PhysGun":
                print("PhysGun");
                break;
            case "GravityGun":
                print("GravityGun");
                break;
            case "PortalGun":
                print("PortalGun");
                break;
            case "Duplicator":
                if (actorClipboard is null) break;
                laser.SetActive(true);
                if (Physics.Raycast(transform.parent.position, transform.parent.forward, out hit, 25))
                {
                    Instantiate(actorClipboard, hit.point, new Quaternion(), null);
                }
                break;
            case "Remover":
                laser.SetActive(true);
                if (Physics.Raycast(transform.parent.position, transform.parent.forward, out hit, 25))
                {
                    var targetActor = hit.collider.GetComponent<Actor>();
                    if (targetActor)
                    {
                        Destroy(targetActor.gameObject);
                        break;
                    }
                    targetActor = hit.collider.transform.parent.GetComponent<Actor>();
                    if (targetActor)
                    {
                        Destroy(targetActor.gameObject);
                        break;
                    }
                    targetActor = hit.collider.transform.parent.transform.parent.GetComponent<Actor>();
                    if (targetActor)
                    {
                        Destroy(targetActor.gameObject);
                    }
                }
                break;
        }
    }
    public override void ItemAction2()
    {
        animator.SetBool("Firing", false);
        animator.SetBool("AltFiring", true);
        var hit = new RaycastHit();
        switch (toolModes[currentToolMode].toolModeName)
        {
            case "PhysGun":
                break;
            case "GravityGun":
                break;
            case "PortalGun":
                break;
            case "Duplicator":
                if (Physics.Raycast(transform.parent.position, transform.parent.forward, out hit, 25))
                {
                    laser.SetActive(true);
                    var targetActor = hit.collider.GetComponent<Actor>();
                    if (targetActor)
                    {
                        actorClipboard = targetActor.gameObject;
                        break;
                    }
                    targetActor = hit.collider.transform.parent.GetComponent<Actor>();
                    if (targetActor)
                    {
                        actorClipboard = targetActor.gameObject;
                        break;
                    }
                    targetActor = hit.collider.transform.parent.transform.parent.GetComponent<Actor>();
                    if (targetActor)
                    {
                        actorClipboard = targetActor.gameObject;
                    }
                }
                break;
            case "Remover":
                break;
        }
    }
    public override void ItemAction3()
    {
        CycleNext();
    }

    public override void ItemReleaseAction1()
    {
        animator.SetBool("Firing", false);
        laser.SetActive(false);
    }
    public override void ItemReleaseAction2()
    {
        animator.SetBool("AltFiring", false);
        laser.SetActive(false);
    }
}

[Serializable]
public class ToolMode
{
    public string toolModeName;
    public Sprite toolModeIcon;
}