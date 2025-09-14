using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Whiptools
{
    public partial class frmBitmap : Form
    {
        public string filename;

        public frmBitmap()
        {
            InitializeComponent();
            this.FormClosing += frmBitmap_FormClosing;
        }

        private void frmBitmap_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (pictureBox?.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog
            {
                Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files (*.*)|*.*",
                FileName = filename.Replace(frmMain.unmangledSuffix, ""),
                Title = "Save As"
            })
            {
                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                using (var bitmap = new Bitmap(pictureBox.Image))
                {
                    string ext = Path.GetExtension(saveDialog.FileName);
                    switch (ext.ToLower())
                    {
                        case ".png":
                            bitmap.Save(saveDialog.FileName, ImageFormat.Png);
                            MessageBox.Show("Saved " + saveDialog.FileName, "RACE OVER",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case ".bmp":
                            bitmap.Save(saveDialog.FileName, ImageFormat.Bmp);
                            MessageBox.Show("Saved " + saveDialog.FileName, "RACE OVER",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            MessageBox.Show("FATALITY!", "NETWORK ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                    }
                }
            }
        }
    }
}