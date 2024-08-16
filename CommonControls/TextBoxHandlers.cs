using System;
using System.Windows.Forms;
using System.Drawing;

namespace Modbus.Common
{
    using BCL;

    internal class TextBoxValueChangedEvent : EventArgs
    {
        public TextBoxValueChangedEvent(ValueChangedEvent trigger)
        {
            Trigger = trigger;
        }

        public ValueChangedEvent Trigger { get; }
    }

    internal class TextBoxOptions
    {
        /// <summary>
        /// Use special background/foreground colors when the input is active
        /// </summary>
        public bool HasActiveFocusDecoration { get; set; }

        /// <summary>
        /// TRUE if the editbox is part of the modbus registers which are involved in the settled modbus operation<br/>
        /// FALSE is the editbox is related to a displayed register value, which will be excluded from the settled
        /// modbus operation. An excluded editbox may be rendered differently
        /// </summary>
        public bool BelongToActiveRegistersScope { get; set; } = true;

        public static bool ExcludedRegistersAreReadOnly = true;
    }
  
    /// <summary>
    /// Event forwarding from the inner trigger <see cref="MBDataItem"/>
    /// </summary>
    internal class TextBoxHandling
    {
        public TextBoxHandling(TextBox target, MBDataItemBase trigger)
        {
            m_target = target;
            trigger.OnValueChanged += raiseValueChanged;
        }
        
        public event EventHandler<TextBoxValueChangedEvent> OnValueChanged;

        /** event forwarding: from the data item to the textbox */
        private void raiseValueChanged(MBDataItemBase sender, ValueChangedEvent change)
        {
            OnValueChanged?.Invoke(m_target, new TextBoxValueChangedEvent(change));
        }

        private readonly TextBox m_target;
    }

    internal class TextBoxHandlers
    {
        public TextBoxHandlers(DataTab owner, ValueFormatting formatting = null)
        {
            m_owner = owner;
            m_formatting = formatting ?? FormattedValue.Default;
        }

        public TextBoxHandling Initialize<TVal>(TextBox tbox, ushort addr, int index, FormatOptions fmt, TextBoxOptions options)
            where TVal : unmanaged, IConvertible, IFormattable, IEquatable<TVal>

        {
            tbox.ReadOnly = options.BelongToActiveRegistersScope ? false : TextBoxOptions.ExcludedRegistersAreReadOnly;
            tbox.BackColor = options.BelongToActiveRegistersScope ? Color.Empty : SystemColors.Info;

            int max_len = m_formatting.GetMaxLengthOutput(Type.GetTypeCode(typeof(TVal)), fmt);
            if (max_len != 0) tbox.MaxLength = max_len;

            var data = MBDataItem.Create(addr, index, FormattedValue.Zero<TVal>(fmt), options);
            tbox.Tag = data;

            tbox.Enter += onEditBoxEnter;
            tbox.KeyDown += onEditBoxKeyDown;
            tbox.KeyUp += onEditBoxKeyUp;
            tbox.Leave += onEditBoxLeave;

            return new TextBoxHandling(tbox, data);
        }

             /// <summary>
        /// Aimed to process a keydown/up event
        /// </summary>
        /// <param name="k"></param>
        /// <returns>TRUE if input key is a digit or an hexa letter (or a navigation key), depending on the supplied
        /// expected input format</returns>
        public static bool IsValidValueInputKey(Keys k, ArithmeticValueType fmt)
        {
            return isDigitInputKey(k, fmt) || isMoveInputKey(k);
        }

        private static bool isDigitInputKey(Keys k, ArithmeticValueType valType)
        {
            if(valType.Format == ArithmeticValueFormat.hexa)
            {
                return (k >= Keys.D0 && k <= Keys.D9) || (k >= Keys.A && k <= Keys.F) || k == Keys.X;
            }
            else if(valType.Format == ArithmeticValueFormat.@decimal)
            {
                if (k >= Keys.D0 && k <= Keys.D9) return true;

                if (valType.IsSigned && k == Keys.OemMinus) return true;

                if (valType.IsFloatingPoint && k == Keys.OemPeriod) return true;

                return false;
            }
            else if(valType.Format == ArithmeticValueFormat.binary)
            {
                return (k == Keys.D0 || k == Keys.D1);
            }
            else
                return false;
        }

        private static bool isMoveInputKey(Keys k)
        {
            return k == Keys.Back || k == Keys.Delete ||
                   k == Keys.Left || k == Keys.Right || 
                   k == Keys.Home || k == Keys.End;
        }

        /// <summary>
        /// aimed to process a keyPress event
        /// </summary>
        /// <param name="c"></param>
        /// <param name="allowHex"></param>
        /// <returns>TRUE if input char is a digit or an hexa letter</returns>
        private bool isValidValueInputChar(char c, bool allowHex)
        {
            if(allowHex)
            {
                //[0-9][A-F][a-f]
                return (c >= 0x30 && c <= 0x39) || (c >= 0x41 && c <= 0x46) || (c >= 0x61 && c <= 0x66)
                    //backspace or xX (hexa)
                   || (c == '\b' || char.ToLower(c) == 'x' );
            }
            else
            {
                return char.IsDigit(c) || c == '\b'; //backspace ok
            }
        }

        /**
         * KeyUP event is easier to handle than KeyPress, because TextBox.Text already contains the new value, whereas 
         * KeyPress does not. It's easier to restore the old value than to speculate on the new one
         */
        private void onEditBoxKeyUp(object sender, KeyEventArgs e)
        {
            var editBox = (TextBox)sender;
            var data = editBox.Tag as MBDataItemBase;

            if(data.TryParse(editBox.Text) == false)
            {
                //if unable to parse a valid input from text, we restore the last (valid) value ----

                //save the cursor position since changing text resets the position
                int cursor = editBox.SelectionStart;
                editBox.Text = data.Text;
                editBox.SelectionStart = cursor;

                e.Handled = true;
            }
        }

        private void onEditBoxKeyDown(object sender, KeyEventArgs e)
        {
            var editBox = (TextBox)sender;
            var data = editBox.Tag as MBDataItemBase;

            bool is_valid = IsValidValueInputKey(e.KeyCode, data.ValueType);

            if (!is_valid)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                return;
            }
            else
            {
                //hijack the event-kdown to block exceeding characters in hex mode (reject extra heading zero, which
                //lead to spurious successful validation and pollute the input)
                //NOTE: this statement is a bit more intelligent than just using an editbox.text.size fixed limit 

                   //navigation must not be inhibited, for sure ... thus only test for digits
                if(isDigitInputKey(e.KeyCode, data.ValueType) && 
                   //do not inhibit an incomplete input
                   editBox.Text.Length >= m_formatting.GetMaxLengthOutput(data.ValueType.ValueCode, data.Formatting) && 
                   //do not prevent deletion of selection
                   editBox.SelectionLength == 0)      
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }
        private void onEditBoxEnter(object sender, EventArgs e)
        {
            var editBox = (TextBox)sender;
            var data = editBox.Tag as MBDataItemBase;

            var uiOptions = data.UserParam as TextBoxOptions;
            if (uiOptions?.HasActiveFocusDecoration == true)
            {
                editBox.BackColor = ControlPaint.LightLight(SystemColors.Highlight);
                editBox.ForeColor = SystemColors.HighlightText;
            }
        }

        private void onEditBoxLeave(object sender, EventArgs e)
        {
            var editBox = (TextBox)sender;
            var data = editBox.Tag as MBDataItemBase;

            editBox.Text = data.Text; //reformat

            var uiOptions = data.UserParam as TextBoxOptions;
            if (uiOptions?.HasActiveFocusDecoration == true)
            {
                editBox.BackColor = uiOptions.BelongToActiveRegistersScope ? Color.Empty : SystemColors.Info;
                editBox.ForeColor = Color.Empty;
            }
        }

        private readonly DataTab m_owner;

        private readonly ValueFormatting m_formatting;
    }

}
