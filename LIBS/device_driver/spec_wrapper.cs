using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LIBS.device_driver
{
    //包装底层sdk设备函数，提供统一接口来控制硬件设备
    class spec_wrapper
    {
        private static OmniDriver.CCoWrapper wrapper;
        public int cnt_of_devices;
        
        public spec_wrapper()
        {
            cnt_of_devices = 0;
            wrapper = new OmniDriver.CCoWrapper();
        }

        //连接设备
        public bool connect()
        {
            cnt_of_devices = wrapper.openAllSpectrometers();
            if (cnt_of_devices > 0) return true;
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

        //一次获取所有通道波长
        public double[] get_wave_all()
        {
            double[] wave_all = new double[10418];
            int wave_index = 0;
            for(int i = 0; i < cnt_of_devices; i++)
            {
                double []wave_temp = wrapper.getWavelengths(i);
                for(int j = 0; j < wave_temp.Length; j++) //以后这里会根据响应的强度对重叠区域进行取舍
                {
                    wave_all[wave_index++] = wave_temp[j];
                }
            }
            return wave_all;
        }
        //一次获取所有通道数据
        public double[] get_spec_all()
        {
            double []spec_all = new double[10418];
            int spec_index = 0;
            for(int i = 0; i < cnt_of_devices; i++)
            {
                double[] spec_temp = wrapper.getWavelengths(i);
                for (int j = 0; j < spec_temp.Length; j++) //以后这里会根据响应的强度对重叠区域进行取舍
                {
                    spec_all[spec_index++] = spec_temp[j];
                }
            }
            return spec_all;
        }
        //高性能读取

        //多线程并发读在设备这一层控制？
    }
}
