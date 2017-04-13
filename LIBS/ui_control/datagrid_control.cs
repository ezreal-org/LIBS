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
    }
}
