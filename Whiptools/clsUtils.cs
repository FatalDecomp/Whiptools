using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        public static void BatchProcess(string filter, string title, string description,
            Func<byte[], byte[]> transform, Func<string, string> getOutputFileName,
            string outputType)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = filter,
                    Title = title,
                    Multiselect = true
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;

                    using (var folderDialog = new FolderBrowserDialog
                    {
                        Description = description
                    })
                    {
                        if (folderDialog.ShowDialog() != DialogResult.OK) return;

                        int countSucc = 0, countFail = 0;
                        string outputFile = "";
                        foreach (string fileName in openDialog.FileNames)
                        {
                            try
                            {
                                byte[] inputData = File.ReadAllBytes(fileName);
                                byte[] outputData = transform(inputData);
                                outputFile = Path.Combine(folderDialog.SelectedPath, getOutputFileName(fileName));
                                File.WriteAllBytes(outputFile, outputData);
                                countSucc++;
                            }
                            catch
                            {
                                countFail++;
                            }
                        }

                        string msg;
                        if (openDialog.FileNames.Length == 1)
                        {
                            if (countSucc == 1)
                                msg = $"Saved {outputFile}";
                            else
                                msg = $"Failed to process {openDialog.FileNames.ElementAt(0)}";
                        }
                        else
                        {
                            msg = "";
                            if (countSucc > 0)
                                msg = $"Saved {countSucc} {outputType} file{(countSucc > 1 ? "s" : "")}" +
                                    $" in {folderDialog.SelectedPath}";
                            if (countFail > 0)
                                msg += $"{(countSucc > 0 ? "\n\n" : "")}Failed to process {countFail} " +
                                    $"file{(countFail > 1 ? "s" : "")}!";
                        }
                        if (countFail > 0) MsgError(msg);
                        else MsgOK(msg);
                    }
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
