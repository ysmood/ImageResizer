﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImageResizer
{
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }

        public int max_width {
            get {
                return (int)num_max_width.Value;
            }
        }

        public int quality {
            get {
                return (int)num_quality.Value;
            }
        }


        private void btn_ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
