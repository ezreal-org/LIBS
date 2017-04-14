using LIBS.device_driver;
using LIBS.storage;
using LIBS.ui_control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LIBS.service_fun
{
    class analysis_cell_click
    {
        public static void process_cell_click(Chart chart1, DataGridView dgv, int click_row, int click_column, select_element[] elements, standard[] standards, sample[] samples, double[] wave_all, double[] env, double[,,] spec_standard, double[,,] spec_sample)
        {
            //判断该标样/样本是否读取
                //绘制波峰图
            //完成全部标样读取
                //计算方程,绘制拟合方程图
                //推算浓度，计算标准差  
            if(click_row<standards.Length && standards[click_row].is_readed || click_row>=standards.Length && samples[click_row - standards.Length].is_read)
            {
                //绘制波峰图
                //chart_select_integral_interval.draw_chart(chart1,)
            }
            else
            {
                MessageBox.Show("需要先完成读取");
                return;
            }
            for(int i = 0; i < standards.Length; i++)
            {
                if (!standards[i].is_readed)
                {
                    MessageBox.Show("需要所有标样完成读取才能计算方程");
                    return;
                }
            }

            //记录样本的多次读取数据
            double[] this_read_integration_average_strenth =null;
            double[] this_read_integration_average_concentration = null;
            int this_read_average_times = 0;

            LinearFit.LfReValue equation = get_equation(wave_all, env, standards, elements[click_column-3], spec_standard);

            if (click_row < standards.Length) //标样
            {
                this_read_average_times = standards[click_row].average_times;
                this_read_integration_average_strenth = get_oneshot_all_strength(this_read_average_times, spec_standard, click_row, wave_all, env, elements[click_column - 3]);
            }
            else
            {
                this_read_average_times = samples[click_row - standards.Length].average_times;
                this_read_integration_average_strenth = get_oneshot_all_strength(this_read_average_times, spec_sample, click_row - standards.Length, wave_all, env, elements[click_column - 3]);

            }
            for (int i = 0; i < this_read_average_times; i++)
            {
                this_read_integration_average_concentration[i] = (this_read_integration_average_strenth[i] - equation.getA()) / equation.getB();
            }
        }

        public static void read_spec_click(spec_wrapper wrapper, DataGridView dgv, int row_index, standard[] standards, sample[] samples, select_element[] elements, double[] wave_all, double[] env, double[,,] spec_standard, double[,,] spec_sample)
        {
            double[] spec_now = null;
            //读标样
            if (row_index < standards.Length)
            {
                for (int i = 0; i < standards[row_index].average_times; i++)
                {
                    //从设备获取数据
                    spec_now = wrapper.get_spec_all();
                    for (int j = 0; j < spec_now.Length; j++) //赋值进数组
                    {
                        spec_standard[row_index, i, j] = spec_now[j];
                    }
                }
                //设置已经读取，设置标样对象;通过标样确定样本读取峰，初始积分区间
                for (int j = 0; j < elements.Length; j++)
                {
                    //寻峰，确定实际读取波长，默认积分区间
                    int seek_start_index = data_util.get_index_by_wave(wave_all, elements[j].peak_wave - elements[j].seek_peak_range / 2);
                    int seek_end_index = data_util.get_index_by_wave(wave_all, elements[j].peak_wave + elements[j].seek_peak_range / 2);
                    int peak_index = data_util.find_peak(spec_now, seek_start_index, seek_end_index);
                    elements[j].peak_wave = wave_all[peak_index];
                    elements[j].interval_start = elements[j].peak_wave - 0.25; //设置默认积分范围为峰左右0.25nm
                    elements[j].interval_end = elements[j].peak_wave + 0.25;
                }
                standards[row_index].is_readed = true;
            }

            //读样本
            if (row_index >= standards.Length)
            {
                int sample_index = row_index - standards.Length;
                for (int i = 0; i < samples[sample_index].average_times; i++)
                {
                    //从设备获取数据
                    spec_now = wrapper.get_spec_all();
                    for (int j = 0; j < spec_now.Length; j++) //赋值进数组
                    {
                        spec_sample[sample_index, i, j] = spec_now[j];
                    }
                }
                
                samples[sample_index].is_read = true;
            }
            
            //更新datagrid
            double[,] standards_val = new double[standards.Length, elements.Length];
            double[,] samples_val = new double[samples.Length, elements.Length];
            //预计算表格数据
            datagrid_control.fill_datagrid_data(wave_all, env, spec_standard, spec_sample, standards, samples, elements, ref standards_val, ref samples_val);
            // 统一调用datagrid_control来控制绘制，调用直接根据规则预先进行计算
            datagrid_control.draw_datagrid_analysis(dgv, elements, standards, samples, standards_val, samples_val);


        }

        //获取某元素波长的方程，前置约束为调用者已经完成所有所有标样的读取
        private static LinearFit.LfReValue get_equation(double []wave_all, double []env,standard []standards, select_element element, double [,,]spec_standard)
        {
            //提取该元素列标样数据，得到方程
            double[] standards_integration_average_strenth = new double[standards.Length];
            //标样平均的积分平均强度
            for (int i = 0; i < standards.Length; i++) //每个标样
            {
                double standard_element_interval_average = 0.0; //平均的平均
                //如果点击的为标样，在计算方程的循环中直接获取多次积分平均值
                for (int t = 0; t < standards[i].average_times; t++)
                {
                    double[] spec_temp_all = new double[10418]; //这个10418以后改为从设备获取
                    for (int k = 0; k < 10418; k++)
                    {
                        spec_temp_all[k] = spec_standard[i, t, k];
                    }
                    double temp_integration_average = data_util.get_integration_average(wave_all, spec_temp_all, env, element.interval_start, element.interval_end);
                    standard_element_interval_average += temp_integration_average;
                }
                standards_integration_average_strenth[i] = standard_element_interval_average / standards[i].average_times;
            }
            double[] concentration = new double[standards.Length]; //浓度
            for (int j = 0; j < standards.Length; j++)
            {
                concentration[j] = standards[j].standard_ppm[element.sequece_index];
            }
            LinearFit.LfReValue equation = LinearFit.linearFitFunc(concentration, standards_integration_average_strenth, standards.Length);


            return equation;
        }

        //获取该cell对应的多次平均强度，index为标样或样本序号
        private static double[] get_oneshot_all_strength(int average_times, double[,,] src_spec,int index, double[] wave_all, double []env, select_element element)
        {
            double[] strenght_oneshot = new double[average_times];
            double[] spec_temp_all = new double[10418]; //这个10418以后改为从设备获取
            for (int t = 0; t < average_times; t++)
            {
                for (int k = 0; k < 10418; k++)
                {
                    spec_temp_all[k] = src_spec[index, t, k];
                }
                strenght_oneshot[t] = data_util.get_integration_average(wave_all, spec_temp_all, env, element.interval_start, element.interval_end);
            }
            return strenght_oneshot;
        }

    }
}
