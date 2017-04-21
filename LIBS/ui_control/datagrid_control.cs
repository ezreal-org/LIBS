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
        public static void draw_datagrid_analysis(DataGridView dgv, spec_metadata spec_obj)
        {
            select_element[] elements = spec_obj.elements;
            standard[] standards = spec_obj.standards;
            sample[] samples = spec_obj.samples;
            int standard_cnt = spec_obj.standard_cnt;
            int sample_cnt = spec_obj.sample_cnt;
            int element_cnt = spec_obj.element_cnt;
            double[,] standard_val = new double[standard_cnt, element_cnt];
            double[,] sample_val = new double[standard_cnt, element_cnt];
            fill_datagrid_data(spec_obj, ref standard_val, ref sample_val);

            //列
            DataTable dt8 = new DataTable();
            dt8.Columns.Add("序号", typeof(string));
            dt8.Columns.Add("溶液标签", typeof(string));
            dt8.Columns.Add("读取状态", typeof(string));
            for (int i = 0; i < element_cnt; i++)
            {
                dt8.Columns.Add(elements[i].element + "(" + elements[i].select_wave + ")", typeof(string));
            }

            //行1 标准样本的数据显示
            for (int i = 0; i < standard_cnt; i++)
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
                for (int j = 0; j < element_cnt; j++)
                {
                    dr8[j + 3] = Math.Round(standard_val[i, j], 1);

                }
                dt8.Rows.Add(dr8);

            }

            //行2 样本数据的显示
            for (int i = 0; i < sample_cnt; i++)
            {
                DataRow dr8 = dt8.NewRow();
                dr8[0] = (standard_cnt + 1 + i).ToString();
                dr8[1] = samples[i].sample_label;
                if (samples[i].is_read)
                {
                    dr8[2] = "已读取";
                }
                else
                {
                    dr8[2] = "未读取";
                }
                for (int j = 0; j < element_cnt; j++)
                {
                    dr8[j + 3] = Math.Round(sample_val[i, j], 1);

                }
                dt8.Rows.Add(dr8);
            }
            dgv.DataSource = dt8;
            for (int i = 0; i < standard_cnt; i++)
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

            for (int i = 0; i < sample_cnt; i++)
            {
                if (samples[i].is_read)
                {
                    dgv.Rows[i + standard_cnt].Cells[2].Style.ForeColor = Color.Blue;
                }
                else
                {
                    dgv.Rows[i + standard_cnt].Cells[2].Style.ForeColor = Color.Gray;
                }
            }

        }

        //根据实际测量数据spec_standard和spec_sample，选择的各元素，标样，样本，计算出将要显示的表格数据
        //返回值standard_val和sample_val分别为计算后表格要显示的标样/样本的浓度或强度数据
        private static void fill_datagrid_data(spec_metadata spec_obj, ref double[,] standard_val, ref double[,] sample_val)
        {
            select_element[] elements = spec_obj.elements;
            standard[] standards = spec_obj.standards;
            sample[] samples = spec_obj.samples;
            double[] wave_all = spec_obj.read_wave_all;
            double[] env = spec_obj.env_spec;
            double[,,] spec_standard = spec_obj.read_standard_spec;
            double[,,] spec_sample = spec_obj.read_sample_spec;
            int standard_cnt = spec_obj.standard_cnt;
            int sample_cnt = spec_obj.sample_cnt;
            int element_cnt = spec_obj.element_cnt;
            int pixel_cnt = spec_obj.read_wave_all.Length;

            double[,] standards_integration_average_strenth = new double[standard_cnt, element_cnt];
            double[,] samples_integration_average_strenth = new double[sample_cnt, element_cnt];
            //标样平均的积分平均强度
            for (int i = 0; i < standard_cnt; i++) //每个标样
            {
                if (!standards[i].is_readed) continue;
                for (int t = 0; t < standards[i].average_times; t++)
                {
                    double[] spec_temp_all = new double[pixel_cnt];
                    for (int k = 0; k < pixel_cnt; k++)
                    {
                        spec_temp_all[k] = spec_standard[i, t, k];
                    }
                    for (int j = 0; j < element_cnt; j++)  //每一个元素
                    {
                        standards_integration_average_strenth[i, j] += data_util.get_integration_average(wave_all, spec_temp_all, env, elements[j].interval_start, elements[j].interval_end);
                    }
                }
                for(int j = 0; j < element_cnt; j++)
                {
                    standards_integration_average_strenth[i, j] /= standards[i].average_times;
                }    
            }
            //样本的平均积分平均强度
            for (int i = 0; i < sample_cnt; i++) //每个标样
            {
                if (!samples[i].is_read) continue;
                for (int t = 0; t < samples[i].average_times; t++)
                {
                    double[] spec_temp_all = new double[pixel_cnt];
                    for (int k = 0; k < pixel_cnt; k++)
                    {
                        spec_temp_all[k] = spec_sample[i, t, k];
                    }
                    for (int j = 0; j < element_cnt; j++)  //每一个元素
                    {
                        samples_integration_average_strenth[i, j] += data_util.get_integration_average(wave_all, spec_temp_all, env, elements[j].interval_start, elements[j].interval_end);
                    }
                }
                for (int j = 0; j < element_cnt; j++)
                {
                    samples_integration_average_strenth[i, j] /= samples[i].average_times;
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
                for (int i = 0; i < standard_cnt; i++)
                {
                    if (!standards[i].is_readed) is_need_calculate = false; //标样没有全部读取完，则不需要计算方程
                    for (int j = 0; j < element_cnt; j++)
                    {
                        standard_val[i, j] = standards[i].standard_ppm[j];
                    }
                }
                //样本需要根据线性方程进行计算
                if (is_need_calculate)
                {
                    // 根据标样浓度和对应强度计算
                    for (int i = 0; i < element_cnt; i++)
                    {
                        double[] concentration = new double[standard_cnt]; //浓度
                        double[] strenth = new double[standard_cnt];
                        for (int j = 0; j < standard_cnt; j++)
                        {
                            concentration[j] = standards[j].standard_ppm[i];
                            strenth[j] = standards_integration_average_strenth[j, i];
                        }
                        LinearFit.LfReValue equation = LinearFit.linearFitFunc(concentration, strenth, standard_cnt);
                        //根据方程进行样本浓度推算
                        for (int j = 0; j < standard_cnt; j++)
                        {
                            sample_val[j, i] = (samples_integration_average_strenth[j, i] - equation.getA()) / equation.getB();
                        }
                    }
                }
                else //不需要计算时设置为0
                {
                    for (int i = 0; i < sample_cnt; i++)
                    {
                        for (int j = 0; j < element_cnt; j++)
                        {
                            sample_val[i, j] = 0;
                        }
                    }
                }
            }
        }

        public static void draw_datagrid_snapshot(DataGridView dgv, double []concentration, double[] strenth)
        {
            DataTable dt9 = new DataTable();
            dt9.Columns.Add("重复项", typeof(string));
            dt9.Columns.Add("浓度", typeof(string));
            dt9.Columns.Add("强度", typeof(string));
            int this_average_times = concentration.Length;
            for (int i=0; i< this_average_times; i++)
            {
                DataRow dr9 = dt9.NewRow();
                dr9[0] = (i + 1).ToString();
                dr9[1] = Math.Round(concentration[i], 3);
                dr9[2] = Math.Round(strenth[i], 3);
                dt9.Rows.Add(dr9);
            }
            dgv.DataSource = dt9;
        }

        public static void draw_disturb_wave(DataGridView dgv, NIST nist, double str)
        {
            int l = 0;
            int k = 0;
            string[] nameNIST2 = new string[20];      //把保存干扰元素名字
            double[] waveNIST2 = new double[20];     //保存干扰元素的波长
            int[] typeNIST2 = new int[20];             //保存干扰光谱的类型
            int[] countNIST2 = new int[20];          //保存干扰的强度计数

            for (k = 0; k < 2359; k++)
            {
                if (nist.waveNIST[k] > (str - 1) && nist.waveNIST[k] < (str + 1))
                {
                    nameNIST2[l] = nist.nameNIST[k];
                    waveNIST2[l] = nist.waveNIST[k];
                    typeNIST2[l] = nist.typeNIST[k];
                    countNIST2[l] = nist.countNIST[k];
                    l++;
                }
            }
            string nam = "..";
            double wav = 0;
            int typ = 0;
            int coun = 0;
            for (int m = 0; m < waveNIST2.Length - 1; m++)
            {
                for (int n = 0; n < waveNIST2.Length - 1 - m; n++)
                {
                    if (waveNIST2[n] < waveNIST2[n + 1])
                    {
                        wav = waveNIST2[n];
                        waveNIST2[n] = waveNIST2[n + 1];
                        waveNIST2[n + 1] = wav;

                        nam = nameNIST2[n];
                        nameNIST2[n] = nameNIST2[n + 1];
                        nameNIST2[n + 1] = nam;

                        typ = typeNIST2[n];
                        typeNIST2[n] = typeNIST2[n + 1];
                        typeNIST2[n + 1] = typ;

                        coun = countNIST2[n];
                        countNIST2[n] = countNIST2[n + 1];
                        countNIST2[n + 1] = coun;

                    }

                }
            }

            DataTable dt3 = new DataTable();
            dt3.Columns.Add("序号", typeof(string));
            dt3.Columns.Add("符号", typeof(string));
            dt3.Columns.Add("波长", typeof(string));
            dt3.Columns.Add("光谱类型", typeof(string));
            dt3.Columns.Add("强度", typeof(string));

            for (l = 0; l < waveNIST2.Length; l++)
            {
                if (nameNIST2[l] != null)
                {
                    DataRow dr = dt3.NewRow();
                    dr[0] = l + 1;
                    dr[1] = nameNIST2[l];
                    dr[2] = waveNIST2[l];
                    dr[3] = typeNIST2[l];
                    dr[4] = countNIST2[l];
                    dt3.Rows.Add(dr);
                }
            }
            dgv.DataSource = dt3;
        }
        public static void draw_datagrid_element_nist(DataGridView dgv, NIST nist, string element)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("序号", typeof(double));
            dt.Columns.Add("波长", typeof(double));
            dt.Columns.Add("光谱类型", typeof(double));
            dt.Columns.Add("强度", typeof(double));
            int l = 0;
            int k = 0;
            for (; k < 2359; k++)
            {
                if (nist.nameNIST[k] == element)
                {
                    DataRow dr = dt.NewRow();
                    dr[0] = l + 1;
                    dr[1] = nist.waveNIST[k];//波长
                    dr[2] = nist.typeNIST[k];//类型
                    dr[3] = nist.countNIST[k];//强度
                    dt.Rows.Add(dr);
                    l++;
                }
            }
            dgv.DataSource = dt;
        }
    }
}
