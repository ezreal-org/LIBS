using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LIBS.ui_control
{
    public partial class UserControl1 : UserControl
    {
        public ComboBox cbx = new ComboBox();
        public UserControl1()
        {
            InitializeComponent();

            cbx.FormattingEnabled = true;
            cbx.Items.AddRange(new object[] {
            "%" ,
            "ppb"   ,
            "ppm"   ,
             });
        }
    }
}
