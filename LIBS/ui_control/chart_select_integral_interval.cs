using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace LIBS
{
    class chart_select_integral_interval
    {
        //用于平滑显示的曲线x,y ;插值后的(x,y)用户通过x快速找到插值曲线的y
        public static point[] spline_point;
        //根据最小坐标、最大坐标、封波长、默认积分区间开始、积分区间结束绘图
        //标样才会显示积分区间调整线
        public static void draw_chart(Chart chart1, double x_show_unit, double y_show_unit, double x_minimal, double x_maximal, double y_minimal, double y_maximal, double peak_wave, double interval_start, double interval_end, double[] wave_all, double[] spec_all, double[] env_all, bool is_standard = true)
        {
            while (chart1.Series.Count > 0)
            {
                chart1.Series.RemoveAt(0);
            }

            //设置默认显示范围
            chart1.ChartAreas[0].AxisX.Minimum = x_minimal;
            chart1.ChartAreas[0].AxisX.Maximum = x_maximal;
            chart1.ChartAreas[0].AxisX.Interval = x_show_unit;

            chart1.ChartAreas[0].AxisY.Interval = y_show_unit;
            chart1.ChartAreas[0].AxisY.Minimum = y_minimal;
            chart1.ChartAreas[0].AxisY.Maximum = y_maximal;

            chart1.ChartAreas[0].AxisX.Title = "波长(nm)";
            chart1.ChartAreas[0].AxisY.Title = "强度";

            //波峰线--serie0
            Series ser_peak = new Series("peak");
            DataTable dt = new DataTable();
            DataRow dr;
            dt.Columns.Add("x1", typeof(double));
            dt.Columns.Add("y1", typeof(double));
            dr = dt.NewRow();
            dr[0] = peak_wave;
            dr[1] = 0;
            dt.Rows.Add(dr);
            dr = dt.NewRow();
            dr[0] = peak_wave;
            dr[1] = y_maximal;
            dt.Rows.Add(dr);
            ser_peak.Points.DataBind(dt.AsEnumerable(), "x1", "y1", "");
            ser_peak.XValueType = ChartValueType.Auto;
            ser_peak.YValueType = ChartValueType.Double;
            ser_peak.ChartType = SeriesChartType.Line;
            ser_peak.IsVisibleInLegend = false;
            ser_peak.Color = Color.Red;
            ser_peak.BorderWidth = 1;
            chart1.Series.Add(ser_peak);

            //mouse_cursor--serie1
            Series ser_mouse_cursor = new Series("mouse_cursor");
            DataTable dt_mc = new DataTable();
            dt_mc.Columns.Add("x_mc", typeof(double));
            dt_mc.Columns.Add("y_mc", typeof(double));
            dr = dt_mc.NewRow();
            dr[0] = x_minimal;
            dr[1] = y_minimal;
            dt_mc.Rows.Add(dr);
            dr = dt_mc.NewRow();
            dr[0] = x_minimal;
            dr[1] = y_maximal;
            dt_mc.Rows.Add(dr);
            ser_mouse_cursor.Points.DataBind(dt_mc.AsEnumerable(), "x_mc", "y_mc", "");
            ser_mouse_cursor.XValueType = ChartValueType.Auto;
            ser_mouse_cursor.YValueType = ChartValueType.Double;
            ser_mouse_cursor.ChartType = SeriesChartType.Line;
            ser_mouse_cursor.IsVisibleInLegend = false;
            ser_mouse_cursor.Color = Color.Red;
            ser_mouse_cursor.BorderWidth = 1;
            ser_mouse_cursor.BorderDashStyle = ChartDashStyle.DashDot;
            ser_mouse_cursor.Enabled = false;
            chart1.Series.Add(ser_mouse_cursor);

            //interval_left--serie2
            Series ser_interval_left = new Series("interval_left");
            DataTable dt_il = new DataTable();
            dt_il.Columns.Add("x", typeof(double));
            dt_il.Columns.Add("y", typeof(double));
            dr = dt_il.NewRow();
            dr[0] = interval_start;
            dr[1] = y_maximal * 0.8;
            dt_il.Rows.Add(dr);
            dr = dt_il.NewRow();
            dr[0] = interval_start;
            dr[1] = y_maximal * 0.7;
            dt_il.Rows.Add(dr);
            ser_interval_left.Points.DataBind(dt_il.AsEnumerable(), "x", "y", "");
            ser_interval_left.XValueType = ChartValueType.Auto;
            ser_interval_left.YValueType = ChartValueType.Double;
            ser_interval_left.ChartType = SeriesChartType.Line;
            ser_interval_left.IsVisibleInLegend = false;
            ser_interval_left.Color = Color.DeepSkyBlue;
            ser_interval_left.BorderWidth = 1;
            chart1.Series.Add(ser_interval_left);
            if (is_standard) ser_interval_left.Enabled = true;
            else ser_interval_left.Enabled = false;

            //interval_mid--serie3
            Series ser_interval_mid = new Series("interval_mid");
            DataTable dt_im = new DataTable();
            dt_im.Columns.Add("x", typeof(double));
            dt_im.Columns.Add("y", typeof(double));
            dr = dt_im.NewRow();
            dr[0] = interval_start;
            dr[1] = y_maximal * 0.75;
            dt_im.Rows.Add(dr);
            dr = dt_im.NewRow();
            dr[0] = interval_end;
            dr[1] = y_maximal * 0.75;
            dt_im.Rows.Add(dr);
            ser_interval_mid.Points.DataBind(dt_im.AsEnumerable(), "x", "y", "");
            ser_interval_mid.XValueType = ChartValueType.Auto;
            ser_interval_mid.YValueType = ChartValueType.Double;
            ser_interval_mid.ChartType = SeriesChartType.Line;
            ser_interval_mid.IsVisibleInLegend = false;
            ser_interval_mid.Color = Color.DeepSkyBlue;
            ser_interval_mid.BorderWidth = 1;
            chart1.Series.Add(ser_interval_mid);
            if (is_standard) ser_interval_mid.Enabled = true;
            else ser_interval_mid.Enabled = false;

            //interval_right--serie4
            Series ser_interval_right = new Series("interval_right");
            DataTable dt_ir = new DataTable();
            dt_ir.Columns.Add("x", typeof(double));
            dt_ir.Columns.Add("y", typeof(double));
            dr = dt_ir.NewRow();
            dr[0] = interval_end;
            dr[1] = y_maximal * 0.8;
            dt_ir.Rows.Add(dr);
            dr = dt_ir.NewRow();
            dr[0] = interval_end;
            dr[1] = y_maximal * 0.7;
            dt_ir.Rows.Add(dr);
            ser_interval_right.Points.DataBind(dt_ir.AsEnumerable(), "x", "y", "");
            ser_interval_right.XValueType = ChartValueType.Auto;
            ser_interval_right.YValueType = ChartValueType.Double;
            ser_interval_right.ChartType = SeriesChartType.Line;
            ser_interval_right.IsVisibleInLegend = false;
            ser_interval_right.Color = Color.DeepSkyBlue;
            ser_interval_right.BorderWidth = 1;
            chart1.Series.Add(ser_interval_right);
            if (is_standard) ser_interval_right.Enabled = true;
            else ser_interval_right.Enabled = false;

            //interval_circle--serie5
            Series ser_interval_circle = new Series("interval_circle");
            DataTable dt_circle = new DataTable();
            dt_circle.Columns.Add("x", typeof(double));
            dt_circle.Columns.Add("y", typeof(double));
            dr = dt_circle.NewRow();
            dr[0] = interval_start;
            dr[1] = 0;
            dt_circle.Rows.Add(dr);
            ser_interval_circle.Points.DataBind(dt_circle.AsEnumerable(), "x", "y", "");
            ser_interval_circle.XValueType = ChartValueType.Auto;
            ser_interval_circle.YValueType = ChartValueType.Double;
            ser_interval_circle.ChartType = SeriesChartType.Point;
            ser_interval_circle.IsVisibleInLegend = false;
            ser_interval_circle.Color = Color.Red;
            ser_interval_circle.BorderWidth = 3;
            ser_interval_circle.Enabled = false;

            chart1.Series.Add(ser_interval_circle);


            //样本--serie6
            DataTable dt3 = new DataTable();
            dt3.Columns.Add("x", typeof(double));
            dt3.Columns.Add("y", typeof(double));

            //样条曲线绘制范围取默认范围两倍
            double range_default = chart1.ChartAreas[0].AxisX.Maximum - chart1.ChartAreas[0].AxisX.Minimum;
            double wave_draw_start_x = chart1.ChartAreas[0].AxisX.Minimum - range_default / 2;
            double wave__draw_end_x = chart1.ChartAreas[0].AxisX.Maximum + range_default / 2;

            int wave_array_index_range_startx = data_util.get_index_by_wave(wave_all, wave_draw_start_x);
            int wave_array_index_range_endx = data_util.get_index_by_wave(wave_all, wave__draw_end_x);
            if (wave_array_index_range_startx > 0) wave_array_index_range_startx--;
            point[] spline_src_point = new point[wave_array_index_range_endx - wave_array_index_range_startx + 1];

            int index_temp = 0;
            for (int i = wave_array_index_range_startx; i <= wave_array_index_range_endx; i++)
            {
                spline_src_point[index_temp] = new point();
                spline_src_point[index_temp].x = wave_all[i];
                spline_src_point[index_temp++].y = spec_all[i];
            }
            //均匀插值1000个点
            spline_point = spline.splineInsertPoint(spline_src_point, wave_draw_start_x, wave__draw_end_x, 1000);

            for (int i = 0; i < 1000; i++)
            {
                dr = dt3.NewRow();
                dr[0] = spline_point[i].x;
                dr[1] = spline_point[i].y;
                dt3.Rows.Add(dr);
            }
            Series ser = new Series("dt3");
            ser.Points.DataBind(dt3.AsEnumerable(), "x", "y", "");
            ser.XValueType = ChartValueType.Auto;
            ser.YValueType = ChartValueType.Double;
            ser.ChartType = SeriesChartType.Line;

            ser.IsVisibleInLegend = false;
            ser.Color = Color.Blue;
            chart1.Series.Add(ser);

            //空白--serie7
            DataTable dt_env = new DataTable();
            dt_env.Columns.Add("x_env", typeof(double));
            dt_env.Columns.Add("y_env", typeof(double));
            for (int i = 0; i < env_all.Length; i++)
            {
                dr = dt_env.NewRow();
                dr[0] = Math.Round(wave_all[i], 3);
                dr[1] = Math.Round(env_all[i], 3);
                dt_env.Rows.Add(dr);
            }
            Series ser_env = new Series("dt");
            ser_env.Points.DataBind(dt_env.AsEnumerable(), "x_env", "y_env", "");
            ser_env.XValueType = ChartValueType.Auto;
            ser_env.YValueType = ChartValueType.Double;
            ser_env.ChartType = SeriesChartType.Spline;
            ser_env.IsVisibleInLegend = false;
            ser_env.Color = Color.Gray;
            ser_env.BorderDashStyle = ChartDashStyle.Dash;
            chart1.Series.Add(ser_env);
        }
    }

    enum e_series:int
    {
        PEAK=0,
        MOUSE_CURSOR,
        INTERVAL_LEFT,
        INTERVAL_MID,
        INTERVAL_RIGHT,
        INTERVAL_CIRCLE,
        SAMPLE,
        EVN
    }
}
