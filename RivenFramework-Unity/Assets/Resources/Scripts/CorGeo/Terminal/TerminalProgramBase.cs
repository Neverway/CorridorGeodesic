using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerminalProgramBase : MonoBehaviour
{
    protected TerminalSession session { get; private set; }

    public void Launch(TerminalSession _session)
    {
        session = _session;
        OnLaunch();
    }

    public void Terminate()
    {
        OnTerminate();
    }
    
    protected abstract void OnLaunch();

    protected virtual void OnTerminate()
    {
        
    }
    
    
   protected void RequestLaunchProgram(string programId, params string[] args) => session.controller.LaunchProgram(programId, args);
   protected void RequestExitToDefault() => session.controller.LaunchProgram(session.controller.defaultProgramId);
   protected void RequestEject() => session.controller.Eject();

}
