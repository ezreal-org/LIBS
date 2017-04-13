using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LIBS
{
    class data_util
    {
        //将(面板/chart)鼠标位置转换成对应波长
        public static double convert_mouseposition_to_wave(int mouse_x, double wave_start_x, double wave_end_y, double controller_width)
        {
            double ret_wave = 0.0;

            double show_range = wave_end_y - wave_start_x;
            double radio_of_range = mouse_x / controller_width;
            ret_wave = wave_start_x + radio_of_range * show_range;

            return ret_wave;
        }

        //从wave数组中找到波长对应的下标,返回index为波长数组中，恰好·大于·给定波长的下标
        public static int get_index_by_wave(double[] wave_array, double wave)
        {
            for(int i=0; i < wave_array.Length; i++)
            {
                if (wave_array[i] >= wave) return i;
            }

            return wave_array.Length-1;
        }

       
        /// <summary>
        /// 根据显示基本单元和最大显示间隔数共同确定显示单位步长，规范后的区间起始结束值
        /// 规范化数据以优化显示效果
        /// 3个约束：显示最小精度、间隔数；规则简单设定为判断实际要规范化的数是否超过显示单位一半
        /// </summary>
        /// <param name="show_unit">显示基本单位</param>
        /// <param name="max_unit_cnt">最大显示间隔数目</param>
        /// <param name="actual_step">返回实际计算的间隔步长</param>
        /// <param name="minimal"></param>
        /// <param name="maximal"></param>
        public static void normalize_data_for_show(double show_unit, int max_unit_cnt, ref double actual_step, ref double minimal, ref double maximal)
        {
           if(minimal - (int)(minimal/show_unit)*show_unit < show_unit / 2)
            {
                minimal = (int)(minimal / show_unit) * show_unit;
            }
            else
            {
                minimal = (int)(minimal / show_unit) * show_unit + show_unit;
            }

            if (maximal - (int)(maximal / show_unit) * show_unit < show_unit / 2)
            {
                maximal = (int)(maximal / show_unit) * show_unit;
            }
            else
            {
                maximal = (int)(maximal / show_unit) * show_unit + show_unit;
            }

            //分段显示，确保显示的段数为 max_unit_cnt/2 ~ max_unit_cnt
            double show_range = maximal - minimal - 0.001; //为了显示
            actual_step = ((int)(show_range / show_unit) / max_unit_cnt + 1) * show_unit;
        }

        /// <summary>
        /// 调整y的显示，简单的采用 1,2,5的步长
        /// 和normalize_data_for_show的不同之处在于,只返回步长
        /// </summary>
        /// <param name="minimal_y"></param>
        /// <param name="maximal_y"></param>
        /// <returns>规范后的y显示间隔</returns>
        public static int normalize_y(double minimal_y,double maximal_y)
        {
            int y_base = 1;
            int temp = (int)(maximal_y - minimal_y);
            while (temp >= 10)
            {
                temp /= 10;
                y_base *= 10;
            }
            //简单的规则，刻度为1,2,5
            if (temp >= 4)
            {
                temp = 5;
            }
            else if (temp == 3)
            {
                temp = 2;
            }
            int y_show_unit = temp * y_base / 10;

            return y_show_unit;
        }

        //根据选择的积分区间，计算积分均值
        public static double get_integration_average(double[] wave_all, double[] spec_all, double[] env_all, double select_integration_start, double select_integration_end)
        {
            double ret_average = 0;

            int start_index = 0, end_index = 0;
            start_index = data_util.get_index_by_wave(wave_all, select_integration_start);
            end_index = data_util.get_index_by_wave(wave_all, select_integration_end) - 1;
            for (int i = start_index; i <= end_index; i++)
            {
                ret_average += (spec_all[i] - env_all[i]);
            }
            ret_average /= (end_index - start_index + 1);
            return ret_average;
        }

        public static int find_peak(double[] spec, int start_index, int end_index)
        {
            int peak_index = 0;
            double maximal_val = -1;
            for(int i=start_index; i<= end_index; i++)
            {
                if (spec[i] > maximal_val)
                {
                    maximal_val = spec[i];
                    peak_index = i;
                }
            }
            return peak_index;
        }


    }
}
