namespace Modbus.Common
{
    partial class DataTab
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.labelTxtSize = new System.Windows.Forms.Label();
            this.txtSize = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtStartAdress = new System.Windows.Forms.TextBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.groupBoxData = new System.Windows.Forms.GroupBox();
            this.paneFloatEncoding = new System.Windows.Forms.GroupBox();
            this.chkSwapBuffer = new System.Windows.Forms.CheckBox();
            this.radioFloatLE = new System.Windows.Forms.RadioButton();
            this.radioFloatBE = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.paneFloatEncoding.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonApply);
            this.groupBox1.Controls.Add(this.labelTxtSize);
            this.groupBox1.Controls.Add(this.txtSize);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.txtStartAdress);
            this.groupBox1.Controls.Add(this.buttonClear);
            this.groupBox1.Controls.Add(this.groupBoxData);
            this.groupBox1.Controls.Add(this.paneFloatEncoding);
            this.groupBox1.Location = new System.Drawing.Point(1, 1);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox1.Size = new System.Drawing.Size(848, 386);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(642, 11);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(86, 28);
            this.buttonApply.TabIndex = 36;
            this.buttonApply.Text = "Apply";
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // labelTxtSize
            // 
            this.labelTxtSize.Location = new System.Drawing.Point(163, 19);
            this.labelTxtSize.Name = "labelTxtSize";
            this.labelTxtSize.Size = new System.Drawing.Size(42, 14);
            this.labelTxtSize.TabIndex = 35;
            this.labelTxtSize.Text = "Size";
            // 
            // txtSize
            // 
            this.txtSize.Location = new System.Drawing.Point(211, 16);
            this.txtSize.MaxLength = 5;
            this.txtSize.Name = "txtSize";
            this.txtSize.Size = new System.Drawing.Size(40, 20);
            this.txtSize.TabIndex = 34;
            this.txtSize.Text = "64";
            this.txtSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSize.TextChanged += new System.EventHandler(this.txtSize_TextChanged);
            this.txtSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.onRegCountKeyPress);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(6, 19);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(74, 14);
            this.label11.TabIndex = 27;
            this.label11.Text = "Start Address";
            // 
            // txtStartAdress
            // 
            this.txtStartAdress.Location = new System.Drawing.Point(86, 16);
            this.txtStartAdress.Name = "txtStartAdress";
            this.txtStartAdress.Size = new System.Drawing.Size(54, 20);
            this.txtStartAdress.TabIndex = 26;
            this.txtStartAdress.Text = "4100";
            this.txtStartAdress.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(740, 11);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(86, 28);
            this.buttonClear.TabIndex = 25;
            this.buttonClear.Text = "Clear";
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // groupBoxData
            // 
            this.groupBoxData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxData.Location = new System.Drawing.Point(0, 44);
            this.groupBoxData.Margin = new System.Windows.Forms.Padding(1);
            this.groupBoxData.Name = "groupBoxData";
            this.groupBoxData.Size = new System.Drawing.Size(848, 336);
            this.groupBoxData.TabIndex = 17;
            this.groupBoxData.TabStop = false;
            // 
            // paneFloatEncoding
            // 
            this.paneFloatEncoding.Controls.Add(this.chkSwapBuffer);
            this.paneFloatEncoding.Controls.Add(this.radioFloatLE);
            this.paneFloatEncoding.Controls.Add(this.radioFloatBE);
            this.paneFloatEncoding.Location = new System.Drawing.Point(402, 8);
            this.paneFloatEncoding.Name = "paneFloatEncoding";
            this.paneFloatEncoding.Size = new System.Drawing.Size(197, 37);
            this.paneFloatEncoding.TabIndex = 3;
            this.paneFloatEncoding.TabStop = false;
            this.paneFloatEncoding.Text = "Float encoding";
            // 
            // chkSwapBuffer
            // 
            this.chkSwapBuffer.AutoSize = true;
            this.chkSwapBuffer.Checked = true;
            this.chkSwapBuffer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSwapBuffer.Location = new System.Drawing.Point(104, 17);
            this.chkSwapBuffer.Name = "chkSwapBuffer";
            this.chkSwapBuffer.Size = new System.Drawing.Size(83, 17);
            this.chkSwapBuffer.TabIndex = 3;
            this.chkSwapBuffer.Text = "Swap buffer";
            this.chkSwapBuffer.UseVisualStyleBackColor = true;
            // 
            // radioFloatLE
            // 
            this.radioFloatLE.AutoSize = true;
            this.radioFloatLE.Location = new System.Drawing.Point(47, 17);
            this.radioFloatLE.Name = "radioFloatLE";
            this.radioFloatLE.Size = new System.Drawing.Size(38, 17);
            this.radioFloatLE.TabIndex = 2;
            this.radioFloatLE.Text = "LE";
            this.radioFloatLE.UseVisualStyleBackColor = true;
            this.radioFloatLE.CheckedChanged += new System.EventHandler(this.onFloatLEChanged);
            // 
            // radioFloatBE
            // 
            this.radioFloatBE.AutoSize = true;
            this.radioFloatBE.Location = new System.Drawing.Point(8, 17);
            this.radioFloatBE.Name = "radioFloatBE";
            this.radioFloatBE.Size = new System.Drawing.Size(39, 17);
            this.radioFloatBE.TabIndex = 1;
            this.radioFloatBE.Text = "BE";
            this.radioFloatBE.UseVisualStyleBackColor = true;
            this.radioFloatBE.CheckedChanged += new System.EventHandler(this.onFloatBEChanged);
            // 
            // DataTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DataTab";
            this.Size = new System.Drawing.Size(850, 395);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.paneFloatEncoding.ResumeLayout(false);
            this.paneFloatEncoding.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.Button buttonApply;
        protected System.Windows.Forms.Label labelTxtSize;
        protected System.Windows.Forms.TextBox txtSize;
        protected System.Windows.Forms.Label label11;
        protected System.Windows.Forms.TextBox txtStartAdress;
        protected System.Windows.Forms.Button buttonClear;
        protected System.Windows.Forms.GroupBox groupBoxData;

        private System.Windows.Forms.GroupBox paneFloatEncoding;
        private System.Windows.Forms.RadioButton radioFloatLE;
        private System.Windows.Forms.RadioButton radioFloatBE;
        private System.Windows.Forms.CheckBox chkSwapBuffer;
    }
}
