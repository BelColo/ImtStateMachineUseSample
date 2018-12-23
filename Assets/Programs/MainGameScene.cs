using IceMilkTea.Core;
using UnityEngine;

public class MainGameScene : MonoBehaviour
{
    // ステートマシンのイベントID列挙型
    private enum StateEventId
    {
        Play,
        Miss,
        Retry,
        Exit,
        AllBlockBloken,
        Finish,
    }



    [SerializeField]
    private int availablePlayCount = 3;
    [SerializeField]
    private Transform ballStartTransform = null;
    [SerializeField]
    private GameObject ball = null;
    [SerializeField]
    private float ballSpeed = 3.0f;
    [SerializeField]
    private Transform playerStartTransform = null;
    [SerializeField]
    private Player player = null;
    [SerializeField]
    private Block[] blocks = null;

    // ステートマシン変数の定義、もちろんコンテキストは MainGameScene クラス
    private ImtStateMachine<MainGameScene> stateMachine;
    private int missCount;



    // コンポーネントの初期化
    private void Awake()
    {
        // ステートマシンの遷移テーブルを構築（コンテキストのインスタンスはもちろん自分自身）
        stateMachine = new ImtStateMachine<MainGameScene>(this);
        stateMachine.AddTransition<ResetState, StandbyState>((int)StateEventId.Finish);
        stateMachine.AddTransition<StandbyState, PlayingState>((int)StateEventId.Play);
        stateMachine.AddTransition<PlayingState, MissState>((int)StateEventId.Miss);
        stateMachine.AddTransition<PlayingState, GameClearState>((int)StateEventId.AllBlockBloken);
        stateMachine.AddTransition<MissState, StandbyState>((int)StateEventId.Retry);
        stateMachine.AddTransition<MissState, GameOverState>((int)StateEventId.Exit);
        stateMachine.AddTransition<GameClearState, ResetState>((int)StateEventId.Finish);
        stateMachine.AddTransition<GameOverState, ResetState>((int)StateEventId.Finish);


        // 起動状態はReset
        stateMachine.SetStartState<ResetState>();
    }


    private void Start()
    {
        // ステートマシンを起動
        stateMachine.Update();
    }


    private void Update()
    {
        // ステートマシンの更新
        stateMachine.Update();
    }


    public void MissSignal()
    {
        // ステートマシンにミスイベントを送る
        stateMachine.SendEvent((int)StateEventId.Miss);
    }


    private class ResetState : ImtStateMachine<MainGameScene>.State
    {
        protected override void Enter()
        {
            foreach (var block in Context.blocks)
            {
                block.Revive();
            }


            Context.player.ResetPosition(Context.playerStartTransform.position);
            Context.player.DisableMove();
            Context.ball.GetComponent<Transform>().position = Context.ballStartTransform.position;
            Context.ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Context.missCount = 0;


            StateMachine.SendEvent((int)StateEventId.Finish);
        }
    }


    private class StandbyState : ImtStateMachine<MainGameScene>.State
    {
        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StateMachine.SendEvent((int)StateEventId.Play);
            }
        }
    }


    private class PlayingState : ImtStateMachine<MainGameScene>.State
    {
        protected override void Enter()
        {
            var xDirection = Random.Range(-1.0f, 1.0f);
            var zDirection = Random.Range(0.5f, 1.0f);
            Context.ball.GetComponent<Rigidbody>().velocity = new Vector3(xDirection, 0.0f, zDirection).normalized * Context.ballSpeed;


            Context.player.EnableMove();
        }


        protected override void Update()
        {
            var blockAllDead = true;
            foreach (var block in Context.blocks)
            {
                if (block.IsAlive)
                {
                    blockAllDead = false;
                    break;
                }
            }


            if (blockAllDead)
            {
                StateMachine.SendEvent((int)StateEventId.AllBlockBloken);
            }
        }
    }


    private class MissState : ImtStateMachine<MainGameScene>.State
    {
        protected override void Enter()
        {
            Context.player.DisableMove();
            Context.ball.GetComponent<Transform>().position = Context.ballStartTransform.position;
            Context.ball.GetComponent<Rigidbody>().velocity = Vector3.zero;


            Context.missCount += 1;
            if (Context.missCount == Context.availablePlayCount)
            {
                StateMachine.SendEvent((int)StateEventId.Exit);
                return;
            }


            StateMachine.SendEvent((int)StateEventId.Retry);
        }
    }


    private class GameClearState : ImtStateMachine<MainGameScene>.State
    {
        protected override void Enter()
        {
            Debug.Log("GameClear!!!");
            StateMachine.SendEvent((int)StateEventId.Finish);
        }
    }


    private class GameOverState : ImtStateMachine<MainGameScene>.State
    {
        protected override void Enter()
        {
            Debug.Log("GameOver...");
            StateMachine.SendEvent((int)StateEventId.Finish);
        }
    }
}