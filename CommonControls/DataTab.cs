using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Modbus.Common
{
    using BCL;

    /// <summary>
    /// A registers page
    /// </summary>
    /// <remarks>
    /// <para>About endianness:<br/>
    /// The settled endianness for encoding targets effective word-encoding. 'Effective' means 'assumed' as it reflects
    /// how the system should interpret contiguous native registers (16bit) to render a larger register. There is no 
    /// 'true' encoding since it finally depends on how the target peer will wrap/unwrap bytes. Besides, there is no
    /// endianness option at byte-level as the modbus (at bytes level) remains a general Big-Endian encoding<br/>
    /// The settled endianness for encoding affects the underlying buffer data. Conversely, when one swaps the endianness
    /// for displaying, the underlying registers are not modified.<br/>
    /// The default floating-point encoding is LittleEndian (word-level)
    /// </para>
    /// </remarks>
    public partial class DataTab : UserControl
    {
        protected int _displayCtrlCount;

        /// <summary>
        /// enter critical section to synchronize <see cref="RegisterData"/> property content <br/>
        /// May be null (meaning no synchronization)
        /// </summary>
        /// <remarks>original application architecture hadn't plan any concurrency policy, while the internal registers
        /// map may be shared ... we introduce here a dirty workaround
        /// </remarks>
        private Action _lockData;
        /// <summary>
        /// exit critical section 
        /// </summary>
        private Action _releaseData;

        /// <summary>
        /// Floating point effective word encoding. LittleEndian is a common choice
        /// </summary>
        private Endianness _floatEncoding;

        /// <summary>
        /// Default word encoding. Applied to virtual integer registers. BigEndian is the default
        /// </summary>
        private Endianness _globalEncoding = Endianness.BE;

        /// <summary>
        /// swap endianness (for display purpose only)
        /// </summary>
        private bool _swapFloatEndianness;

        private DisplayFormat _dispFormat;

        private bool _isDirty;

        private bool _addrIsHexaFormatted;

        /// <summary>
        /// Defer the whole data table rebuild
        /// </summary>
        private bool _deferUIupdate;

        /// <summary>
        /// internal flag to prevent from redundant buffer updates due to ui feedback
        /// </summary>
        private bool _lockBufferUpdate;

        /// <summary>
        /// flag to emit an info warning before the endianness change
        /// </summary>
        private bool _warnToggleEndianness = true;

        /// <summary>
        /// registers count
        /// </summary>
        /// <remarks>CODE SMELL: <see cref="DataLength"/></remarks>
        private ushort _regCount;

        private ushort[] _registersData;

        private ModbusRegistersBuffer _buffer;

        private TextBoxHandlers _textBoxHandlers;

        private ArithmeticValueFormat addrValueFormat
        {
            get => AddrIsHexaFormatted ? ArithmeticValueFormat.hexa : ArithmeticValueFormat.@decimal;
        }

        /// <summary>
        /// Get the endianness that is used for displaying floating point. It's the settled endianness encoding which
        /// might be reversed according to the 'swap' rendering alternative 
        /// </summary>
        private Endianness displayedFloatEndianness
        {
            get => _swapFloatEndianness ? _floatEncoding.Swapped() : _floatEncoding;
        }

        /// <summary>
        /// Get the actual word endianness. It depends on the displayed format. 
        /// </summary>
        private Endianness actualEncoding
        {
            get => _dispFormat == DisplayFormat.Float32 ? FloatEncodingEndianness : _globalEncoding;
        }

        /// <summary>
        /// Some properties have been modified, which make pending an ui data-table update, needed for consistently 
        /// rendering the underlying registers buffer. It is not about the data table values, but about the structure
        /// </summary>
        private bool dataTableIsDirty
        {
            set
            {
                if (value) buttonApply.Text = "Apply*";
                else buttonApply.Text = "Apply";

                _isDirty = value;
            }
        }

        /** 
         * Must use monospace fonts to better sizing the fixed layout. Consolas should be available on all Windoze 
         * platforms
         * NOTE: a lot of fonts don't support the whole block of superscript/subscript characters, especially the small
         * 'h', which is used for hexa mode indication (only a few basic fonts support it such as Tahoma, TimesNewRoman,
         * or even Segoe ... but they are not monospace). Most modern alternative such as consolas, cascadia are mono
         * but they don't support the whole super/sub/script block. Nevertheless, it does not really matter as some 
         * suitable substitution (should) occur
         */
        private Font _labelFont = new Font("Consolas", 7.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
        /** 
         * better rendering for small typo with lucida than consolas or cascadia. Courier-new is awful. And others
         * fonts might be unvailable on some platforms
         */
        private Font _labelSmallFont = new Font("Lucida Console", 6.8f, FontStyle.Regular, GraphicsUnit.Point, 0);
        private Font _dataTableFont = new Font("Consolas", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
        private Font _dataTableSmallFont = new Font("Consolas", 8.50f, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>
        /// For float-format: arbitrary limit of output size to fit in textbox
        /// </summary>
        private readonly FormatOptions _uiFloatFormat = new FormatOptions(ArithmeticValueFormat.@decimal, maxLen: 10);
        private readonly FormatOptions _uiHexFormat = FormatOptions.Default(ArithmeticValueFormat.hexa);
        private readonly FormatOptions _uiIntFormat = FormatOptions.Default(ArithmeticValueFormat.@decimal);
        private readonly FormatOptions _uiBinFormat = new FormatOptions(ArithmeticValueFormat.binary, useAlt: true);

        /// <summary>
        /// The 1th place to customize formatting
        /// </summary>
        private readonly ValueFormatting _formatting = FormattedValue.Default;

        public DataTab()
        {
            InitializeComponent();

            _textBoxHandlers = new TextBoxHandlers(this, _formatting);

            var options = new TextBoxOptions()
            {
                BelongToActiveRegistersScope = true,
                HasActiveFocusDecoration = false
            };

            //event handling: when address changes, we flag the data table as refresh needed
            FormatOptions format = AddrIsHexaFormatted ? _uiHexFormat : _uiIntFormat;
            _textBoxHandlers.Initialize<ushort>(txtStartAdress, 0xffff, -1, format, options)
                .OnValueChanged += (_, evt) => dataTableIsDirty = true;
        }

        #region properties

        public ushort StartAddress
        {
            get => (txtStartAdress.Tag as MBNativeDataItem<ushort>).Value.Value;
            set
            {
                var data = txtStartAdress.Tag as MBNativeDataItem<ushort>;
                txtStartAdress.Text = data.Update(value);
            }
        }

        /// <summary>
        /// The registers count that may be relevant for the actual modbus operation (and a hint for the table layout)
        /// </summary>
        /// <remarks>
        /// <para>
        /// [1-125] is the accepted values range (modbus spec) for HR/IR, but [1-2000] for DI/CO. For now, we are stuck
        /// with the native registers limit
        /// </para>
        /// <para>CODE SMELL:<br/>
        /// - it reflects the number of the registers that will actually related to the UI settled modbus operation <br/>
        /// - it must not be confused with the displayed registers count, which depends on the window layout and size 
        ///  (which is more or less static ...). The displayed registers count may be greater when _regCount is lesser
        ///  than the layout capacity. And for sure, the displayed count is lesser when _regCount exceeds the layout
        ///  capacity.<br/>
        /// - it may be definitely confusing as the register type coils/DI are defined at bit-level, whereas the type
        /// HR/IR are defined at word level. Last but not least, when one refers to extended registers, we could tell
        /// about contiguous native registers, but also about whole extended registers (dword or qword) <br/>
        /// - in other words, the displayed registers are a subset or a superset of the actual sent/received registers
        /// </para>
        /// </remarks>
        public ushort DataLength
        {
            get => _regCount;

            set
            {
                if (_regCount == value) return;
                else dataTableIsDirty = true;

                //modbus limit
                _regCount = Math.Max((ushort)1, Math.Min(value, (ushort)125));
                txtSize.Text = _regCount.ToString();
            }
        }

        public bool ShowDataLength
        {
            get
            {
                return txtSize.Visible;
            }
            set
            {
                txtSize.Visible = value;
                labelTxtSize.Visible = value;
            }
        }

        public event EventHandler OnApply;

        /// <summary>
        /// Reference onto the underlying registers values buffer
        /// </summary>
        public ushort[] RegisterData
        {
            get => _registersData;
            set
            {
                if (_registersData == value) return;
                else _registersData = value;

                _buffer = new ModbusRegistersBuffer(value);

                dataTableIsDirty = true;
            }
        }

        /// <summary>
        /// The format for rendering the displayed sub-part of the underlying registers buffer 
        /// </summary>
        /// <remarks>The data table is not updated, call <see cref="UpdateDataTable"/> when all property</remarks>
        public DisplayFormat DisplayFormat
        {
            get => _dispFormat;
            set
            {
                if (_dispFormat == value) return;
                else _dispFormat = value;

                paneFloatEncoding.Enabled = value == DisplayFormat.Float32;
                dataTableIsDirty = true;
            }
        }

        /// <summary>
        /// The actual word-endianness for encodineg floating-point into the underlying buffer
        /// </summary>
        /// <remarks>UI is not updated</remarks>
        public Endianness FloatEncodingEndianness
        {
            get => _floatEncoding;

            set
            {
                if (value != _floatEncoding)
                {
                    _deferUIupdate = true;

                    _floatEncoding = value;
                    if (_floatEncoding == Endianness.BE) radioFloatBE.Checked = true;
                    else if (_floatEncoding == Endianness.LE) radioFloatLE.Checked = true;
                }
            }
        }

        /// <summary>
        /// When TRUE, even if any prefix '0x' doesn't prepend an address value, we assume it's or it should be an hexa 
        /// formatted address value
        /// </summary>
        public bool AddrIsHexaFormatted
        {
            get => _addrIsHexaFormatted;
            set
            {
                if (_addrIsHexaFormatted == value) return;
                else _addrIsHexaFormatted = value;

                //if general directive has been changed, we need to broadcast the change to the inner value
                var addr = txtStartAdress.Tag as MBDataItem<ushort>;
                addr.Formatting = value ? _uiHexFormat : _uiIntFormat;
                txtStartAdress.Text = addr.Text;
                txtStartAdress.MaxLength = _formatting.GetMaxLengthOutput(TypeCode.UInt16, addrValueFormat);
            }
        }

        #endregion

        /// <summary>
        /// Call this to make access to the registered data synchronizeable
        /// </summary>
        /// <param name="lock"></param>
        /// <param name="release"></param>
        public void SetSynchronize(Action @lock, Action release)
        {
            _lockData = @lock;
            _releaseData = release;
        }

        /// <summary>
        /// Should the settled encoding endianness be swapped for floating point displaying. Notice that the underlying
        /// registers values are not affected by this feature (which is a convenience-only purpose to help to debug or
        /// understand the data flow)
        /// </summary>
        /// <param name="updateUI">micro opt to delay the ui refreshing</param>
        public void SwapFloatEndianness(bool swap = true, bool updateUI = true)
        {
            if (_swapFloatEndianness != swap)
            {
                _swapFloatEndianness = swap;
                UpdateDataTable();
            }
        }

        private string getActiveBufferAddressRange()
        {
            return $"[{FormattedValue.Format(StartAddress, addrValueFormat)}-{FormattedValue.Format(StartAddress+DataLength, addrValueFormat)}]";
        }

        #region Data Table

        public void RefreshData()
        {
            const int binTextBoxWidth = 120;
            const int regularTextBoxWidth = 55; //size for hex|int 
            const int addrTextBoxWidth = 35; //addr label size
            const int longAddrTextBoxWidth = 55;
            const int fptTextBoxWidth = 71; //size for floating-point
            const int textBoxHeight = 20;
            const int regularTextBoxColumnMarginL = 40; //offset.x (must exceed addr-label size)
            const int fptTextBoxColumnMarginL = 35; //offset.x 
            const int regularTextBoxColumnMarginR = 10; //padding after textbox 
            const int binTextBoxColumnMarginR = 6;
            const int textBoxRowPadding = 5; //margin H for next textboxes row
            const int regularColumnSize = regularTextBoxColumnMarginL + regularTextBoxWidth + regularTextBoxColumnMarginR;
            const int binaryColumnSize = regularTextBoxColumnMarginL + binTextBoxWidth + binTextBoxColumnMarginR;

            //if addr is not hex-mode, we need 5 digits instead of 4+sub
            bool smallerFont = AddrIsHexaFormatted == false;

            // Create as many textboxes as fit into window
            groupBoxData.Visible = false;
            groupBoxData.Controls.Clear();
            var idxControl = 0;
            var screen = new Point(0, 20);

            ushort offset = StartAddress;

            //using alternative format for address labels
            FormatOptions addrFormat = new FormatOptions(addrValueFormat, useAlt: true);

            while (screen.X < groupBoxData.Size.Width - 95)
            {
                var labData = new Label();
                groupBoxData.Controls.Add(labData);
                labData.Location = screen;
                labData.Font = smallerFont ? _labelSmallFont : _labelFont;
                labData.TextAlign = ContentAlignment.MiddleRight;
                labData.Margin = Padding.Empty;

                if (_dispFormat != DisplayFormat.LED)
                {
                    labData.Size = new Size(addrTextBoxWidth, textBoxHeight);
                    labData.Text = FormattedValue.Format((ushort)(offset + idxControl), addrFormat);
                }
                else
                {
                    labData.Size = new Size(longAddrTextBoxWidth, textBoxHeight);
                    labData.Text = $"{FormattedValue.Format((ushort)(offset + idxControl / 16), addrFormat)}.{idxControl % 16}";
                }

                TextBox tbox = null;

                switch (DisplayFormat)
                {
                    case DisplayFormat.LED:
                    {
                        var bulb = new LedBulb();
                        groupBoxData.Controls.Add(bulb);
                        bulb.Size = new Size(25, 25);
                        bulb.Location = screen + new Size(55, -5);
                        bulb.Padding = new Padding(3);
                        bulb.Color = Color.Red;
                        bulb.On = false;
                        bulb.Tag = idxControl;
                        bulb.Enabled = (idxControl / 16) < _regCount;
                        bulb.Click += BulbClick;
                        screen.Offset(0, bulb.Size.Height + 10);
                        break;
                    }
                    case DisplayFormat.Binary:
                    {
                        tbox = makeNumericTextBox<ushort>(idxControl, _uiBinFormat, 
                            new Size(binTextBoxWidth, textBoxHeight), //extended size
                            x:regularTextBoxColumnMarginL,//relative location of textbox.left.x from current screen pos
                            _dataTableFont,
                            ref screen); //current screen pos is moved to the next control
                        break;
                    }
                    case DisplayFormat.Hex:
                    {
                        tbox = makeNumericTextBox<ushort>(idxControl, _uiHexFormat, 
                            new Size(regularTextBoxWidth, textBoxHeight),
                            x:regularTextBoxColumnMarginL, 
                            _dataTableFont,
                            ref screen);
                        break;
                    }
                    case DisplayFormat.Integer:
                    {
                        tbox = makeNumericTextBox<short>(idxControl, _uiIntFormat, 
                            new Size(regularTextBoxWidth, textBoxHeight),
                            x:regularTextBoxColumnMarginL, 
                            _dataTableFont,
                            ref screen);
                        break;
                    }
                    case DisplayFormat.Float32:
                    // Float values require two registers, thus skip every second control (thus hide even controls)
                    if ((idxControl & 1) == 0)
                    {
                        tbox = makeNumericTextBox<float>(idxControl, _uiFloatFormat, 
                            new Size(fptTextBoxWidth, textBoxHeight),
                            x: fptTextBoxColumnMarginL, 
                            _dataTableSmallFont,
                            ref screen);

                        //skip 1 row
                        screen.Offset(0, textBoxHeight + textBoxRowPadding); // Float Values Require Two Registers
                    }
                    else
                    {
                        labData.Visible = false;
                    }
                    break;
                }

                if (tbox != null) groupBoxData.Controls.Add(tbox);

                idxControl++;

                if (screen.Y > groupBoxData.Size.Height - 30) //30: arbitrary margin
                {
                    //start a new column
                    screen.X += DisplayFormat == DisplayFormat.Binary ? binaryColumnSize : regularColumnSize;
                    screen.Y = textBoxHeight;
                }
            }

            _displayCtrlCount = idxControl;
            UpdateDataTable();
            groupBoxData.Visible = true;

            dataTableIsDirty = false;

            TextBox makeNumericTextBox<TVal>(int index, FormatOptions format, Size size, int x, Font font, ref Point lscreen)
                where TVal : unmanaged, IConvertible, IFormattable, IEquatable<TVal>
            {
                var options = new TextBoxOptions()
                {
                    //regCount is adjusted with sizeof(val) to evince partial extended register (i.e to align on dword or 
                    //qword boundary)
                    BelongToActiveRegistersScope = index < _regCount - (_regCount % PrimitiveTraits<TVal>.Size),
                    HasActiveFocusDecoration = true
                };
                var tbox = new TextBox();

                var handling = _textBoxHandlers.Initialize<TVal>(tbox, (ushort)(StartAddress + index), index, format, options);
                handling.OnValueChanged += onUIRegisterValueChanged;

                tbox.Font = font;
                tbox.TextAlign = HorizontalAlignment.Right;
                tbox.Margin = Padding.Empty;
                tbox.Size = size;
                tbox.Location = lscreen + new Size(x, -2);

                //goto next control
                lscreen.Offset(0, size.Height + textBoxRowPadding);

                return tbox;
            }
        }

        private void BulbClick(object sender, EventArgs e)
        {
            var bulb = (LedBulb)sender;
            bulb.On = !bulb.On;
            int bulbNumber = Convert.ToInt32(bulb.Tag);
            var index = bulbNumber / 16;
            int bit = bulbNumber % 16;
            ushort mask = (ushort)(0x1 << bit);

            try
            {
                _lockData?.Invoke();

                if (bulb.On)
                {
                    RegisterData[StartAddress + index] |= mask;
                }
                else
                {
                    mask = (ushort)~mask; ;
                    RegisterData[StartAddress + index] &= mask;
                }
            }
            finally { _releaseData?.Invoke(); }
        }

        /// <summary>
        /// Update the UI with the supplied registers values
        /// </summary>
        /// <param name="data">the registers sub-part copy</param>
        private void updateDataTable(ushort[] data)
        {
            _deferUIupdate = false;
            _lockBufferUpdate = true;

            var buffer = new ModbusRegistersBuffer(data, StartAddress);
            ushort offset = 0;
            ushort reg_size = (ushort)(_dispFormat.Is32Bit() ? 2 : 1);
            Endianness endian = _dispFormat != DisplayFormat.Float32 ? Endianness.BE : displayedFloatEndianness;

            // ------------------------------------------------------------------------
            // Put new data into text boxes
            foreach (Control ctrl in groupBoxData.Controls)
            {
                if (ctrl is TextBox editBox)
                {
                    var dataItem = ctrl.Tag as MBDataItemBase;
                    int x = dataItem.Index;

                    if (x <= data.GetUpperBound(0))
                    {
                        dataItem.ReadFrom(buffer, endian);

                        ctrl.Text = dataItem.Text;
                        ctrl.Visible = true;
                        offset += reg_size;
                    }
                    else ctrl.Text = "";
                }
                else if (ctrl is LedBulb)
                {
                    var led = (LedBulb)ctrl;
                    var bulbNumber = Convert.ToInt32(ctrl.Tag);
                    int bit = bulbNumber % 16;
                    int mask = 0x1 << bit;
                    led.On = (mask & data[bulbNumber/16]) != 0;
                }
            }

            _lockBufferUpdate = false;
        }

        public void UpdateDataTable()
        {
            ushort[] data;
            try
            {
                _lockData?.Invoke();
                data = copyDataRegisters();

            }
            finally { _releaseData?.Invoke(); }

            updateDataTable(data);
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (txtStartAdress.Text != "")
            {
                try
                {
                    var address = StartAddress;
                    if (OnApply != null) OnApply(this, new EventArgs());
                    RefreshData();
                }
                catch (Exception)
                {
                    txtStartAdress.Text = "";
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            try
            {
                _lockData?.Invoke();

                if (RegisterData != null)
                {
                    var count = DataLength;
                    for (int i = StartAddress; i < RegisterData.Length && count-- != 0; i++)
                    {
                        RegisterData[i] = 0;
                    }
                    RefreshData();
                }
            }
            finally { _releaseData?.Invoke(); }
        }

        #endregion

        #region buffer registers operations

        /// <summary>
        /// Get a copy of the subset of underlying registers values, which should be displayed
        /// </summary>
        /// <returns></returns>
        private ushort[] copyDataRegisters()
        {
            var data = new ushort[_displayCtrlCount * (DisplayFormat.Is32Bit() ? 2 : 1)];

            //we keep direct reference on the owner backed data ... as a result, some concurrent access policy must be 
            //implemented by the caller somewhere ...

            Array.Copy(RegisterData, StartAddress, data, 0, Math.Min(data.Length, RegisterData.Length - StartAddress));

            return data;
        }

        private void onUIRegisterValueChanged(object sender, TextBoxValueChangedEvent evt)
        {
            if (_lockBufferUpdate) return;

            //safe
            var data = (sender as TextBox).Tag as MBDataItemBase;

            try
            {
                _lockData?.Invoke();

                //about word encoding parameter: 
                //only relevant for float32 register (and future int32)
                data.WriteTo(_buffer, actualEncoding);
            }
            finally { _releaseData?.Invoke(); }
        }

        #endregion

        #region address and size handling

        private void onRegCountKeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(char.IsDigit(e.KeyChar) || e.KeyChar == '\b'); //backspace ok
        }

        private void txtSize_TextChanged(object sender, EventArgs e)
        {
            ushort count;
            if (!ushort.TryParse(txtSize.Text, out count)) DataLength = 1;

            DataLength = count;
        }

        #endregion

        #region floating point encoding

        /// <summary>
        /// Toggles the word endianness of the registers (aligned-2) from StartAddress, thus updating the underlying
        /// data buffer. The registers pair is swapped, irrespectively of whether the data actually reflect 32bit or 
        /// just 2 standalone native registers
        /// </summary>
        /// <remarks>PRE: lock</remarks>
        private void toggleWordEndianness()
        {
            for (ushort i = StartAddress; i < DataLength - 1; i += 2)
            {
                ushort tmp = RegisterData[i];
                RegisterData[i] = RegisterData[i + 1];
                RegisterData[i + 1] = tmp;
            }
        }

        private void onFloatEncodingChanged()
        {
            var endian = radioFloatBE.Checked ? Endianness.BE : Endianness.LE;
            if (_floatEncoding == endian) return;

            bool swapBuffer = chkSwapBuffer.Checked;

            if (_warnToggleEndianness)
            {
                string message;

                if (swapBuffer)
                    message =
                $@"You are about to alter the active buffer due to an endianness change.
As the directive 'Swap buffer' is enabled, each registers 16bit pair will be swapped to keep the current floating point values;
Only the active buffer subset is involved {getActiveBufferAddressRange()}, but ensure that all registers convey floating point values.
Click RETRY to change endianness and swap the word order
Click ABORT to cancel any endianness or buffer change
Click IGNORE to disable this message for further operation (for now, nothing will be done)
";
                else
                    message =
                $@"You are about to apply an endianness change w/o any buffer change.
As the directive 'Swap buffer' is not enabled, current floating point values will be invalidated.
Only the active buffer subset is involved {getActiveBufferAddressRange()}. If endianness is not consistent across the whole buffer, you might need to review it one by one.
Click RETRY to change endianness, keeping the word order as-is (only further inputs will consider the actual endianness)
Click ABORT to cancel any endianness change
Click IGNORE to disable this message for further operation (for now, nothing will be done)
";
                var result = MessageBox.Show(message, "Please review endianness change impacts ...", MessageBoxButtons.AbortRetryIgnore);
                if (result == DialogResult.Ignore)
                {
                    //nothing to be done, besides store the 'do not show this message anymore'
                    _warnToggleEndianness = false;
                    restore();
                    return;
                }
                else if(result == DialogResult.Abort)
                {
                    restore();
                    return;
                }
                else if(result == DialogResult.Retry)
                {
                    //if one doesn't want to swap the buffer, it's (just) an endianness specification that enables further
                    //(only) input conversions w/o altering the current underlying buffer values ... drawback is that the 
                    //high level current values are broken
                }
                else
                {//not reached
                    restore();
                    return;
                }
            }
            else
            {
                //silent mode
            }

            _floatEncoding = endian;

            ushort[] data;
            try
            {
                _lockData?.Invoke();

                if(swapBuffer) toggleWordEndianness();

                if (_deferUIupdate) return;

                data = copyDataRegisters();
            }
            finally { _releaseData?.Invoke(); }

            //update ui
            updateDataTable(data);

            void restore()
            {
                //discard the swap endianness directive
                if (radioFloatLE.Checked) radioFloatBE.Checked = true;
                else radioFloatLE.Checked = true;
            }
        }

        private void onFloatLEChanged(object sender, EventArgs e)
        {
            onFloatEncodingChanged();
        }

        private void onFloatBEChanged(object sender, EventArgs e)
        {
            onFloatEncodingChanged();
        }

        #endregion
    }
}
