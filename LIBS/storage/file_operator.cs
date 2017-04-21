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
        public static void save_spec_metadat(string url, spec_metadata temp)
        {
            FileStream fs = new FileStream(@url, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, temp);
            fs.Close();
        }

        public static void save_select_element(string url, select_element temp)
        {
            FileStream fs = new FileStream(@url, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, temp);
            fs.Close();
        }

        public static void save_standard(string url, standard temp)
        {
            FileStream fs = new FileStream(@url, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, temp);
            fs.Close();
        }

        public static void save_sample(string url, sample temp)
        {
            FileStream fs = new FileStream(@url, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, temp);
            fs.Close();
        }

        public static spec_metadata restore_spec_metadata(string url)
        {
            FileStream fs = new FileStream(@url, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            spec_metadata p = bf.Deserialize(fs) as spec_metadata;
            return p;
        }

        public static select_element restore_select_element(string url)
        {
            FileStream fs = new FileStream(@url, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            select_element p = bf.Deserialize(fs) as select_element;
            return p;
        }

        public static standard restore_standard(string url)
        {
            FileStream fs = new FileStream(@url, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            standard p = bf.Deserialize(fs) as standard;
            return p;
        }

        public static sample restore(string url)
        {
            FileStream fs = new FileStream(@url, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            sample p = bf.Deserialize(fs) as sample;
            return p;
        }
    }
}
