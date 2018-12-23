using IceMilkTea.Core;
using UnityEngine;

public class Block : MonoBehaviour
{
    // 状態イベントの定義
    private enum StateEventId
    {
        Dead,
        Revive,
    }


    // 現在の状態が生存状態なら生存していることを返すプロパティ
    public bool IsAlive => stateMachine.IsCurrentState<AliveState>();


    private ImtStateMachine<Block> stateMachine;



    private void Awake()
    {
        stateMachine = new ImtStateMachine<Block>(this);
        stateMachine.AddTransition<AliveState, DeadState>((int)StateEventId.Dead);
        stateMachine.AddTransition<DeadState, AliveState>((int)StateEventId.Revive);


        stateMachine.SetStartState<AliveState>();
    }


    private void Start()
    {
        stateMachine.Update();
    }


    private void Update()
    {
        stateMachine.Update();
    }


    private void OnCollisionEnter(Collision collision)
    {
        // 衝突した相手がボールなら
        if (collision.gameObject.name == "Ball")
        {
            // 死亡イベントを送る
            stateMachine.SendEvent((int)StateEventId.Dead);
        }
    }


    public void Revive()
    {
        // ステートマシンに復活イベントを送る
        stateMachine.SendEvent((int)StateEventId.Revive);
    }



    private class AliveState : ImtStateMachine<Block>.State
    {
        protected override void Enter()
        {
            Context.GetComponent<MeshRenderer>().enabled = true;
            Context.GetComponent<Collider>().enabled = true;
        }
    }


    private class DeadState : ImtStateMachine<Block>.State
    {
        protected override void Enter()
        {
            Context.GetComponent<MeshRenderer>().enabled = false;
            Context.GetComponent<Collider>().enabled = false;
        }
    }
}