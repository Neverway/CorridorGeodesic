using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Events;

public class Logic_Elevator : MonoBehaviour
{
    [Tooltip("How fast the elevator moves")]
    [SerializeField] private float moveSpeed = 3;
    
    [Tooltip("While powered, the elevator is allowed to move")]
    public LogicInput<bool> EnableElevator = new(false);
    [Tooltip("When Powered, the elevator will travel to the floor above the current floor index")]
    public LogicInput<bool> goUpFloor = new(false);
    [Tooltip("When Powered, the elevator will travel to the floor below the current floor index")]
    public LogicInput<bool> goDownFloor = new(false);
    [Tooltip("When powered, the elevator will start moving")]
    public LogicInput<bool> StartMovement = new(false);
    [Tooltip("When powered, the elevator will stop moving")]
    public LogicInput<bool> StopMovement = new(false);
    [Tooltip("When powered, the elevator doors will open")]
    public LogicInput<bool> OpenDoor = new(false);
    [Tooltip("Powered when the elevator is on its target floor")]
    public LogicOutput<bool> OnFloorReached = new(false);


    public LogicOutput<ElevatorHandle> elevatorHandle = new (new ElevatorHandle());
    public Logic_ElevatorFloor currentFloor;
    
    
    [Tooltip("What 'Floor' the elevator is moving towards, (The floor is the index in floorTargets)")]
    public LogicOutput<int> targetFloor;
    public List<Transform> elevatorFloorTargets;
    public UnityEvent OnDoorOpen, OnDoorClose;
    private EventInstance elevatorMoveInstance;
    
    private bool elevatorIsMoving;
    [SerializeField] private Animator animator;

    
    private void Start()
    {
        if (StartMovement.HasLogicOutputSource) StartMovement.CallOnSourceChanged(FuncStartMovement);
        if (StopMovement.HasLogicOutputSource) StopMovement.CallOnSourceChanged(FuncStopMovement);
        if (OpenDoor.HasLogicOutputSource) OpenDoor.CallOnSourceChanged(FuncOpenDoor);
        if (goUpFloor.HasLogicOutputSource) goUpFloor.CallOnSourceChanged(GoUpFloor);
        if (goDownFloor.HasLogicOutputSource) goDownFloor.CallOnSourceChanged(GoDownFloor);
        
        elevatorHandle.Set(new ElevatorHandle());
        elevatorHandle.Get().SetElevatorTarget(this);
        elevatorMoveInstance = Audio_FMODAudioManager.CreateInstance(Audio_FMODEvents.Instance.elevatorMove);
    }

    private void Update()
    {
        Update3DAttributes();
    }

    private void Update3DAttributes()
    {
        FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(transform.position);

        elevatorMoveInstance.set3DAttributes(attributes);
    }
    
    private IEnumerator MoveElevator(Vector3 targetPosition)
    {
        elevatorIsMoving = true;
        elevatorMoveInstance.start();
        OnFloorReached.Set(false);

        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        elevatorIsMoving = false;
        elevatorMoveInstance.stop(STOP_MODE.IMMEDIATE);
        OnFloorReached.Set(true);
        targetFloor.Set(targetFloor.Get()+1);

        if (targetFloor == elevatorFloorTargets.Count)
        {
            targetFloor.Set(0);
        }
        Debug.Log("Elevator Reached target");
    }

    private IEnumerator MoveElevator(Logic_ElevatorFloor elevatorFloor)
    {
        Debug.Log("MoveElevator Called");
        Vector3 targetPos = elevatorFloor.targetPosition.position;
        yield return MoveElevator(targetPos);
        currentFloor = elevatorFloor;
    }

    private void FuncStartMovement()
    {
        if (EnableElevator.HasLogicOutputSource) if (EnableElevator.Get() == false) return;
        if (elevatorIsMoving) return;
        StartCoroutine(MoveElevator(elevatorFloorTargets[targetFloor].position));
    }

    private void FuncStopMovement()
    {
        if (EnableElevator.HasLogicOutputSource) if (EnableElevator.Get() == false) return;
        elevatorIsMoving = false;
        StopAllCoroutines();
    }

    private void FuncOpenDoor()
    {
        if (OpenDoor.Get()) OnDoorOpen.Invoke();
        else OnDoorClose.Invoke();
        animator.SetBool("Powered", OpenDoor.Get());
    }

    public void GoToFloor(Logic_ElevatorFloor elevatorFloor)
    {
        Debug.Log("Called GoToFloor");
        if (EnableElevator.HasLogicOutputSource) if (EnableElevator.Get() == false) return;
        if (elevatorIsMoving) return;
        Debug.Log("GoToFloor Passed");
        StartCoroutine(MoveElevator(elevatorFloor));
    }

    private void NextFloor(bool goingDown)
    {
        int currentFloorIndex = elevatorHandle.Get().floors.IndexOf(currentFloor);
        int targetFloorIndex = currentFloorIndex + (goingDown ? -1 : 1);
        if (elevatorHandle.Get().floors.IsIndexOutOfRange(targetFloorIndex))
        {
            return;
        }
        // -3-
        GoToFloor(elevatorHandle.Get().floors[targetFloorIndex]);
    }

    private void GoUpFloor()
    {
        if (goUpFloor.Get()) NextFloor(false);
    }

    private void GoDownFloor()
    {
        if (goDownFloor.Get()) NextFloor(true);
    }
    
    [ContextMenu("Create Elevator Handle")]
    private void CreateElevatorHandle()
    {
        elevatorHandle = new LogicOutput<ElevatorHandle>(new ElevatorHandle());
    }
    
    
    private void OnDestroy()
    {
        elevatorMoveInstance.stop(STOP_MODE.IMMEDIATE);
        elevatorMoveInstance.release();
    }
    private void OnDisable()
    {
        elevatorMoveInstance.stop(STOP_MODE.IMMEDIATE);
        elevatorMoveInstance.release();
    }
    
    
    
    
    
    
    
    
    
    
}

// Welcome to "The very very bottom"
public class ElevatorHandle
{
    public List<Logic_ElevatorFloor> floors = new ();
    public Logic_Elevator elevatorReference;

    public void SetElevatorTarget(Logic_Elevator targetElevator)
    {
        elevatorReference = targetElevator;
    }

    /// <summary>
    /// Registers a floor with an elevator's travel positions
    /// </summary>
    public void AddFloor(Logic_ElevatorFloor floor)
    {
        Debug.Log("AddFloor");
        floors.Add(floor);
        SortFloors();
        if (elevatorReference.currentFloor == null)
        {
            elevatorReference.currentFloor = floor;
        }
        else
        {
            Vector3 currentFloorPos = elevatorReference.currentFloor.targetPosition.position;
            Vector3 newFloorPos = floor.targetPosition.position;
            if (Vector3.Distance(elevatorReference.transform.position, currentFloorPos) > Vector3.Distance(elevatorReference.transform.position, newFloorPos))
            {
                elevatorReference.currentFloor = floor;
            }
        }
    }

    /// <summary>
    /// Sorts the Logic_ElevatorFloors by the Y height of the floor's target position
    /// </summary>
    private void SortFloors()
    {
        floors = floors.OrderBy(x => x.targetPosition.position.y).ToList();
    }
}