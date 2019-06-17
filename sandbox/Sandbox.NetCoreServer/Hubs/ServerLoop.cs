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



    public class BattleLogic : IGameLoopAction
    {
        IGroup group;
        DateTime initialTime;
        Random random;

        public BattleLogic(IGroup group)
        {
            this.group = group;
            this.initialTime = DateTime.UtcNow;
            this.random = new Random();
        }

        // tick per server-side frame.
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

            // inmemory-logics.
            var storage = group.GetInMemoryStorage<GameUserData>();
            var targets = storage.AllValues.ToArray();

            var target = targets[random.Next(0, targets.Length)];

            var damage = random.Next(10, 100);
            target.Hp -= damage;

            // Broadcast All.
            group.Broadcast<ILoopReceiver>().Damage(-1, target.Id, damage);

            return true;
        }
    }

    public class GameUserData
    {
        public int Id;
        public int Hp;
        public Guid ConnectionId;
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

            group.AtomicRegister("BattleLogicMainLoop", () =>
            {
                var logic = new BattleLogic(group);
                var thread = pool.GetLoopThread();
                thread.RegisterAction(logic);

                return logic;
            });
        }
    }
}
