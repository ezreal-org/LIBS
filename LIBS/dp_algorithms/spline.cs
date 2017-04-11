using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace LIBS
{
    class spline
    {
        public static point[] splineInsertPoint(point[] points, double start_x, double end_x, int insert_point_cnt)
        {
            double[] xs = new double[insert_point_cnt];
            point[] insertRes = new point[insert_point_cnt];
            for (int i=0;i<insert_point_cnt;i++)
            {
                insertRes[i] = new point();
                xs[i] = start_x + (end_x - start_x) * i / insert_point_cnt;
                insertRes[i].x = xs[i];
            }
           
            
            int plength = points.Length;
            double[] h = new double[plength];
            double[] f = new double[plength];
            double[] l = new double[plength];
            double[] v = new double[plength];
            double[] g = new double[plength];

            for (int i = 0; i < plength - 1; i++)
            {
                h[i] = points[i + 1].x - points[i].x;
                f[i] = (points[i + 1].y - points[i].y) / h[i];
            }

            for (int i = 1; i < plength - 1; i++)
            {
                l[i] = h[i] / (h[i - 1] + h[i]);
                v[i] = h[i - 1] / (h[i - 1] + h[i]);
                g[i] = 3 * (l[i] * f[i - 1] + v[i] * f[i]);
            }

            double[] b = new double[plength];
            double[] tem = new double[plength];
            double[] m = new double[plength];
            double f0 = (points[0].y - points[1].y) / (points[0].x - points[1].x);
            double fn = (points[plength - 1].y - points[plength - 2].y) / (points[plength - 1].x - points[plength - 2].x);

            b[1] = v[1] / 2;
            for (int i = 2; i < plength - 2; i++)
            {
                // Console.Write(" " + i);
                b[i] = v[i] / (2 - b[i - 1] * l[i]);
            }
            tem[1] = g[1] / 2;
            for (int i = 2; i < plength - 1; i++)
            {
                //Console.Write(" " + i);
                tem[i] = (g[i] - l[i] * tem[i - 1]) / (2 - l[i] * b[i - 1]);
            }
            m[plength - 2] = tem[plength - 2];
            for (int i = plength - 3; i > 0; i--)
            {
                //Console.Write(" " + i);
                m[i] = tem[i] - b[i] * m[i + 1];
            }
            m[0] = 3 * f[0] / 2.0;
            m[plength - 1] = fn;
            int xlength = xs.Length;

            for (int i = 0; i < xlength; i++)
            {
                int j = 0;
                for (j = 0; j < plength; j++)
                {
                    if (xs[i] < points[j].x)
                        break;
                }
                j = j - 1;
                // Console.WriteLine(j);
                try
                {
                    if (j == -1 || j == points.Length - 1)
                    {
                        if (j == -1)
                            throw new Exception("插值下边界超出");
                        if (j == points.Length - 1 && xs[i] == points[j].x)
                            insertRes[i].y = points[j].y;
                        else
                            throw new Exception("插值下边界超出");
                    }
                    else
                    {
                        double p1;
                        p1 = (xs[i] - points[j + 1].x) / (points[j].x - points[j + 1].x);
                        p1 = p1 * p1;
                        double p2; p2 = (xs[i] - points[j].x) / (points[j + 1].x - points[j].x);
                        p2 = p2 * p2;
                        double p3; p3 = p1 * (1 + 2 * (xs[i] - points[j].x) / (points[j + 1].x - points[j].x)) * points[j].y + p2 * (1 + 2 * (xs[i] - points[j + 1].x) / (points[j].x - points[j + 1].x)) * points[j + 1].y;

                        double p4; p4 = p1 * (xs[i] - points[j].x) * m[j] + p2 * (xs[i] - points[j + 1].x) * m[j + 1];
                        //         Console.WriteLine(m[j] + " " + m[j + 1] + " " + j);
                        p4 = p4 + p3;
                        insertRes[i].y = p4;
                        Console.WriteLine("f(" + xs[i] + ")= " + p4);
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "插值下边界超出") continue;
                }
            }


            Console.ReadLine();
            return insertRes;
        }
    }

    class point
    {
        public double x = 0;
        public double y = 0;
        public point()
        {
            x = 0; y = 0;
        }
        //-------写一个排序函数，使得输入的点按顺序排列，是因为插值算法的要求是，x轴递增有序的---------
        public static point[] DeSortX(point[] points)
        {
            int length = points.Length;
            double temx, temy;
            for (int i = 0; i < length - 1; i++)
            {
                for (int j = 0; j < length - i - 1; j++)
                    if (points[j].x > points[j + 1].x)
                    {

                        temx = points[j + 1].x;
                        points[j + 1].x = points[j].x;
                        points[j].x = temx;
                        temy = points[j + 1].y;
                        points[j + 1].y = points[j].y;
                        points[j].y = temy;
                    }
            }
            return points;
        }
    }
}
