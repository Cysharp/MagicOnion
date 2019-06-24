using MagicOnion;
using System.Linq;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Sandbox.NetCoreServer.Hubs
{

    public interface ILoopReceiver
    {
        void Damage(int senderUser, int targetId, int damage);
    }

    public interface ILoopHub : IStreamingHub<ILoopHub, ILoopReceiver>
    {
        Task JoinAsync(int id);
    }


    public interface IBattleAction
    {
        void Execute(BattleContext context, DateTimeOffset clock);
    }

    public class BattleContext
    {
        GameUserData[] members;

        public IReadOnlyList<GameUserData> Members => members;
        public Random Random { get; }

        public BattleContext()
        {
            members = new GameUserData[4];
            for (int i = 0; i < members.Length; i++)
            {
                // fill CPU Characters
                members[i] = new GameUserData
                {
                    ConnectionId = Guid.Empty,
                    IsCpu = true,
                    Hp = 1000,
                };
            }

            Random = new Random();
        }

        bool AddHumanMember(GameUserData data)
        {
            return true;

            // find CPU member
            //int? memberIndex = null;
            //for (int i = 0; i < members.Length; i++)
            //{
            //    if (members[i].IsCpu)
            //    {
            //        memberIndex = i;
            //        break;
            //    }
            //}

            //if (memberIndex == null) return false;

            //members[memberIndex] = data;
        }
    }

    //public class CpuAction : IBattleAction
    //{
    //    Guid selfId;
    //    DateTimeOffset executeReserveTime;

    //    public CpuAction(Guid selfId, DateTimeOffset executeReserveTime)
    //    {
    //        this.selfId = selfId;
    //        this.executeReserveTime = executeReserveTime;
    //    }

    //    public void Execute(BattleContext context, DateTimeOffset clock)
    //    {
    //        if (executeReserveTime < clock)
    //        {
    //            return;
    //        }

    //        contex

    //    }
    //}


    public class BattleMainLoop : IGameLoopAction
    {
        readonly IGroup group;
        readonly DateTime initialTime;
        readonly object gate = new object();
        readonly Random random;


        ILoopReceiver broadcaster;
        BattleContext context;

        Queue<Action<IReadOnlyList<GameUserData>>> commandQueue = new Queue<Action<IReadOnlyList<GameUserData>>>();
        Queue<GameUserData> joinQueue = new Queue<GameUserData>();
        Queue<Guid> leaveQueue = new Queue<Guid>();

        public BattleMainLoop(IGroup group)
        {
            this.group = group;
            this.initialTime = DateTime.UtcNow;
            this.random = new Random();



            broadcaster = group.CreateBroadcaster<ILoopReceiver>();
        }

        // Join is called StreamingHub method, it is multi-thread.
        void Join(GameUserData userData)
        {
            lock (gate)
            {
                joinQueue.Enqueue(userData);
            }
        }

        void Leave(Guid connectionId)
        {
            lock (gate)
            {
                leaveQueue.Enqueue(connectionId);
            }
        }

        void DequeueAll<T>(Queue<T> q, Span<T> span)
        {
            if (q.Count == 0) return;
            var i = 0;
            while (q.Count != 0)
            {
                span[i++] = q.Dequeue();
            }
        }

        // tick per server-side frame, run on single-thread.
        public bool MoveNext()
        {
            // When group is empty, finish loop.
            // (If you want to save state when all guests leaved(and wait re-join), keep loop)
            if (group.IsEmpty)
            {
                return false;
            }

            // lifetime of loop(avoid zombie loop)
            if (DateTime.UtcNow - initialTime > TimeSpan.FromHours(1))
            {
                return false;
            }

            // dequeue from waiting queues
            lock (gate)
            {
                while (joinQueue.TryDequeue(out var joinUser))
                {
                    //roomData.Add(joinUser);
                }

                while (leaveQueue.TryDequeue(out var leaveUser))
                {
                    //roomData.RemoveAll(x => x.ConnectionId == leaveUser);
                }

                if (commandQueue.Count != 0)
                {

                }

            }

            // do commands




            // inmemory-logics.
            var storage = group.GetInMemoryStorage<GameUserData>();
            var targets = storage.AllValues.ToArray();

            var target = targets[random.Next(0, targets.Length)];

            var damage = random.Next(10, 100);
            target.Hp -= damage;

            // Broadcast All.
            broadcaster.Damage(-1, target.Id, damage);

            return true;
        }
    }

    public class GameUserData
    {
        public int Id;
        public int Hp;
        public Guid ConnectionId;
        public bool IsCpu;
    }

    public class LoopHub : StreamingHubBase<ILoopHub, ILoopReceiver>, ILoopHub
    {
        GameLoopThreadPool pool;

        public LoopHub(GameLoopThreadPool pool)
        {
            var loop = pool.GetLoopThread();
        }

        public async Task JoinAsync(int id)
        {
            var (group, _) = await Group.AddAsync("battle_group", new GameUserData
            {
                Id = id,
                Hp = 9999,
                ConnectionId = Context.ContextId
            });

            group.AtomicInvoke("BattleLogicMainLoop", () =>
            {
                var logic = new BattleMainLoop(group);
                var thread = pool.GetLoopThread();
                thread.RegisterAction(logic);
            });
        }
    }
}
