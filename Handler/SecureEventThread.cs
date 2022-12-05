using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VMP_CNR.Handler
{
    /*
    public sealed class SecureEventThread
    {
        public static SecureEventThread Instance { get; } = new SecureEventThread();

        private ConcurrentDictionary<string, ConcurrentQueue<SecureEventTask>> m_Queues = new ConcurrentDictionary<string, ConcurrentQueue<SecureEventTask>>();
        private ConcurrentDictionary<string, Thread> m_Threads = new ConcurrentDictionary<string, Thread>();

        public bool Initialized = false;


        private SecureEventThread()
        {
        }

        public void Init()
        {
            Initialized = true;
            SecureEventHandler l_InitHandler = new SecureEventHandler();
            l_InitHandler.Init();

            foreach (var l_SecureEvent in l_InitHandler.SecureEventMethods)
            {
                m_Queues[l_SecureEvent.Name] = new ConcurrentQueue<SecureEventTask>();
                m_Threads[l_SecureEvent.Name] = new Thread(() =>
                {
                    SecureEventHandler l_SecureEventHandler = new SecureEventHandler();
                    l_SecureEventHandler.Init();

                    while (true)
                    {
                        if (m_Queues[l_SecureEvent.Name].TryDequeue(out SecureEventTask l_Task))
                        {
                            if (!ValidateEvent(l_Task))
                                continue;

                            l_SecureEventHandler.ExecuteEvent(l_Task.EventObject.dbPlayer.Player, l_Task.EventObject.data.EventName, l_Task.EventObject.data.Args);
                        }
                        else
                            Thread.Sleep(5);
                    }
                });

                m_Threads[l_SecureEvent.Name].Priority = ThreadPriority.Normal;
                m_Threads[l_SecureEvent.Name].IsBackground = true;
                m_Threads[l_SecureEvent.Name].Start();
                Console.WriteLine($"Started Thread for Secure-Event: {l_SecureEvent.Name} ({m_Threads[l_SecureEvent.Name].ManagedThreadId}");
            }
        }

        private bool ValidateEvent(SecureEventTask p_Task)
        {
            switch (p_Task.EventObject.data.EventName)
            {
                case "getWeaponAmmoAnswer":
                    if (p_Task.EventObject.dbPlayer.Dimension[0] == 9999)
                        return false;

                    if (p_Task.EventObject.dbPlayer.HasData("packgun-timestamp"))
                    {
                        DateTime l_EventTime = p_Task.TimeReceived;
                        DateTime l_PackgunTime = DateTime.Parse(p_Task.EventObject.dbPlayer.GetData("packgun-timestamp").ToString());

                        if (l_EventTime < l_PackgunTime || l_EventTime < l_PackgunTime.AddSeconds(28))
                            return false;
                    }

                    break;
                default:
                    break;
            }

            return true;
        }

        public void AddToQueue(EventInformation p_Event)
        {
            SecureEventTask l_Task = new SecureEventTask()
            {
                TimeReceived = DateTime.Now,
                EventObject = p_Event
            };

            m_Queues[p_Event.data.EventName].Enqueue(l_Task);
        }
    }

    class SecureEventTask
    {
        public DateTime TimeReceived { get; set; }
        public EventInformation EventObject { get; set; }
    }*/
}
