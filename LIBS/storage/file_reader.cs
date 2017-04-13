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
}
