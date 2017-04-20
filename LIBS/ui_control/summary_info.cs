using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LIBS.ui_control
{
    class summary_info
    {
        public static void draw_summary_info(Label label_info, double average_concentration, double average_strenth)
        {
            label_info.Text = "" + "        " + "浓度" + "        " + "强度" + "\r\n";
            label_info.Text = label_info.Text + "平均值：" + Math.Round(average_concentration, 3) + "       " + Math.Round(average_strenth, 3) + "\r\n";
            //label_info.Text = label_info.Text + "SD:" + "\t" + Math.Round(sampleSD[es, ew, 0], 3) + "\t" + Math.Round(sampleSD[es, ew, 1], 3) + "\r\n";
            //label_info.Text = label_info.Text + "RSD(%):" + "\t" + Math.Round(sampleRSD[es, ew, 0], 3) + "\t" + Math.Round(sampleRSD[es, ew, 1], 3) + "\r\n";
            //label_info.Text = label_info.Text + "背景:" + "\t" + "N/A" + "\t" + Math.Round(standardSpeArray[0, 1, ew], 3);
        }
    }
}
