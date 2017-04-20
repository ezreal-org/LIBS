using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LIBS.ui_control
{
    class equation_chart
    {
        public static void draw_equation(Chart chart2, Label label2, double []concention, double []strenth, double a, double b, double r)
        {
            double minx = concention[0], maxx = concention[0];
            for(int i = 0; i < concention.Length; i++)
            {
                if (concention[i] < minx) minx = concention[i];
                if (concention[i] > maxx) maxx = concention[i];
            }

            double show_unit = 0.1;
            double actual_step = 0;
            data_util.normalize_data_for_show(show_unit, 8, ref actual_step, ref minx, ref maxx);

            chart2.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart2.ChartAreas[0].AxisX.Minimum = minx;
            chart2.ChartAreas[0].AxisX.Maximum = maxx;
            chart2.ChartAreas[0].AxisX.Interval = actual_step;
            chart2.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = false;

            DataTable dt2 = new DataTable();
            dt2.Columns.Add("x", typeof(double));
            dt2.Columns.Add("y", typeof(double));

            DataRow dr2;
            for(int i = 0; i < concention.Length; i++)
            {
                dr2 = dt2.NewRow();
                dr2[0] = Math.Round(concention[i], 2);
                dr2[1] = Math.Round(strenth[i],2);
                dt2.Rows.Add(dr2);
            }
            Series ser2 = new Series("dt");
            ser2.Points.DataBind(dt2.AsEnumerable(), "x", "y", "");
            ser2.XValueType = ChartValueType.Auto;
            ser2.YValueType = ChartValueType.Double;
            ser2.ChartType = SeriesChartType.Spline;
            ser2.IsVisibleInLegend = false;
           
            Series ser = new Series("src_point");
            ser.Points.DataBind(dt2.AsEnumerable(), "x", "y", "");
            ser.XValueType = ChartValueType.Auto;
            ser.YValueType = ChartValueType.Double;
            ser.ChartType = SeriesChartType.Point;
            ser.MarkerStyle = MarkerStyle.Circle;
            ser.Color = Color.Gray;
            ser.IsVisibleInLegend = false;

            while (chart2.Series.Count > 0)
            {
                chart2.Series.RemoveAt(0);
            }
            chart2.Series.Add(ser2);
            chart2.Series.Add(ser);
            chart2.ChartAreas[0].RecalculateAxesScale();
            label2.Text = "回归方程" + "\r" + "强度 = " + Math.Round(b, 3) + " * 浓度 + " + Math.Round(a, 3) + "\t" + "    相关系数(r): " + Math.Round(r, 4);
        }

        public static void add_point_now(Chart chart2, double x, double y, Color cc, MarkerStyle ms)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("1", typeof(double));
            dt.Columns.Add("2", typeof(double));
            DataRow dr = dt.NewRow();
            dr[0] = x;
            dr[1] = y;
            dt.Rows.Add(dr);

            Series ser2 = new Series("src_point_now");
            ser2.Points.DataBind(dt.AsEnumerable(), "1", "2", "");
            ser2.XValueType = ChartValueType.Auto;
            ser2.YValueType = ChartValueType.Double;
            ser2.ChartType = SeriesChartType.Point;
            ser2.MarkerSize = 8;
            ser2.MarkerStyle = ms;
            ser2.Color = cc;
            ser2.IsVisibleInLegend = false;
            chart2.Series.Add(ser2);
        }
    }
}
