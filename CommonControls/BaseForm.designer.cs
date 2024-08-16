﻿namespace Modbus.Common
{
    partial class BaseForm
    {
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
        protected void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Modbus.Common.DataTab dataTab1;
            Modbus.Common.DataTab dataTab2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseForm));
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnContact = new System.Windows.Forms.Button();
            this.txtAppVersion = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonPauseLog = new System.Windows.Forms.Button();
            this.listBoxCommLog = new System.Windows.Forms.ListBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSlaveDelay = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxSlaveID = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkSwapFloatEndian = new System.Windows.Forms.CheckBox();
            this.radioButtonLED = new System.Windows.Forms.RadioButton();
            this.radioBtnFloat32 = new System.Windows.Forms.RadioButton();
            this.radioButtonInteger = new System.Windows.Forms.RadioButton();
            this.radioButtonHex = new System.Windows.Forms.RadioButton();
            this.radioButtonBinary = new System.Windows.Forms.RadioButton();
            this.buttonImport = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.grpStart = new System.Windows.Forms.GroupBox();
            this.btnLoadCom = new System.Windows.Forms.Button();
            this.groupBoxRTU = new System.Windows.Forms.GroupBox();
            this.comboBoxStopBits = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBoxDataBits = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBoxParity = new System.Windows.Forms.ComboBox();
            this.labelParity = new System.Windows.Forms.Label();
            this.comboBoxBaudRate = new System.Windows.Forms.ComboBox();
            this.comboBoxSerialPorts = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBoxMode = new System.Windows.Forms.GroupBox();
            this.radioButtonRTU = new System.Windows.Forms.RadioButton();
            this.radioButtonUDP = new System.Windows.Forms.RadioButton();
            this.radioButtonTCP = new System.Windows.Forms.RadioButton();
            this.groupBoxTCP = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.btnSaveCom = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.grpExchange = new System.Windows.Forms.GroupBox();
            this.donate = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1 = new Modbus.Common.TabControlEx();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            dataTab1 = new Modbus.Common.DataTab();
            dataTab2 = new Modbus.Common.DataTab();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.grpStart.SuspendLayout();
            this.groupBoxRTU.SuspendLayout();
            this.groupBoxMode.SuspendLayout();
            this.groupBoxTCP.SuspendLayout();
            this.grpExchange.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnContact);
            this.groupBox4.Controls.Add(this.txtAppVersion);
            this.groupBox4.Controls.Add(this.buttonSave);
            this.groupBox4.Controls.Add(this.buttonPauseLog);
            this.groupBox4.Controls.Add(this.listBoxCommLog);
            this.groupBox4.Controls.Add(this.buttonClear);
            this.groupBox4.Location = new System.Drawing.Point(7, 699);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(859, 194);
            this.groupBox4.TabIndex = 20;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Communication Log";
            // 
            // btnContact
            // 
            this.btnContact.Image = global::Modbus.Common.Properties.Resources.share32;
            this.btnContact.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnContact.Location = new System.Drawing.Point(222, 14);
            this.btnContact.Name = "btnContact";
            this.btnContact.Size = new System.Drawing.Size(62, 34);
            this.btnContact.TabIndex = 28;
            this.btnContact.Text = "Info";
            this.btnContact.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.toolTip.SetToolTip(this.btnContact, "Follow on GitHub");
            this.btnContact.UseVisualStyleBackColor = true;
            this.btnContact.Click += new System.EventHandler(this.onBtnContactClick);
            this.btnContact.MouseLeave += new System.EventHandler(this.onBtnContactLeave);
            this.btnContact.MouseHover += new System.EventHandler(this.onBtnContactHover);
            // 
            // txtAppVersion
            // 
            this.txtAppVersion.AutoSize = true;
            this.txtAppVersion.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.txtAppVersion.Location = new System.Drawing.Point(10, 27);
            this.txtAppVersion.Name = "txtAppVersion";
            this.txtAppVersion.Size = new System.Drawing.Size(70, 13);
            this.txtAppVersion.TabIndex = 27;
            this.txtAppVersion.Text = "{app-version}";
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(760, 19);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(86, 28);
            this.buttonSave.TabIndex = 26;
            this.buttonSave.Text = "Save";
            this.buttonSave.Click += new System.EventHandler(this.buttonSaveLog_ClickAsync);
            // 
            // buttonPauseLog
            // 
            this.buttonPauseLog.Location = new System.Drawing.Point(576, 19);
            this.buttonPauseLog.Name = "buttonPauseLog";
            this.buttonPauseLog.Size = new System.Drawing.Size(86, 28);
            this.buttonPauseLog.TabIndex = 25;
            this.buttonPauseLog.Text = "Pause";
            this.buttonPauseLog.Click += new System.EventHandler(this.buttonPauseLog_Click);
            // 
            // listBoxCommLog
            // 
            this.listBoxCommLog.BackColor = System.Drawing.Color.Black;
            this.listBoxCommLog.ForeColor = System.Drawing.Color.LimeGreen;
            this.listBoxCommLog.FormattingEnabled = true;
            this.listBoxCommLog.HorizontalScrollbar = true;
            this.listBoxCommLog.Location = new System.Drawing.Point(3, 54);
            this.listBoxCommLog.Name = "listBoxCommLog";
            this.listBoxCommLog.Size = new System.Drawing.Size(847, 134);
            this.listBoxCommLog.TabIndex = 3;
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(668, 19);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(86, 28);
            this.buttonClear.TabIndex = 24;
            this.buttonClear.Text = "Clear";
            this.buttonClear.Click += new System.EventHandler(this.ButtonClearLogClick);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(226, 239);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 14);
            this.label1.TabIndex = 30;
            this.label1.Text = "Slave delay (ms)";
            // 
            // textBoxSlaveDelay
            // 
            this.textBoxSlaveDelay.Location = new System.Drawing.Point(318, 233);
            this.textBoxSlaveDelay.Name = "textBoxSlaveDelay";
            this.textBoxSlaveDelay.Size = new System.Drawing.Size(40, 20);
            this.textBoxSlaveDelay.TabIndex = 29;
            this.textBoxSlaveDelay.Text = "1";
            this.textBoxSlaveDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(601, 19);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 16);
            this.label7.TabIndex = 28;
            this.label7.Text = "Slave ID";
            // 
            // textBoxSlaveID
            // 
            this.textBoxSlaveID.Location = new System.Drawing.Point(649, 15);
            this.textBoxSlaveID.Name = "textBoxSlaveID";
            this.textBoxSlaveID.Size = new System.Drawing.Size(35, 20);
            this.textBoxSlaveID.TabIndex = 27;
            this.textBoxSlaveID.Text = "1";
            this.textBoxSlaveID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxSlaveID.Validated += new System.EventHandler(this.onSlaveIDValidated);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkSwapFloatEndian);
            this.groupBox3.Controls.Add(this.radioButtonLED);
            this.groupBox3.Controls.Add(this.radioBtnFloat32);
            this.groupBox3.Controls.Add(this.radioButtonInteger);
            this.groupBox3.Controls.Add(this.radioButtonHex);
            this.groupBox3.Controls.Add(this.radioButtonBinary);
            this.groupBox3.Location = new System.Drawing.Point(7, 144);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(198, 110);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Display Format";
            // 
            // checkSwapFloatEndian
            // 
            this.checkSwapFloatEndian.AutoSize = true;
            this.checkSwapFloatEndian.Enabled = false;
            this.checkSwapFloatEndian.Location = new System.Drawing.Point(80, 68);
            this.checkSwapFloatEndian.Name = "checkSwapFloatEndian";
            this.checkSwapFloatEndian.Size = new System.Drawing.Size(88, 17);
            this.checkSwapFloatEndian.TabIndex = 6;
            this.checkSwapFloatEndian.Text = "Swap endian";
            this.checkSwapFloatEndian.UseVisualStyleBackColor = true;
            this.checkSwapFloatEndian.CheckedChanged += new System.EventHandler(this.onChangeSwapFloatEndian);
            // 
            // radioButtonLED
            // 
            this.radioButtonLED.Location = new System.Drawing.Point(10, 19);
            this.radioButtonLED.Name = "radioButtonLED";
            this.radioButtonLED.Size = new System.Drawing.Size(67, 21);
            this.radioButtonLED.TabIndex = 1;
            this.radioButtonLED.Tag = "LED";
            this.radioButtonLED.Text = "LED";
            this.radioButtonLED.Click += new System.EventHandler(this.RadioButtonDisplayFormatCheckedChanged);
            // 
            // radioBtnFloat32
            // 
            this.radioBtnFloat32.Location = new System.Drawing.Point(10, 65);
            this.radioBtnFloat32.Name = "radioBtnFloat32";
            this.radioBtnFloat32.Size = new System.Drawing.Size(62, 21);
            this.radioBtnFloat32.TabIndex = 5;
            this.radioBtnFloat32.Tag = "Float32";
            this.radioBtnFloat32.Text = "Float32";
            this.radioBtnFloat32.Click += new System.EventHandler(this.RadioButtonDisplayFormatCheckedChanged);
            // 
            // radioButtonInteger
            // 
            this.radioButtonInteger.Checked = true;
            this.radioButtonInteger.Location = new System.Drawing.Point(80, 19);
            this.radioButtonInteger.Name = "radioButtonInteger";
            this.radioButtonInteger.Size = new System.Drawing.Size(67, 21);
            this.radioButtonInteger.TabIndex = 4;
            this.radioButtonInteger.TabStop = true;
            this.radioButtonInteger.Tag = "Integer";
            this.radioButtonInteger.Text = "Integer";
            this.radioButtonInteger.Click += new System.EventHandler(this.RadioButtonDisplayFormatCheckedChanged);
            // 
            // radioButtonHex
            // 
            this.radioButtonHex.Location = new System.Drawing.Point(80, 42);
            this.radioButtonHex.Name = "radioButtonHex";
            this.radioButtonHex.Size = new System.Drawing.Size(67, 20);
            this.radioButtonHex.TabIndex = 3;
            this.radioButtonHex.Tag = "Hex";
            this.radioButtonHex.Text = "Hexa";
            this.radioButtonHex.Click += new System.EventHandler(this.RadioButtonDisplayFormatCheckedChanged);
            // 
            // radioButtonBinary
            // 
            this.radioButtonBinary.Location = new System.Drawing.Point(10, 42);
            this.radioButtonBinary.Name = "radioButtonBinary";
            this.radioButtonBinary.Size = new System.Drawing.Size(61, 21);
            this.radioButtonBinary.TabIndex = 2;
            this.radioButtonBinary.Tag = "Binary";
            this.radioButtonBinary.Text = "Binary";
            this.radioButtonBinary.Click += new System.EventHandler(this.RadioButtonDisplayFormatCheckedChanged);
            // 
            // buttonImport
            // 
            this.buttonImport.Location = new System.Drawing.Point(39, 42);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(86, 28);
            this.buttonImport.TabIndex = 26;
            this.buttonImport.Text = "Import";
            this.buttonImport.Click += new System.EventHandler(this.ButtonImportClick);
            // 
            // buttonExport
            // 
            this.buttonExport.Location = new System.Drawing.Point(39, 76);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(86, 28);
            this.buttonExport.TabIndex = 25;
            this.buttonExport.Text = "Export";
            this.buttonExport.Click += new System.EventHandler(this.ButtonExportClick);
            // 
            // grpStart
            // 
            this.grpStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpStart.Controls.Add(this.btnLoadCom);
            this.grpStart.Controls.Add(this.groupBoxRTU);
            this.grpStart.Controls.Add(this.groupBoxMode);
            this.grpStart.Controls.Add(this.groupBoxTCP);
            this.grpStart.Controls.Add(this.btnSaveCom);
            this.grpStart.Controls.Add(this.textBoxSlaveID);
            this.grpStart.Controls.Add(this.label7);
            this.grpStart.Location = new System.Drawing.Point(7, 12);
            this.grpStart.Name = "grpStart";
            this.grpStart.Size = new System.Drawing.Size(692, 126);
            this.grpStart.TabIndex = 18;
            this.grpStart.TabStop = false;
            this.grpStart.Text = "Communication";
            // 
            // btnLoadCom
            // 
            this.btnLoadCom.Location = new System.Drawing.Point(604, 91);
            this.btnLoadCom.Name = "btnLoadCom";
            this.btnLoadCom.Size = new System.Drawing.Size(75, 23);
            this.btnLoadCom.TabIndex = 32;
            this.btnLoadCom.Text = "Load";
            this.btnLoadCom.UseVisualStyleBackColor = true;
            this.btnLoadCom.Click += new System.EventHandler(this.btnLoadComClick);
            // 
            // groupBoxRTU
            // 
            this.groupBoxRTU.Controls.Add(this.comboBoxStopBits);
            this.groupBoxRTU.Controls.Add(this.label10);
            this.groupBoxRTU.Controls.Add(this.comboBoxDataBits);
            this.groupBoxRTU.Controls.Add(this.label9);
            this.groupBoxRTU.Controls.Add(this.comboBoxParity);
            this.groupBoxRTU.Controls.Add(this.labelParity);
            this.groupBoxRTU.Controls.Add(this.comboBoxBaudRate);
            this.groupBoxRTU.Controls.Add(this.comboBoxSerialPorts);
            this.groupBoxRTU.Controls.Add(this.label4);
            this.groupBoxRTU.Controls.Add(this.label5);
            this.groupBoxRTU.Enabled = false;
            this.groupBoxRTU.Location = new System.Drawing.Point(269, 13);
            this.groupBoxRTU.Name = "groupBoxRTU";
            this.groupBoxRTU.Size = new System.Drawing.Size(325, 106);
            this.groupBoxRTU.TabIndex = 25;
            this.groupBoxRTU.TabStop = false;
            this.groupBoxRTU.Text = "RTU";
            // 
            // comboBoxStopBits
            // 
            this.comboBoxStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStopBits.FormattingEnabled = true;
            this.comboBoxStopBits.Items.AddRange(new object[] {
            "None",
            "1 Bit",
            "1.5 Bits",
            "2 Bits"});
            this.comboBoxStopBits.Location = new System.Drawing.Point(221, 48);
            this.comboBoxStopBits.Name = "comboBoxStopBits";
            this.comboBoxStopBits.Size = new System.Drawing.Size(94, 21);
            this.comboBoxStopBits.TabIndex = 27;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(169, 52);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(49, 13);
            this.label10.TabIndex = 26;
            this.label10.Text = "Stop Bits";
            // 
            // comboBoxDataBits
            // 
            this.comboBoxDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDataBits.FormattingEnabled = true;
            this.comboBoxDataBits.Items.AddRange(new object[] {
            "7 Bits",
            "8 Bits"});
            this.comboBoxDataBits.Location = new System.Drawing.Point(221, 21);
            this.comboBoxDataBits.Name = "comboBoxDataBits";
            this.comboBoxDataBits.Size = new System.Drawing.Size(94, 21);
            this.comboBoxDataBits.TabIndex = 25;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(169, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Data Bits";
            // 
            // comboBoxParity
            // 
            this.comboBoxParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxParity.FormattingEnabled = true;
            this.comboBoxParity.Items.AddRange(new object[] {
            "None"});
            this.comboBoxParity.Location = new System.Drawing.Point(66, 74);
            this.comboBoxParity.Name = "comboBoxParity";
            this.comboBoxParity.Size = new System.Drawing.Size(94, 21);
            this.comboBoxParity.TabIndex = 23;
            // 
            // labelParity
            // 
            this.labelParity.AutoSize = true;
            this.labelParity.Location = new System.Drawing.Point(31, 78);
            this.labelParity.Name = "labelParity";
            this.labelParity.Size = new System.Drawing.Size(33, 13);
            this.labelParity.TabIndex = 22;
            this.labelParity.Text = "Parity";
            // 
            // comboBoxBaudRate
            // 
            this.comboBoxBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBaudRate.FormattingEnabled = true;
            this.comboBoxBaudRate.Items.AddRange(new object[] {
            "115200",
            "57600",
            "38400",
            "19200",
            "14400",
            "9600",
            "7200",
            "4800",
            "2400",
            "1800",
            "1200",
            "600",
            "300",
            "150"});
            this.comboBoxBaudRate.Location = new System.Drawing.Point(66, 47);
            this.comboBoxBaudRate.Name = "comboBoxBaudRate";
            this.comboBoxBaudRate.Size = new System.Drawing.Size(94, 21);
            this.comboBoxBaudRate.TabIndex = 21;
            // 
            // comboBoxSerialPorts
            // 
            this.comboBoxSerialPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialPorts.FormattingEnabled = true;
            this.comboBoxSerialPorts.Items.AddRange(new object[] {
            "None"});
            this.comboBoxSerialPorts.Location = new System.Drawing.Point(66, 19);
            this.comboBoxSerialPorts.Name = "comboBoxSerialPorts";
            this.comboBoxSerialPorts.Size = new System.Drawing.Size(94, 21);
            this.comboBoxSerialPorts.TabIndex = 0;
            this.comboBoxSerialPorts.SelectionChangeCommitted += new System.EventHandler(this.onSerialPortsTextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Port Name";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(31, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Baud";
            // 
            // groupBoxMode
            // 
            this.groupBoxMode.Controls.Add(this.radioButtonRTU);
            this.groupBoxMode.Controls.Add(this.radioButtonUDP);
            this.groupBoxMode.Controls.Add(this.radioButtonTCP);
            this.groupBoxMode.Location = new System.Drawing.Point(6, 19);
            this.groupBoxMode.Name = "groupBoxMode";
            this.groupBoxMode.Size = new System.Drawing.Size(74, 100);
            this.groupBoxMode.TabIndex = 0;
            this.groupBoxMode.TabStop = false;
            this.groupBoxMode.Text = "Mode";
            // 
            // radioButtonRTU
            // 
            this.radioButtonRTU.AutoSize = true;
            this.radioButtonRTU.Location = new System.Drawing.Point(6, 59);
            this.radioButtonRTU.Name = "radioButtonRTU";
            this.radioButtonRTU.Size = new System.Drawing.Size(48, 17);
            this.radioButtonRTU.TabIndex = 3;
            this.radioButtonRTU.Text = "RTU";
            this.radioButtonRTU.UseVisualStyleBackColor = true;
            this.radioButtonRTU.CheckedChanged += new System.EventHandler(this.RadioButtonModeChanged);
            // 
            // radioButtonUDP
            // 
            this.radioButtonUDP.AutoSize = true;
            this.radioButtonUDP.Location = new System.Drawing.Point(6, 39);
            this.radioButtonUDP.Name = "radioButtonUDP";
            this.radioButtonUDP.Size = new System.Drawing.Size(48, 17);
            this.radioButtonUDP.TabIndex = 2;
            this.radioButtonUDP.Text = "UDP";
            this.radioButtonUDP.UseVisualStyleBackColor = true;
            this.radioButtonUDP.CheckedChanged += new System.EventHandler(this.RadioButtonModeChanged);
            // 
            // radioButtonTCP
            // 
            this.radioButtonTCP.AutoSize = true;
            this.radioButtonTCP.Checked = true;
            this.radioButtonTCP.Location = new System.Drawing.Point(6, 19);
            this.radioButtonTCP.Name = "radioButtonTCP";
            this.radioButtonTCP.Size = new System.Drawing.Size(46, 17);
            this.radioButtonTCP.TabIndex = 1;
            this.radioButtonTCP.TabStop = true;
            this.radioButtonTCP.Text = "TCP";
            this.radioButtonTCP.UseVisualStyleBackColor = true;
            this.radioButtonTCP.CheckedChanged += new System.EventHandler(this.RadioButtonModeChanged);
            // 
            // groupBoxTCP
            // 
            this.groupBoxTCP.Controls.Add(this.label8);
            this.groupBoxTCP.Controls.Add(this.txtIP);
            this.groupBoxTCP.Controls.Add(this.label6);
            this.groupBoxTCP.Controls.Add(this.textBoxPort);
            this.groupBoxTCP.Location = new System.Drawing.Point(86, 13);
            this.groupBoxTCP.Name = "groupBoxTCP";
            this.groupBoxTCP.Size = new System.Drawing.Size(177, 106);
            this.groupBoxTCP.TabIndex = 0;
            this.groupBoxTCP.TabStop = false;
            this.groupBoxTCP.Text = "TCP";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(5, 50);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 14);
            this.label8.TabIndex = 11;
            this.label8.Text = "IP Address";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(69, 47);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(97, 20);
            this.txtIP.TabIndex = 10;
            this.txtIP.Text = "127.0.0.1";
            this.txtIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtIP.Validated += new System.EventHandler(this.onAddressValidated);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(5, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 14);
            this.label6.TabIndex = 9;
            this.label6.Text = "Port";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(69, 19);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(44, 20);
            this.textBoxPort.TabIndex = 8;
            this.textBoxPort.Text = "502";
            this.textBoxPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxPort.Validated += new System.EventHandler(this.onIPPortValidated);
            // 
            // btnSaveCom
            // 
            this.btnSaveCom.Location = new System.Drawing.Point(604, 52);
            this.btnSaveCom.Name = "btnSaveCom";
            this.btnSaveCom.Size = new System.Drawing.Size(75, 23);
            this.btnSaveCom.TabIndex = 31;
            this.btnSaveCom.Text = "Save";
            this.btnSaveCom.UseVisualStyleBackColor = true;
            this.btnSaveCom.Click += new System.EventHandler(this.btnSaveComClick);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // grpExchange
            // 
            this.grpExchange.Controls.Add(this.buttonImport);
            this.grpExchange.Controls.Add(this.buttonExport);
            this.grpExchange.Location = new System.Drawing.Point(701, 144);
            this.grpExchange.Name = "grpExchange";
            this.grpExchange.Size = new System.Drawing.Size(159, 110);
            this.grpExchange.TabIndex = 36;
            this.grpExchange.TabStop = false;
            this.grpExchange.Text = "Data table";
            // 
            // donate
            // 
            this.donate.Image = global::Modbus.Common.Properties.Resources.forked;
            this.donate.Location = new System.Drawing.Point(701, 82);
            this.donate.Margin = new System.Windows.Forms.Padding(0);
            this.donate.Name = "donate";
            this.donate.Size = new System.Drawing.Size(167, 54);
            this.donate.TabIndex = 37;
            this.donate.UseVisualStyleBackColor = true;
            this.donate.Click += new System.EventHandler(this.donate_Click);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 0;
            this.toolTip.ReshowDelay = 100;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl1.Location = new System.Drawing.Point(5, 260);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(10, 3);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(862, 437);
            this.tabControl1.TabIndex = 35;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(dataTab1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(854, 411);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Address1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataTab1
            // 
            dataTab1.DataLength = ((ushort)(125));
            dataTab1.DisplayFormat = Modbus.Common.DisplayFormat.Integer;
            dataTab1.Location = new System.Drawing.Point(3, 3);
            dataTab1.Margin = new System.Windows.Forms.Padding(0);
            dataTab1.Name = "dataTab1";
            dataTab1.RegisterData = new ushort[0];
            dataTab1.ShowDataLength = false;
            dataTab1.Size = new System.Drawing.Size(842, 406);
            dataTab1.StartAddress = 0;
            dataTab1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(dataTab2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(852, 411);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "...";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataTab2
            // 
            dataTab2.DataLength = ((ushort)(125));
            dataTab2.DisplayFormat = Modbus.Common.DisplayFormat.Integer;
            dataTab2.Location = new System.Drawing.Point(3, 3);
            dataTab2.Margin = new System.Windows.Forms.Padding(0);
            dataTab2.Name = "dataTab2";
            dataTab2.RegisterData = new ushort[] {
        ((ushort)(0))};
            dataTab2.ShowDataLength = false;
            dataTab2.Size = new System.Drawing.Size(839, 406);
            dataTab2.StartAddress = 0;
            dataTab2.TabIndex = 0;
            // 
            // BaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 901);
            this.Controls.Add(this.donate);
            this.Controls.Add(this.grpExchange);
            this.Controls.Add(this.textBoxSlaveDelay);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.grpStart);
            this.Controls.Add(this.groupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BaseForm";
            this.Text = "{Modbus window}";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BaseFormClosing);
            this.Load += new System.EventHandler(this.BaseFormLoading);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.grpStart.ResumeLayout(false);
            this.grpStart.PerformLayout();
            this.groupBoxRTU.ResumeLayout(false);
            this.groupBoxRTU.PerformLayout();
            this.groupBoxMode.ResumeLayout(false);
            this.groupBoxMode.PerformLayout();
            this.groupBoxTCP.ResumeLayout(false);
            this.groupBoxTCP.PerformLayout();
            this.grpExchange.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.GroupBox groupBox4;
        protected System.Windows.Forms.ListBox listBoxCommLog;
        protected System.Windows.Forms.Label label7;
        protected System.Windows.Forms.TextBox textBoxSlaveID;
        protected System.Windows.Forms.GroupBox groupBox3;
        protected System.Windows.Forms.RadioButton radioButtonLED;
        protected System.Windows.Forms.RadioButton radioButtonInteger;
        protected System.Windows.Forms.RadioButton radioButtonHex;
        protected System.Windows.Forms.RadioButton radioButtonBinary;
        protected System.Windows.Forms.Button buttonClear;
        protected System.Windows.Forms.Button buttonImport;
        protected System.Windows.Forms.Button buttonExport;
        protected System.Windows.Forms.GroupBox grpStart;
        protected System.Windows.Forms.GroupBox groupBoxRTU;
        protected System.Windows.Forms.ComboBox comboBoxSerialPorts;
        protected System.Windows.Forms.Label label4;
        protected System.Windows.Forms.Label label5;
        protected System.Windows.Forms.GroupBox groupBoxMode;
        protected System.Windows.Forms.RadioButton radioButtonUDP;
        protected System.Windows.Forms.RadioButton radioButtonTCP;
        protected System.Windows.Forms.GroupBox groupBoxTCP;
        protected System.Windows.Forms.Label label6;
        protected System.Windows.Forms.TextBox textBoxPort;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.TextBox textBoxSlaveDelay;
        protected System.Windows.Forms.ComboBox comboBoxBaudRate;
        protected System.Windows.Forms.RadioButton radioButtonRTU;
        protected System.Windows.Forms.OpenFileDialog openFileDialog;
        protected System.Windows.Forms.SaveFileDialog saveFileDialog;
        protected System.Windows.Forms.Label label8;
        protected System.Windows.Forms.TextBox txtIP;
        protected System.Windows.Forms.ComboBox comboBoxStopBits;
        protected System.Windows.Forms.Label label10;
        protected System.Windows.Forms.ComboBox comboBoxDataBits;
        protected System.Windows.Forms.Label label9;
        protected System.Windows.Forms.ComboBox comboBoxParity;
        protected System.Windows.Forms.Label labelParity;
        protected System.Windows.Forms.GroupBox grpExchange;
        protected System.Windows.Forms.Button buttonPauseLog;
        protected TabControlEx tabControl1;
        protected System.Windows.Forms.RadioButton radioBtnFloat32;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button donate;
        protected System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button btnSaveCom;
        private System.Windows.Forms.Button btnLoadCom;
        private System.Windows.Forms.Label txtAppVersion;
        private System.Windows.Forms.Button btnContact;
        private System.Windows.Forms.ToolTip toolTip;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.CheckBox checkSwapFloatEndian;
    }
}