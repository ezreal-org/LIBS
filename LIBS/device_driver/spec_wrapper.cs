using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LIBS.device_driver
{
    //包装底层sdk设备函数，提供统一接口来控制硬件设备
    class spec_wrapper
    {
        private static OmniDriver.CCoWrapper wrapper;
        public int cnt_of_devices;
        public int[] map_index_waverange2device; //逻辑设备索引到实际设备索引的映射
        public int build_num; //记录第几次读取
        public bool is_test_model; //是否是测试模式
        private double[] wave_all;
        private double[,] wave_array;
        private static double[,] spec_array; //每个通道的强度计数，在线程函数里进行读取设置
        private static int[] channel_pixels;
        private static Thread[] read_threads; //每个通道分配一个read_spec的线程



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
                spec_array = new double[6, 2068];
                init_wave_all(); //连接时初始化波长，并初始化索引映射的map
                return true;
            }
            cnt_of_devices = wrapper.openAllSpectrometers();
            if (cnt_of_devices > 0)
            {
                //分配光强数组空间
                spec_array = new double[cnt_of_devices, 2068];
                init_wave_all(); //连接时初始化波长
                //定义读取线程数组
                read_threads = new Thread[cnt_of_devices];
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
            // @jayce 设备驱动层做用户看到的index到设备实际通道index的映射
            device_index = map_index_waverange2device[device_index];
            wrapper.setIntegrationTime(device_index, value);
        }
        public int get_intergration_time(int device_index)
        {
            // @jayce 设备驱动层做用户看到的index到设备实际通道index的映射
            device_index = map_index_waverange2device[device_index];
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
            for (int i = 0; i < cnt_of_devices; i++)
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
            for (int i = 0; i < cnt_of_devices; i++)
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
                    string rd = "";
                    while ((rd = SReader.ReadLine()) != null)
                    {
                        wave_all[k] = Convert.ToDouble(rd);
                        k++;
                    }
                    SReader.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("读取测试波长失败" + e.Message.ToString());
                }
                return;
            }
            //实模式
            wave_array = new double[cnt_of_devices, 2068];
            map_index_waverange2device = new int[cnt_of_devices];
            channel_pixels = new int[cnt_of_devices]; //记录每个通道的像素
            for (int i = 0; i < cnt_of_devices; i++)
            {
                double[] wave_temp = wrapper.getWavelengths(i);
                channel_pixels[i] = wave_temp.Length;
                for (int j = 0; j < wave_temp.Length; j++) //以后这里会根据响应的强度对重叠区域进行取舍
                {
                    wave_array[i, j] = wave_temp[j];
                }

            }
            //根据实际响应排序
            for (int i = 0; i < cnt_of_devices; i++)
            {
                if (wave_array[i, 0] > 185 && wave_array[i, 0] < 186)
                {
                    //System.Console.WriteLine("找到第一通道：i={0}", i.ToString());
                    map_index_waverange2device[0] = i; // @jayce i号硬件设备索引，对应逻辑用户看到的0号索引
                    for (int j = 121; j <= 1653; j++)
                    {
                        wave_all[j-121] = wave_array[i, j];
                    }
                }
                else if (wave_array[i, 0] > 228 && wave_array[i, 0] < 238)
                {
                    map_index_waverange2device[1] = i;
                    for (int j = 167; j <= 1886; j++)
                    {
                        wave_all[1533 + j - 167] = wave_array[i, j];
                    }
                }
                else if (wave_array[i, 0] > 320 && wave_array[i, 0] < 321)
                {
                    map_index_waverange2device[2] = i;
                    for (int j = 86; j <= 1834; j++)
                    {
                        wave_all[3252 + j - 86] = wave_array[i, j];
                    }
                }
                else if (wave_array[i, 0] > 398 && wave_array[i, 0] < 399)
                {
                    map_index_waverange2device[3] = i;
                    try
                    {
                        for (int j = 99; j <= 1736; j++)
                        {
                            wave_all[5001 + j-99] = wave_array[i, j];
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("第四通道发生错误： " + ex.Message.ToString()); };
                }
                else if (wave_array[i, 0] > 508 && wave_array[i, 0] < 509)
                {
                    map_index_waverange2device[4] = i;
                    for (int j = 23; j <= 1784; j++)
                    {
                        wave_all[6639 + j - 23] = wave_array[i, j];
                    }
                }
                else if (wave_array[i, 0] > 607 && wave_array[i, 0] < 608)
                {
                    map_index_waverange2device[5] = i;
                    for (int j = 24; j < 2041; j++)
                    {
                        wave_all[8401 + j - 24] = wave_array[i, j];
                    }
                }
            }
        }

        //读取线程的函数
        public static void thread_read(object device_index)
        {
            int index = int.Parse(device_index.ToString());
            double[] spec_temp = wrapper.getSpectrum(index);
            for (int j = 0; j < spec_temp.Length; j++)
            {
                spec_array[index, j] = spec_temp[j];
            }
        }

        //一次获取所有通道数据
        public double[] get_spec_all()
        {
            double[] spec_all = new double[10418];
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
            //多线程读取
            for (int i = 0; i < cnt_of_devices; i++)
            {
                //采用多线程读取
                read_threads[i] = new Thread(new ParameterizedThreadStart(thread_read));
                read_threads[i].Start(i);
            }
            //等待所以线程读完所以通道数据
            for (int i = 0; i < cnt_of_devices; i++)
            {
                read_threads[i].Join();
            }

            for (int i = 0; i < cnt_of_devices; i++)
            {
                if (wave_array[i, 0] > 185 && wave_array[i, 0] < 186)
                {
                    //System.Console.WriteLine("找到第一通道：i={0}", i.ToString());
                    map_index_waverange2device[0] = i; // @jayce i号硬件设备索引，对应逻辑用户看到的0号索引
                    for (int j = 121; j <= 1653; j++)
                    {
                        spec_all[j - 121] = spec_array[i, j];
                    }
                }
                else if (wave_array[i, 0] > 228 && wave_array[i, 0] < 238)
                {
                    map_index_waverange2device[1] = i;
                    for (int j = 167; j <= 1886; j++)
                    {
                        spec_all[1533 + j - 167] = spec_array[i, j];
                    }
                }
                else if (wave_array[i, 0] > 320 && wave_array[i, 0] < 321)
                {
                    map_index_waverange2device[2] = i;
                    for (int j = 86; j <= 1834; j++)
                    {
                        spec_all[3252 + j - 86] = spec_array[i, j];
                    }
                }
                else if (wave_array[i, 0] > 398 && wave_array[i, 0] < 399)
                {
                    map_index_waverange2device[3] = i;
                    try
                    {
                        for (int j = 99; j <= 1736; j++)
                        {
                            spec_all[5001 + j - 99] = spec_array[i, j];
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("第四通道发生错误： " + ex.Message.ToString()); };
                }
                else if (wave_array[i, 0] > 508 && wave_array[i, 0] < 509)
                {
                    map_index_waverange2device[4] = i;
                    for (int j = 23; j <= 1784; j++)
                    {
                        spec_all[6639 + j - 23] = spec_array[i, j];
                    }
                }
                else if (wave_array[i, 0] > 607 && wave_array[i, 0] < 608)
                {
                    map_index_waverange2device[5] = i;
                    for (int j = 24; j < 2041; j++)
                    {
                        spec_all[8401 + j - 24] = spec_array[i, j];
                    }
                }

            }
            return spec_all;
        }
        //高性能读取

        //多线程并发读在设备这一层控制？
    }
}
