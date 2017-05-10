using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LIBS.ui_control
{
    class summary_info
    {
        //@haze:添加两个参数：强度数组和浓度数组用于计算方差和标准差
        public static void draw_summary_info(Label label_info, double average_concentration, 
            double average_strenth,double[] array_qiandu,double[] array_londu)
        {
            label_info.Text = "" + "        " + "浓度" + "        " + "强度" + "\r\n";
            label_info.Text = label_info.Text + "平均值:" + Math.Round(average_concentration, 3) + "       " + Math.Round(average_strenth, 3) + "\r\n";
            //@haze:添加方差和标准差的显示
            label_info.Text = label_info.Text + "SD:    " + "\t" + Math.Round(data_util.calc_SD(array_londu), 3) + "       " + Math.Round(data_util.calc_SD(array_qiandu), 3) + "\r\n";
            label_info.Text = label_info.Text + "%RSD:  " + "\t" + Math.Round(data_util.calc_RSD(array_londu), 3) + "       " + Math.Round(data_util.calc_RSD(array_qiandu), 3) + "\r\n";
        }
    }
}
