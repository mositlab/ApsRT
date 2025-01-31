using Rodana.Core.Session;

namespace Rodana.ZMK
{
    public partial class MainForm : Form
    {
        public MainForm(Session<Guid, string, Guid, string, int, Guid, string> sessionLink)
        {
            InitializeComponent();
            this.sessionLink = sessionLink;

            this.FormClosed += MainForm_FormClosed;
        }

        private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            sessionLink.EndSession(0);
        }

        Session<Guid, string, Guid, string, int, Guid, string> sessionLink;

    }
}
