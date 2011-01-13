using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace GSXtended
{
    public class TextBoxStreamWriter : TextWriter
    {
        private delegate void CharInvoke(char value);
        TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            if (_output.InvokeRequired)
            { // Wenn Invoke nötig ist, ...
                // dann rufen wir die Methode selbst per Invoke auf
                _output.Invoke(new CharInvoke(Write), value);
                return;
            }
            base.Write(value);
            _output.AppendText(value.ToString());
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
