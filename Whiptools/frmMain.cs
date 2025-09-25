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
        private frmBitmap frmBitmap;

        // bitmap creator
        private Bitmap newBitmap;
        private Color[] newPalette;
        private string newBitmapName;

        public const string mangledSuffix = "_mang";
        public const string unmangledSuffix = "_unmang";

        public frmMain()
        {
            InitializeComponent();
            this.FormClosing += frmMain_FormClosing;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // RUBBISH RACER
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            newBitmap?.Dispose();
            newBitmap = null;
            DisposeBitmapForm();
        }

        private void DisposeBitmapForm()
        {
            if (frmBitmap != null && !frmBitmap.IsDisposed)
            {
                frmBitmap.Close();
                frmBitmap.Dispose();
                frmBitmap = null;
            }
        }

        // file unmangling

        private void btnUnmangleFiles_Click(object sender, EventArgs e)
        {
            FileMangling(true);
        }

        private void btnMangleFiles_Click(object sender, EventArgs e)
        {
            FileMangling(false);
        }

        private void FileMangling(bool unmangle)
        {
            using (var openDialog = new OpenFileDialog
            {
                Filter = $"{MangleType(!unmangle)}d Files (*.BM;*.DRH;*.HMP;*.KC;*.RAW;*.RBP;*.RFR;*.RGE;*.RSS;*.TRK)|" +
                    "*.BM;*.DRH;*.HMP;*.KC;*.RAW;*.RBP;*.RFR;*.RGE;*.RSS;*.TRK|All Files (*.*)|*.*",
                Title = $"Select {MangleType(!unmangle)}d Files",
                Multiselect = true
            })
            {
                if (openDialog.ShowDialog() != DialogResult.OK)
                    return;

                using (var folderDialog = new FolderBrowserDialog
                {
                    Description = $"Save {MangleType(unmangle).ToLower()}d files in:"
                })
                {
                    if (folderDialog.ShowDialog() != DialogResult.OK)
                        return;

                    int countSucc = 0;
                    int countFail = 0;
                    string displayoutputfile = ""; // for msgbox only
                    int firstFileSet = 0;
                    var filelist = openDialog.FileNames
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(fi => fi.Length);
                    var sw = Stopwatch.StartNew();
                    Parallel.ForEach(filelist, fi =>
                    {
                        try
                        {
                            byte[] inputData = File.ReadAllBytes(fi.FullName);
                            byte[] outputData = unmangle ? Unmangler.Unmangle(inputData) : Mangler.Mangle(inputData);
                            if (outputData.Length == 0)
                            {
                                Interlocked.Increment(ref countFail);
                            }
                            else
                            {
                                string outputfile = $"{folderDialog.SelectedPath}\\" +
                                    Path.GetFileNameWithoutExtension(fi.FullName) +
                                    (unmangle ? unmangledSuffix : mangledSuffix) + Path.GetExtension(fi.FullName);
                                File.WriteAllBytes(outputfile, outputData);
                                Interlocked.Increment(ref countSucc);
                                if (Interlocked.CompareExchange(ref firstFileSet, 1, 0) == 0)
                                    displayoutputfile = outputfile;
                            }
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
                            msg = $"Saved {displayoutputfile}";
                        else
                            msg = $"Failed to {MangleType(unmangle).ToLower()} " +
                                openDialog.FileNames.ElementAt(0);
                    }
                    else
                    {
                        if (countSucc > 0)
                            msg = $"Saved {countSucc} {MangleType(unmangle).ToLower()}d file(s) in "
                                + folderDialog.SelectedPath;
                        if (countFail > 0)
                            msg += (countSucc > 0 ? "\n\n" : "") +
                                $"Failed to {MangleType(unmangle).ToLower()} {countFail} file(s)!";
                    }
                    if (countFail > 0)
                        MessageBox.Show(msg, $"FATALITY! ({sw.Elapsed.TotalSeconds:F3}s)",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show(msg, $"RACE OVER ({sw.Elapsed.TotalSeconds:F3}s)",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public static string MangleType(bool unmangle)
        {
            return unmangle ? "Unmangle" : "Mangle";
        }

        // file decoding

        private void btnDecodeCheatAudio_Click(object sender, EventArgs e)
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
                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return;
                    
                    using (var folderDialog = new FolderBrowserDialog
                    {
                        Description = "Save RAW files in:"
                    })
                    {
                        if (folderDialog.ShowDialog() != DialogResult.OK)
                            return;

                        string outputfile = "";
                        foreach (String filename in openDialog.FileNames)
                        {
                            byte[] rawData = File.ReadAllBytes(filename);
                            byte[] decodedData = FibCipher.Decode(rawData, 115, 150);
                            outputfile = folderDialog.SelectedPath +
                                $"\\{Path.GetFileName(filename)}.RAW";
                            File.WriteAllBytes(outputfile, decodedData);
                        }
                        string msg = "";
                        if (openDialog.FileNames.Length == 1)
                            msg = $"Saved {outputfile}";
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

        private void btnDecodeFatalIni_Click(object sender, EventArgs e)
        {
            DecodeIniFile("FATAL.INI", 77, 101);
        }

        private void btnDecodePasswordIni_Click(object sender, EventArgs e)
        {
            DecodeIniFile("PASSWORD.INI", 23, 37);
        }

        private void DecodeIniFile(string IniFilename, int a0, int a1)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash INI Files (*.INI)|*.INI|All Files (*.*)|*.*",
                    Title = $"Select {IniFilename} File",
                    Multiselect = false
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return;

                    string filename = openDialog.FileName;
                    byte[] rawData = File.ReadAllBytes(filename);
                    byte[] decodedData = FibCipher.Decode(rawData, a0, a1);
                    using (var saveDialog = new SaveFileDialog
                    {
                        Filter = "Whiplash INI Files (*.INI)|*.INI|All Files (*.*)|*.*",
                        FileName = $"{Path.GetFileNameWithoutExtension(filename)}_decoded" + 
                            Path.GetExtension(filename),
                        Title = $"Save Decoded {IniFilename} As"
                    })
                    {
                        if (saveDialog.ShowDialog() != DialogResult.OK)
                            return;

                        string savefile = saveDialog.FileName;
                        File.WriteAllBytes(savefile, decodedData);
                        MessageBox.Show($"Saved {savefile}", "RACE OVER",
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

        private void btnConvertRAWAudio_Click(object sender, EventArgs e)
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
                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return;

                    using (var folderDialog = new FolderBrowserDialog
                    {
                        Description = "Save WAV files in:"
                    })
                    {
                        if (folderDialog.ShowDialog() != DialogResult.OK)
                            return;

                        string outputfile = "";
                        foreach (String filename in openDialog.FileNames)
                        {
                            byte[] rawData = File.ReadAllBytes(filename);
                            byte[] wavData = WavAudio.ConvertRawToWav(rawData);
                            outputfile = folderDialog.SelectedPath +
                                $"\\{Path.GetFileName(filename)}.WAV";
                            File.WriteAllBytes(outputfile, wavData);
                        }
                        string msg = "";
                        if (openDialog.FileNames.Length == 1)
                            msg = $"Saved {outputfile}";
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

        private void btnConvertHMPMIDI_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog
            {
                Filter = "HMP MIDI Files (*.HMP)|*.HMP|All Files (*.*)|*.*",
                Title = "Select HMP MIDI Files (Original Format)",
                Multiselect = true
            })
            {
                if (openDialog.ShowDialog() != DialogResult.OK)
                    return;

                using (var folderDialog = new FolderBrowserDialog
                {
                    Description = "Save revised HMP files in:"
                })
                {
                    if (folderDialog.ShowDialog() != DialogResult.OK)
                        return;

                    int countSucc = 0;
                    int countFail = 0;
                    string outputfile = "";
                    foreach (String filename in openDialog.FileNames)
                    {
                        try
                        {
                            byte[] inputData = File.ReadAllBytes(filename);
                            byte[] outputData = HMPMIDI.ConvertToRevisedFormat(inputData);
                            outputfile = folderDialog.SelectedPath +
                                $"\\{Path.GetFileNameWithoutExtension(filename)}_revised.HMP";
                            File.WriteAllBytes(outputfile, outputData);
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
                            msg = $"Saved {outputfile}";
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

        private void btnLoadBitmap_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Bitmaps (*.BM;*.DRH)|*.BM;*.DRH|All Files (*.*)|*.*",
                    Title = "Select Bitmap File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return;

                    string filename = openDialog.FileName;
                    bitmapData = File.ReadAllBytes(filename);
                    bitmapName = Path.GetFileName(filename);

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

                    txtBitmapPath.Text = Path.GetFullPath(filename);
                    lblBitmapLoaded.Text = $"Loaded {bitmapData.Length} bytes, {countColors + 1} colours";
                }
            }
            catch
            {
                lblBitmapLoaded.Text = "YOU NEED MORE PRACTICE!";
            }
        }

        private void txtDimWidth_TextChanged(object sender, EventArgs e)
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

        private void btnLoadPalette_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Palettes (*.PAL)|*.PAL|All Files (*.*)|*.*",
                    Title = "Select Palette File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return;

                    string filename = openDialog.FileName;
                    paletteData = Bitmapper.ConvertRGBToPalette(File.ReadAllBytes(filename));
                    paletteName = Path.GetFileName(filename);

                    txtPalettePath.Text = Path.GetFullPath(filename);
                    lblPaletteLoaded.Text = $"Loaded {paletteData.Length} colours";
                }
            }
            catch
            {
                lblPaletteLoaded.Text = "YOU NEED MORE PRACTICE!";
            }
        }

        private void btnExportPalette_Click(object sender, EventArgs e)
        {
            try
            {
                string filename = Path.GetFileNameWithoutExtension(paletteName) + "_palette";
                using (var saveDialog = new SaveFileDialog
                {
                    Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files (*.*)|*.*",
                    FileName = filename.Replace(unmangledSuffix, ""),
                    Title = "Export Palette"
                })
                {
                    if (saveDialog.ShowDialog() != DialogResult.OK)
                        return;

                    using (var bitmap = Bitmapper.ConvertPaletteToBitmap(paletteData))
                    {
                        string ext = Path.GetExtension(saveDialog.FileName);
                        switch (ext.ToLower())
                        {
                            case ".png":
                                bitmap.Save(saveDialog.FileName, ImageFormat.Png);
                                MessageBox.Show($"Saved {saveDialog.FileName}", "RACE OVER",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            case ".bmp":
                                bitmap.Save(saveDialog.FileName, ImageFormat.Bmp);
                                MessageBox.Show($"Saved {saveDialog.FileName}", "RACE OVER",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewBitmap_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] rgbData = Bitmapper.CreateRGBArray(bitmapData, paletteData);

                int bitmapWidth = Convert.ToInt32(double.Parse(txtDimWidth.Text));
                int bitmapHeight = Convert.ToInt32(Math.Ceiling(bitmapData.Length / (double)bitmapWidth));

                DisposeBitmapForm();
                frmBitmap = new frmBitmap();
                frmBitmap.pictureBox.Image = Bitmapper.CreateBitmapFromRGB(bitmapWidth, bitmapHeight, rgbData);
                frmBitmap.pictureBox.Location = new Point(0, 0);
                frmBitmap.pictureBox.Size = new Size(bitmapWidth, bitmapHeight);
                frmBitmap.Width = Math.Max(320, Math.Min(bitmapWidth, Convert.ToInt32(Screen.PrimaryScreen.Bounds.Width * 0.95))) + 16;
                frmBitmap.Height = Math.Min(bitmapHeight, Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height * 0.95)) + 39;
                frmBitmap.Text = $"{bitmapName} | {paletteName} | {bitmapWidth} x {bitmapHeight} | Click on image to save";
                frmBitmap.filename = Path.GetFileNameWithoutExtension(bitmapName);
                frmBitmap.Show();
            }
            catch
            {
                MessageBox.Show("YOU'VE GOT TO TRY HARDER!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // bitmap creator

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Image Files (*.png;*.bmp)|*.png;*.bmp|All Files (*.*)|*.*",
                    Title = "Select Image File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return;

                    newBitmapName = openDialog.FileName;
                    using (var bitmap = new Bitmap(newBitmapName))
                    {
                        Color[] palette = Bitmapper.GetPaletteFromBitmap(bitmap);
                        if (palette.Length > 256)
                        {
                            MessageBox.Show($"Too many colours! ({Convert.ToString(palette.Length)})",
                                "YOU NEED MORE PRACTICE", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        else
                        {
                            newBitmap?.Dispose();
                            newBitmap = (Bitmap)bitmap.Clone();
                            newPalette = palette;
                            txtImagePath.Text = newBitmapName;
                            lblImageLoaded.Text = $"Loaded {newBitmap.Width} x " +
                                $"{newBitmap.Height}, {newPalette.Length} colours";
                        }
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
                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                string filename = saveDialog.FileName;
                File.WriteAllBytes(filename, Bitmapper.GetPaletteArray(palette));
                MessageBox.Show($"Saved {filename}", "RACE OVER", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSaveNewPalette_Click(object sender, EventArgs e)
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

        private void btnAddToExistingPalette_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Palettes (*.PAL)|*.PAL|All Files (*.*)|*.*",
                    Title = "Select Palette File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return;

                    string filename = openDialog.FileName;
                    Color[] inputPalette = Bitmapper.ConvertRGBToPalette(File.ReadAllBytes(filename));
                    int maxOffset = 256 - newPalette.Length;
                    string userInput = Microsoft.VisualBasic.Interaction.InputBox(
                        $"Add at position (0-{maxOffset.ToString()}):", "Add to Palette", "0");
                    int offset = Convert.ToInt32(userInput);
                    if (offset < 0 || offset > maxOffset)
                        throw new Exception();

                    int newLength = Math.Max(inputPalette.Length, offset + newPalette.Length);
                    Color[] outputPalette = new Color[newLength];
                    for (int i = 0; i < inputPalette.Length; i++)
                        outputPalette[i] = inputPalette[i];
                    for (int i = 0; i < newPalette.Length; i++)
                        outputPalette[i + offset] = newPalette[i];
                    SavePalette(outputPalette, filename);
                }
            }
            catch
            {
                MessageBox.Show("FATALITY!", "NETWORK ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveBMFile_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "Whiplash Palettes (*.PAL)|*.PAL|All Files (*.*)|*.*",
                    Title = "Select Palette File"
                })
                {
                    if (openDialog.ShowDialog() != DialogResult.OK)
                        return;

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
                        if (saveDialog.ShowDialog() != DialogResult.OK)
                            return;

                        string savefile = saveDialog.FileName;
                        File.WriteAllBytes(savefile, outputArray);
                        MessageBox.Show($"Saved {savefile}", "RACE OVER", MessageBoxButtons.OK, MessageBoxIcon.Information);
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