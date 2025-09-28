using System;
using System.Drawing;
using System.Windows.Forms;

namespace Whiptools
{
    public partial class FrmBitmap : Form
    {
        public string fileName;

        public FrmBitmap()
        {
            InitializeComponent();
            this.FormClosing += FrmBitmap_FormClosing;
        }

        private void FrmBitmap_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (pictureBox?.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }
        }

        private void PictureBox_Click(object sender, EventArgs e) =>
            Utils.SaveBitmap(
                () => new Bitmap(pictureBox.Image),
                fileName.Replace(Utils.unmangledSuffix, ""),
                "Save As");
    }
}