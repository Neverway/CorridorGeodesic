using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class Logic_ElevatorFloor : MonoBehaviour
{
    public LogicInput<bool> GoToFloor = new(false);
    public LogicInput<ElevatorHandle> elevator = new(null);
    public LogicOutput<bool> IsOnFloor = new(false);
    public Transform targetPosition;

    private void Start()
    {
        if (elevator.Get() != null)
        {
            elevator.Get().AddFloor(this);
            GoToFloor.CallOnSourceChanged(BringElevatorToThisFloor);
        }
    }

    private void Update()
    {
        if (elevator.Get() != null)
        {
            IsOnFloor.Set(elevator.Get().elevatorReference.currentFloor == this);
        }
    }

    private void BringElevatorToThisFloor()
    {
        if (GoToFloor.Get())
        {
            elevator.Get().elevatorReference.GoToFloor(this);
        }
    }
}
