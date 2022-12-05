using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Customization
{
    public class AppearanceData
    {
        private int BeardColor;
        private int EyebrowColor;
        private int LipstickColor;

        public AppearanceData(int beardColor, int eyebrowColor, int lipstickColor)
        {
            BeardColor = beardColor;
            EyebrowColor = eyebrowColor;
            LipstickColor = lipstickColor;
        }
    }
}
