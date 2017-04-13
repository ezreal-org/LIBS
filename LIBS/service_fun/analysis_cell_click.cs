using LIBS.device_driver;
using LIBS.storage;
using LIBS.ui_control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LIBS.service_fun
{
    class analysis_cell_click
    {
        public static void process_cell_click(DataGridView dgv, int click_row, int click_column, select_element[] elements, standard[] standards, sample[] samples, double[] wave_all, double[] env, double[,,] spec_standard, double[,,] spec_sample)
        {
            //和fill_datagrid很多部分可以合并
            int average_times = 0;
            if (click_row < standards.Length)
            {
                average_times = standards[click_row].average_times;
            }
            else
            {
                average_times = samples[click_row - standards.Length].average_times;
            }
            //提取该元素列标样数据，得到方程
            double[] standards_integration_average_strenth = new double[standards.Length];
            double[,] this_read_integration_average_strenth_concentration = new double[average_times, 2];

            //标样平均的积分平均强度
            for (int i = 0; i < standards.Length; i++) //每个标样
            {
                double standard_element_interval_average = 0.0; //平均的平均
                for (int t = 0; t < standards[i].average_times; t++)
                {
                    double[] spec_temp_all = new double[10418]; //这个10418以后改为从设备获取
                    for (int k = 0; k < 10418; k++)
                    {
                        spec_temp_all[k] = spec_standard[i, t, k];
                    }
                    double temp_integration_average = data_util.get_integration_average(wave_all, spec_temp_all, env, elements[click_column - 3].interval_start, elements[click_column - 3].interval_end);
                    standard_element_interval_average += temp_integration_average;
                    if (i == click_row) this_read_integration_average_strenth_concentration[t,0] = temp_integration_average;
                }
                standards_integration_average_strenth[i] = standard_element_interval_average / standards[i].average_times;
            }
            double[] concentration = new double[standards.Length]; //浓度
            for (int j = 0; j < standards.Length; j++)
            {
                concentration[j] = standards[j].standard_ppm[click_column-3];
            }
            LinearFit.LfReValue equation = LinearFit.linearFitFunc(concentration, standards_integration_average_strenth, standards.Length);
            //根据方程进行样本浓度推算
            //点击行为样本时，记录样本的多次读取数据
            if (click_row > standards.Length)
            {
                for (int t = 0; t < samples[click_row-standards.Length].average_times; t++)
                {
                    double[] spec_temp_all = new double[10418]; //这个10418以后改为从设备获取
                    for (int k = 0; k < 10418; k++)
                    {
                        spec_temp_all[k] = spec_sample[click_row - standards.Length, t, k];
                    }
                    double temp_integration_average = data_util.get_integration_average(wave_all, spec_temp_all, env, elements[click_column - 3].interval_start, elements[click_column - 3].interval_end);
                    this_read_integration_average_strenth_concentration[t, 0] = temp_integration_average;
                }
            }
            for(int i = 0; i < average_times; i++)
            {
                this_read_integration_average_strenth_concentration[i, 1] = (this_read_integration_average_strenth_concentration[i, 0] - equation.getA()) / equation.getB();
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
                //设置已经读取，设置标样对象
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
                //仅根据标样确定方程，样本不改变积分区间和峰
                ////设置已经读取，设置标样对象
                //for (int j = 0; j < elements.Length; j++)
                //{
                //    //寻峰，确定实际读取波长，默认积分区间
                //    int seek_start_index = data_util.get_index_by_wave(wave_all, elements[j].peak_wave - elements[j].seek_peak_range / 2);
                //    int seek_end_index = data_util.get_index_by_wave(wave_all, elements[j].peak_wave + elements[j].seek_peak_range / 2);
                //    int peak_index = data_util.find_peak(spec_now, seek_start_index, seek_end_index);
                //    elements[j].peak_wave = wave_all[peak_index];
                //    elements[j].interval_start = elements[j].peak_wave - 0.25; //设置默认积分范围为峰左右0.25nm
                //    elements[j].interval_end = elements[j].peak_wave + 0.25;
                //}
                samples[sample_index].is_read = true;
            }

            //更新datagrid
            double[,] standards_val = new double[standards.Length, elements.Length];
            double[,] samples_val = new double[samples.Length, elements.Length];
            //预计算表格数据
            fill_datagrid_data(wave_all, env, spec_standard, spec_sample, standards, samples, elements, ref standards_val, ref samples_val);
            // 统一调用datagrid_control来控制绘制，调用直接根据规则预先进行计算
            datagrid_control.draw_datagrid_analysis(dgv, elements, standards, samples, standards_val, samples_val);


        }

        static void fill_datagrid_data(double[] wave_all, double[] env, double[,,] spec_standard, double[,,] spec_sample, standard[] standards, sample[] samples, select_element[] elements, ref double[,] standard_val, ref double[,] sample_val)
        {
            double[,] standards_integration_average_strenth = new double[standards.Length, elements.Length];
            double[,] samples_integration_average_strenth = new double[samples.Length, elements.Length];
            //标样平均的积分平均强度
            for (int i = 0; i < standards.Length; i++) //每个标样
            {
                for (int j = 0; j < elements.Length; j++)  //每一个元素
                {
                    double standard_element_interval_average = 0.0; //平均的平均
                    for (int t = 0; t < standards[i].average_times; t++)
                    {
                        double[] spec_temp_all = new double[10418]; //这个10418以后改为从设备获取
                        for (int k = 0; k < 10418; k++)
                        {
                            spec_temp_all[k] = spec_standard[i, t, k];
                        }
                        standard_element_interval_average += data_util.get_integration_average(wave_all, spec_temp_all, env, elements[j].interval_start, elements[j].interval_end);
                    }
                    standards_integration_average_strenth[i, j] = standard_element_interval_average / standards[i].average_times;
                }
            }
            //样本的平均积分平均强度
            for (int i = 0; i < samples.Length; i++) //每个样本
            {
                for (int j = 0; j < elements.Length; j++)  //每一个元素
                {
                    double sample_element_interval_average = 0.0; //平均的平均
                    for (int t = 0; t < samples[i].average_times; t++)
                    {
                        double[] spec_temp_all = new double[10418]; //这个10418以后改为从设备获取
                        for (int k = 0; k < 10418; k++)
                        {
                            spec_temp_all[k] = spec_sample[i, t, k];
                        }
                        sample_element_interval_average += data_util.get_integration_average(wave_all, spec_temp_all, env, elements[j].interval_start, elements[j].interval_end);
                    }
                    samples_integration_average_strenth[i, j] = sample_element_interval_average / samples[i].average_times;
                }
            }
            //显示强度,直接提取spec数组数据,根据元素(波长)积分区间来得到计算值
            if (datagrid_control.show_strenth)
            {
                standard_val = standards_integration_average_strenth;
                sample_val = samples_integration_average_strenth;
            }
            // 当已经读取所有标样，则计算线性拟合方程，计算样本浓度
            else
            {
                //标样显示浓度为设置的值
                bool is_need_calculate = true;
                for (int i = 0; i < standards.Length; i++)
                {
                    if (!standards[i].is_readed) is_need_calculate = false; //标样没有全部读取完，则不需要计算方程
                    for (int j = 0; j < elements.Length; j++)
                    {
                        standard_val[i, j] = standards[i].standard_ppm[j];
                    }
                }
                //样本需要根据线性方程进行计算
                if (is_need_calculate)
                {
                    // 根据标样浓度和对应强度计算
                    for (int i = 0; i < elements.Length; i++)
                    {
                        double[] concentration = new double[standards.Length]; //浓度
                        double[] strenth = new double[standards.Length];
                        for (int j = 0; j < standards.Length; j++)
                        {
                            concentration[j] = standards[j].standard_ppm[i];
                            strenth[j] = standards_integration_average_strenth[j, i];
                        }
                        LinearFit.LfReValue equation = LinearFit.linearFitFunc(concentration, strenth, standards.Length);
                        //根据方程进行样本浓度推算
                        for (int j = 0; j < samples.Length; j++)
                        {
                            sample_val[j, i] = (samples_integration_average_strenth[j, i] - equation.getA()) / equation.getB();
                        }
                    }
                }
                else //不需要计算时设置为0
                {
                    for (int i = 0; i < samples.Length; i++)
                    {
                        for (int j = 0; j < elements.Length; j++)
                        {
                            sample_val[i, j] = 0;
                        }
                    }
                }
            }
        }
    }
}
