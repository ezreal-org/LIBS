using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LIBS
{
    class LinearFit
    {
        //LfReValue : LinearFit return value
        //拟合函数返回值定义类型，包含a,b,r
        public struct LfReValue
        {
            private double a, b, r;

            public double getA()
            {
                return a;
            }
            public double getB()
            {
                return b;
            }
            public double getR()
            {
                return r;
            }
            public void setA(double i)
            {
                a = i;
            }
            public void setB(double i)
            {
                b = i;
            }
            public void setR(double i)
            {
                r = i;
            }

        }

        //线性拟合（不一定过原点）
        //公式：http://i1.piimg.com/588926/c241d90e3eb2cfdb.png
        public static LfReValue linearFitFunc(double[] a, double[] b, int n)
        {
            LfReValue temp = new LfReValue();
            double a_sum = 0, b_sum = 0;
            double a_avg, b_avg;
            double p1 = 0, p2 = 0, p3 = 0, p4 = 0;
            double a_temp, b_temp,r_temp;

            for (int i = 0; i < n; i++)
            {
                a_sum += a[i];
                b_sum += b[i];
                p3 += a[i] * a[i];//p3
            }

            a_avg = a_sum / n;
            b_avg = b_sum / n;

            for (int i = 0; i < n; i++)
            {
                p1 += a[i] * b[i];//p1
            }

            p2 = n * a_avg * b_avg;//p2
            p4 = n * a_avg * a_avg;//p4

            b_temp = (p1 - p2) / (p3 - p4);//求出拟合参数b
            a_temp = b_avg - b_temp * a_avg;//求出拟合参数a

            //下面计算相关系数r

            double r_value1 = 0, r_value2 = 0, r_value3 = 0;

            for (int i = 0; i < n; i++)
            {
                r_value1 += (a[i] - a_avg) * (b[i] - b_avg);
                r_value2 += (a[i] - a_avg) * (a[i] - a_avg);
                r_value3 += (b[i] - b_avg) * (b[i] - b_avg);
            }

            r_temp = r_value1 / (Math.Sqrt(r_value2) * Math.Sqrt(r_value3));

            temp.setA(a_temp);
            temp.setB(b_temp);
            temp.setR(r_temp);

            return temp;
        }



        //线性拟合：过原点情况
        //公式：http://i4.buimg.com/588926/822a2c20ebbe24a6.png
        public static LfReValue linearFitFunc_zero(double[] x, double[] y, int n)
        {
            LfReValue temp = new LfReValue();
            
            double shan = 0, xia = 0;

            for (int i=0;i<x.Length;i++)
            {
                shan += x[i] * y[i];
                xia += x[i] * x[i];

            }

            temp.setA(0);
            temp.setB(shan/xia);
            temp.setR(calc_r_xgxs(x,y));
            return temp;
        }

        //线性拟合过空白 
        //公式：http://chuantu.biz/t5/85/1494685716x2890171456.gif
        public static LfReValue linearFitFunc_blank(double[] a, double[] b, int n)
        {
            LfReValue temp = new LfReValue();
            double a_sum = 0, b_sum = 0;
            double a_avg, b_avg;
            double p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0, p6 = 0, p7 = 0;
            double a_temp, b_temp, r_temp;
            //空白坐标
            double x0 = a[0];
            double y0 = b[0];
            for (int i = 1; i < n; i++)
            {
                a_sum += a[i];
                b_sum += b[i];
                p1 += a[i] * b[i];
                p5 += a[i] * a[i];
            }

            a_avg = a_sum / (n - 1);
            b_avg = b_sum / (n - 1);

            p2 = (n - 1) * x0 * y0;
            p3 = (n - 1) * y0 * a_avg;
            p4 = (n - 1) * x0 * b_avg;
            p6 = 2 * (n - 1) * x0 * a_avg;
            p7 = (n - 1) * x0 * x0;

            b_temp = (p1 + p2 - p3 - p4) / (p5 - p6 + p7);
            a_temp = y0 - b_temp * x0;

            r_temp = calc_r_xgxs(a, b);
            temp.setA(a_temp);
            temp.setB(b_temp);
            temp.setR(r_temp);

            return temp;

        }

        //计算"相关系数r"
        //公式：http://i1.piimg.com/1949/15d68cb1fc522dff.png
        public static double calc_r_xgxs(double[] a, double[] b)
        {
            double temp = 0 ;
            double a_avg = 0, b_avg = 0;
            int n = a.Length;
            for (int i=0;i<a.Length;i++)
            {
                a_avg += a[i];
                b_avg += b[i];
            }
            a_avg = a_avg / a.Length;
            b_avg = b_avg / a.Length;

            double r_value1 = 0, r_value2 = 0, r_value3 = 0;

            for (int i = 0; i < n; i++)
            {
                r_value1 += (a[i] - a_avg) * (b[i] - b_avg);
                r_value2 += (a[i] - a_avg) * (a[i] - a_avg);
                r_value3 += (b[i] - b_avg) * (b[i] - b_avg);
            }

            temp = r_value1 / (Math.Sqrt(r_value2) * Math.Sqrt(r_value3));

            return temp;
        }

    }
}
