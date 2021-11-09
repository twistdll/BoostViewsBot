using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoostViewsBot
{
    public partial class UC_VideoPage : UserControl
    {
        private ChromeContoller _chromeContoller;

        public UC_VideoPage()
        {
            InitializeComponent();
        }

        private void siticoneButton1_Click(object sender, EventArgs e)
        {
            if (siticoneButton1.Text != "Cancel" && IsUrlCorrect())
            {
                _chromeContoller = new ChromeContoller();
                _chromeContoller.SetUrl(siticoneTextBox1.Text);
                _chromeContoller.BoostViews();
                siticoneButton1.Text = "Cancel";
                siticoneTextBox1.Enabled = false;
            }
            else if (IsUrlCorrect())
            {
                _chromeContoller.CloseAllDrivers();
                siticoneButton1.Text = "Start";
                siticoneTextBox1.Enabled = true;
            }
        }

        private void siticoneButton2_Click(object sender, EventArgs e)
        {
            if (siticoneButton1.Text != "Start")
                viewsCountLabel.Text = _chromeContoller.ViewsCount;
        }

        private bool IsUrlCorrect()
        {
            if (siticoneTextBox1.Text.Contains("vk.com/video") == false || siticoneTextBox1.Text.Contains("vk.com/videos"))
            {
                siticoneTextBox1.Text = "Incorrect URL";
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
