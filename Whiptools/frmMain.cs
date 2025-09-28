using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Whiptools
{
    public partial class FrmMain : Form
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

        public FrmMain()
        {
            InitializeComponent();
            FormClosing += FrmMain_FormClosing;
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

        private void BtnUnmangleFiles_Click(object sender, EventArgs e) =>
            FileMangling(true);

        private void BtnMangleFiles_Click(object sender, EventArgs e) =>
            FileMangling(false);

        private void FileMangling(bool isUnmangle)
        {
            string suffix = isUnmangle ? Utils.unmangledSuffix : Utils.mangledSuffix;
            Utils.BatchProcess(
                filter: $"{MangleType(!isUnmangle)}d Files (*.BM;*.DRH;*.HMP;*.KC;*.RAW;*.RBP;*.RFR;*.RGE;*.RSS;*.TRK)|" +
                    "*.BM;*.DRH;*.HMP;*.KC;*.RAW;*.RBP;*.RFR;*.RGE;*.RSS;*.TRK|All Files (*.*)|*.*",
                title: $"Select {MangleType(!isUnmangle)}d Files",
                folderDescription: $"Save {MangleType(isUnmangle).ToLower()}d files in:",
                transform: input =>
                {
                    byte[] output = isUnmangle ? Unmangler.Unmangle(input) : Mangler.Mangle(input);
                    if (!isUnmangle && !VerifyMangle.Verify(input, output))
                        throw new InvalidOperationException();
                    return output;
                },
                outputFileName: filePath => Path.GetFileNameWithoutExtension(filePath) +
                    suffix + Path.GetExtension(filePath),
                outputType: MangleType(isUnmangle).ToLower() + "d",
                actionType: MangleType(isUnmangle).ToLower(),
                extraMsg: (countSucc, countFail, inputSize, outputSize, timeElapsed) =>
                {
                    if (isUnmangle) return "";
                    string msg = $"\n\nTime elapsed: {timeElapsed:F2}s";
                    if (outputSize > 0)
                        msg += $"\nCompression ratio: {(double)outputSize / inputSize:P2}";
                    return msg;
                }
            );
        }

        private static string MangleType(bool isUnmangle) =>
            isUnmangle ? "Unmangle" : "Mangle";

        // file decoding

        private void BtnDecodeCheatAudio_Click(object sender, EventArgs e) =>
            Utils.BatchProcess(
                "Whiplash Cheat Audio (*.KC)|*.KC|All Files (*.*)|*.*",
                "Select Cheat Audio Files",
                "Save RAW files in:",
                inputData => FibCipher.Decode(inputData, 115, 150),
                outputFile => Path.GetFileName(outputFile) + ".RAW",
                "RAW", "decode");

        private void BtnDecodeFatalIni_Click(object sender, EventArgs e) =>
            DecodeIniFile("FATAL.INI", 77, 101);

        private void BtnDecodePasswordIni_Click(object sender, EventArgs e) =>
            DecodeIniFile("PASSWORD.INI", 23, 37);

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

                    Utils.SaveBytes(decodedData,
                        "Whiplash INI Files (*.INI)|*.INI|All Files (*.*)|*.*",
                        $"{Path.GetFileNameWithoutExtension(fileName)}_decoded" +
                            Path.GetExtension(fileName),
                        $"Save Decoded {iniFilename} As");
                }
            }
            catch
            {
                Utils.MsgError();
            }
        }

        // audio tools

        private void BtnConvertRAWAudio_Click(object sender, EventArgs e) =>
            Utils.BatchProcess(
                "Whiplash Raw Audio (*.RAW;*.RBP;*.RFR;*.RGE;*.RSS)|*.RAW;*.RBP;*.RFR;*.RGE;*.RSS|All Files (*.*)|*.*",
                "Select Raw Audio Files",
                "Save WAV files in:",
                inputData => WavAudio.ConvertRawToWav(inputData),
                outputFile => Path.GetFileName(outputFile) + ".WAV",
                "WAV", "convert");

        private void BtnConvertHMPMIDI_Click(object sender, EventArgs e) =>
            Utils.BatchProcess(
                "HMP MIDI Files (*.HMP)|*.HMP|All Files (*.*)|*.*",
                "Select HMP MIDI Files (Original Format)",
                "Save revised HMP files in:",
                inputData => HMPMIDI.ConvertToRevisedFormat(inputData),
                outputFile => Path.GetFileNameWithoutExtension(outputFile) + "_revised.HMP",
                "HMP", "convert");

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
                        if (b > countColors) countColors = b;

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
            string fileName = Path.GetFileNameWithoutExtension(paletteName) + "_palette";
            Utils.SaveBitmap(
                () => Bitmapper.ConvertPaletteToBitmap(paletteData),
                fileName.Replace(Utils.unmangledSuffix, ""),
                "Save As");
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
                Utils.MsgError();
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
                            Utils.MsgError($"Too many colours! ({Convert.ToString(palette.Length)})");
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
                Utils.MsgError();
            }
        }

        private void SavePalette(Color[] palette, string defaultFileName) =>
            Utils.SaveBytes(Bitmapper.GetPaletteArray(palette),
                "Palette Files (*.PAL)|*.PAL|All Files (*.*)|*.*",
                Path.GetFileNameWithoutExtension(defaultFileName),
                "Save Palette As");

        private void BtnSaveNewPalette_Click(object sender, EventArgs e) =>
            SavePalette(newPalette, newBitmapName);

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
                Utils.MsgError();
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
                        Utils.MsgError("Incorrect palette!");
                        return;
                    }
                    Utils.SaveBytes(outputArray,
                        "BM File (*.BM)|*.BM|DRH File (*.DRH)|*.DRH|All Files (*.*)|*.*",
                        Path.GetFileNameWithoutExtension(newBitmapName),
                        "Save Bitmap As");
                }
            }
            catch
            {
                Utils.MsgError();
            }
        }
    }
}