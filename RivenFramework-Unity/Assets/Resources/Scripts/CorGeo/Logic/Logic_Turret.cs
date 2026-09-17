//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logic_Turret : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public LogicInput<bool> powerTurret = new(false);


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Animator animator;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        if (powerTurret.HasLogicOutputSource is false) return;
        powerTurret.CallOnSourceChanged(Toggle);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void Toggle()
    {
        if (powerTurret.Get())
        {
            animator.Play("Deploy");
        }
        else
        {
            animator.Play("Stow");
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
