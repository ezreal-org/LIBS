using LIBS.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LIBS.ui_control
{
    class calc_spec_wave
    {
        //输入波长范围，输出对应的波长数组
        public double[] find_wave_from_range(double[] x,double[] y, double begin,double end)
        {
            
            List<double> wave_temp = new List<double>();
            List<double> spec_temp = new List<double>();

            for (int i = 0; i < x.Length; i++)
            {
                if ((x[i] >= begin) && (x[i] <= end))
                {
                    wave_temp.Add(x[i]);
                    spec_temp.Add(y[i]);
                }
            }
            double[] wave_v = wave_temp.ToArray();
            double[] spec_v = spec_temp.ToArray();

            return wave_v;

        }

        //输入波长范围，输出对应的光强数组
        public double[] find_spec_from_range(double[] x, double[] y, double begin, double end)
        {

            List<double> wave_temp = new List<double>();
            List<double> spec_temp = new List<double>();

            for (int i = 0; i < x.Length; i++)
            {
                if ((x[i] >= begin) && (x[i] <= end))
                {
                    wave_temp.Add(x[i]);
                    spec_temp.Add(y[i]);
                }
            }
            double[] wave_v = wave_temp.ToArray();
            double[] spec_v = spec_temp.ToArray();

            return spec_v;

        }

        //输入参考波长和寻峰范围，输出该范围内峰顶对应的波长和光强
        public double[] findWaveSpec_Range(spec_metadata spec_data, double wave_example, double range)
        {

            double[] re = new double[2];
            double begin = wave_example - range / 2;
            double end = wave_example + range / 2;
            re[0] = -1; re[1] = -1;

            for (int i = 0; i < spec_data.read_wave_all.Length; i++)
            {
                if ((begin <= spec_data.read_wave_all[i]) && (spec_data.read_wave_all[i] <= end))
                {
                    if (spec_data.read_spec_all_now[i] > re[1])
                    {
                        re[0] = spec_data.read_wave_all[i];
                        re[1] = spec_data.read_spec_all_now[i];
                    }
                }
                else
                {
                    continue;
                }
            }
            return re;
        }


        








    }
}
