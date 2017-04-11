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

        /// <summary>
        /// 拟合计算原理：http://i.imgur.com/Qhrmfwt.png
        /// 相关系数r计算原理：http://i.imgur.com/xsODpm3.png
        /// </summary>
        /// <param name="a">数组x地址</param>
        /// <param name="b">数组y地址</param>
        /// <param name="n">数组个数</param>
        /// <returns>返回值为LfReValue自定义结构，主要包含参数a,b和r</returns>
        /// 注意：因为在静态main函数中测试调用，所以定义为了static，实际项目中static可以去除
        public static LfReValue linearFitFunc(double[] a, double[] b, int n)
        {
            LfReValue temp = new LfReValue();

            double a_sum = 0, b_sum = 0;
            double a_avg, b_avg;
            double p1 = 0, p2 = 0, p3 = 0, p4 = 0;
            double a_temp, b_temp, r_temp = 0;


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

        /// <summary>
        /// 计算某数组的方差
        /// 公式：http://i.imgur.com/LGzicJe.png
        /// </summary>
        /// <param name="x">double数组名</param>
        /// <param name="n">数组元素个数</param>
        /// <returns></returns>
        public static double cal_variance(double[] x, int n)
        {
            double x_sum = 0, x_avg, temp1 = 0;

            for (int i = 0; i < n; i++)
            {
                x_sum += x[i];
            }

            x_avg = x_sum / n;

            for (int i = 0; i < n; i++)
            {
                temp1 += (x[i] - x_avg) * (x[i] - x_avg);
            }

            return temp1 / n;
        }

        //double[] a = {35.3,29.7,30.8,58.8,61.4,71.3,74.4,76.6,70.7,57.5,46.4,28.9,
        //        28.1,39.1,46.8,48.5,59.3,70.0,70.0,74.5,72.1,58.1,44.6,33.4,28.6 };
        //double[] b = { 10.98,11.13,12.51,8.40,9.27,8.73,6.36,8.50,7.82,9.14,8.24,
        //        12.19,11.88,9.57,10.94,9.58,10.09,8.11,6.83,8.88,7.68,8.47,8.86,10.38,11.08};
        //double[] c = { 10.0, 11.0, 12.0 };

        ////线性拟合调用示例
        //LfReValue itemp = linearFitFunc(a, b, a.Length);
        //Console.WriteLine(itemp.getA());
        //    Console.WriteLine(itemp.getB());
        //    Console.WriteLine(itemp.getR());
        //    //计算方差调用示例
        //    Console.WriteLine(cal_variance(c, c.Length));
    }
}
