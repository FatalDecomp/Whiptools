using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Whiptools
{
    public static class Utils
    {
        public const string mangledSuffix = "_mang";
        public const string unmangledSuffix = "_unmang";

        public static void MsgOK(string msg) =>
            MessageBox.Show(msg, "RACE OVER", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void MsgError(string msg = "FATALITY!")
        {
            Random rand = new Random();
            string title = (rand.Next(2) == 0) ?
                "YOU NEED MORE PRACTICE" : "YOU'VE GOT TO TRY HARDER";
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void SaveBytes(byte[] data, string filter, string defaultFileName, string title)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog
                {
                    Filter = filter,
                    FileName = defaultFileName,
                    Title = title
                })
                {
                    if (saveDialog.ShowDialog() != DialogResult.OK) return;

                    File.WriteAllBytes(saveDialog.FileName, data);
                    MsgOK($"Saved {saveDialog.FileName}");
                }
            }
            catch
            {
                MsgError();
            }
        }

        public static void SaveBitmap(Func<Bitmap> bitmapFactory, string defaultFileName, string title)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog
                {
                    Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files (*.*)|*.*",
                    FileName = defaultFileName,
                    Title = title
                })
                {
                    if (saveDialog.ShowDialog() != DialogResult.OK) return;

                    using (Bitmap bitmap = bitmapFactory())
                    {
                        ImageFormat format;
                        switch (saveDialog.FilterIndex)
                        {
                            case 1:
                                format = ImageFormat.Png;
                                break;
                            case 2:
                                format = ImageFormat.Bmp;
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        string fileName = saveDialog.FileName;
                        string desiredExt = "." + format.ToString().ToLower();
                        if (!fileName.EndsWith(desiredExt, StringComparison.OrdinalIgnoreCase))
                            fileName = Path.ChangeExtension(fileName, format.ToString().ToLower());
                        bitmap.Save(fileName, format);
                        MsgOK($"Saved {fileName}");
                    }
                }
            }
            catch
            {
                MsgError();
            }
        }
    }
}
