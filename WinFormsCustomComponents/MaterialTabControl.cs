using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WinFormsCustomComponents
{
    public class MaterialTabControl : TabControl
    {

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !DesignMode) m.Result = (nint)1;
            else base.WndProc(ref m);
        }
    }
}
