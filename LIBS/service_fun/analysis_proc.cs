using LIBS.device_driver;
using LIBS.storage;
using LIBS.ui_control;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LIBS.service_fun
{
    class analysis_proc
    {
        public static void process_cell_click(Chart chart1,Chart chart2,Label label2,Label label_info, DataGridView dataGridView9, int click_row, int click_column, spec_metadata spec_obj)
        {
            select_element[] elements = spec_obj.elements;
            standard[] standards = spec_obj.standards;
            sample[] samples = spec_obj.samples;
            double[] wave_all = spec_obj.read_wave_all;
            double[] env = spec_obj.env_spec;
            double[,,] spec_standard = spec_obj.read_standard_spec;
            double[,,] spec_sample = spec_obj.read_sample_spec;
            int standard_cnt = spec_obj.standard_cnt;
            int pixel_cnt = spec_obj.read_wave_all.Length;

            //判断该标样/样本是否读取
            //绘制波峰图
            //完成全部标样读取
            //计算方程,绘制拟合方程图
            //推算浓度，计算标准差  
            if (click_row< standard_cnt && standards[click_row].is_readed || click_row>= standard_cnt && samples[click_row - standard_cnt].is_read)
            {
                //绘制波峰图,设置显示配置
                //chart_select_integral_interval.draw_chart(chart1,)
                double[] spec_selected = new double[pixel_cnt];
                double peak_wave = elements[click_column - 3].peak_wave;
                double x_minimal=0, x_maximal=0, y_minimal=0, y_maximal=0, x_show_unit=0;
                int y_show_unit=0;
                
                prepare_peak_chart_data(spec_obj, click_row, click_column, ref spec_selected, ref x_minimal, ref x_maximal, ref x_show_unit, ref  y_minimal, ref y_maximal,ref y_show_unit);

                double interval_start_wave = elements[click_column - 3].interval_start;
                double interval_end_wave = elements[click_column - 3].interval_end;
                bool is_standard = false;
                if (click_row < standard_cnt) is_standard = true;
                chart_select_integral_interval.draw_chart(chart1, x_show_unit, y_show_unit, x_minimal, x_maximal, y_minimal, y_maximal, peak_wave, interval_start_wave, interval_end_wave, wave_all, spec_selected, env, is_standard);

            }
            else
            {
                MessageBox.Show("需要先完成读取");
                return;
            }
            for(int i = 0; i < standard_cnt; i++)
            {
                if (!standards[i].is_readed)
                {
                    MessageBox.Show("需要所有标样完成读取才能计算方程");
                    return;
                }
            }

            //记录样本的多次读取数据
            double[] this_read_integration_strenths = null;
            int this_read_average_times = 0;
            //用于计算方程的浓度和强度
            double []element_concentrations = null;
            double []element_average_strenths = null;
            LinearFit.LfReValue equation = get_equation(spec_obj, click_column-3, ref element_concentrations, ref element_average_strenths);

            this_read_integration_strenths = get_oneshot_all_strength(spec_obj, click_row, click_column - 3);
            this_read_average_times = this_read_integration_strenths.Length;
            double[] this_read_integration_concentrations = new double[this_read_average_times];
            double this_read_strenth_average=0, this_read_concentration_average = 0;
            for (int i = 0; i < this_read_average_times; i++)
            {
                this_read_integration_concentrations[i] = (this_read_integration_strenths[i] - equation.getA()) / equation.getB();
            }
            for(int i = 0; i < this_read_average_times; i++)
            {
                this_read_strenth_average += this_read_integration_strenths[i];
            }
            this_read_strenth_average /= this_read_average_times;
            this_read_concentration_average = (this_read_strenth_average - equation.getA()) / equation.getB();

            equation_chart.draw_equation(chart2 ,label2, element_concentrations, element_average_strenths, equation.getA(), equation.getB(), equation.getR());
            if (click_row < standard_cnt)
                equation_chart.add_point_now(chart2, element_concentrations[click_row], element_average_strenths[click_row], Color.Red, MarkerStyle.Circle);
            else
                equation_chart.add_point_now(chart2, this_read_concentration_average, this_read_strenth_average, Color.Green, MarkerStyle.Triangle);
            datagrid_control.draw_datagrid_snapshot(dataGridView9, this_read_integration_concentrations, this_read_integration_strenths);
            summary_info.draw_summary_info(label_info, this_read_concentration_average, this_read_strenth_average);
        }

        public static void read_spec_click(NumericUpDown textbox_average_times, spec_wrapper wrapper, DataGridView dgv, int row_index, spec_metadata spec_obj)
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

            double[] spec_now = null;
            int this_read_average_times = int.Parse(textbox_average_times.Text);
            if (this_read_average_times < 1 || this_read_average_times > 5) this_read_average_times = 1;
            //读标样
            if (row_index < standard_cnt)
            {
                standards[row_index].average_times = this_read_average_times;
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
                for (int j = 0; j < element_cnt; j++)
                {
                    //寻峰，确定实际读取波长，默认积分区间
                    int seek_start_index = data_util.get_index_by_wave(wave_all, elements[j].select_wave - elements[j].seek_peak_range / 2);
                    int seek_end_index = data_util.get_index_by_wave(wave_all, elements[j].select_wave + elements[j].seek_peak_range / 2);
                    int peak_index = data_util.find_peak(spec_now, seek_start_index, seek_end_index);
                    elements[j].peak_wave = wave_all[peak_index];
                    elements[j].interval_start = elements[j].peak_wave - 0.05; //设置默认积分范围为峰左右0.25nm
                    elements[j].interval_end = elements[j].peak_wave + 0.05;
                }
                standards[row_index].is_readed = true;
            }

            //读样本
            if (row_index >= standard_cnt)
            {
                int sample_index = row_index - standard_cnt;
                samples[sample_index].average_times = this_read_average_times;
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
            double[,] standards_val = new double[standard_cnt, element_cnt];
            double[,] samples_val = new double[sample_cnt, element_cnt];

            //预计算表格数据，统一调用datagrid_control来控制绘制，调用直接根据规则预先进行计算
            datagrid_control.draw_datagrid_analysis(dgv, spec_obj);


        }

        //获取某元素波长的方程，前置约束为调用者已经完成所有所有标样的读取
        //函数返回方程的同时，将返回方程进行计算时用到的标样点
        public static LinearFit.LfReValue get_equation(spec_metadata spec_obj, int element_index, ref double[] concentration, ref double[] standards_integration_average_strenth)
        {
            double[] wave_all = spec_obj.read_wave_all;
            double[] env = spec_obj.env_spec;
            standard[] standards = spec_obj.standards;
            int standard_cnt = spec_obj.standard_cnt;
            select_element element = spec_obj.elements[element_index];
            double[,,] spec_standard = spec_obj.read_standard_spec;

            //提取该元素列标样数据，得到方程
            standards_integration_average_strenth = new double[standard_cnt];
            //标样平均的积分平均强度
            for (int i = 0; i < standard_cnt; i++) //每个标样
            {
                double standard_element_interval_average = 0.0; //平均的平均
                //如果点击的为标样，在计算方程的循环中直接获取多次积分平均值
                double[] strenght_oneshot = get_oneshot_all_strength(spec_obj, i, element_index);
                for (int t = 0; t < standards[i].average_times; t++)
                {
                    standard_element_interval_average += strenght_oneshot[t];
                }
                standards_integration_average_strenth[i] = standard_element_interval_average / standards[i].average_times;
            }
            concentration = new double[standard_cnt]; //浓度
            for (int j = 0; j < standard_cnt; j++)
            {
                concentration[j] = standards[j].standard_ppm[element.sequece_index];
            }
            LinearFit.LfReValue equation = LinearFit.linearFitFunc(concentration, standards_integration_average_strenth, standard_cnt);

            return equation;
        }

        //获取该cell对应的多次平均强度，index为标样或样本序号
        public static double[] get_oneshot_all_strength(spec_metadata spec_obj, int rowindex, int element_index)
        {
            int this_read_average_times = 1;
            int standard_cnt = spec_obj.standard_cnt;
            double[,,] src_spec = null;
            int index = 0;
            double[] wave_all = spec_obj.read_wave_all;
            double[] env = spec_obj.env_spec;
            standard[] standards = spec_obj.standards;
            sample[] samples = spec_obj.samples;
            double interval_start = spec_obj.elements[element_index].interval_start;
            double interval_end = spec_obj.elements[element_index].interval_end;
            int pixel_cnt = spec_obj.read_wave_all.Length;

            if (rowindex < standard_cnt) //标样
            {
                this_read_average_times = standards[rowindex].average_times;
                src_spec = spec_obj.read_standard_spec;
                index = rowindex;
            }
            else
            {
                this_read_average_times = samples[rowindex - standard_cnt].average_times;
                src_spec = spec_obj.read_sample_spec;
                index = rowindex - standard_cnt;
             }
            double[] strenght_oneshot = new double[this_read_average_times];
            double[] spec_temp_all = new double[pixel_cnt]; 
            for (int t = 0; t < this_read_average_times; t++)
            {
                for (int k = 0; k < pixel_cnt; k++)
                {
                    spec_temp_all[k] = src_spec[index, t, k];
                }
                strenght_oneshot[t] = data_util.get_integration_average(wave_all, spec_temp_all, env, interval_start, interval_end);
            }
            return strenght_oneshot;
        }

        //准备分析窗体峰视图显示的x_minimal,x_maximal,x_unit,y_minimal,y_maiximal,y_unit以及数据spec_selected
        private static void prepare_peak_chart_data(spec_metadata spec_obj, int click_row, int click_column, ref double[] spec_selected, ref double x_minimal, ref double x_maximal, ref double x_show_unit, ref double y_minimal, ref double y_maximal,ref int y_show_unit )
        {
            standard[] standards = spec_obj.standards;
            int standard_cnt = spec_obj.standard_cnt;
            double[,,] spec_standard = spec_obj.read_standard_spec;
            double[,,] spec_sample = spec_obj.read_sample_spec;
            double[] wave_all = spec_obj.read_wave_all;
            double[] env = spec_obj.env_spec;
            select_element element = spec_obj.elements[click_column - 3];
            int pixel_cnt = spec_obj.read_wave_all.Length;

            spec_selected = new double[pixel_cnt];
            if (click_row < standard_cnt)
            {
                for (int i = 0; i < pixel_cnt; i++)
                {
                    spec_selected[i] = spec_standard[click_row, 0, i]; //展示波峰图时，简单的展示第0次平均的值
                }
            }
            else
            {
                for (int i = 0; i < pixel_cnt; i++)
                {
                    spec_selected[i] = spec_sample[click_row - standard_cnt, 0, i]; //展示波峰图时，简单的展示第0次平均的值
                }
            }
            //设置显示配置
            double peak_wave = element.peak_wave;
            x_minimal = peak_wave - 0.25;
            x_maximal = peak_wave + 0.25;

            int show_x_start = data_util.get_index_by_wave(wave_all, x_minimal);
            int show_x_end = data_util.get_index_by_wave(wave_all, x_maximal);
            y_minimal = spec_selected[show_x_start];
            y_maximal = spec_selected[show_x_start];
            for (int i = show_x_start; i < show_x_end; i++)
            {
                if (spec_selected[i] < y_minimal)
                {
                    y_minimal = spec_selected[i];
                }
                if (spec_selected[i] > y_maximal)
                {
                    y_maximal = spec_selected[i];
                }
                if (env[i] < y_minimal)
                {
                    y_minimal = env[i];
                }
                if (env[i] > y_maximal)
                {
                    y_maximal = env[i];
                }
            }
            y_maximal += (y_maximal-y_minimal)*0.1; // maximal + 10% * range

            x_show_unit = 0.05;
            y_show_unit = data_util.normalize_y(y_minimal, y_maximal);
            //y的显示不随放大缩小改变, x将利用 data_util 来规范显示数据,初始时,x_show_unit固定为0.05
            y_minimal = (int)((y_minimal - 0.001) / y_show_unit - 1) * y_show_unit;
            y_maximal = (int)((y_maximal - 0.001) / y_show_unit + 1) * y_show_unit;
            data_util.normalize_data_for_show(x_show_unit, 10, ref x_show_unit, ref x_minimal, ref x_maximal); //
        }
    }
}
