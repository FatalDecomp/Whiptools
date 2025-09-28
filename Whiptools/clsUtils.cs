using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public static void BatchProcess(string filter, string title, string folderDescription,
            Func<byte[], byte[]> transform, Func<string, string> outputFileName,
            string outputType, string actionType,
            Func<int, int, long, long, double, string> extraMsg = null)
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
                        Description = folderDescription
                    })
                    {
                        if (folderDialog.ShowDialog() != DialogResult.OK) return;

                        int countSucc = 0, countFail = 0;
                        int inputSize = 0, outputSize = 0;
                        string displayOutputFile = ""; // for msgbox only
                        int firstFileSet = 0;
                        var fileList = openDialog.FileNames.OrderByDescending(f => new FileInfo(f).Length);
                        var sw = Stopwatch.StartNew();
                        Parallel.ForEach(fileList, f =>
                        {
                            try
                            {
                                byte[] inputData = File.ReadAllBytes(f);
                                byte[] outputData = transform(inputData);
                                string outputFile = Path.Combine(folderDialog.SelectedPath, outputFileName(f));
                                File.WriteAllBytes(outputFile, outputData);

                                Interlocked.Increment(ref countSucc);
                                Interlocked.Add(ref inputSize, inputData.Length);
                                Interlocked.Add(ref outputSize, outputData.Length);
                                if (Interlocked.CompareExchange(ref firstFileSet, 1, 0) == 0)
                                    displayOutputFile = outputFile;
                            }
                            catch
                            {
                                Interlocked.Increment(ref countFail);
                            }
                        });
                        sw.Stop();

                        string msg;
                        if (openDialog.FileNames.Length == 1)
                        {
                            msg = countSucc == 1 ? $"Saved {displayOutputFile}" :
                                $"Failed to {actionType} {openDialog.FileNames[0]}";
                        }
                        else
                        {
                            msg = "";
                            if (countSucc > 0)
                                msg = $"Saved {countSucc} {outputType} file{(countSucc > 1 ? "s" : "")}" +
                                    $" in {folderDialog.SelectedPath}";
                            if (countFail > 0)
                                msg += $"{(countSucc > 0 ? "\n\n" : "")}Failed to {actionType} {countFail} " +
                                    $"file{(countFail > 1 ? "s" : "")}!";
                        }
                        if (extraMsg != null)
                            msg += extraMsg(countSucc, countFail, inputSize, outputSize, sw.Elapsed.TotalSeconds);

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