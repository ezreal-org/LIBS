using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LIBS.storage
{
    class NIST
    {
        public string[] nameNIST = new string[2359];      //把保存NIST库元素名字
        public double[] waveNIST = new double[2359];     //保存NIST库元素的波长
        public int[] typeNIST = new int[2359];             //保存NIST库光谱的类型
        public int[] countNIST = new int[2359];          //保存NIST库的强度计数

        public void read_NIST()
        {
            try
            {
                StreamReader SReader1 = new StreamReader("NIST\\nameNIST.txt", Encoding.Default);
                int k = 0;//
                string rd = "00";
                while ((rd = SReader1.ReadLine()) != null)
                {
                    nameNIST[k] = Convert.ToString(rd);
                    k++;
                }
                //MessageBox.Show("读取NIST成功");
                StreamReader SReader2 = new StreamReader("NIST\\waveNIST.txt", Encoding.Default);
                k = 0;//
                while ((rd = SReader2.ReadLine()) != null)
                {
                    waveNIST[k] = Convert.ToDouble(rd);
                    k++;
                }
                StreamReader SReader3 = new StreamReader("NIST\\typeNIST.txt", Encoding.Default);
                k = 0;//
                while ((rd = SReader3.ReadLine()) != null)
                {
                    typeNIST[k] = Convert.ToInt32(rd);
                    k++;
                }
                StreamReader SReader4 = new StreamReader("NIST\\countNIST.txt", Encoding.Default);
                k = 0;//
                while ((rd = SReader4.ReadLine()) != null)
                {
                    countNIST[k] = Convert.ToInt32(rd);
                    k++;
                } 
            }
            catch (Exception ex) { MessageBox.Show("读取NIST库时产生错误：" + ex.Message.ToString()); }
        }//读取元素光谱标准数据文件
    }
}
