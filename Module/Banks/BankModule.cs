using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMP_CNR.Module.Banks
{
    public sealed class BankModule : SqlModule<BankModule, Bank, uint>
    {
        public static int MinToBreakAgain = 30;
        public static int MaxAtmsBreakableAtOneTime = 10;

        protected override string GetQuery()
        {
            return "SELECT * FROM `bank` ORDER BY id;";
        }

        public override void OnFiveMinuteUpdate()
        {
            try
            {
                int breakableAtMoment = GetAll().Values.Where(b => b.ActivatedToBreak && b.Type == 1).Count();
                // Bereits genug ATMs verfügbar
                if (breakableAtMoment >= MaxAtmsBreakableAtOneTime) return;

                List<Bank> availableAtms = GetAll().Values.Where(b => !b.ActivatedToBreak && b.Type == 1 && b.LastBreaked.AddMinutes(MinToBreakAgain) < DateTime.Now).ToList();

                Random rnd = new Random();

                for (int i = 1; i <= (MaxAtmsBreakableAtOneTime - breakableAtMoment); i++)
                {
                    int index = rnd.Next(availableAtms.Count);
                    availableAtms[index].ActivatedToBreak = true;
                }
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }
    }
}
