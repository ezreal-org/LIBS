using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LIBS.storage
{
    class file_operator
    {


        public static  void save_spec_metadat(string url, spec_metadata temp)
        {
            FileStream fs = new FileStream(url, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, temp);
            fs.Close();
        }

       

        public static spec_metadata read_spec_metadata(string url)
        {
            FileStream fs = new FileStream(@url, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            spec_metadata p = bf.Deserialize(fs) as spec_metadata;
            return p;
        }

        
    }
}
