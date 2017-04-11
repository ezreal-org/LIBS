using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LIBS.service_fun
{
    class calculate_integration_average
    {
        //根据选择的积分区间，计算积分均值
        public static double get_integration_average(double[] wave_all, double[] spec_all,double[]env_all, double select_integration_start, double select_integration_end)
        {
            double ret_average = 0;

            int start_index = 0, end_index = 0;
            start_index = data_util.get_index_by_wave(wave_all, select_integration_start);
            end_index = data_util.get_index_by_wave(wave_all, select_integration_end)-1;
            for(int i = start_index; i <= end_index; i++)
            {
                ret_average += (spec_all[i]-env_all[i]);
            }
            ret_average /= (end_index - start_index+1);
            return ret_average;
        }
    }
}
