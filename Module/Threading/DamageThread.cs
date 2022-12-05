using System;
using System.Collections.Concurrent;
using System.Threading;
using GTANetworkAPI;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Threading
{
    public sealed class DamageThread
    {
        public static DamageThread Instance { get; } = new DamageThread();
        private DamageThread() { }

        private bool Started = false;
        private ConcurrentQueue<DamageLogItem> m_PendingLogs    = new ConcurrentQueue<DamageLogItem>();
        private ConcurrentBag<Thread> m_DamageThreads           = new ConcurrentBag<Thread>();

        public void InitThreads()
        {
            if (Started)
                return;

            for (uint lItr = 0; lItr < Configurations.Configuration.Instance.DamageThreads; lItr++)
            {
                Thread l_Thread = new Thread(ThreadLoop);
                m_DamageThreads.Add(l_Thread);
            }

            uint threadIndex = 0;
            foreach (Thread l_Thread in m_DamageThreads)
            {
                threadIndex++;

                l_Thread.Priority       = ThreadPriority.BelowNormal;
                l_Thread.IsBackground   = true;
                l_Thread.Start();

                Logger.Print($"Damage-Thread {threadIndex} started!");
            }

            Started = true;
        }

        private void ThreadLoop()
        {
            while (true)
            {
                if (m_PendingLogs.TryDequeue(out DamageLogItem l_Logitem))
                {
                    DbPlayer source = l_Logitem.SourcePlayer.GetPlayer();
                    if (source == null || !source.IsValid()) continue;
                    DbPlayer target = l_Logitem.TargetPlayer.GetPlayer();
                    if (target == null || !target.IsValid()) continue;

                    string l_Query = $"INSERT INTO `log_damage` (`playerid`, `playerid_hit`, `distance`, `bone`, `damage`, `weapon`, `timestamp`) VALUES ('{source.Id}', '{target.Id}', '{l_Logitem.Distance}', '{l_Logitem.Bone}','{l_Logitem.Damage}', '{l_Logitem.PlayerWeapon}', '{l_Logitem.Timestamp}');";
                    MySQLHandler.ExecuteAsync(l_Query, Sync.MySqlSyncThread.MysqlQueueTypes.Damage);
                }
                else
                    Thread.Sleep(50);
            }
        }

        public void AddToDamageLogs(DamageLogItem p_LogData)
        {
            m_PendingLogs.Enqueue(p_LogData);
        }
    }

    public class DamageLogItem
    {
        public Player SourcePlayer { get; private set; }
        public Player TargetPlayer { get; private set; }
        public uint Damage { get; private set; }
        public uint Distance { get; private set; }
        public uint Bone { get; private set; }
        public long PlayerWeapon { get; private set; }
        public DateTime Timestamp { get; private set; }

        public DamageLogItem(Player source, Player target, uint distance, uint damage, uint bone, long playerWeapon)
        {
            SourcePlayer    = source;
            TargetPlayer    = target;
            Damage          = damage;
            Distance        = distance;
            Bone            = bone;
            PlayerWeapon    = playerWeapon;
            Timestamp       = DateTime.Now;
        }
    }
}
