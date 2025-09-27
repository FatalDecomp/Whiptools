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
    public partial class frmMain : Form
    {
        // bitmap viewer
        private byte[] bitmapData;
        private Color[] paletteData;
        private string bitmapName, paletteName;
        private FrmBitmap frmBitmap;

        // bitmap creator
        private Bitmap newBitmap;
        private Color[] newPalette;
        private string newBitmapName;

        public const string mangledSuffix = "_mang";
        public const string unmangledSuffix = "_unmang";

        public frmMain()
        {
            InitializeComponent();
            this.FormClosing += FrmMain_FormClosing;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            // RUBBISH RACER
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            newBitmap?.Dispose();
            newBitmap = null;
            DisposeBitmapForm();
        }

        private void DisposeBitmapForm()
        {
            if (frmBitmap != null && !frmBitmap.IsDisposed)
            {
                frmBitmap.Dispose();
                frmBitmap = null;
            }
        }

        // file unmangling

        private void BtnUnmangleFiles_Click(object sender, EventArgs e)
        {
            FileMangling(true);
        }

        private void BtnMangleFiles_Click(object sender, EventArgs e)
        {
            FileMangling(false);
        }

        private void FileMangling(bool isUnmangle)
        {
            using (var openDialog = new OpenFileDialog
            {
                Filter = $"{MangleType(!isUnmangle)}d Files (*.BM;*.DRH;*.HMP;*.KC;*.RAW;*.RBP;*.RFR;*.RGE;*.RSS;*.TRK)|" +
                    "*.BM;*.DRH;*.HMP;*.KC;*.RAW;*.RBP;*.RFR;*.RGE;*.RSS;*.TRK|All Files (*.*)|*.*",
                Title = $"Select {MangleType(!isUnmangle)}d Files",
                Multiselect = true
            })
            {
                if (openDialog.ShowDialog() != DialogResult.OK) return;

                using (var folderDialog = new FolderBrowserDialog
                {
                    Description = $"Save {MangleType(isUnmangle).ToLower()}d files in:"
                })
                {
                    if (folderDialog.ShowDialog() != DialogResult.OK) return;

                    int countSucc = 0, countFail = 0;
                    int inputSize = 0, outputSize = 0;
                    string displayOutputFile = ""; // for msgbox only
                    int firstFileSet = 0;
                    var fileList = openDialog.FileNames
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.Length);
                    var sw = Stopwatch.StartNew();
                    Parallel.ForEach(fileList, fi =>
                    {
                        try
                        {
                            byte[] inputData = File.ReadAllBytes(fi.FullName);
                            byte[] outputData = isUnmangle ?
                                Unmangler.Unmangle(inputData) : Mangler.Mangle(inputData);

                            // verify mangled output
                            if (!isUnmangle && !VerifyMangle.Verify(inputData, outputData))
                                throw new InvalidOperationException();

                            string outputFile = Path.Combine(folderDialog.SelectedPath,
                                Path.GetFileNameWithoutExtension(fi.FullName) +
                                (isUnmangle ? unmangledSuffix : mangledSuffix) +
                                Path.GetExtension(fi.FullName));
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
                    string msg = "";
                    if (openDialog.FileNames.Length == 1)
                    {
                        if (countSucc == 1)
                            msg = $"Saved {displayOutputFile}";
                        else
                            msg = $"Failed to {MangleType(isUnmangle).ToLower()} " +
                                openDialog.FileNames.ElementAt(0);
                    }
                    else
                    {
                        if (countSucc > 0)
                            msg = $"Saved {countSucc} {MangleType(isUnmangle).ToLower()}d file(s) in "
                                + folderDialog.SelectedPath;
                        if (countFail > 0)
                            msg += (countSucc > 0 ? "\n\n" : "") +
                                $"Failed to {MangleType(isUnmangle).ToLower()} {countFail} file(s)!";
                    }
                    if (!isUnmangle)
                    {
                        msg += $"\n\nTime elapsed: {sw.Elapsed.TotalSeconds:F2}s";
                        if (countSucc > 0)
                            if (outputSize > 0)
                                msg += $"\nCompression ratio: {(double)outputSize / inputSize:P2}";
                    }
                    if (countFail > 0)
                        MessageBox.Show(msg, "FATALITY!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show(msg, "RACE OVER", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public static string MangleType(bool isUnmangle) =>
            isUnmangle ? "Unmangle" : "Mangle";

        // file decoding

        private void BtnDecodeCheatAudio_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Cheat Audio (*.KC)|*.KC|All Files (*.*)|*.*",
                    Title = "Select Cheat Audio Files",
                    Multiselect = true
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;
                    
                    using (var folderDialog = new FolderBrowserDialog
                    {
                        Description = "Save RAW files in:"
                    })
                    {
                        if (folderDialog.ShowDialog() != DialogResult.OK) return;

                        string outputFile = "";
                        foreach (String fileName in openDialog.FileNames)
                        {
                            byte[] rawData = File.ReadAllBytes(fileName);
                            byte[] decodedData = FibCipher.Decode(rawData, 115, 150);
                            outputFile = Path.Combine(folderDialog.SelectedPath,
                                $"{Path.GetFileName(fileName)}.RAW");
                            File.WriteAllBytes(outputFile, decodedData);
                        }
                        string msg = "";
                        if (openDialog.FileNames.Length == 1)
                            msg = $"Saved {outputFile}";
                        else
                            msg = $"Saved {openDialog.FileNames.Length} RAW files in " +
                                folderDialog.SelectedPath;
                        MessageBox.Show(msg, "RACE OVER", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDecodeFatalIni_Click(object sender, EventArgs e)
        {
            DecodeIniFile("FATAL.INI", 77, 101);
        }

        private void BtnDecodePasswordIni_Click(object sender, EventArgs e)
        {
            DecodeIniFile("PASSWORD.INI", 23, 37);
        }

        private void DecodeIniFile(string iniFilename, int a0, int a1)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash INI Files (*.INI)|*.INI|All Files (*.*)|*.*",
                    Title = $"Select {iniFilename} File",
                    Multiselect = false
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;

                    string fileName = openDialog.FileName;
                    byte[] rawData = File.ReadAllBytes(fileName);
                    byte[] decodedData = FibCipher.Decode(rawData, a0, a1);
                    using (var saveDialog = new SaveFileDialog
                    {
                        Filter = "Whiplash INI Files (*.INI)|*.INI|All Files (*.*)|*.*",
                        FileName = $"{Path.GetFileNameWithoutExtension(fileName)}_decoded" + 
                            Path.GetExtension(fileName),
                        Title = $"Save Decoded {iniFilename} As"
                    })
                    {
                        if (saveDialog.ShowDialog() != DialogResult.OK) return;

                        string saveFile = saveDialog.FileName;
                        File.WriteAllBytes(saveFile, decodedData);
                        MessageBox.Show($"Saved {saveFile}", "RACE OVER",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // audio tools

        private void BtnConvertRAWAudio_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Raw Audio (*.RAW;*.RBP;*.RFR;*.RGE;*.RSS)|" +
                        "*.RAW;*.RBP;*.RFR;*.RGE;*.RSS|All Files (*.*)|*.*",
                    Title = "Select Raw Audio Files",
                    Multiselect = true
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;

                    using (var folderDialog = new FolderBrowserDialog
                    {
                        Description = "Save WAV files in:"
                    })
                    {
                        if (folderDialog.ShowDialog() != DialogResult.OK) return;

                        string outputFile = "";
                        foreach (String fileName in openDialog.FileNames)
                        {
                            byte[] rawData = File.ReadAllBytes(fileName);
                            byte[] wavData = WavAudio.ConvertRawToWav(rawData);
                            outputFile = Path.Combine(folderDialog.SelectedPath,
                                $"{Path.GetFileName(fileName)}.WAV");
                            File.WriteAllBytes(outputFile, wavData);
                        }
                        string msg = "";
                        if (openDialog.FileNames.Length == 1)
                            msg = $"Saved {outputFile}";
                        else
                            msg = $"Saved {openDialog.FileNames.Length} WAV files in " +
                                folderDialog.SelectedPath;
                        MessageBox.Show(msg, "RACE OVER", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnConvertHMPMIDI_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog
            {
                Filter = "HMP MIDI Files (*.HMP)|*.HMP|All Files (*.*)|*.*",
                Title = "Select HMP MIDI Files (Original Format)",
                Multiselect = true
            })
            {
                if (openDialog.ShowDialog() != DialogResult.OK) return;

                using (var folderDialog = new FolderBrowserDialog
                {
                    Description = "Save revised HMP files in:"
                })
                {
                    if (folderDialog.ShowDialog() != DialogResult.OK) return;

                    int countSucc = 0;
                    int countFail = 0;
                    string outputFile = "";
                    foreach (String fileName in openDialog.FileNames)
                    {
                        try
                        {
                            byte[] inputData = File.ReadAllBytes(fileName);
                            byte[] outputData = HMPMIDI.ConvertToRevisedFormat(inputData);
                            outputFile = Path.Combine(folderDialog.SelectedPath,
                                $"{Path.GetFileNameWithoutExtension(fileName)}_revised.HMP");
                            File.WriteAllBytes(outputFile, outputData);
                            countSucc++;
                        }
                        catch
                        {
                            countFail++;
                        }
                    }
                    string msg = "";
                    if (openDialog.FileNames.Length == 1)
                    {
                        if (countSucc == 1)
                            msg = $"Saved {outputFile}";
                        else
                            msg = $"Failed to convert {openDialog.FileNames.ElementAt(0)}";
                    }
                    else
                    {
                        if (countSucc > 0)
                            msg = $"Saved {countSucc} revised HMP file(s) in {folderDialog.SelectedPath}";
                        if (countFail > 0)
                            msg += $"{(countSucc > 0 ? "\n\n" : "")}Failed to convert {countFail} file(s)!";
                    }
                    if (countFail > 0)
                        MessageBox.Show(msg, "FATALITY!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show(msg, "RACE OVER", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // bitmap viewer

        private void BtnLoadBitmap_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Bitmaps (*.BM;*.DRH)|*.BM;*.DRH|All Files (*.*)|*.*",
                    Title = "Select Bitmap File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;

                    string fileName = openDialog.FileName;
                    bitmapData = File.ReadAllBytes(fileName);
                    bitmapName = Path.GetFileName(fileName);

                    int countColors = 0;
                    foreach (byte b in bitmapData)
                        if (b > countColors)
                            countColors = b;

                    for (int i = (int)Math.Sqrt(bitmapData.Length); i > 1; i--)
                    {
                        double guessWidth = (double)bitmapData.Length / i;
                        if (guessWidth == (int)guessWidth)
                        {
                            txtDimWidth.Text = guessWidth.ToString();
                            lblDimHeight.Text = $"x {i}";
                            break;
                        }
                    }

                    txtBitmapPath.Text = Path.GetFullPath(fileName);
                    lblBitmapLoaded.Text = $"Loaded {bitmapData.Length} bytes, {countColors + 1} colours";
                }
            }
            catch
            {
                lblBitmapLoaded.Text = "YOU NEED MORE PRACTICE!";
            }
        }

        private void TxtDimWidth_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int bitmapWidth = Convert.ToInt32(double.Parse(txtDimWidth.Text));
                int bitmapHeight = Convert.ToInt32(Math.Ceiling(bitmapData.Length / (double)bitmapWidth));
                lblDimHeight.Text = $"x {bitmapHeight}";
            }
            catch
            {
                lblDimHeight.Text = "x ?";
            }
        }

        private void BtnLoadPalette_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Palettes (*.PAL)|*.PAL|All Files (*.*)|*.*",
                    Title = "Select Palette File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;

                    string fileName = openDialog.FileName;
                    paletteData = Bitmapper.ConvertRGBToPalette(File.ReadAllBytes(fileName));
                    paletteName = Path.GetFileName(fileName);

                    txtPalettePath.Text = Path.GetFullPath(fileName);
                    lblPaletteLoaded.Text = $"Loaded {paletteData.Length} colours";
                }
            }
            catch
            {
                lblPaletteLoaded.Text = "YOU NEED MORE PRACTICE!";
            }
        }

        private void BtnExportPalette_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(paletteName) + "_palette";
                using (var saveDialog = new SaveFileDialog
                {
                    Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files (*.*)|*.*",
                    FileName = fileName.Replace(unmangledSuffix, ""),
                    Title = "Export Palette"
                })
                {
                    if (saveDialog.ShowDialog() != DialogResult.OK) return;

                    using (var bitmap = Bitmapper.ConvertPaletteToBitmap(paletteData))
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
                        MessageBox.Show($"Saved {saveDialog.FileName}", "RACE OVER",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnViewBitmap_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] rgbData = Bitmapper.CreateRGBArray(bitmapData, paletteData);

                int bitmapWidth = Convert.ToInt32(double.Parse(txtDimWidth.Text));
                int bitmapHeight = Convert.ToInt32(Math.Ceiling(bitmapData.Length / (double)bitmapWidth));

                DisposeBitmapForm();
                frmBitmap = new FrmBitmap();
                frmBitmap.pictureBox.Image = Bitmapper.CreateBitmapFromRGB(bitmapWidth, bitmapHeight, rgbData);
                frmBitmap.pictureBox.Location = new Point(0, 0);
                frmBitmap.pictureBox.Size = new Size(bitmapWidth, bitmapHeight);
                frmBitmap.Width = Math.Max(320, Math.Min(bitmapWidth,
                    Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width * 0.95))) + 16;
                frmBitmap.Height = Math.Min(bitmapHeight,
                    Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height * 0.95)) + 39;
                frmBitmap.Text = $"{bitmapName} | {paletteName} | {bitmapWidth} x {bitmapHeight} | Click on image to save";
                frmBitmap.fileName = Path.GetFileNameWithoutExtension(bitmapName);
                frmBitmap.Show();
            }
            catch
            {
                MessageBox.Show("YOU'VE GOT TO TRY HARDER!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // bitmap creator

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Image Files (*.png;*.bmp)|*.png;*.bmp|All Files (*.*)|*.*",
                    Title = "Select Image File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;

                    newBitmapName = openDialog.FileName;
                    using (var bitmap = new Bitmap(newBitmapName))
                    {
                        Color[] palette = Bitmapper.GetPaletteFromBitmap(bitmap);
                        if (palette.Length > 256)
                        {
                            MessageBox.Show($"Too many colours! ({Convert.ToString(palette.Length)})",
                                "YOU NEED MORE PRACTICE", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                        newBitmap?.Dispose();
                        newBitmap = (Bitmap)bitmap.Clone();
                        newPalette = palette;
                        txtImagePath.Text = newBitmapName;
                        lblImageLoaded.Text = $"Loaded {newBitmap.Width} x " +
                            $"{newBitmap.Height}, {newPalette.Length} colours";
                    }
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SavePalette(Color[] palette, string defaultFileName)
        {
            using (var saveDialog = new SaveFileDialog
            {
                Filter = "Palette Files (*.PAL)|*.PAL|All Files (*.*)|*.*",
                Title = "Save Palette As",
                FileName = Path.GetFileNameWithoutExtension(defaultFileName)
            })
            {
                if (saveDialog.ShowDialog() != DialogResult.OK) return;

                string fileName = saveDialog.FileName;
                File.WriteAllBytes(fileName, Bitmapper.GetPaletteArray(palette));
                MessageBox.Show($"Saved {fileName}", "RACE OVER", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSaveNewPalette_Click(object sender, EventArgs e)
        {
            try
            {
                SavePalette(newPalette, newBitmapName);
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddToExistingPalette_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Palettes (*.PAL)|*.PAL|All Files (*.*)|*.*",
                    Title = "Select Palette File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;

                    string fileName = openDialog.FileName; 
                    Color[] inputPalette = Bitmapper.ConvertRGBToPalette(File.ReadAllBytes(fileName));
                    int maxOffset = 256 - newPalette.Length;
                    string userInput = Microsoft.VisualBasic.Interaction.InputBox(
                        $"Add at position (0-{maxOffset}):", "Add to Palette", "0");
                    int offset = Convert.ToInt32(userInput);
                    if (offset < 0 || offset > maxOffset) throw new ArgumentOutOfRangeException();

                    int newLength = Math.Max(inputPalette.Length, offset + newPalette.Length);
                    Color[] outputPalette = new Color[newLength];
                    for (int i = 0; i < inputPalette.Length; i++)
                        outputPalette[i] = inputPalette[i];
                    for (int i = 0; i < newPalette.Length; i++)
                        outputPalette[i + offset] = newPalette[i];
                    SavePalette(outputPalette, fileName);
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveBMFile_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Palettes (*.PAL)|*.PAL|All Files (*.*)|*.*",
                    Title = "Select Palette File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK) return;

                    Color[] palette = Bitmapper.ConvertRGBToPalette(File.ReadAllBytes(openDialog.FileName));
                    byte[] outputArray;
                    try
                    {
                        outputArray = Bitmapper.GetBitmapArray(newBitmap, palette);
                    }
                    catch
                    {
                        MessageBox.Show("Incorrect palette!", "YOU NEED MORE PRACTICE",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    using (var saveDialog = new SaveFileDialog
                    {
                        Filter = "BM File (*.BM)|*.BM|DRH File (*.DRH)|*.DRH|All Files (*.*)|*.*",
                        FileName = Path.GetFileNameWithoutExtension(newBitmapName),
                        Title = "Save Bitmap As"
                    })
                    {
                        if (saveDialog.ShowDialog() != DialogResult.OK) return;

                        string saveFile = saveDialog.FileName;
                        File.WriteAllBytes(saveFile, outputArray);
                        MessageBox.Show($"Saved {saveFile}", "RACE OVER",
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