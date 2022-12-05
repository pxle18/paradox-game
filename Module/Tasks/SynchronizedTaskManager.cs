using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module.Tasks
{
    public sealed class SynchronizedTaskManager
    {
        public static SynchronizedTaskManager Instance { get; } = new SynchronizedTaskManager();

        private readonly Thread mainThread;
        private bool hasToStop;

        private readonly ConcurrentQueue<SynchronizedTask> taskQueue = new ConcurrentQueue<SynchronizedTask>();

        private SynchronizedTaskManager()
        {
            mainThread = new Thread(MainLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };

            mainThread.Start();
        }

        public void Add(SynchronizedTask synchronizedTask)
        {
            if (!synchronizedTask.CanExecute())
                return;

            taskQueue.Enqueue(synchronizedTask);
        }

        public void Shutdown()
        {
            hasToStop = true;
            mainThread.Abort();
        }

        private void MainLoop()
        {
            while (!hasToStop)
            {
                try
                {
                    if (taskQueue.TryDequeue(out SynchronizedTask syncTask))
                    {
                        if (syncTask.CanExecute())
                        {
                            try
                            {
                                syncTask.Execute();
                            }
                            catch (Exception e)
                            {
                                Logger.Print("TASK EXECUTION FAILURE");
                                Logger.Print(e.ToString());
                            }
                        }
                    }
                    else
                        Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    Logger.Print("SYNCHRONIZATION FAILURE");
                    Logger.Print(ex.ToString());
                }
            }
        }
    }
}