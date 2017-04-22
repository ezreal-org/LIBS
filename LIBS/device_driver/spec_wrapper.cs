using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LIBS.device_driver
{
    //包装底层sdk设备函数，提供统一接口来控制硬件设备
    class spec_wrapper
    {
        private static OmniDriver.CCoWrapper wrapper;
        public int cnt_of_devices;
        public int build_num; //记录第几次读取
        public bool is_test_model; //是否是测试模式
        private double[] wave_all;
        private double[,] wave_array;
        private int[] channel_pixels;
        
        public spec_wrapper()
        {
            cnt_of_devices = 0;
            build_num = 0;
            is_test_model = true;
            wrapper = new OmniDriver.CCoWrapper();
        }

        //连接设备
        public bool connect()
        {
            if (is_test_model)
            {
                cnt_of_devices = 6;
                init_wave_all(); //连接时初始化波长
                return true;
            }
            cnt_of_devices = wrapper.openAllSpectrometers();
            if (cnt_of_devices > 0)
            {
                init_wave_all(); //连接时初始化波长
                return true;
            }
            else return false;
        }

        //断开设备
        public void disconnect()
        {
            wrapper.closeAllSpectrometers();
        }

        //设置/获取积分时间,微秒为单位
        public void set_intergration_time(int device_index, int value) //micro s
        {
            wrapper.setIntegrationTime(device_index, value);
        }
        public int get_intergration_time(int device_index)
        {
            return wrapper.getIntegrationTime(device_index);
        }

        //设置/获取平滑宽度
        public void set_boxcar_width(int device_index, int value)
        {
            wrapper.setBoxcarWidth(device_index, value);
        }
        public int get_boxcar_width(int device_index)
        {
            return wrapper.getBoxcarWidth(device_index);
        }

        //设置/获取平均次数
        public void set_times_for_average(int times)
        {
            for(int i=0; i < cnt_of_devices; i++)
            {
                wrapper.setScansToAverage(i, times);
            }
           
        }
        public int get_times_for_average()
        {
            if (cnt_of_devices > 0)
                return wrapper.getScansToAverage(0);
            return -1;
        }
        //暗电流矫正
        public void set_correct_for_electrical_dart(int flag)
        {
            for(int i = 0; i < cnt_of_devices; i++)
            {
                wrapper.setCorrectForElectricalDark(i, flag);
            }
        }

        //获取通道波长数组
        public double[] get_wave_by_index(int device_index)
        {
            return wrapper.getWavelengths(device_index);
        }

        //读取通道数据
        public double[] get_spectrum_by_index(int device_index)
        {
            return wrapper.getSpectrum(device_index);
        }
        //获取所以通道波长数组
        public double[] get_wave_all()
        {
            return wave_all;
        }
        //初始化波长数组
        private void init_wave_all()
        {
            wave_all = new double[10418];
            if (is_test_model)
            {
                try
                {
                    StreamReader SReader = new StreamReader("TestData\\readAllWave.txt", Encoding.Default);
                    int k = 0;//
                    string rd="";
                    while ((rd = SReader.ReadLine()) != null)
                    {
                        wave_all[k] = Convert.ToDouble(rd);
                        k++;
                    }
                    SReader.Close();
                }catch(Exception e)
                {
                    MessageBox.Show("读取测试波长失败" + e.Message.ToString());
                }
                return;
            }
            //实模式
            wave_array = new double[cnt_of_devices, 2068];
            channel_pixels = new int[cnt_of_devices]; //记录每个通道的像素
            for (int i = 0; i < cnt_of_devices; i++)
            {
                double[] wave_temp = wrapper.getWavelengths(i);
                channel_pixels[i] = wave_temp.Length;
                for (int j = 0; j < wave_temp.Length; j++) //以后这里会根据响应的强度对重叠区域进行取舍
                {
                    wave_array[i,j] = wave_temp[j];
                }

            }
            //根据实际响应排序
            for(int i = 0; i < cnt_of_devices; i++)
            {
                if (wave_array[i, 0] > 185 && wave_array[i, 0] < 186)
                {
                    //System.Console.WriteLine("找到第一通道：i={0}", i.ToString());
                    for (int j = 0; j < (channel_pixels[i] - 122); j++)
                    {
                        wave_all[j] = wave_array[i, (121 + j)];
                    }
                }               
                else if (wave_array[i, 0] > 228 && wave_array[i, 0] < 238)
                {
                    for (int j = 0; j < (channel_pixels[i] - 168); j++)
                    {
                        wave_all[1533+j] = wave_array[i, (167 + j)];
                    }
                }
                else if (wave_array[i, 0] > 320 && wave_array[i, 0] < 321)
                {
                    for (int j = 0; j < (channel_pixels[i] - 87); j++)
                    {
                        wave_all[3253+j] = wave_array[i, (86 + j)];
                    }
                }
                else if (wave_array[i, 0] > 398 && wave_array[i, 0] < 399)
                {
                    try
                    {
                        for (int j = 0; j < (channel_pixels[i] - 100); j++)
                        {
                            wave_all[5002+j] = wave_array[i, (99 + j)];
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("第四通道发生错误： " + ex.Message.ToString()); };
                }
                else if (wave_array[i, 0] > 508 && wave_array[i, 0] < 509)
                {
                    for (int j = 0; j < (channel_pixels[i] - 181); j++)
                    {
                        wave_all[6814+j] = wave_array[i, (180 + j)];
                    }
                }
                else if (wave_array[i, 0] > 607 && wave_array[i, 0] < 608)
                {
                    for (int j = 0; j < 1918; j++)
                    {
                        wave_all[8500+j] = wave_array[i, (72 + j)];
                    }
                }
            }
        }
        //一次获取所有通道数据
        public double[] get_spec_all()
        {
            double []spec_all = new double[10418];
            if (is_test_model)
            {
                Random random = new Random();
                string rd = "";
                try
                {
                    if (build_num == 0)
                    {
                        StreamReader SReader2 = new StreamReader("TestData\\空白.txt", Encoding.Default);
                        int l = 0;
                        while ((rd = SReader2.ReadLine()) != null)
                        {
                            spec_all[l] = Convert.ToDouble(rd) + random.Next(1, 10);
                            l++;
                        }
                        SReader2.Close();
                    }
                    else if (build_num == 1)
                    {
                        StreamReader SReader2 = new StreamReader("TestData\\Al50.txt", Encoding.Default);
                        int l = 0;  //其实k等于l
                        while ((rd = SReader2.ReadLine()) != null)
                        {
                            spec_all[l] = Convert.ToDouble(rd) + random.Next(1, 10);
                            l++;
                        }
                        SReader2.Close();
                    }
                    else if (build_num == 2)
                    {
                        StreamReader SReader2 = new StreamReader("TestData\\Al10.txt", Encoding.Default);
                        int l = 0;  //其实k等于l
                        while ((rd = SReader2.ReadLine()) != null)
                        {
                            spec_all[l] = Convert.ToDouble(rd) + random.Next(1, 10);
                            l++;
                        }
                        SReader2.Close();
                    }
                    else
                    {
                        StreamReader SReader2 = new StreamReader("TestData\\Al50.txt", Encoding.Default);
                        int l = 0;  //其实k等于l
                        while ((rd = SReader2.ReadLine()) != null)
                        {
                            spec_all[l] = Convert.ToDouble(rd) + random.Next(1, 10) * 300;
                            l++;
                        }
                        SReader2.Close();
                    }
                    build_num++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
                return spec_all;
            }
            double[,] spec_array = new double[cnt_of_devices, 2068];
            for (int i = 0; i < cnt_of_devices; i++)
            {
                double[] spec_temp = wrapper.getSpectrum(i);
                channel_pixels[i] = spec_temp.Length;
                for (int j = 0; j < spec_temp.Length; j++) //以后这里会根据响应的强度对重叠区域进行取舍
                {
                    spec_array[i, j] = spec_temp[j];
                }

            }
            //根据实际响应排序
            for (int i = 0; i < cnt_of_devices; i++)
            {
                if (wave_array[i, 0] > 185 && wave_array[i, 0] < 186)
                {
                    //System.Console.WriteLine("找到第一通道：i={0}", i.ToString());
                    for (int j = 0; j < (channel_pixels[i] - 122); j++)
                    {
                        spec_all[j] = spec_array[i, (121 + j)];
                    }
                }
                else if (wave_array[i, 0] > 228 && wave_array[i, 0] < 238)
                {
                    for (int j = 0; j < (channel_pixels[i] - 168); j++)
                    {
                        spec_all[1533 + j] = spec_array[i, (167 + j)];
                    }
                }
                else if (wave_array[i, 0] > 320 && wave_array[i, 0] < 321)
                {
                    for (int j = 0; j < (channel_pixels[i] - 87); j++)
                    {
                        spec_all[3253 + j] = spec_array[i, (86 + j)];
                    }
                }
                else if (wave_array[i, 0] > 398 && wave_array[i, 0] < 399)
                {
                    try
                    {
                        for (int j = 0; j < (channel_pixels[i] - 100); j++)
                        {
                            spec_all[5002 + j] = spec_array[i, (99 + j)];
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("第四通道发生错误： " + ex.Message.ToString()); };
                }
                else if (wave_array[i, 0] > 508 && wave_array[i, 0] < 509)
                {
                    for (int j = 0; j < (channel_pixels[i] - 181); j++)
                    {
                        spec_all[6814 + j] = spec_array[i, (180 + j)];
                    }
                }
                else if (wave_array[i, 0] > 607 && wave_array[i, 0] < 608)
                {
                    for (int j = 0; j < 1918; j++)
                    {
                        spec_all[8500 + j] = spec_array[i, (72 + j)];
                    }
                }
            }
            return spec_all;
        }
        //高性能读取

        //多线程并发读在设备这一层控制？
    }
}
