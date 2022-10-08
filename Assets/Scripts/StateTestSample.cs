using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTestSample : StateMachineBase<StateTestSample>
{
    private void Start()
    {
        ChangeState(new StateTestSample.AState(this));
    }

    private class AState : StateBase<StateTestSample>
    {
        public AState(StateTestSample _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            Debug.Log("AStateに入りました");
        }
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                machine.ChangeState(new StateTestSample.BState(machine));
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                machine.ChangeState(new StateTestSample.CState(machine));
            }
        }
    }

    private class BState : StateBase<StateTestSample>
    {
        public BState(StateTestSample _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            Debug.Log("BStateに入りました");
        }
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                machine.ChangeState(new StateTestSample.AState(machine));
            }
        }
    }

    private class CState : StateBase<StateTestSample>
    {
        public CState(StateTestSample _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            Debug.Log("CStateに入りました");
        }
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                machine.ChangeState(new StateTestSample.AState(machine));
            }
        }
    }
}
