using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        private void PictureBox_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog
                {
                    Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files (*.*)|*.*",
                    FileName = fileName.Replace(FrmMain.unmangledSuffix, ""),
                    Title = "Save As"
                })
                {
                    if (saveDialog.ShowDialog() != DialogResult.OK) return;

                    using (var bitmap = new Bitmap(pictureBox.Image))
                    {
                        string ext = Path.GetExtension(saveDialog.FileName);
                        switch (ext.ToLower())
                        {
                            case ".png":
                                bitmap.Save(saveDialog.FileName, ImageFormat.Png);
                                break;
                            case ".bmp":
                                bitmap.Save(saveDialog.FileName, ImageFormat.Bmp);
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        MessageBox.Show("Saved " + saveDialog.FileName, "RACE OVER",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}