using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Calling
{
    public class CallingModule : Module<CallingModule>
    {
        public List<Call> ActiveCallList = new List<Call>();

        public void AddCall(Call call)
        {
            ActiveCallList.Add(call);
        }

    }
}
