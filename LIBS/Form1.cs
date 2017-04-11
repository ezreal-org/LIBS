using LIBS.device_driver;
using LIBS.service_fun;
using LIBS.storage;
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
        double[] spec_all;
        double[] wave_all;
        double[] env_all;
    
        double interval_start_wave;
        double interval_end_wave;
        double peak_wave;
        int interval_change_status; // 0-没有改变, 1-改变左区间, 2-改变右区间

        public Form1()
        {
            InitializeComponent();
        }
        private void read_testdata()
        {
            spec_all = file_reader.read_testdata("TestData\\Al50.txt");
            env_all = file_reader.read_testdata("TestData\\空白.txt");
            wave_all = file_reader.read_testdata("TestData\\readAllWave.txt");

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

            peak_wave = 396.2;

            interval_start_wave = peak_wave - 0.05;
            interval_end_wave = peak_wave + 0.05;

            chart_select_integral_interval.draw_chart(chart1, x_show_unit, y_show_unit, x_minimal, x_maximal, y_minimal, y_maximal, peak_wave, interval_start_wave, interval_end_wave, wave_all, spec_all, env_all);

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
            chart_select_integral_interval.draw_chart(chart1, x_show_unit, y_show_unit, x_minimal, x_maximal, y_minimal, y_maximal, peak_wave, interval_start_wave, interval_end_wave, wave_all, spec_all, env_all);

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

            if (interval_change_status == 1) //选择左区间
            {
                if (spline_xvalue > peak_wave)
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

                interval_start_wave = spline_xvalue;

            }
            if (interval_change_status == 2) //选择右区间
            {
                if (spline_xvalue < peak_wave)
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

                interval_end_wave = spline_xvalue;

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
            Point p_lable = new Point(e.X + 20, e.Y);
            label1.Location = p_lable;

        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            double pixel_x = chart1.ChartAreas[0].AxisX.ValueToPixelPosition(chart1.Series[(int)e_series.INTERVAL_LEFT].Points[0].XValue);
            if (e.X - pixel_x < 5 && e.X - pixel_x > -5) //调整左边积分区间
            {
                //MessageBox.Show("");
                interval_change_status = 1;
                return;
            }
            pixel_x = chart1.ChartAreas[0].AxisX.ValueToPixelPosition(chart1.Series[(int)e_series.INTERVAL_RIGHT].Points[0].XValue);
            if (e.X - pixel_x < 5 && e.X - pixel_x > -5)
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
                MessageBox.Show(interval_start_wave + " ," + interval_end_wave);

                interval_change_status = 0;
                chart1.Series[(int)e_series.INTERVAL_LEFT].BorderWidth = 1;
                chart1.Series[(int)e_series.INTERVAL_RIGHT].BorderWidth = 1;
                chart1.Series[(int)e_series.INTERVAL_CIRCLE].Enabled = false;

                //重新根据积分区间计算

                    // 计算该点的(wave,积分平均强度)
                    // 重新计算方程 更新视图
                    // 重新绘制方程图
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //计算需要减去本底
            double average = calculate_integration_average.get_integration_average(wave_all, spec_all, env_all, interval_start_wave, interval_end_wave);
            MessageBox.Show(average.ToString());
        }
    }
}
