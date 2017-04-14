using LIBS.storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LIBS.ui_control
{
    class datagrid_control
    {
        public static bool show_strenth = false; //默认显示浓度
        public static bool is_datagrid_analysis_exist = false; //根据这个标识判断是否需要new新表格还是重用旧表格

        //这部分只负责根据数据绘图,standard_val/sample_val为强度或者浓度
        public static void draw_datagrid_analysis(DataGridView dgv, select_element[] elements, standard[] standards, sample[] samples, double[,] standard_val, double[,] sample_val)
        {
            //列
            DataTable dt8 = new DataTable();
            dt8.Columns.Add("序号", typeof(string));
            dt8.Columns.Add("溶液标签", typeof(string));
            dt8.Columns.Add("读取状态", typeof(string));
            for (int i = 0; i < elements.Length; i++)
            {
                dt8.Columns.Add(elements[i].element + "(" + elements[i].select_wave + ")", typeof(string));
            }

            //行1 标准样本的数据显示
            for (int i = 0; i < standards.Length; i++)
            {
                DataRow dr8 = dt8.NewRow();
                dr8[0] = (i + 1).ToString();
                dr8[1] = standards[i].standard_label;
                if (standards[i].is_readed)
                {
                    dr8[2] = "已读取";
                }
                else
                {
                    dr8[2] = "未读取";
                }
                for (int j = 0; j < elements.Length; j++)
                {
                    dr8[j + 3] = Math.Round(standard_val[i, j], 1);

                }
                dt8.Rows.Add(dr8);

            }

            //行2 样本数据的显示
            for (int i = 0; i < samples.Length; i++)
            {
                DataRow dr8 = dt8.NewRow();
                dr8[0] = (standards.Length + 1 + i).ToString();
                dr8[1] = samples[i].sample_label;
                if (samples[i].is_read)
                {
                    dr8[2] = "已读取";
                }
                else
                {
                    dr8[2] = "未读取";
                }
                for (int j = 0; j < elements.Length; j++)
                {
                    dr8[j + 3] = Math.Round(sample_val[i, j], 1);

                }
                dt8.Rows.Add(dr8);
            }
            dgv.DataSource = dt8;
            for (int i = 0; i < standards.Length; i++)
            {
                if (standards[i].is_readed)
                {
                    dgv.Rows[i].Cells[2].Style.ForeColor = Color.Blue;
                }
                else
                {
                    dgv.Rows[i].Cells[2].Style.ForeColor = Color.Gray;
                }
            }

            for (int i = 0; i < samples.Length; i++)
            {
                if (samples[i].is_read)
                {
                    dgv.Rows[i + standards.Length].Cells[2].Style.ForeColor = Color.Blue;
                }
                else
                {
                    dgv.Rows[i + standards.Length].Cells[2].Style.ForeColor = Color.Gray;
                }
            }

        }

        //根据实际测量数据spec_standard和spec_sample，选择的各元素，标样，样本，计算出将要显示的表格数据
        //返回值standard_val和sample_val分别为计算后表格要显示的标样/样本的浓度或强度数据
        public static void fill_datagrid_data(double[] wave_all, double[] env, double[,,] spec_standard, double[,,] spec_sample, standard[] standards, sample[] samples, select_element[] elements, ref double[,] standard_val, ref double[,] sample_val)
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
                //所有标样都已经读取完，才需要计算样本浓度
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
