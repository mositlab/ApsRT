using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Rodana.ZMK.CustomComponents
{
    public class MaterialTabControl : TabControl
    {

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !DesignMode) m.Result = (IntPtr)1;
            else base.WndProc(ref m);
        }
    }
}
