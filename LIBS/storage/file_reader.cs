using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LIBS.storage
{
    class file_reader
    {
        public static double[] read_testdata(string path)
        {
            double[] filedata = new double[10418];
            int index = 0;
            string rd;
            try
            {
                StreamReader SReader0 = new StreamReader(path, Encoding.Default);
                while ((rd = SReader0.ReadLine()) != null)
                {
                    filedata[index] = Convert.ToDouble(rd);
                    index++;
                }
                SReader0.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("文件读取异常");
            }

            return filedata;
        }
    }


    //设备设置
    //   积分时间、平均次数、平滑宽度、暗电流矫正
    /*****************************/
    //选择元素序列(选择顺序)
    struct select_element
    {
        int sequece_index;
        string element;
        string label;
        double select_wave;
        double seek_peak_range; //硬件设备确定的情况下,某个波长的偏移也确定
        double interval_start_end; //寻峰和找积分区间可以和波长绑定
    }
    //标样序列
    struct standard_labels
    {
        int standard_index; // 0号位置留给空白标样
        string standard_label;
        double[] standard_ppm;
        int average_times;
        bool is_readed;
    }
    //样本序列
    struct sample
    {
        int sample_index;
        string sample_label;
        int average_times;
        bool is_read;
        int weight; //以下目前没用到
        int volume;
        int coefficient;
    }
    /************************************/
    //元数据
    //2048==pixel_of_channel && 10418==all_pixel_of_device
    //read_wave_all[10418] <= channel_wave[2048]
    //read_spec_all_now[10418] <= channel_spec[6][2048]
    //env_spec[10418]
    //read_standard_spec[x][ave_times][10418] => average_standard_spec
    //read_sample_spec[x][ave_times[10418] => average_sample_spec





}
