using System.Windows.Forms;

namespace Whiptools
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnExportPalette = new System.Windows.Forms.Button();
            this.lblDimHeight = new System.Windows.Forms.Label();
            this.txtDimWidth = new System.Windows.Forms.TextBox();
            this.txtPalettePath = new System.Windows.Forms.TextBox();
            this.txtBitmapPath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblPaletteLoaded = new System.Windows.Forms.Label();
            this.lblBitmapLoaded = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLoadPalette = new System.Windows.Forms.Button();
            this.btnLoadBitmap = new System.Windows.Forms.Button();
            this.btnViewBitmal = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnAddToPalette = new System.Windows.Forms.Button();
            this.btnSavePalette = new System.Windows.Forms.Button();
            this.lblImageLoaded = new System.Windows.Forms.Label();
            this.txtImagePath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.btnSaveBMFile = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnMangle = new System.Windows.Forms.Button();
            this.btnUnmangle = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnConvertHMPMIDI = new System.Windows.Forms.Button();
            this.btnConvertRAWAudio = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnDecodePasswordIni = new System.Windows.Forms.Button();
            this.btnDecodeFatalIni = new System.Windows.Forms.Button();
            this.btnDecodeCheatAudio = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExportPalette);
            this.groupBox1.Controls.Add(this.lblDimHeight);
            this.groupBox1.Controls.Add(this.txtDimWidth);
            this.groupBox1.Controls.Add(this.txtPalettePath);
            this.groupBox1.Controls.Add(this.txtBitmapPath);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lblPaletteLoaded);
            this.groupBox1.Controls.Add(this.lblBitmapLoaded);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnLoadPalette);
            this.groupBox1.Controls.Add(this.btnLoadBitmap);
            this.groupBox1.Controls.Add(this.btnViewBitmal);
            this.groupBox1.Location = new System.Drawing.Point(12, 240);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(556, 192);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bitmap Viewer";
            // 
            // btnExportPalette
            // 
            this.btnExportPalette.Location = new System.Drawing.Point(441, 95);
            this.btnExportPalette.Name = "btnExportPalette";
            this.btnExportPalette.Size = new System.Drawing.Size(105, 36);
            this.btnExportPalette.TabIndex = 5;
            this.btnExportPalette.Text = "Export Palette";
            this.btnExportPalette.UseVisualStyleBackColor = true;
            this.btnExportPalette.Click += new System.EventHandler(this.BtnExportPalette_Click);
            // 
            // lblDimHeight
            // 
            this.lblDimHeight.AutoSize = true;
            this.lblDimHeight.Location = new System.Drawing.Point(175, 65);
            this.lblDimHeight.Name = "lblDimHeight";
            this.lblDimHeight.Size = new System.Drawing.Size(21, 13);
            this.lblDimHeight.TabIndex = 21;
            this.lblDimHeight.Text = "x ?";
            // 
            // txtDimWidth
            // 
            this.txtDimWidth.Location = new System.Drawing.Point(128, 62);
            this.txtDimWidth.Name = "txtDimWidth";
            this.txtDimWidth.Size = new System.Drawing.Size(41, 20);
            this.txtDimWidth.TabIndex = 2;
            this.txtDimWidth.TextChanged += new System.EventHandler(this.TxtDimWidth_TextChanged);
            // 
            // txtPalettePath
            // 
            this.txtPalettePath.Location = new System.Drawing.Point(64, 95);
            this.txtPalettePath.Name = "txtPalettePath";
            this.txtPalettePath.ReadOnly = true;
            this.txtPalettePath.Size = new System.Drawing.Size(260, 20);
            this.txtPalettePath.TabIndex = 3;
            // 
            // txtBitmapPath
            // 
            this.txtBitmapPath.Location = new System.Drawing.Point(64, 19);
            this.txtBitmapPath.Name = "txtBitmapPath";
            this.txtBitmapPath.ReadOnly = true;
            this.txtBitmapPath.Size = new System.Drawing.Size(260, 20);
            this.txtBitmapPath.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(61, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Dimensions:";
            // 
            // lblPaletteLoaded
            // 
            this.lblPaletteLoaded.AutoSize = true;
            this.lblPaletteLoaded.Location = new System.Drawing.Point(61, 118);
            this.lblPaletteLoaded.Name = "lblPaletteLoaded";
            this.lblPaletteLoaded.Size = new System.Drawing.Size(72, 13);
            this.lblPaletteLoaded.TabIndex = 17;
            this.lblPaletteLoaded.Text = "No file loaded";
            // 
            // lblBitmapLoaded
            // 
            this.lblBitmapLoaded.AutoSize = true;
            this.lblBitmapLoaded.Location = new System.Drawing.Point(61, 42);
            this.lblBitmapLoaded.Name = "lblBitmapLoaded";
            this.lblBitmapLoaded.Size = new System.Drawing.Size(72, 13);
            this.lblBitmapLoaded.TabIndex = 16;
            this.lblBitmapLoaded.Text = "No file loaded";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Palette file:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Bitmap file:";
            // 
            // btnLoadPalette
            // 
            this.btnLoadPalette.Location = new System.Drawing.Point(330, 95);
            this.btnLoadPalette.Name = "btnLoadPalette";
            this.btnLoadPalette.Size = new System.Drawing.Size(106, 36);
            this.btnLoadPalette.TabIndex = 4;
            this.btnLoadPalette.Text = "Load Palette";
            this.btnLoadPalette.UseVisualStyleBackColor = true;
            this.btnLoadPalette.Click += new System.EventHandler(this.BtnLoadPalette_Click);
            // 
            // btnLoadBitmap
            // 
            this.btnLoadBitmap.Location = new System.Drawing.Point(330, 19);
            this.btnLoadBitmap.Name = "btnLoadBitmap";
            this.btnLoadBitmap.Size = new System.Drawing.Size(106, 36);
            this.btnLoadBitmap.TabIndex = 1;
            this.btnLoadBitmap.Text = "Load Bitmap";
            this.btnLoadBitmap.UseVisualStyleBackColor = true;
            this.btnLoadBitmap.Click += new System.EventHandler(this.BtnLoadBitmap_Click);
            // 
            // btnViewBitmal
            // 
            this.btnViewBitmal.Location = new System.Drawing.Point(9, 146);
            this.btnViewBitmal.Name = "btnViewBitmal";
            this.btnViewBitmal.Size = new System.Drawing.Size(537, 36);
            this.btnViewBitmal.TabIndex = 6;
            this.btnViewBitmal.Text = "View Image";
            this.btnViewBitmal.UseVisualStyleBackColor = true;
            this.btnViewBitmal.Click += new System.EventHandler(this.BtnViewBitmap_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnAddToPalette);
            this.groupBox2.Controls.Add(this.btnSavePalette);
            this.groupBox2.Controls.Add(this.lblImageLoaded);
            this.groupBox2.Controls.Add(this.txtImagePath);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.btnLoadImage);
            this.groupBox2.Controls.Add(this.btnSaveBMFile);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox2.Location = new System.Drawing.Point(12, 438);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(556, 156);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bitmap Creator";
            // 
            // btnAddToPalette
            // 
            this.btnAddToPalette.Location = new System.Drawing.Point(440, 59);
            this.btnAddToPalette.Name = "btnAddToPalette";
            this.btnAddToPalette.Size = new System.Drawing.Size(106, 36);
            this.btnAddToPalette.TabIndex = 3;
            this.btnAddToPalette.Text = "Add to Existing";
            this.btnAddToPalette.UseVisualStyleBackColor = true;
            this.btnAddToPalette.Click += new System.EventHandler(this.BtnAddToExistingPalette_Click);
            // 
            // btnSavePalette
            // 
            this.btnSavePalette.Location = new System.Drawing.Point(330, 59);
            this.btnSavePalette.Name = "btnSavePalette";
            this.btnSavePalette.Size = new System.Drawing.Size(106, 36);
            this.btnSavePalette.TabIndex = 2;
            this.btnSavePalette.Text = "Save New Palette";
            this.btnSavePalette.UseVisualStyleBackColor = true;
            this.btnSavePalette.Click += new System.EventHandler(this.BtnSaveNewPalette_Click);
            // 
            // lblImageLoaded
            // 
            this.lblImageLoaded.AutoSize = true;
            this.lblImageLoaded.Location = new System.Drawing.Point(60, 42);
            this.lblImageLoaded.Name = "lblImageLoaded";
            this.lblImageLoaded.Size = new System.Drawing.Size(72, 13);
            this.lblImageLoaded.TabIndex = 26;
            this.lblImageLoaded.Text = "No file loaded";
            // 
            // txtImagePath
            // 
            this.txtImagePath.Location = new System.Drawing.Point(63, 19);
            this.txtImagePath.Name = "txtImagePath";
            this.txtImagePath.ReadOnly = true;
            this.txtImagePath.Size = new System.Drawing.Size(260, 20);
            this.txtImagePath.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Image file:";
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.Location = new System.Drawing.Point(330, 19);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(106, 34);
            this.btnLoadImage.TabIndex = 1;
            this.btnLoadImage.Text = "Load Image";
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.BtnLoadImage_Click);
            // 
            // btnSaveBMFile
            // 
            this.btnSaveBMFile.Location = new System.Drawing.Point(10, 110);
            this.btnSaveBMFile.Name = "btnSaveBMFile";
            this.btnSaveBMFile.Size = new System.Drawing.Size(536, 36);
            this.btnSaveBMFile.TabIndex = 4;
            this.btnSaveBMFile.Text = "Select Palette and Save BM File";
            this.btnSaveBMFile.UseVisualStyleBackColor = true;
            this.btnSaveBMFile.Click += new System.EventHandler(this.BtnSaveBMFile_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnMangle);
            this.groupBox3.Controls.Add(this.btnUnmangle);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(557, 70);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "File Mangling";
            // 
            // btnMangle
            // 
            this.btnMangle.Location = new System.Drawing.Point(282, 19);
            this.btnMangle.Name = "btnMangle";
            this.btnMangle.Size = new System.Drawing.Size(264, 36);
            this.btnMangle.TabIndex = 1;
            this.btnMangle.Text = "Mangle Files";
            this.btnMangle.UseVisualStyleBackColor = true;
            this.btnMangle.Click += new System.EventHandler(this.BtnMangleFiles_Click);
            // 
            // btnUnmangle
            // 
            this.btnUnmangle.Location = new System.Drawing.Point(10, 19);
            this.btnUnmangle.Name = "btnUnmangle";
            this.btnUnmangle.Size = new System.Drawing.Size(264, 36);
            this.btnUnmangle.TabIndex = 0;
            this.btnUnmangle.Text = "Unmangle Files";
            this.btnUnmangle.UseVisualStyleBackColor = true;
            this.btnUnmangle.Click += new System.EventHandler(this.BtnUnmangleFiles_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.btnConvertHMPMIDI);
            this.groupBox5.Controls.Add(this.btnConvertRAWAudio);
            this.groupBox5.Location = new System.Drawing.Point(12, 164);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(556, 70);
            this.groupBox5.TabIndex = 2;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Audio Tools";
            // 
            // btnConvertHMPMIDI
            // 
            this.btnConvertHMPMIDI.Location = new System.Drawing.Point(282, 19);
            this.btnConvertHMPMIDI.Name = "btnConvertHMPMIDI";
            this.btnConvertHMPMIDI.Size = new System.Drawing.Size(264, 36);
            this.btnConvertHMPMIDI.TabIndex = 1;
            this.btnConvertHMPMIDI.Text = "Convert HMP MIDI Format";
            this.btnConvertHMPMIDI.UseVisualStyleBackColor = true;
            this.btnConvertHMPMIDI.Click += new System.EventHandler(this.BtnConvertHMPMIDI_Click);
            // 
            // btnConvertRAWAudio
            // 
            this.btnConvertRAWAudio.Location = new System.Drawing.Point(10, 19);
            this.btnConvertRAWAudio.Name = "btnConvertRAWAudio";
            this.btnConvertRAWAudio.Size = new System.Drawing.Size(264, 36);
            this.btnConvertRAWAudio.TabIndex = 0;
            this.btnConvertRAWAudio.Text = "Convert Raw Audio to WAV";
            this.btnConvertRAWAudio.UseVisualStyleBackColor = true;
            this.btnConvertRAWAudio.Click += new System.EventHandler(this.BtnConvertRAWAudio_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnDecodePasswordIni);
            this.groupBox4.Controls.Add(this.btnDecodeFatalIni);
            this.groupBox4.Controls.Add(this.btnDecodeCheatAudio);
            this.groupBox4.Location = new System.Drawing.Point(12, 88);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(557, 70);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "File Decoding";
            // 
            // btnDecodePasswordIni
            // 
            this.btnDecodePasswordIni.Location = new System.Drawing.Point(372, 19);
            this.btnDecodePasswordIni.Name = "btnDecodePasswordIni";
            this.btnDecodePasswordIni.Size = new System.Drawing.Size(174, 36);
            this.btnDecodePasswordIni.TabIndex = 2;
            this.btnDecodePasswordIni.Text = "Decode PASSWORD.INI";
            this.btnDecodePasswordIni.UseVisualStyleBackColor = true;
            this.btnDecodePasswordIni.Click += new System.EventHandler(this.BtnDecodePasswordIni_Click);
            // 
            // btnDecodeFatalIni
            // 
            this.btnDecodeFatalIni.Location = new System.Drawing.Point(191, 19);
            this.btnDecodeFatalIni.Name = "btnDecodeFatalIni";
            this.btnDecodeFatalIni.Size = new System.Drawing.Size(173, 36);
            this.btnDecodeFatalIni.TabIndex = 1;
            this.btnDecodeFatalIni.Text = "Decode FATAL.INI";
            this.btnDecodeFatalIni.UseVisualStyleBackColor = true;
            this.btnDecodeFatalIni.Click += new System.EventHandler(this.BtnDecodeFatalIni_Click);
            // 
            // btnDecodeCheatAudio
            // 
            this.btnDecodeCheatAudio.Location = new System.Drawing.Point(9, 19);
            this.btnDecodeCheatAudio.Name = "btnDecodeCheatAudio";
            this.btnDecodeCheatAudio.Size = new System.Drawing.Size(174, 36);
            this.btnDecodeCheatAudio.TabIndex = 0;
            this.btnDecodeCheatAudio.Text = "Decode Cheat Audio";
            this.btnDecodeCheatAudio.UseVisualStyleBackColor = true;
            this.btnDecodeCheatAudio.Click += new System.EventHandler(this.BtnDecodeCheatAudio_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 606);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Whiptools";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblDimHeight;
        private System.Windows.Forms.TextBox txtDimWidth;
        private System.Windows.Forms.TextBox txtPalettePath;
        private System.Windows.Forms.TextBox txtBitmapPath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblPaletteLoaded;
        private System.Windows.Forms.Label lblBitmapLoaded;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLoadPalette;
        private System.Windows.Forms.Button btnLoadBitmap;
        private System.Windows.Forms.Button btnViewBitmal;
        private System.Windows.Forms.Button btnExportPalette;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSaveBMFile;
        private System.Windows.Forms.TextBox txtImagePath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Label lblImageLoaded;
        private System.Windows.Forms.Button btnSavePalette;
        private System.Windows.Forms.Button btnAddToPalette;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnUnmangle;
        private GroupBox groupBox5;
        private Button btnConvertRAWAudio;
        private GroupBox groupBox4;
        private Button btnDecodePasswordIni;
        private Button btnDecodeFatalIni;
        private Button btnDecodeCheatAudio;
        private Button btnMangle;
        private Button btnConvertHMPMIDI;
    }
}