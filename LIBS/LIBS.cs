using LIBS.device_driver;
using LIBS.service_fun;
using LIBS.storage;
using LIBS.ui_control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LIBS
{
    public partial class LIBS : Form
    {
        spec_metadata spec_data; //程序内流动改对象
        int interval_change_status; // 0-没有改变, 1-改变左区间, 2-改变右区间
        spec_wrapper wrapper;
        NIST nist;
        string select_element_now; //当前正在选择的元素
        string select_element_control_name; //记录上一个选择的元素的控件name

        public LIBS()
        {
            InitializeComponent();

            //初始化
            select_element_control_name = null;
            wrapper = new spec_wrapper();
            spec_data = new spec_metadata();
            spec_data.read_wave_all = new double[10418];  //波长
            spec_data.read_spec_all_now = new double[10418];
            spec_data.read_standard_spec = new double[20, 5, 10418]; //最多支持20个标样，5次平均；实际存储是以实际数目为准
            spec_data.read_sample_spec = new double[20, 5, 10418]; //20个样本的5次平均
            spec_data.samples = new sample[20];
            spec_data.standards = new standard[20];
            spec_data.elements = new select_element[20];
            spec_data.env_spec = new double[10418];

            for (int i = 0; i < 20; i++)
            {
                spec_data.samples[i] = new sample();
                spec_data.standards[i] = new standard();
                spec_data.standards[i].standard_ppm = new double[20];
                spec_data.elements[i] = new select_element();
            }
            //初始化nist对象
            nist = new NIST();
            nist.read_NIST();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void read_testdata()
        {
            spec_data.read_spec_all_now = file_reader.read_testdata("TestData\\Al50.txt");
            spec_data.env_spec = file_reader.read_testdata("TestData\\空白.txt");
            spec_data.read_wave_all = file_reader.read_testdata("TestData\\readAllWave.txt");

        }

        private void test_case_setting()
        {
            select_element[] se = new select_element[2];
            se[0] = new select_element();
            se[1] = new select_element();
            sample[] sp = new sample[2];
            sp[0] = new sample();
            sp[1] = new sample();
            standard[] sd = new standard[2];
            sd[0] = new standard();
            sd[1] = new standard();

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
            sd[0].standard_ppm = new double[20];
            sd[1].average_times = 2;
            sd[1].is_readed = true;
            sd[1].standard_index = 1;
            sd[1].standard_label = "标样1";
            sd[1].standard_ppm = new double[20];
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
                spec_data.read_standard_spec[1, 1, i] = spec_data.read_spec_all_now[i] + 500 * rr.NextDouble();
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
                spec_data.read_sample_spec[0, 0, i] = spec_data.read_spec_all_now[i] * 0.5;
            }
        }


        private void LIBS_Load(object sender, EventArgs e)
        {
            //read_testdata();
            // test_case_setting();
            wrapper.connect();
            spec_data.read_wave_all = wrapper.get_wave_all();
            spec_data.standard_cnt = 2;
            spec_data.standards[0].standard_index = 0;
            spec_data.standards[0].standard_label = "空白";
            spec_data.standards[0].average_times = 1;
            spec_data.standards[0].is_readed = false;
            spec_data.standards[1].standard_index = 1;
            spec_data.standards[1].standard_label = "标样1";
            spec_data.standards[1].average_times = 1;
            spec_data.standards[1].is_readed = false;
        }

        private void dgv_analysis_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //switch case
            //读取数据
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                analysis_proc.read_spec_click(wrapper, dgv_analysis, e.RowIndex, spec_data);
            }
            else if (e.ColumnIndex > 2 && e.RowIndex >= 0)
            {
                analysis_proc.process_cell_click(chart1, chart2, label_equation, l_info, dgv_thisshot, e.RowIndex, e.ColumnIndex, spec_data);
            }

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
            if (chart1.Series.Count < 2) return;
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
            if (dgv_analysis.SelectedCells.Count > 0)
            {
                click_element_index = dgv_analysis.SelectedCells[0].ColumnIndex - 3;
            }
            if (interval_change_status == 1 && click_element_index >= 0) //选择左区间
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
            if (interval_change_status == 2 && click_element_index >= 0) //选择右区间
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
            label_mouse.Text = wave + ", " + strenth;
            Point p_lable = new Point(e.X + 20 + chart1.Location.X, e.Y);
            label_mouse.Location = p_lable;
            label_mouse.Show();
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
                if (dgv_analysis.SelectedCells.Count > 0)
                {
                    int element_index = dgv_analysis.SelectedCells[0].ColumnIndex - 3;
                    //MessageBox.Show(spec_data.elements[element_index].interval_start + " ," + spec_data.elements[element_index].interval_end);
                }
                interval_change_status = 0;
                chart1.Series[(int)e_series.INTERVAL_LEFT].BorderWidth = 1;
                chart1.Series[(int)e_series.INTERVAL_RIGHT].BorderWidth = 1;
                chart1.Series[(int)e_series.INTERVAL_CIRCLE].Enabled = false;

                double[,] standard_val = new double[spec_data.standard_cnt, spec_data.element_cnt];
                double[,] sample_val = new double[spec_data.sample_cnt, spec_data.element_cnt];
                //draw_datagrid_analysis函数内会准备好用于表格显示的数据，并绘制表格
                int origin_select_row = dgv_analysis.SelectedCells[0].RowIndex;
                int origin_select_column = dgv_analysis.SelectedCells[0].ColumnIndex;
                datagrid_control.draw_datagrid_analysis(dgv_analysis, spec_data);
                dgv_analysis.ClearSelection();
                dgv_analysis.Rows[origin_select_row].Cells[origin_select_column].Selected = true;

                //重新根据积分区间计算
                //只负责更新积分区间，调用datagrid_control重新fill表格数据
                // 计算该点的(wave,积分平均强度)
                // 重新计算方程 更新视图
                // 重新绘制方程图
                double[] element_concentrations = null;
                double[] element_average_strenths = null;
                int click_column = dgv_analysis.SelectedCells[0].ColumnIndex;
                int click_row = dgv_analysis.SelectedCells[0].RowIndex;
                LinearFit.LfReValue equation = analysis_proc.get_equation(spec_data, click_column - 3, ref element_concentrations, ref element_average_strenths);

                double[] this_read_integration_strenths = analysis_proc.get_oneshot_all_strength(spec_data, click_row, click_column - 3);
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

                equation_chart.draw_equation(chart2, label_equation, element_concentrations, element_average_strenths, equation.getA(), equation.getB(), equation.getR());
                if (click_row < spec_data.standard_cnt)
                    equation_chart.add_point_now(chart2, element_concentrations[click_row], element_average_strenths[click_row], Color.Red, MarkerStyle.Circle);
                else
                    equation_chart.add_point_now(chart2, this_read_concentration_average, this_read_strenth_average, Color.Green, MarkerStyle.Triangle);
                datagrid_control.draw_datagrid_snapshot(dgv_thisshot, this_read_integration_concentrations, this_read_integration_strenths);
                summary_info.draw_summary_info(l_info, this_read_concentration_average, this_read_strenth_average);
            }
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex == 1)
            {
                //绘制已选元素表格   
                datagrid_control.draw_datagrid_select_element(dataGridView1, spec_data.elements, spec_data.element_cnt);
            }
            if (e.TabPageIndex == 3)
            {
                textBox15.Text = spec_data.standard_cnt.ToString();
                datagrid_control.draw_datagrid_standard_setting(dataGridView5, spec_data.elements, spec_data.standards, spec_data.element_cnt, spec_data.standard_cnt);
            }
            if (e.TabPageIndex == 4)
            {
                textBox16.Text = spec_data.sample_cnt.ToString();
                datagrid_control.draw_datagrid_sample_setting(dataGridView7, spec_data.samples, spec_data.sample_cnt);

            }
            if (e.TabPageIndex == 5)
            {
                datagrid_control.draw_datagrid_analysis(dgv_analysis, spec_data);
            }
            
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            e.ToString();
        }

        private void toolStripComboBox1_TextChanged(object sender, EventArgs e)
        {
            if(toolStripComboBox1.Text == "浓度")
            {
                datagrid_control.show_strenth = false;
            }
            else
            {
                datagrid_control.show_strenth = true; ;
            }
            datagrid_control.draw_datagrid_analysis(dgv_analysis, spec_data);
        }

        private void chart1_MouseLeave(object sender, EventArgs e)
        {
            //if (chart1.Series.Count < 2) return;
            label_mouse.Hide();
        }

        //统一的元素选择
        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("Clicked--" + ((CheckBox)sender).Text);
            //去掉以前的选择
            if(select_element_control_name!=null)
                ((CheckBox)(Controls.Find(select_element_control_name, true)[0])).Checked = false;
            select_element_now = ((CheckBox)sender).Text;
            datagrid_control.draw_datagrid_element_nist(dataGridView2, nist, select_element_now);
            label4.Text = "元素信息("+ select_element_now + ")                                     添加";
            select_element_control_name = ((CheckBox)sender).Name;
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            double select_wave = (double)dataGridView2.Rows[e.RowIndex].Cells[1].Value;//记录下所选择的波长
            label5.Text = "在" + select_element_now + "(" + select_wave.ToString() + ")处可能的干扰";
            datagrid_control.draw_disturb_wave(dataGridView3, nist, select_wave);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            //用了布局，不好直接添加button
            double select_wave = (double)dataGridView2.Rows[dataGridView2.SelectedCells[0].RowIndex].Cells[1].Value;//记录下所选择的波长
                                                                                                                    //添加元素
            spec_data.elements[spec_data.element_cnt].sequece_index = spec_data.element_cnt;
            spec_data.elements[spec_data.element_cnt].element = select_element_now;
            spec_data.elements[spec_data.element_cnt].label = select_element_now;
            spec_data.elements[spec_data.element_cnt].select_wave = select_wave;
            spec_data.elements[spec_data.element_cnt].seek_peak_range = 0.5; //设置默认寻峰范围
            spec_data.elements[spec_data.element_cnt].interval_start = select_wave-0.05; //设置默认积分区间，程序中在分析窗体点读取，将根据实际峰,寻峰范围重新设置
            spec_data.elements[spec_data.element_cnt].interval_end = select_wave + 0.05;
            spec_data.elements[spec_data.element_cnt].peak_wave = select_wave;
            spec_data.element_cnt++;

            //重绘已选元素表
            datagrid_control.draw_datagrid_select_element(dataGridView1, spec_data.elements, spec_data.element_cnt);
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            file_operator.save_spec_metadat(@"h:\specdate.dat",spec_data);
        }

        //保存不同标样的浓度数据
        private void button6_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < spec_data.standard_cnt; i++)
            {
                for(int j = 0; j < spec_data.element_cnt; j++)
                {
                    spec_data.standards[i].standard_ppm[j] = Double.Parse(dataGridView5.Rows[i+1].Cells[j+2].Value.ToString());
                }
            }
        }

        private void textBox15_Click(object sender, EventArgs e)
        {
            if (textBox15.Text != null && textBox15.Text != "")
            {
                int sc = int.Parse(textBox15.Text);
                if (sc >= 2 && sc <= 20)
                {
                    if (sc > spec_data.standard_cnt)
                    {
                        for (int k = spec_data.standard_cnt; k < sc; k++)
                        {
                            spec_data.standards[k].standard_index = k;
                            spec_data.standards[k].standard_label = "标样" + k;
                            spec_data.standards[k].average_times = 1;
                            spec_data.standards[k].is_readed = false;
                        }

                    }
                    spec_data.standard_cnt = sc;

                }
                else if (sc < 2)
                {
                    textBox15.Text = (sc + 1).ToString(); MessageBox.Show("标样数目异常");
                }
                else
                {
                    textBox15.Text = (sc - 1).ToString(); MessageBox.Show("标样数目异常");
                }
            }
            datagrid_control.draw_datagrid_standard_setting(dataGridView5, spec_data.elements, spec_data.standards, spec_data.element_cnt, spec_data.standard_cnt);
        }

        private void textBox16_Click(object sender, EventArgs e)
        {
            if (textBox16.Text != null && textBox16.Text != "")
            {
                int sc = int.Parse(textBox16.Text);
                if (sc >= 1 && sc <= 20)
                {
                    if (sc > spec_data.sample_cnt)
                    {
                        for (int k = spec_data.sample_cnt; k < sc; k++)
                        {
                            spec_data.samples[k].sample_index = k;
                            spec_data.samples[k].sample_label = "样本" + (k+1).ToString();
                            spec_data.samples[k].average_times = 1;
                            spec_data.samples[k].is_read = false;
                        }

                    }
                    spec_data.sample_cnt = sc;

                }
                else if (sc < 1)
                {
                    textBox16.Text = (sc + 1).ToString(); MessageBox.Show("样本数目异常");
                }
                else
                {
                    textBox16.Text = (sc - 1).ToString(); MessageBox.Show("样本数目异常");
                }
                datagrid_control.draw_datagrid_sample_setting(dataGridView7, spec_data.samples, spec_data.sample_cnt);

            }
        }

        //添加样本
        private void button7_Click(object sender, EventArgs e)
        {

        }
    }
}
