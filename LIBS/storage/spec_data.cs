using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LIBS.storage
{

    //设备设置
    //   积分时间、平均次数、平滑宽度、暗电流矫正
    /*****************************/
    //选择元素序列(选择顺序)
    struct select_element
    {
        public int sequece_index;
        public string element;
        public string label;
        public double select_wave;
        public double peak_wave;       //实际读取波长
        public double seek_peak_range; //硬件设备确定的情况下,某个波长的偏移也确定，寻峰和找积分区间可以和波长绑定
        public double interval_start;  //积分区间开始和结束
        public double interval_end;   
    }
    //标样序列
    struct standard
    {
        public int standard_index; // 0号位置留给空白标样
        public string standard_label;
        public double[] standard_ppm;
        public int average_times;
        public bool is_readed;
    }
    //样本序列
    struct sample
    {
        public int sample_index;
        public string sample_label;
        public int average_times;
        public bool is_read;
        public int weight; //以下目前没用到
        public int volume;
        public int coefficient;
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
