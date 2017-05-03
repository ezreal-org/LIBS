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

        //（start）画坐标轴定义的变量---------------------------------------------------------------------------------
        static double[] x_init = { 1.1f, 3.2f, 4.3f, 5.1f, 16.8f };//传入数据的x数组
        static double[] y_init = { 4.2f, 5.6f, 19.0f, 7.5f, 5.9f };//传入数据的y数组

        double[] x = x_init;//用于缓存放大坐标区域后的数据
        double[] y = y_init;//用于缓存放大坐标区域后的数据

        double move = 50;//坐标轴边距
        static double x_min_1, x_max_1, y_min_1, y_max_1;//tabpage1的坐标图像的x,y取值范围
        static double x_min_3, x_max_3, y_min_3, y_max_3;//tabpage3的坐标图像的x,y取值范围
        static double x_min_3_init, x_max_3_init;
        int x_count_1 = 10, y_count_1 = 10;
        bool ifExistPlot_1 = false;//判断tabpage1的坐标图像是否已经绘制

        Bitmap myBitmap_1;
        Graphics g_bitmap_1;
        Graphics gg_1;//tabpage1上panel的的画布

        Bitmap myBitmap_3;
        Graphics g_bitmap_3;
        Graphics gg_3;//tabpage1上panel的的画布

        //绘制矩形的x,y取值范围，用于确定放大范围后的坐标轴范围
        static double x_ChangeBegin_1, y_ChangeBegin_1, x_ChangeEnd_1, y_ChangeEnd_1;
        static double x_ChangeBegin_3, y_ChangeBegin_3, x_ChangeEnd_3, y_ChangeEnd_3;

        ToolTip toolTip_1 = new ToolTip();//随鼠标显示当前坐标点的ToolTip

        Point recStart_1 = new Point();//实现鼠标拖拉矩形的功能
        Point recEnd_1 = new Point();//实现鼠标拖拉矩形的功能
        Point recStart_3 = new Point();//实现鼠标拖拉矩形的功能
        Point recEnd_3 = new Point();//实现鼠标拖拉矩形的功能
        bool blnDraw_1 = false;//是否开始画矩形
        bool blnDraw_3 = false;//是否开始画矩形

        Point locNow_1 = new Point();//用于实现绘制鼠标当前点垂直线的功能
        PointF locNowP1_1 = new Point();//用于实现绘制鼠标当前点垂直线的功能
        PointF locNowP2_1 = new Point();//用于实现绘制鼠标当前点垂直线的功能
        bool locNowBoolean_1 = false;//是否开始画当前点垂直线
        bool locNowBoolean_3 = false;//是否开始画当前点垂直线

        Point locNow_3 = new Point();//用于实现绘制鼠标当前点垂直线的功能
        PointF locNowP1_3 = new Point();//用于实现绘制鼠标当前点垂直线的功能
        PointF locNowP2_3 = new Point();//用于实现绘制鼠标当前点垂直线的功能

        calc_spec_wave calc_s_w = new calc_spec_wave();

        double x_min_timer, y_min_timer, x_max_timer, y_max_timer;

        bool ifStartTimer = false;

        file_operator f_operator = new file_operator();

        DataTable dt4;//填充条件chart的table
        int tableRowCount = -1;//当前鼠标指向的表的行数

        static double[] x_init_tabpage3_array= { 1.1f, 2.2f };
        static double[] y_init_tabpage3_array= { 3.4 , 4.4f};
        double[] x_tabpage3_array = x_init_tabpage3_array;
        double[] y_tabpage3_array = y_init_tabpage3_array;

        public LIBS()
        {
            InitializeComponent();
            init_application();
        }

        private void init_application()
        {
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
            //read_testdata();
            // test_case_setting();
            //初始至少两个标样
            spec_data.standard_cnt = 2;
            spec_data.standards[0].standard_index = 0;
            spec_data.standards[0].standard_label = "空白";
            spec_data.standards[0].average_times = 1;
            spec_data.standards[0].is_readed = false;
            spec_data.standards[1].standard_index = 1;
            spec_data.standards[1].standard_label = "标样1";
            spec_data.standards[1].average_times = 1;
            spec_data.standards[1].is_readed = false;
            //初始化nist对象
            nist = new NIST();
            nist.read_NIST();

            //测试
            //代码转移到了连接按钮点击响应事件中了
            //wrapper.connect();
            //spec_data.read_wave_all = wrapper.get_wave_all();


            //画坐标图初始变量
            gg_1 = panel_XY.CreateGraphics();//panel_XY上的全局画笔
            myBitmap_1 = new Bitmap(panel_XY.Width, panel_XY.Height);
            g_bitmap_1 = Graphics.FromImage(myBitmap_1);
            g_bitmap_1.Clear(Color.White);

            gg_3 = panel3_XY.CreateGraphics();
            myBitmap_3 = new Bitmap(panel3_XY.Width,panel3_XY.Height);
            g_bitmap_3 = Graphics.FromImage(myBitmap_3);
            g_bitmap_3.Clear(Color.White);

        }

        //打开文件点击响应事件
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "LIBS配置文件|*.libs";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //MessageBox.Show(openFileDialog1.FileName);
                try
                {
                    spec_metadata vv_spec_metadata = file_operator.read_spec_metadata(openFileDialog1.FileName);
                    spec_data = vv_spec_metadata;
                    x_init = spec_data.read_wave_all;
                    y_init = spec_data.read_spec_all_now;


                    //MessageBox.Show(spec_data.element_cnt+"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("读取文件出错，请检查！" + "\n" + ex);
                }

            }
        }

        //private void read_testdata()
        //{
        //    spec_data.read_spec_all_now = file_reader.read_testdata("TestData\\Al50.txt");
        //    spec_data.env_spec = file_reader.read_testdata("TestData\\空白.txt");
        //    spec_data.read_wave_all = file_reader.read_testdata("TestData\\readAllWave.txt");

        //}

        //private void test_case_setting()
        //{
        //    select_element[] se = new select_element[2];
        //    se[0] = new select_element();
        //    se[1] = new select_element();
        //    sample[] sp = new sample[2];
        //    sp[0] = new sample();
        //    sp[1] = new sample();
        //    standard[] sd = new standard[2];
        //    sd[0] = new standard();
        //    sd[1] = new standard();

        //    se[0].element = "Hg";
        //    se[0].label = "Hg";
        //    se[0].seek_peak_range = 0.25;
        //    se[0].select_wave = 253.65;
        //    se[0].sequece_index = 0;
        //    se[1].element = "Al";
        //    se[1].label = "Al";
        //    se[1].seek_peak_range = 0.25;
        //    se[1].select_wave = 396.16;
        //    se[1].sequece_index = 1;
        //    spec_data.elements[0] = se[0];
        //    spec_data.elements[1] = se[1];
        //    spec_data.element_cnt = 2;

        //    sd[0].average_times = 1;
        //    sd[0].is_readed = true;
        //    sd[0].standard_index = 0;
        //    sd[0].standard_label = "空白";
        //    sd[0].standard_ppm = new double[20];
        //    sd[1].average_times = 2;
        //    sd[1].is_readed = true;
        //    sd[1].standard_index = 1;
        //    sd[1].standard_label = "标样1";
        //    sd[1].standard_ppm = new double[20];
        //    sd[1].standard_ppm[0] = 0.5;
        //    sd[1].standard_ppm[1] = 8;
        //    spec_data.standards[0] = sd[0];
        //    spec_data.standards[1] = sd[1];
        //    spec_data.standard_cnt = 2;

        //    sp[0].sample_index = 0;
        //    sp[0].sample_label = "样本1";
        //    sp[0].average_times = 1;
        //    sp[0].is_read = true;
        //    sp[1].sample_index = 1;
        //    sp[1].sample_label = "样本2";
        //    sp[1].average_times = 1;
        //    sp[1].is_read = false;
        //    spec_data.samples[0] = sp[0];
        //    spec_data.samples[1] = sp[1];
        //    spec_data.sample_cnt = 2;

        //    //初始标样1
        //    Random rr = new Random();
        //    for (int i = 0; i < 10418; i++)
        //    {
        //        spec_data.read_standard_spec[1, 0, i] = spec_data.read_spec_all_now[i];
        //        spec_data.read_standard_spec[1, 1, i] = spec_data.read_spec_all_now[i] + 500 * rr.NextDouble();
        //    }
        //    //设置已经读取，设置标样对象;通过标样确定样本读取峰，初始积分区间
        //    for (int j = 0; j < spec_data.element_cnt; j++)
        //    {
        //        //寻峰，确定实际读取波长，默认积分区间
        //        int seek_start_index = data_util.get_index_by_wave(spec_data.read_wave_all, spec_data.elements[j].select_wave - spec_data.elements[j].seek_peak_range / 2);
        //        int seek_end_index = data_util.get_index_by_wave(spec_data.read_wave_all, spec_data.elements[j].select_wave + spec_data.elements[j].seek_peak_range / 2);
        //        int peak_index = data_util.find_peak(spec_data.read_spec_all_now, seek_start_index, seek_end_index);
        //        spec_data.elements[j].peak_wave = spec_data.read_wave_all[peak_index];
        //        spec_data.elements[j].interval_start = spec_data.elements[j].peak_wave - 0.05; //设置默认积分范围为峰左右0.25nm
        //        spec_data.elements[j].interval_end = spec_data.elements[j].peak_wave + 0.05;
        //    }
        //    //初始样本1
        //    for (int i = 0; i < 10418; i++)
        //    {
        //        spec_data.read_sample_spec[0, 0, i] = spec_data.read_spec_all_now[i] * 0.5;
        //    }
        //}


        private void LIBS_Load(object sender, EventArgs e)
        {
            
        }

        private void dgv_analysis_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //switch case
            //读取数据
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                //获取平均次数用于下一次读取
                analysis_proc.read_spec_click(textBox17, wrapper, dgv_analysis, e.RowIndex, spec_data);
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
            if (chart1.Series.Count < 4) return;
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
                //@jayce tab间切换会发生datagridview焦点丢失，让用户重新去选择cell再提供积分区间 调整
                if (dgv_analysis.SelectedCells.Count <1 || dgv_analysis.SelectedCells[0].RowIndex <0 || dgv_analysis.SelectedCells[0].ColumnIndex<3)
                {
                    return;
                    //int element_index = dgv_analysis.SelectedCells[0].ColumnIndex - 3;
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
            if (e.TabPageIndex == 2)
            {
                dt4 = new DataTable();
                dt4.Columns.Add("序号", typeof(string));
                dt4.Columns.Add("标签", typeof(string));
                dt4.Columns.Add("参考波长", typeof(string));
                dt4.Columns.Add("读取波长", typeof(string));
                dt4.Columns.Add("参考强度", typeof(string));
                dt4.Columns.Add("读取强度", typeof(string));

                int k = 0;

                for (; k < spec_data.element_cnt; k++)
                {

                    DataRow dr4 = dt4.NewRow();

                    dr4[0] = spec_data.elements[k].sequece_index+1;
                    dr4[1] = spec_data.elements[k].element;
                    dr4[2] = spec_data.elements[k].select_wave;
                    double[] realSpecWave = findWaveSpec_Range(spec_data.elements[k].select_wave, spec_data.elements[k].seek_peak_range);

                    //@jayce 条件窗体只改变元素的寻峰范围，峰的确定会在分析窗体的样本/标样的读取中根据实际读取值进行设置
                    //spec_data.elements[k].peak_wave = realSpecWave[0];
                    dr4[3] = realSpecWave[0];
                    dr4[4] = getSpec_Example(spec_data.elements[k].element, spec_data.elements[k].select_wave);
                    dr4[5] = realSpecWave[1];
                    dt4.Rows.Add(dr4);
                }

                this.dataGridView4.DataSource = dt4;
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

            bool ifExistElem = false;
            for (int i=0;i<spec_data.element_cnt;i++)
            {
                if (select_wave== spec_data.elements[i].select_wave)
                {
                    ifExistElem = true;
                }
            }

            if (!ifExistElem)
            {
                //添加元素
                spec_data.elements[spec_data.element_cnt].sequece_index = spec_data.element_cnt;
                spec_data.elements[spec_data.element_cnt].element = select_element_now;
                spec_data.elements[spec_data.element_cnt].label = select_element_now;
                spec_data.elements[spec_data.element_cnt].select_wave = select_wave;
                spec_data.elements[spec_data.element_cnt].seek_peak_range = 0.5; //设置默认寻峰范围
                spec_data.elements[spec_data.element_cnt].interval_start = select_wave - 0.05; //设置默认积分区间，程序中在分析窗体点读取，将根据实际峰,寻峰范围重新设置
                spec_data.elements[spec_data.element_cnt].interval_end = select_wave + 0.05;
                spec_data.elements[spec_data.element_cnt].peak_wave = select_wave;
                spec_data.element_cnt++;

                //重绘已选元素表
                datagrid_control.draw_datagrid_select_element(dataGridView1, spec_data.elements, spec_data.element_cnt);
            }
            else
            {
                MessageBox.Show("该元素已存在");
            }



            
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            float a = -13.5f;
            MessageBox.Show((a-a%10+10)+"");
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
            }
            datagrid_control.draw_datagrid_standard_setting(dataGridView5, spec_data.elements, spec_data.standards, spec_data.element_cnt, spec_data.standard_cnt);
        }

        private void textBox16_Click(object sender, EventArgs e)
        {
            if (textBox16.Text != null && textBox16.Text != "")
            {
                int sc = int.Parse(textBox16.Text);
                if (sc >= 0 && sc <= 20)
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
                datagrid_control.draw_datagrid_sample_setting(dataGridView7, spec_data.samples, spec_data.sample_cnt);

            }
        }

        //添加样本
        private void button7_Click(object sender, EventArgs e)
        {

        }


        //连接按钮点击响应事件
        private void button1_Click(object sender, EventArgs e)
        {
            if (!wrapper.connect())
            {
                MessageBox.Show("设备连接失败");
            }
            spec_data.read_wave_all = wrapper.get_wave_all();
        }

        //设置按钮点击响应事件
        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                wrapper.set_correct_for_electrical_dart(1);
            }
            else
            {
                wrapper.set_correct_for_electrical_dart(0);
            }
            int average_times = int.Parse(textBox8.Text);
            wrapper.set_times_for_average(average_times);
            int intergration_time1 = int.Parse(textBox1.Text);
            int intergration_time2 = int.Parse(textBox2.Text);
            int intergration_time3 = int.Parse(textBox3.Text);
            int intergration_time4 = int.Parse(textBox4.Text);
            int intergration_time5 = int.Parse(textBox5.Text);
            int intergration_time6 = int.Parse(textBox6.Text);
            wrapper.set_intergration_time(0, intergration_time1);
            wrapper.set_intergration_time(1, intergration_time2);
            wrapper.set_intergration_time(2, intergration_time3);
            wrapper.set_intergration_time(3, intergration_time4);
            wrapper.set_intergration_time(4, intergration_time5);
            wrapper.set_intergration_time(5, intergration_time6);
        }

        //读取按钮点击响应事件
        private void button3_Click(object sender, EventArgs e)
        {
            //调取新数据
            spec_data.read_spec_all_now = wrapper.get_spec_all();
            // @增加扣除本底
            if (radioButton_subbase.Checked)
            {
                for (int i = 0; i < spec_data.read_spec_all_now.Length; i++)
                {
                    spec_data.read_spec_all_now[i] -= spec_data.env_spec[i];
                }

            }
            x_init = spec_data.read_wave_all;
            y_init = spec_data.read_spec_all_now;
            x = x_init;
            y = y_init;

            clearPaint_1();
            drawLine_Group_1(x,y);
            ifExistPlot_1 = true;


        }


        //保存为本底按钮点击响应事件
        private void btn_saveas_base_Click(object sender, EventArgs e)
        {
            spec_data.env_spec = spec_data.read_spec_all_now;
            //打开本地扣除选项
            radioButton_subbase.Enabled = true;
        }

        //断开按钮点击响应事件（空）
        private void button4_Click(object sender, EventArgs e)
        {
            wrapper.disconnect();
        }

        //------------------------------------------------------------------------------
        private void panel_XY_MouseDown(object sender, MouseEventArgs e)
        {
            if (ifExistPlot_1 && !ifStartTimer)
            {
                x_ChangeBegin_1 = pixToData_1(e.X, e.Y).X;
                y_ChangeBegin_1 = pixToData_1(e.X, e.Y).Y;
                //x_min_timer = x_ChangeBegin_1;
                //y_min_timer = y_ChangeBegin_1;
                //按下鼠标画矩形逻辑
                recStart_1.X = e.X;
                recStart_1.Y = e.Y;
                recEnd_1.X = e.X;
                recEnd_1.Y = e.Y;
                blnDraw_1 = true;
            }
        }

        private float locX = 0, LocY = 0;//消除tooltip闪烁问题，如果鼠标位置与当前locX，LocY位置相同则不刷新
        private void panel_XY_MouseMove(object sender, MouseEventArgs e)
        {
            if (ifExistPlot_1 && !ifStartTimer)
            {
                float x_pix = e.X, y_pix = e.Y;//当前鼠标的像素数据
                float x_real = -1, y_real = -1;//当前鼠标的实验数据
                x_real = pixToData_1(x_pix, y_pix).X;
                y_real = pixToData_1(x_pix, y_pix).Y;
                float x_real_show = x_real;
                float y_real_show = y_real;

                for (int i=0;i<x.Length-2;i++)
                {
                    if (x_real<x[i])
                    {
                        x_real_show = (float)x_init[i];
                        y_real_show = (float)y_init[i];
                        break;
                    }
                    else if (x_real>x[x.Length-1])
                    {
                        x_real_show = (float)x[x.Length - 1];
                        y_real_show = (float)y[x.Length - 1];
                        break;
                    }
                    else
                    {
                        if ((x_real>=x[i])&&(x_real<=x[i+1]))
                        {
                            x_real_show =(float) x[i];
                            y_real_show =(float) y[i];
                            break;
                        }
                    }
                }

                


                if ((x_real < x_min_1) || (x_real > x_max_1) || (y_real < y_min_1) || (y_real > y_max_1))
                {
                    //若鼠标范围超出数据轴范围，则不显示toolTip
                    toolTip_1.Show("", this.panel_XY, new Point(e.X, e.Y));
                }
                else
                {
                    if ((e.X != locX) || (e.Y != LocY))//防止显示闪烁
                    {
                        toolTip_1.Show(x_real_show + " , " + y_real_show + "", this.panel_XY, new Point(e.X + 5, e.Y - 28));
                        drawDot_1(convert_1(x_real_show, y_real_show));

                        locX = e.X;
                        LocY = e.Y;
                    }

                    // 鼠标点显示垂直轴
                    if (locNowBoolean_1)
                    {
                        //如果locNowBoolean为真，说明已经画过垂直线，则需要先擦除之前的垂直线
                        g_bitmap_1.DrawLine(new Pen(Color.White, 1), locNowP1_1, locNowP2_1);
                        gg_1.DrawImage(myBitmap_1, 0, 0);
                        // 重绘图像补全空白处
                        //clearPaint_1();
                        drawLine_Group_1(x, y);

                    }
                    PointF p1 = new PointF(e.X, 0);
                    PointF p2 = new PointF(e.X, (float)(panel_XY.Height - move));
                    Pen mPen = new Pen(Color.Blue, 1);
                    g_bitmap_1.DrawLine(mPen, p1, p2);
                    gg_1.DrawImage(myBitmap_1, 0, 0);
                    locNow_1.X = e.X;
                    locNow_1.Y = e.Y;
                    locNowP1_1 = p1;
                    locNowP2_1 = p2;
                    locNowBoolean_1 = true;

                    

                }

                //按下鼠标移动过程中画矩形逻辑
                if (blnDraw_1)
                {
                    //先擦除
                    g_bitmap_1.DrawRectangle(new Pen(Color.White, 2), recStart_1.X, recStart_1.Y, recEnd_1.X - recStart_1.X, recEnd_1.Y - recStart_1.Y);
                    recEnd_1.X = e.X;
                    recEnd_1.Y = e.Y;
                    //再画
                    g_bitmap_1.DrawRectangle(new Pen(Color.Blue, 2), recStart_1.X, recStart_1.Y, recEnd_1.X - recStart_1.X, recEnd_1.Y - recStart_1.Y);
                    gg_1.DrawImage(myBitmap_1, 0, 0);
                }
            }//(end) if (ifExistPlot_1 && !ifStartTimer)
            else if (ifExistPlot_1 && ifStartTimer)
            {
                float x_pix = e.X, y_pix = e.Y;//当前鼠标的像素数据
                float x_real = -1, y_real = -1;//当前鼠标的实验数据
                x_real = pixToData_1_timer(x_pix, y_pix).X;
                y_real = pixToData_1_timer(x_pix, y_pix).Y;
                float x_real_show = x_real;
                float y_real_show = y_real;

                for (int i = 0; i < x.Length - 2; i++)
                {
                    if (x_real < x[i])
                    {
                        x_real_show = (float)x_init[i];
                        y_real_show = (float)y_init[i];
                        break;
                    }
                    else if (x_real > x[x.Length - 1])
                    {
                        x_real_show = (float)x[x.Length - 1];
                        y_real_show = (float)y[x.Length - 1];
                        break;
                    }
                    else
                    {
                        if ((x_real >= x[i]) && (x_real <= x[i + 1]))
                        {
                            x_real_show = (float)x[i];
                            y_real_show = (float)y[i];
                            break;
                        }
                    }
                }

                if ((x_real < x_min_timer) || (x_real > x_max_timer) || (y_real < y_min_timer) || (y_real > y_max_timer))
                {
                    //若鼠标范围超出数据轴范围，则不显示toolTip
                    toolTip_1.Show("", this.panel_XY, new Point(e.X, e.Y));
                }
                else
                {
                    if ((e.X != locX) || (e.Y != LocY))//防止显示闪烁
                    {
                        toolTip_1.Show(x_real_show + " , " + y_real_show + "", this.panel_XY, new Point(e.X + 5, e.Y - 28));
                        locX = e.X;
                        LocY = e.Y;
                    }

                    // 鼠标点显示垂直轴
                    if (locNowBoolean_1)
                    {
                        //如果locNowBoolean为真，说明已经画过垂直线，则需要先擦除之前的垂直线
                        g_bitmap_1.DrawLine(new Pen(Color.White, 1), locNowP1_1, locNowP2_1);
                        gg_1.DrawImage(myBitmap_1, 0, 0);
                        // 重绘图像补全空白处
                        //drawLine_Group_1(x, y);
                        drawLine_group_timer_1(x, y, x_min_timer, x_max_timer);

                    }
                    PointF p1 = new PointF(e.X, 0);
                    PointF p2 = new PointF(e.X, (float)(panel_XY.Height - move));
                    Pen mPen = new Pen(Color.Blue, 1);
                    g_bitmap_1.DrawLine(mPen, p1, p2);
                    gg_1.DrawImage(myBitmap_1, 0, 0);
                    locNow_1.X = e.X;
                    locNow_1.Y = e.Y;
                    locNowP1_1 = p1;
                    locNowP2_1 = p2;
                    locNowBoolean_1 = true;

                }



            }

        }//(end) void panel_XY_MouseMove

        private void panel_XY_MouseUp(object sender, MouseEventArgs e)
        {
            if (ifExistPlot_1 && !ifStartTimer)
            {
                g_bitmap_1.DrawRectangle(new Pen(Color.Blue), recStart_1.X, recStart_1.Y, e.X - recStart_1.X, e.Y - recStart_1.Y);
                gg_1.DrawImage(myBitmap_1, 0, 0);
                blnDraw_1 = false;

                //获取终止点参数用于从新画图
                x_ChangeEnd_1 = pixToData_1(e.X, e.Y).X;
                y_ChangeEnd_1 = pixToData_1(e.X, e.Y).Y;
                //x_max_timer = x_ChangeEnd_1;
                //y_max_timer = y_ChangeEnd_1;
                //MessageBox.Show(x_ChangeBegin_1 + "  " + y_ChangeBegin_1 + ";    " + x_ChangeEnd_1 + "   " + y_ChangeEnd_1);

                //防止单击时重绘
                if (x_ChangeBegin_1 < x_ChangeEnd_1)
                {

                    List<double> x_temp = new List<double>();
                    List<double> y_temp = new List<double>();

                    for (int i = 0; i < x.Length; i++)
                    {
                        if (x[i] >= x_ChangeBegin_1 && x[i] <= x_ChangeEnd_1)
                        {
                            x_temp.Add(x[i]);
                            y_temp.Add(y[i]);
                        }
                    }
                    x = x_temp.ToArray();
                    y = y_temp.ToArray();

                    g_bitmap_1.Clear(Color.White);
                    drawLine_Group_1(x, y);
                }


            }
        }
        //---------------------------------------------------------------------------------




        /// 画坐标图功能性函数----------------------------------------
        /// 

        //画tabpage1的x,y轴
        public void drawXY_1()
        {
            double newX = this.panel_XY.Width - move;
            double newY = this.panel_XY.Height - move;

            //绘制X轴,  
            PointF px1 = new PointF((float)move, (float)newY);
            PointF px2 = new PointF((float)newX, (float)newY);
            g_bitmap_1.DrawLine(new Pen(Brushes.Black, 2), px1, px2);
            //绘制Y轴  
            PointF py1 = new PointF((float)move, (float)move);
            PointF py2 = new PointF((float)move, (float)newY);
            g_bitmap_1.DrawLine(new Pen(Brushes.Black, 2), py1, py2);

            gg_1.DrawImage(myBitmap_1, 0, 0);
        }

        public void drawXY_3()
        {
            double newX = this.panel3_XY.Width - move;
            double newY = this.panel3_XY.Height - move;

            //绘制X轴,  
            PointF px1 = new PointF((float)move, (float)newY);
            PointF px2 = new PointF((float)newX, (float)newY);
            g_bitmap_3.DrawLine(new Pen(Brushes.Black, 2), px1, px2);
            //绘制Y轴  
            PointF py1 = new PointF((float)move, (float)move);
            PointF py2 = new PointF((float)move, (float)newY);
            g_bitmap_3.DrawLine(new Pen(Brushes.Black, 2), py1, py2);

            gg_3.DrawImage(myBitmap_3, 0, 0);
        }

        //恢复原图像点击响应事件
        private void button11_Click(object sender, EventArgs e)
        {
            x = x_init;
            y = y_init;
            clearPaint_1();
            drawLine_Group_1(x,y);
        }

        //开始按钮点击响应事件
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            //动态添加一个定时器
            this.timer1.Enabled = true;//可以使用
            this.timer1.Interval = 1500;//定时时间为1500毫秒
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);

            this.timer1.Start();//启动定时器
            ifStartTimer = true;
        }


        //timer响应事件
        private void timer1_Tick(object sender, EventArgs e)
        {
            clearPaint_1();
            //@jayce 需要动态从设备拿最新数据
            spec_data.read_spec_all_now = wrapper.get_spec_all();
            // @增加扣除本底
            if (radioButton_subbase.Checked)
            {
                for (int i = 0; i < spec_data.read_spec_all_now.Length; i++)
                {
                    spec_data.read_spec_all_now[i] -= spec_data.env_spec[i];
                }

            }

            x_init = spec_data.read_wave_all;
            y_init = spec_data.read_spec_all_now;
            x = x_init;
            y = y_init;

            drawLine_group_timer_1(x, y, x_min_timer, x_max_timer);
            


        }

        //保存文件点击响应事件
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "LIBS文件 (*.libs)|*.libs";
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = "spec_metadat";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                file_operator.save_spec_metadat(saveFileDialog1.FileName,spec_data);
            }

        }

        

        //停止按钮点击响应事件
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
            ifStartTimer = false;
            //clearPaint_1();
        }

        //dataGridView4:单击cell响应事件
        private void dataGridView4_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex!=-1)
            {
                tableRowCount = e.RowIndex;
                textBox20.Text = spec_data.elements[tableRowCount].seek_peak_range + "";
            }
        }

        //dataGridView4:双击cell响应事件
        private void dataGridView4_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex!=-1)
            {
                //MessageBox.Show(e.RowIndex + "  " + e.ColumnIndex);
                double wave_example = spec_data.elements[e.RowIndex].select_wave;
                double wave_begin = wave_example - 20;
                double wave_end = wave_example + 20;
                x_min_3 = wave_begin;
                x_max_3 = wave_end;
                x_min_3_init = wave_begin;
                x_max_3_init = wave_end;

                List<double> x_temp = new List<double>();
                List<double> y_temp = new List<double>();


                for (int i = 0; i < x_init.Length; i++)
                {
                    if (x_init[i] >= wave_begin && x_init[i] <= wave_end)
                    {
                        x_temp.Add(x_init[i]);
                        y_temp.Add(y_init[i]);
                    }
                }

                x_init_tabpage3_array= x_temp.ToArray();
                y_init_tabpage3_array= y_temp.ToArray();
                x_tabpage3_array = x_init_tabpage3_array;
                y_tabpage3_array = y_init_tabpage3_array;

                clearPaint_3();
                drawLine_Group_3(x_tabpage3_array, y_tabpage3_array);
                ifExistPlot_3 = true;




            }// if (e.RowIndex!=-1)

        }

        private void button9_Click(object sender, EventArgs e)
        {
            double get = double.Parse(textBox20.Text);
            spec_data.elements[tableRowCount].seek_peak_range = get;
            double[] dd = findWaveSpec_Range(spec_data.elements[tableRowCount].select_wave, get);
            //@jayce 分析窗体读取样本/标样时会根据寻峰范围对其当此读取寻峰，这里只需要设置元素寻峰范围
            //spec_data.elements[tableRowCount].peak_wave = dd[0];
            dt4.Rows[tableRowCount][3] = dd[0];
            dt4.Rows[tableRowCount][5] = dd[1];
            this.dataGridView4.DataSource = dt4;
        }

        //画tabpage1的x轴刻度
        public void drawXLineValue_1(double minX, double maxX, int count)
        {
            double LenX = this.panel_XY.Width - 2 * move;

            for (int i = 0; i <= count; i++)
            {
                PointF py1 = new PointF((float)(LenX * i / count + move), (float)(this.panel_XY.Height - move - 4));
                PointF py2 = new PointF((float)(LenX * i / count + move), (float)(this.panel_XY.Height - move));
                double va = Math.Round((maxX - minX) * i / count + minX, 3); //刻度值取3位小数
                string sy = va.ToString();
                g_bitmap_1.DrawLine(new Pen(Brushes.Black, 2), py1, py2);
                g_bitmap_1.DrawString(sy, new Font("宋体", 8f), Brushes.Black, new PointF((float)(LenX * i / count + move), (float)(this.panel_XY.Height - move / 1.1f)));
            }
            Pen pen = new Pen(Color.Black, 1);
            g_bitmap_1.DrawString("X轴", new Font("宋体 ", 8f), Brushes.Black, new PointF((float)(this.panel_XY.Width - move / 1.5f), (float)(this.panel_XY.Height - move / 1.5f)));
            gg_1.DrawImage(myBitmap_1, 0, 0);
        }

        public void drawXLineValue_3(double minX, double maxX, int count)
        {
            double LenX = this.panel3_XY.Width - 2 * move;

            for (int i = 0; i <= count; i++)
            {
                PointF py1 = new PointF((float)(LenX * i / count + move), (float)(this.panel3_XY.Height - move - 4));
                PointF py2 = new PointF((float)(LenX * i / count + move), (float)(this.panel3_XY.Height - move));
                double va = Math.Round((maxX - minX) * i / count + minX, 3); //刻度值取3位小数
                string sy = va.ToString();
                g_bitmap_3.DrawLine(new Pen(Brushes.Black, 2), py1, py2);
                g_bitmap_3.DrawString(sy, new Font("宋体", 8f), Brushes.Black, new PointF((float)(LenX * i / count + move), (float)(this.panel3_XY.Height - move / 1.1f)));
            }
            Pen pen = new Pen(Color.Black, 1);
            g_bitmap_3.DrawString("X轴", new Font("宋体 ", 8f), Brushes.Black, new PointF((float)(this.panel3_XY.Width - move / 1.5f), (float)(this.panel3_XY.Height - move / 1.5f)));
            gg_3.DrawImage(myBitmap_3, 0, 0);
        }


        //画tabpage1的y轴刻度
        public void drawYLineValue_1(float minY, float maxY, int count)
        {
            double LenY = this.panel_XY.Height - 2 * move;

            for (int i = 0; i <= count; i++)
            {
                PointF px1 = new PointF((float)move, (float)(LenY * i / count + move));
                PointF px2 = new PointF((float)(move + 4), (float)(LenY * i / count + move));
                double va = Math.Round(maxY - (maxY - minY) * i / count, 3); //刻度值取四位小数
                string sx = va.ToString();
                g_bitmap_1.DrawLine(new Pen(Brushes.Black, 2), px1, px2);
                StringFormat drawFormat = new StringFormat();
                drawFormat.Alignment = StringAlignment.Far;
                drawFormat.LineAlignment = StringAlignment.Center;
                g_bitmap_1.DrawString(sx, new Font("宋体", 8f), Brushes.Black, new PointF((float)(move / 1.2f), (float)(LenY * i / count + move * 1.1f)), drawFormat);
            }
            Pen pen = new Pen(Color.Black, 1);
            g_bitmap_1.DrawString("Y轴", new Font("宋体 ", 8f), Brushes.Black, new PointF((float)(move / 3), (float)(move / 2f)));
            gg_1.DrawImage(myBitmap_1, 0, 0);
        }

        public void drawYLineValue_3(float minY, float maxY, int count)
        {
            double LenY = this.panel3_XY.Height - 2 * move;

            for (int i = 0; i <= count; i++)
            {
                PointF px1 = new PointF((float)move, (float)(LenY * i / count + move));
                PointF px2 = new PointF((float)(move + 4), (float)(LenY * i / count + move));
                double va = Math.Round(maxY - (maxY - minY) * i / count, 3); //刻度值取四位小数
                string sx = va.ToString();
                g_bitmap_3.DrawLine(new Pen(Brushes.Black, 2), px1, px2);
                StringFormat drawFormat = new StringFormat();
                drawFormat.Alignment = StringAlignment.Far;
                drawFormat.LineAlignment = StringAlignment.Center;
                g_bitmap_3.DrawString(sx, new Font("宋体", 8f), Brushes.Black, new PointF((float)(move / 1.2f), (float)(LenY * i / count + move * 1.1f)), drawFormat);
            }
            Pen pen = new Pen(Color.Black, 1);
            g_bitmap_3.DrawString("Y轴", new Font("宋体 ", 8f), Brushes.Black, new PointF((float)(move / 3), (float)(move / 2f)));
            gg_3.DrawImage(myBitmap_3, 0, 0);
        }

        //=====================================================================================================

        bool ifExistPlot_3 = false;
        ToolTip toolTip_3=new ToolTip();

        private void panel3_XY_MouseDown(object sender, MouseEventArgs e)
        {
            if (ifExistPlot_3)
            {
                x_ChangeBegin_3 = pixToData_3(e.X, e.Y).X;
                y_ChangeBegin_3 = pixToData_3(e.X, e.Y).Y;
                //按下鼠标画矩形逻辑
                recStart_3.X = e.X;
                recStart_3.Y = e.Y;
                recEnd_3.X = e.X;
                recEnd_3.Y = e.Y;
                blnDraw_3 = true;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            clearPaint_3();
            x_tabpage3_array = x_init_tabpage3_array;
            y_tabpage3_array = y_init_tabpage3_array;
            x_min_3 = x_min_3_init;
            x_max_3 = x_max_3_init;
            drawLine_Group_3(x_tabpage3_array, y_tabpage3_array);

        }

        private void panel3_XY_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel_XY_MouseLeave(object sender, EventArgs e)
        {
            toolTip_1.Hide(this.panel_XY);
        }

        private void panel3_XY_MouseLeave(object sender, EventArgs e)
        {
            toolTip_3.Hide(this.panel3_XY);
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void panel_XY_Paint(object sender, PaintEventArgs e)
        {

        }

        private float locX_3 = 0, LocY_3 = 0;//消除tooltip闪烁问题，如果鼠标位置与当前locX，LocY位置相同则不刷新
        private void panel3_XY_MouseMove(object sender, MouseEventArgs e)
        {
            if (ifExistPlot_3)
            {
                float x_pix_3 = e.X, y_pix_3 = e.Y;//当前鼠标的像素数据
                float x_real_3 = -1, y_real_3 = -1;//当前鼠标的实验数据
                x_real_3 = pixToData_3(x_pix_3, y_pix_3).X;
                y_real_3 = pixToData_3(x_pix_3, y_pix_3).Y;
                float x_real_3_show = x_real_3;
                float y_real_3_show = y_real_3;

                for (int i = 0; i < x_tabpage3_array.Length - 2; i++)
                {
                    if (x_real_3 < x_tabpage3_array[i])
                    {
                        x_real_3_show = (float)x_tabpage3_array[i];
                        y_real_3_show = (float)y_tabpage3_array[i];
                        break;
                    }
                    else if (x_real_3 > x_tabpage3_array[x_tabpage3_array.Length - 1])
                    {
                        x_real_3_show = (float)x_tabpage3_array[x_tabpage3_array.Length - 1];
                        y_real_3_show = (float)y_tabpage3_array[x_tabpage3_array.Length - 1];
                        break;
                    }
                    else
                    {
                        if ((x_real_3 >= x_tabpage3_array[i]) && (x_real_3 <= x_tabpage3_array[i + 1]))
                        {
                            x_real_3_show = (float)x_tabpage3_array[i];
                            y_real_3_show = (float)y_tabpage3_array[i];
                            break;
                        }
                    }
                }

                if ((x_real_3 < x_min_3) || (x_real_3 > x_max_3) || (y_real_3 < y_min_3) || (y_real_3 > y_max_3))
                {
                    //若鼠标范围超出数据轴范围，则不显示toolTip
                    toolTip_3.Show("", this.panel3_XY, new Point(e.X, e.Y));
                }
                else
                {
                    if ((e.X != locX_3) || (e.Y != LocY_3))//防止显示闪烁
                    {
                        toolTip_3.Show(x_real_3_show + " , " + y_real_3_show + "", this.panel3_XY, new Point(e.X + 5, e.Y - 28));
                        locX_3 = e.X;
                        LocY_3 = e.Y;
                    }

                    // 鼠标点显示垂直轴
                    if (locNowBoolean_3)
                    {
                        //如果locNowBoolean为真，说明已经画过垂直线，则需要先擦除之前的垂直线
                        g_bitmap_3.DrawLine(new Pen(Color.White, 1), locNowP1_3, locNowP2_3);
                        gg_3.DrawImage(myBitmap_3, 0, 0);
                        // 重绘图像补全空白处
                        drawLine_Group_3(x_tabpage3_array, y_tabpage3_array);
                    }

                    PointF p1 = new PointF(e.X, 0);
                    PointF p2 = new PointF(e.X, (float)(panel3_XY.Height - move));
                    Pen mPen = new Pen(Color.Blue, 1);
                    g_bitmap_3.DrawLine(mPen, p1, p2);
                    gg_3.DrawImage(myBitmap_3, 0, 0);
                    locNow_3.X = e.X;
                    locNow_3.Y = e.Y;
                    locNowP1_3 = p1;
                    locNowP2_3 = p2;
                    locNowBoolean_3 = true;

                }


                // 按下鼠标移动过程中画矩形逻辑
                if (blnDraw_3)
                {
                    //先擦除
                    g_bitmap_3.DrawRectangle(new Pen(Color.White, 2), recStart_3.X, recStart_3.Y, recEnd_3.X - recStart_3.X, recEnd_3.Y - recStart_3.Y);
                    recEnd_3.X = e.X;
                    recEnd_3.Y = e.Y;
                    //再画
                    g_bitmap_3.DrawRectangle(new Pen(Color.Blue, 2), recStart_3.X, recStart_3.Y, recEnd_3.X - recStart_3.X, recEnd_3.Y - recStart_3.Y);
                    gg_3.DrawImage(myBitmap_3, 0, 0);
                }

            }//(end)if (ifExistPlot_3)
        }

        private void panel3_XY_MouseUp(object sender, MouseEventArgs e)
        {
            if (ifExistPlot_3)
            {
                g_bitmap_3.DrawRectangle(new Pen(Color.Blue), recStart_3.X, recStart_3.Y, e.X - recStart_3.X, e.Y - recStart_3.Y);
                gg_3.DrawImage(myBitmap_3, 0, 0);
                blnDraw_3 = false;

                //获取终止点参数用于从新画图
                x_ChangeEnd_3 = pixToData_3(e.X, e.Y).X;
                y_ChangeEnd_3 = pixToData_3(e.X, e.Y).Y;
                //MessageBox.Show(x_ChangeBegin_1 + "  " + y_ChangeBegin_1 + ";    " + x_ChangeEnd_1 + "   " + y_ChangeEnd_1);

                //防止单击时重绘
                if (x_ChangeBegin_3 < x_ChangeEnd_3)
                {

                    List<double> x_temp = new List<double>();
                    List<double> y_temp = new List<double>();

                    for (int i = 0; i < x_init.Length; i++)
                    {
                        if (x_init[i] >= x_ChangeBegin_3 && x_init[i] <= x_ChangeEnd_3)
                        {
                            x_temp.Add(x_init[i]);
                            y_temp.Add(y_init[i]);
                        }
                    }
                    x_tabpage3_array = x_temp.ToArray();
                    y_tabpage3_array = y_temp.ToArray();

                    x_min_3 = x_ChangeBegin_3;
                    x_max_3 = x_ChangeEnd_3;

                    g_bitmap_3.Clear(Color.White);
                    drawLine_Group_3(x_tabpage3_array, y_tabpage3_array);
                }


            }
        }

        //===================================================================================================


        //画tabpage1上的坐标点
        void drawDot_1(PointF p)
        {
            g_bitmap_1.FillEllipse(Brushes.Red, p.X, p.Y, 2.5f, 2.5f);
            gg_1.DrawImage(myBitmap_1, 0, 0);
        }

        void drawDot_3(PointF p)
        {
            g_bitmap_3.FillEllipse(Brushes.Red, p.X, p.Y, 2f, 2f);
            gg_3.DrawImage(myBitmap_3, 0, 0);
        }

        //tabpage1上:输入两个点画出这两点及之间的直线
        void drawLine_twoPoint_1(PointF p1, PointF p2)
        {
            drawDot_1(p1);
            drawDot_1(p2);
            Pen myPen = new Pen(Color.Blue, 1);
            g_bitmap_1.DrawLine(myPen, p1, p2);
            gg_1.DrawImage(myBitmap_1, 0, 0);
        }

        //把 实际参数 转换成 坐标系中点的参数
        PointF convert_1(float x, float y)
        {
            PointF p = new PointF();
            double x_each_len = (panel_XY.Width - 2 * move) / (x_max_1 - x_min_1);
            double y_each_len = (panel_XY.Height - 2 * move) / (y_max_1 - y_min_1);
            double x_temp = move + (x - x_min_1) * x_each_len;
            double y_temp = panel_XY.Height - move - (y - y_min_1) * y_each_len;
            p.X = (float)x_temp;
            p.Y = (float)y_temp;
            return p;
        }

        PointF convert_3(float x, float y)
        {
            PointF p = new PointF();
            double x_each_len = (panel3_XY.Width - 2 * move) / (x_max_3 - x_min_3);
            double y_each_len = (panel3_XY.Height - 2 * move) / (y_max_3 - y_min_3);
            double x_temp = move + (x - x_min_3) * x_each_len;
            double y_temp = panel3_XY.Height - move - (y - y_min_3) * y_each_len;
            p.X = (float)x_temp;
            p.Y = (float)y_temp;
            return p;
        }

        //将鼠标的像素数值转化成实验数值
        PointF pixToData_1(float x_pix, float y_pix)
        {
            PointF ptemp = new PointF();
            double x_tran, y_tran;
            x_tran = (this.panel_XY.Width - move - move) / (x_max_1 - x_min_1);
            y_tran = (this.panel_XY.Height - move - move) / (y_max_1 - y_min_1);
            ptemp.X = (float)((x_pix - move) / x_tran + x_min_1);
            ptemp.Y = (float)(y_max_1 - (y_pix - move) / y_tran);
            return ptemp;
        }

        PointF pixToData_1_timer(float x_pix, float y_pix)
        {
            PointF ptemp = new PointF();
            double x_tran, y_tran;
            x_tran = (this.panel_XY.Width - move - move) / (x_max_timer - x_min_timer);
            y_tran = (this.panel_XY.Height - move - move) / (y_max_timer - y_min_timer);
            ptemp.X = (float)((x_pix - move) / x_tran + x_min_timer);
            ptemp.Y = (float)(y_max_timer - (y_pix - move) / y_tran);
            return ptemp;
        }

        PointF pixToData_3(float x_pix, float y_pix)
        {
            PointF ptemp = new PointF();
            double x_tran, y_tran;
            x_tran = (this.panel3_XY.Width - move - move) / (x_max_3 - x_min_3);
            y_tran = (this.panel3_XY.Height - move - move) / (y_max_3 - y_min_3);
            ptemp.X = (float)((x_pix - move) / x_tran + x_min_3);
            ptemp.Y = (float)(y_max_3 - (y_pix - move) / y_tran);
            return ptemp;
        }

        ///  输入一组坐标数组，在坐标轴中依次画点连直线
        ///  自动调整坐标轴范围
        void drawLine_Group_1(double[] x, double[] y)
        {
            int n = x.Length;
            if (n <= 0)
            {
                //MessageBox.Show("传入数组没有元素");
                drawXY_1();
                drawXLineValue_1(x_min_1, x_max_1, 10);
                drawYLineValue_1( (float)y_min_1, (float)y_max_1, 10);
            }
            else
            {
                //将x,y数组合并成PointF类型数组
                PointF[] pGroup = new PointF[n];
                for (int i = 0; i < n; i++)
                {
                    pGroup[i].X = (float)x[i];
                    pGroup[i].Y = (float)y[i];
                }

                //找出数组中x,y的最大最小值用于确定坐标测度
                float x_l = pGroup[0].X, x_h = pGroup[0].X;//所给数据中x的最小值，最大值
                float y_l = pGroup[0].Y, y_h = pGroup[0].Y;//所给数据中y的最大值，最小值

                for (int i = 0; i < n; i++)
                {
                    if (pGroup[i].Y < y_l)
                    {
                        y_l = pGroup[i].Y;
                    }
                    if (pGroup[i].Y > y_h)
                    {
                        y_h = pGroup[i].Y;
                    }
                    if (pGroup[i].X < x_l)
                    {
                        x_l = pGroup[i].X;
                    }
                    if (pGroup[i].X > x_h)
                    {
                        x_h = pGroup[i].X;
                    }
                }

                //确定x,y轴取值范围  (规则)
                x_min_1 = (int)(x_l);
                x_max_1 = (int)(x_h) + 1;
                y_min_1 = y_l - y_l % 10 - 10;//a-a%10-10
                y_max_1 = y_h - y_h % 10 + 10;//a-a%10+10

                //确定x,y轴取值范围  (规则)
                //if (y_l >= 0)
                //    {
                //        x_min_1 = (int)(x_l );
                //        x_max_1 = (int)(x_h) + 1;
                //        y_min_1 = (int)(y_l - y_l % 10);
                //        y_max_1 = (int)(y_h) + 1;
                //    }
                //    else
                //    {
                //        x_min_1 = (int)(x_l );
                //        x_max_1 = (int)(x_h) + 1;
                //        //y_min_1 = (int)(y_l - y_l % 10);
                //        y_max_1 = (int)(y_h) + 1;

                //        y_min_1 = (int)(-((-y_l) - (-y_l) % 10 + 10));

                //    }
                
                x_min_timer = x_min_1;
                x_max_timer = x_max_1;
                y_min_timer = y_min_1;
                y_max_timer = y_max_1;
                

                //根据x,y轴取值范围画坐标轴与刻度
                drawXY_1();
                drawXLineValue_1( x_min_1, x_max_1, 10);
                drawYLineValue_1((float)y_min_1, (float)y_max_1, 10);


                //画图
                if (n == 1)
                {
                    drawDot_1(convert_1(pGroup[0].X, pGroup[0].Y));
                }
                else
                {
                    //先把图画在内存中的bitmap再画在插件上
                    Pen myPen = new Pen(Color.Green, 1);
                    for (int i = 0; i < n - 1; i++)
                    {
                        //drawLine_twoPoint(convert(pGroup[i].X, pGroup[i].Y), convert(pGroup[i + 1].X, pGroup[i + 1].Y));
                        PointF p1 = convert_1(pGroup[i].X, pGroup[i].Y);
                        PointF p2 = convert_1(pGroup[i + 1].X, pGroup[i + 1].Y);
                        g_bitmap_1.FillEllipse(Brushes.Green, p1.X, p1.Y, 2f, 2f);
                        g_bitmap_1.FillEllipse(Brushes.Green, p2.X, p2.Y, 2f, 2f);
                        g_bitmap_1.DrawLine(myPen, p1, p2);

                    }
                    gg_1.DrawImage(myBitmap_1, 0, 0);

                }
            }

        }//(end) void drawLine_Group_1

        void drawLine_Group_3(double[] x, double[] y)
        {
            int n = x.Length;
            if (n <= 0)
            {
                //MessageBox.Show("传入数组没有元素");
                drawXY_3();
                drawXLineValue_3(0, 12, 12);
                drawYLineValue_3(2,  8,  6);
            }
            else
            {
                //将x,y数组合并成PointF类型数组
                PointF[] pGroup = new PointF[n];
                for (int i = 0; i < n; i++)
                {
                    pGroup[i].X = (float)x[i];
                    pGroup[i].Y = (float)y[i];
                }

                //找出数组中x,y的最大最小值用于确定坐标测度
                float x_l = pGroup[0].X, x_h = pGroup[0].X;//所给数据中x的最小值，最大值
                float y_l = pGroup[0].Y, y_h = pGroup[0].Y;//所给数据中y的最大值，最小值

                for (int i = 0; i < n; i++)
                {
                    if (pGroup[i].Y < y_l)
                    {
                        y_l = pGroup[i].Y;
                    }
                    if (pGroup[i].Y > y_h)
                    {
                        y_h = pGroup[i].Y;
                    }
                    if (pGroup[i].X < x_l)
                    {
                        x_l = pGroup[i].X;
                    }
                    if (pGroup[i].X > x_h)
                    {
                        x_h = pGroup[i].X;
                    }
                }

                //确定x,y轴取值范围  (规则)
                if (y_l >= 0)
                {
                    //x_min_3 = (int)(x_l - x_l % 10);
                    //x_max_3 = (int)(x_h) + 1;
                    y_min_3 = (int)(y_l - y_l % 10);
                    y_max_3 = (int)(y_h) + 1;
                }
                else
                {
                    //x_min_3 = (int)(x_l - x_l % 10);
                    //x_max_3 = (int)(x_h) + 1;
                    //y_min_1 = (int)(y_l - y_l % 10);
                    y_max_3 = (int)(y_h) + 1;

                    y_min_3 = (int)(-((-y_l) - (-y_l) % 10 + 10));

                }


                //根据x,y轴取值范围画坐标轴与刻度
                drawXY_3();
                drawXLineValue_3(x_min_3, x_max_3, 10);
                drawYLineValue_3((float)y_min_3, (float)y_max_3, 10);


                //画图
                if (n == 1)
                {
                    drawDot_3(convert_3(pGroup[0].X, pGroup[0].Y));
                }
                else
                {
                    //先把图画在内存中的bitmap再画在插件上
                    Pen myPen = new Pen(Color.Green, 1);
                    for (int i = 0; i < n - 1; i++)
                    {
                        //drawLine_twoPoint(convert(pGroup[i].X, pGroup[i].Y), convert(pGroup[i + 1].X, pGroup[i + 1].Y));
                        PointF p1 = convert_3(pGroup[i].X, pGroup[i].Y);
                        PointF p2 = convert_3(pGroup[i + 1].X, pGroup[i + 1].Y);
                        g_bitmap_3.FillEllipse(Brushes.Green, p1.X, p1.Y, 2f, 2f);
                        g_bitmap_3.FillEllipse(Brushes.Green, p2.X, p2.Y, 2f, 2f);
                        g_bitmap_3.DrawLine(myPen, p1, p2);

                    }
                    gg_3.DrawImage(myBitmap_3, 0, 0);

                }
            }

        }//(end) void drawLine_Group_3


        //清除屏幕痕迹
        void clearPaint_1()
        {
            g_bitmap_1.Clear(Color.White);
            gg_1.DrawImage(myBitmap_1, 0, 0);
        }

        void clearPaint_3()
        {
            g_bitmap_3.Clear(Color.White);
            gg_3.DrawImage(myBitmap_3, 0, 0);
        }



        //timer线程画坐标图
        void drawLine_group_timer_1(double[] xx,double[] yy,double x_min,double x_max)
        {
            List<double> x_temp = new List<double>();
            List<double> y_temp = new List<double>();

            for (int i = 0; i < xx.Length; i++)
            {
                if (xx[i] >= x_min && xx[i] <= x_max)
                {
                    x_temp.Add(xx[i]);
                    y_temp.Add(yy[i]);
                }
            }
            x = x_temp.ToArray();
            y = y_temp.ToArray();

            int n = x.Length;

            //将x,y数组合并成PointF类型数组
            PointF[] pGroup = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                pGroup[i].X = (float)x[i];
                pGroup[i].Y = (float)y[i];
            }

            //找出数组中x,y的最大最小值用于确定坐标测度
            float x_l = pGroup[0].X, x_h = pGroup[0].X;//所给数据中x的最小值，最大值
            float y_l = pGroup[0].Y, y_h = pGroup[0].Y;//所给数据中y的最大值，最小值

            for (int i = 0; i < n; i++)
            {
                if (pGroup[i].Y < y_l)
                {
                    y_l = pGroup[i].Y;
                }
                if (pGroup[i].Y > y_h)
                {
                    y_h = pGroup[i].Y;
                }
                if (pGroup[i].X < x_l)
                {
                    x_l = pGroup[i].X;
                }
                if (pGroup[i].X > x_h)
                {
                    x_h = pGroup[i].X;
                }
            }

            //当前坐标轴x和y的取值范围
            x_min_1 = x_min;
            x_max_1 = x_max;
            y_min_1 = y_l - y_l%10 - 10;//a-a%10-10
            y_max_1 = y_h - y_h % 10 + 10;//a-a%10+10

            x_min_timer = x_min_1;
            x_max_timer = x_max_1;
            y_min_timer = y_min_1;
            y_max_timer = y_max_1;

            //根据x,y轴取值范围画坐标轴与刻度
            drawXY_1();
            drawXLineValue_1(x_min_1, x_max_1, 10);
            drawYLineValue_1((float)y_min_1, (float)y_max_1, 10);

            //画图
            if (n<=0)
            {

            }
            else if (n == 1)
            {
                drawDot_1(convert_1(pGroup[0].X, pGroup[0].Y));
            }
            else
            {
                //先把图画在内存中的bitmap再画在插件上
                Pen myPen = new Pen(Color.Green, 1);
                for (int i = 0; i < n - 1; i++)
                {
                    //drawLine_twoPoint(convert(pGroup[i].X, pGroup[i].Y), convert(pGroup[i + 1].X, pGroup[i + 1].Y));
                    PointF p1 = convert_1(pGroup[i].X, pGroup[i].Y);
                    PointF p2 = convert_1(pGroup[i + 1].X, pGroup[i + 1].Y);
                    g_bitmap_1.FillEllipse(Brushes.Green, p1.X, p1.Y, 2f, 2f);
                    g_bitmap_1.FillEllipse(Brushes.Green, p2.X, p2.Y, 2f, 2f);
                    g_bitmap_1.DrawLine(myPen, p1, p2);

                }
                gg_1.DrawImage(myBitmap_1, 0, 0);

            }

        }

        /// 输入参考波长和寻峰范围，输出该范围内最大的峰（光强）和对应的波长
        /// 返回值re[0]代表波长，返回值re[1]代表光强
        double[] findWaveSpec_Range(double wave_example, double range)
        {
            double a = 0, b = 0, c = 0;

            double[] re = new double[2];
            double begin = wave_example - range / 2;
            double end = wave_example + range / 2;
            re[0] = -1; re[1] = -1;

            //MessageBox.Show(spec_data.read_wave_all.Length+"");
            for (int i = 0; i < spec_data.read_wave_all.Length; i++)
            {
                a++;
                if ((begin <= spec_data.read_wave_all[i]) && (spec_data.read_wave_all[i] <= end))
                {
                    b++;
                    if (spec_data.read_spec_all_now[i] > re[1])
                    {
                        re[0] = spec_data.read_wave_all[i];
                        re[1] = spec_data.read_spec_all_now[i];
                    }
                }
                else
                {
                    c++;
                    continue;
                }
            }
            //MessageBox.Show(a + "  " + b + "   " + c);
            return re;

        }

        //输入标准波长，输出对应的标准光强
        private double getSpec_Example(string element, double wave_example)
        {
            double t = 0;
            for (int i = 0; i < nist.nameNIST.Length; i++)
            {
                if (nist.nameNIST[i] == element && Math.Abs(nist.waveNIST[i] - wave_example) < 0.05)
                {
                    t = nist.countNIST[i];
                }
            }
            return t;
        }

    }//(end) public partial class LIBS : Form
}//(end)namespace LIBS
