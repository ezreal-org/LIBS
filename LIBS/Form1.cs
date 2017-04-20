using LIBS.device_driver;
using LIBS.service_fun;
using LIBS.storage;
using LIBS.ui_control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LIBS
{
    public partial class Form1 : Form
    {
        int interval_change_status; // 0-没有改变, 1-改变左区间, 2-改变右区间
        spec_metadata spec_data; //程序内流动改对象

        public Form1()
        {
            InitializeComponent();
            //chush
            spec_data = new spec_metadata();
            spec_data.read_wave_all = new double[10418];  //波长
            spec_data.read_spec_all_now = new double[10418];
            spec_data.read_standard_spec = new double[20, 5, 10418]; //最多支持20个标样，5次平均；实际存储是以实际数目为准
            spec_data.read_sample_spec = new double[20, 5, 10418]; //20个样本的5次平均
            spec_data.samples = new sample[20];
            spec_data.standards = new standard[20];
            spec_data.elements = new select_element[20];
        }
        private void read_testdata()
        {
            spec_data.read_spec_all_now = file_reader.read_testdata("TestData\\Al50.txt");
            spec_data.env_spec = file_reader.read_testdata("TestData\\空白.txt");
            spec_data.read_wave_all = file_reader.read_testdata("TestData\\readAllWave.txt");

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            read_testdata();

            double x_minimal = 395.95; //peak wave - 0.25
            double x_maximal = 396.45; //peak wave + 0.25

            double y_minimal = -100;
            double y_maximal = 2000 + 200; // maximal + 10% * range

            double x_show_unit = 0.05;

            int y_show_unit = data_util.normalize_y(y_minimal, y_maximal);
            //y的显示不随放大缩小改变, x将利用 data_util 来规范显示数据,初始时,x_show_unit固定为0.05
            y_minimal = (int)((y_minimal - 0.001) / y_show_unit - 1) * y_show_unit;
            y_maximal = (int)((y_maximal - 0.001) / y_show_unit + 1) * y_show_unit;
            data_util.normalize_data_for_show(x_show_unit, 10, ref x_show_unit, ref x_minimal, ref x_maximal); //

            double peak_wave = 396.2;

            double interval_start_wave = peak_wave - 0.05;
            double interval_end_wave = peak_wave + 0.05;

            chart_select_integral_interval.draw_chart(chart1, x_show_unit, y_show_unit, x_minimal, x_maximal, y_minimal, y_maximal, peak_wave, interval_start_wave, interval_end_wave, spec_data.read_wave_all, spec_data.read_spec_all_now, spec_data.env_spec);
            datagrid_control.draw_datagrid_analysis(dataGridView1, spec_data);
        }

        private void left_offset_Click(object sender, EventArgs e)
        {
            MessageBox.Show("左偏移");
        }

        private void right_offset_Click(object sender, EventArgs e)
        {
            MessageBox.Show("右偏移");
        }

        
        private void 绘图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double x_minimal = 395.95;
            double x_maximal = 396.45;

            double y_minimal = -100;
            double y_maximal = 2000 + 200; // maximal + 10% * range

            double x_show_unit = 0.05;
            int y_base = 1;
            int temp = (int)(y_maximal - y_minimal);
            while (temp >= 10)
            {
                temp /= 10;
                y_base *= 10;
            }
            double y_show_unit = temp * y_base / 10;
            //y的显示不随放大缩小改变, x将利用 data_util 来规范显示数据,初始时,x_show_unit固定为0.05
            y_minimal = (int)((y_minimal - 0.001) / y_show_unit - 1) * y_show_unit;
            y_maximal = (int)((y_maximal - 0.001) / y_show_unit + 1) * y_show_unit;
            data_util.normalize_data_for_show(x_show_unit, 10, ref x_show_unit, ref x_minimal, ref x_maximal); //

            double peak_wave = 396.2;
            double interval_start_wave = peak_wave - 0.05;
            double interval_end_wave = peak_wave + 0.05;
            chart_select_integral_interval.draw_chart(chart1, x_show_unit, y_show_unit, x_minimal, x_maximal, y_minimal, y_maximal, peak_wave, interval_start_wave, interval_end_wave, spec_data.read_wave_all, spec_data.read_spec_all_now, spec_data.env_spec);

        }

        //每次放大20%
        private void 放大ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double show_unit = 0.05;
            double step = 0.0;
            double show_minimal = chart1.ChartAreas[0].AxisX.Minimum;
            double show_maximal = chart1.ChartAreas[0].AxisX.Maximum;
            double chart1_x_range = show_maximal - show_minimal;
            if (chart1_x_range < 5 * show_unit) { MessageBox.Show("已经到最大放到倍数"); return; }

            show_minimal += chart1_x_range * 0.2; // chart1_x_range * 0.2/10 > show_unit/2 为true
            show_maximal -= chart1_x_range * 0.2;

            data_util.normalize_data_for_show(show_unit, 10, ref step, ref show_minimal, ref show_maximal);

            chart1.ChartAreas[0].AxisX.Minimum = show_minimal;
            chart1.ChartAreas[0].AxisX.Maximum = show_maximal;
            chart1.ChartAreas[0].AxisX.Interval = step;

        }

        private void 缩小ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double show_unit = 0.05;
            double step = 0.0;
            double show_minimal = chart1.ChartAreas[0].AxisX.Minimum;
            double show_maximal = chart1.ChartAreas[0].AxisX.Maximum;
            double chart1_x_range = show_maximal - show_minimal;

            show_minimal -= chart1_x_range * 0.2; // chart1_x_range * 0.2/10 > show_unit/2 为true
            show_maximal += chart1_x_range * 0.2;

            data_util.normalize_data_for_show(show_unit, 10, ref step, ref show_minimal, ref show_maximal);

            chart1.ChartAreas[0].AxisX.Minimum = show_minimal;
            chart1.ChartAreas[0].AxisX.Maximum = show_maximal;
            chart1.ChartAreas[0].AxisX.Interval = step;

        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            double spline_xvalue = 0.0;
            try
            {
                spline_xvalue = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
            }
            catch (Exception e1)
            {
                //MessageBox.Show("积分区间选择异常");
                return;
            }
            //调整积分区间需要知道是在操作哪一个元素
            int click_element_index = -1;
            if (dataGridView1.SelectedCells.Count > 0)
            {
                click_element_index = dataGridView1.SelectedCells[0].ColumnIndex - 3;
            }    
            if (interval_change_status == 1 && click_element_index>=0) //选择左区间
            {
                if (spline_xvalue > spec_data.elements[click_element_index].peak_wave)
                {
                    MessageBox.Show("积分区间选择有误");
                    interval_change_status = 0;
                    return;
                }
                chart1.Series[(int)e_series.INTERVAL_LEFT].Points[0].XValue = spline_xvalue;
                chart1.Series[(int)e_series.INTERVAL_LEFT].Points[1].XValue = spline_xvalue;
                chart1.Series[(int)e_series.INTERVAL_LEFT].BorderWidth = 2;
                // interval_mid
                chart1.Series[(int)e_series.INTERVAL_MID].Points[0].XValue = spline_xvalue;
                //更新积分区间
                spec_data.elements[click_element_index].interval_start = spline_xvalue;

            }
            if (interval_change_status == 2 && click_element_index>=0) //选择右区间
            {
                if (spline_xvalue < spec_data.elements[click_element_index].peak_wave)
                {
                    MessageBox.Show("积分区间选择有误");
                    interval_change_status = 0;
                    return;
                }
                chart1.Series[(int)e_series.INTERVAL_RIGHT].Points[0].XValue = spline_xvalue;
                chart1.Series[(int)e_series.INTERVAL_RIGHT].Points[1].XValue = spline_xvalue;
                chart1.Series[(int)e_series.INTERVAL_RIGHT].BorderWidth = 2;
                // interval_mid
                chart1.Series[(int)e_series.INTERVAL_MID].Points[1].XValue = spline_xvalue;
                //更新积分区间
                spec_data.elements[click_element_index].interval_end = spline_xvalue;

            }
            //用于平滑显示的曲线x,y ;插值后的(x,y)用户通过x快速找到插值曲线的y
            point[] spline_point = chart_select_integral_interval.spline_point;
            if (interval_change_status > 0)
            {
                // interval_circle
                chart1.Series[(int)e_series.INTERVAL_CIRCLE].Points[0].XValue = spline_xvalue;
                for (int i = 0; i < spline_point.Length; i++)
                {
                    if (spline_point[i].x > spline_xvalue)
                    {
                        chart1.Series[(int)e_series.INTERVAL_CIRCLE].Points[0].YValues[0] = spline_point[i].y;
                        break;
                    }
                }
                chart1.Series[(int)e_series.INTERVAL_CIRCLE].Enabled = true;
            }
            //
            chart1.Series[(int)e_series.MOUSE_CURSOR].Points[0].XValue = spline_xvalue;
            chart1.Series[(int)e_series.MOUSE_CURSOR].Points[1].XValue = spline_xvalue;
            chart1.Series[(int)e_series.MOUSE_CURSOR].Enabled = true;

            double wave = Math.Round(spline_xvalue, 3);
            double strenth = 0;
            try
            {
                strenth = Math.Round(chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y), 3);
            }
            catch (Exception e1)
            {
               // MessageBox.Show("选择积分区间异常");
                return;
            }
            for (int i = 0; i < 1000; i++)
            {
                if (spline_point[i].x > wave)
                {
                    strenth = spline_point[i].y; break;
                }
            }
            strenth = Math.Round(strenth, 3);
            label1.Text = wave + ", " + strenth;
            Point p_lable = new Point(e.X + 20 + chart1.Location.X, e.Y);
            label1.Location = p_lable;

        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            double pixel_x = chart1.ChartAreas[0].AxisX.ValueToPixelPosition(chart1.Series[(int)e_series.INTERVAL_LEFT].Points[0].XValue);
            if (e.X - pixel_x < 5 && e.X - pixel_x > -5 && chart1.Series[(int)e_series.INTERVAL_LEFT].Enabled) //调整左边积分区间
            {
                //MessageBox.Show("");
                interval_change_status = 1;
                return;
            }
            pixel_x = chart1.ChartAreas[0].AxisX.ValueToPixelPosition(chart1.Series[(int)e_series.INTERVAL_RIGHT].Points[0].XValue);
            if (e.X - pixel_x < 5 && e.X - pixel_x > -5 && chart1.Series[(int)e_series.INTERVAL_RIGHT].Enabled)
            {
                //MessageBox.Show("");
                interval_change_status = 2;
                return;
            }
        }

        private void chart1_MouseUp(object sender, MouseEventArgs e)
        {
            if (interval_change_status > 0)
            {
                if (dataGridView1.SelectedCells.Count > 0)
                {
                    int element_index = dataGridView1.SelectedCells[0].ColumnIndex - 3;
                    //MessageBox.Show(spec_data.elements[element_index].interval_start + " ," + spec_data.elements[element_index].interval_end);
                }
                interval_change_status = 0;
                chart1.Series[(int)e_series.INTERVAL_LEFT].BorderWidth = 1;
                chart1.Series[(int)e_series.INTERVAL_RIGHT].BorderWidth = 1;
                chart1.Series[(int)e_series.INTERVAL_CIRCLE].Enabled = false;

                double[,] standard_val = new double[spec_data.standard_cnt, spec_data.element_cnt];
                double[,] sample_val = new double[spec_data.sample_cnt, spec_data.element_cnt];
                //draw_datagrid_analysis函数内会准备好用于表格显示的数据，并绘制表格
                int origin_select_row = dataGridView1.SelectedCells[0].RowIndex;
                int origin_select_column = dataGridView1.SelectedCells[0].ColumnIndex;
                datagrid_control.draw_datagrid_analysis(dataGridView1, spec_data);
                dataGridView1.ClearSelection();
                dataGridView1.Rows[origin_select_row].Cells[origin_select_column].Selected = true;

                //重新根据积分区间计算
                //只负责更新积分区间，调用datagrid_control重新fill表格数据
                // 计算该点的(wave,积分平均强度)
                // 重新计算方程 更新视图
                // 重新绘制方程图
                double[] element_concentrations = null;
                double[] element_average_strenths = null;
                int click_column = dataGridView1.SelectedCells[0].ColumnIndex;
                int click_row = dataGridView1.SelectedCells[0].RowIndex;
                LinearFit.LfReValue equation = analysis_proc.get_equation(spec_data, click_column - 3, ref element_concentrations, ref element_average_strenths);

                double []this_read_integration_strenths = analysis_proc.get_oneshot_all_strength(spec_data, click_row, click_column - 3);
                int this_read_average_times = this_read_integration_strenths.Length;
                double[] this_read_integration_concentrations = new double[this_read_average_times];
                double this_read_strenth_average = 0, this_read_concentration_average = 0;
                for (int i = 0; i < this_read_average_times; i++)
                {
                    this_read_integration_concentrations[i] = (this_read_integration_strenths[i] - equation.getA()) / equation.getB();
                }
                for (int i = 0; i < this_read_average_times; i++)
                {
                    this_read_strenth_average += this_read_integration_strenths[i];
                }
                this_read_strenth_average /= this_read_average_times;
                this_read_concentration_average = (this_read_strenth_average - equation.getA()) / equation.getB();

                equation_chart.draw_equation(chart2, label2, element_concentrations, element_average_strenths, equation.getA(), equation.getB(), equation.getR());
                if (click_row < spec_data.standard_cnt)
                    equation_chart.add_point_now(chart2, element_concentrations[click_row], element_average_strenths[click_row], Color.Red, MarkerStyle.Circle);
                else
                    equation_chart.add_point_now(chart2, this_read_concentration_average, this_read_strenth_average, Color.Green, MarkerStyle.Triangle);
                datagrid_control.draw_datagrid_snapshot(dataGridView9, this_read_integration_concentrations, this_read_integration_strenths);
                summary_info.draw_summary_info(label_info, this_read_concentration_average, this_read_strenth_average);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //计算需要减去本底
           // double average = calculate_integration_average.get_integration_average(spec_data.read_wave_all, spec_data.read_spec_all_now, spec_data.env_spec, interval_start_wave, interval_end_wave);
            //MessageBox.Show(average.ToString());
        }

        //void test(double[] xx)
        //{
        //    xx[0] = 0.1;
        //    xx[1] = 0.2;

           
        //}
        private void button2_Click(object sender, EventArgs e)
        {
            //double[] xx = new double[2];
            //xx[0] = 9.9; xx[1] = 8.8;
            //test(xx);
            //MessageBox.Show(xx[0].ToString() + " " + xx[1].ToString());

            select_element []se = new select_element[2];
            sample []sp = new sample[2];
            standard []sd = new standard[2];

            se[0].element = "Hg";
            se[0].label = "Hg";
            se[0].seek_peak_range = 0.25;
            se[0].select_wave = 253.65;
            se[0].sequece_index = 0;
            se[1].element = "Al";
            se[1].label = "Al";
            se[1].seek_peak_range = 0.25;
            se[1].select_wave = 396.16;
            se[1].sequece_index = 1;
            spec_data.elements[0] = se[0];
            spec_data.elements[1] = se[1];
            spec_data.element_cnt = 2;

            sd[0].average_times = 1;
            sd[0].is_readed = true;
            sd[0].standard_index = 0;
            sd[0].standard_label = "空白";
            sd[0].standard_ppm = new double[2];
            sd[1].average_times = 2;
            sd[1].is_readed = true;
            sd[1].standard_index = 1;
            sd[1].standard_label = "标样1";
            sd[1].standard_ppm = new double[2];
            sd[1].standard_ppm[0] = 0.5;
            sd[1].standard_ppm[1] = 8;
            spec_data.standards[0] = sd[0];
            spec_data.standards[1] = sd[1];
            spec_data.standard_cnt = 2;

            sp[0].sample_index = 0;
            sp[0].sample_label = "样本1";
            sp[0].average_times = 1;
            sp[0].is_read = true;
            sp[1].sample_index = 1;
            sp[1].sample_label = "样本2";
            sp[1].average_times = 1;
            sp[1].is_read = false;
            spec_data.samples[0] = sp[0];
            spec_data.samples[1] = sp[1];
            spec_data.sample_cnt = 2;

            //初始标样1
            Random rr = new Random();
            for (int i = 0; i < 10418; i++)
            {
                spec_data.read_standard_spec[1, 0, i] = spec_data.read_spec_all_now[i];
                spec_data.read_standard_spec[1, 1, i] = spec_data.read_spec_all_now[i] + 500*rr.NextDouble();
            }
            //设置已经读取，设置标样对象;通过标样确定样本读取峰，初始积分区间
            for (int j = 0; j < spec_data.element_cnt; j++)
            {
                //寻峰，确定实际读取波长，默认积分区间
                int seek_start_index = data_util.get_index_by_wave(spec_data.read_wave_all, spec_data.elements[j].select_wave - spec_data.elements[j].seek_peak_range / 2);
                int seek_end_index = data_util.get_index_by_wave(spec_data.read_wave_all, spec_data.elements[j].select_wave + spec_data.elements[j].seek_peak_range / 2);
                int peak_index = data_util.find_peak(spec_data.read_spec_all_now, seek_start_index, seek_end_index);
                spec_data.elements[j].peak_wave = spec_data.read_wave_all[peak_index];
                spec_data.elements[j].interval_start = spec_data.elements[j].peak_wave - 0.05; //设置默认积分范围为峰左右0.25nm
                spec_data.elements[j].interval_end = spec_data.elements[j].peak_wave + 0.05;
            }
            //初始样本1
            for (int i = 0; i < 10418; i++)
            {
                spec_data.read_sample_spec[0, 0, i] = spec_data.read_spec_all_now[i]*0.5;
            }

            datagrid_control.draw_datagrid_analysis(dataGridView1, spec_data);

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            analysis_proc.process_cell_click(chart1,chart2, label2,label_info, dataGridView1, dataGridView9, e.RowIndex, e.ColumnIndex, spec_data);
        }
    }
}
