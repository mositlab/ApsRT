using ApsRT.Core.Settings;
using Newtonsoft.Json;
using System.Text;

namespace ApsRT.ErrorsToggler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SettingsClass settings;

        void SetError(bool value)
        {
            settings.LogErrors = value;
            if (value)
            {
                label2.Text = "¬ À";
                label2.ForeColor = Color.Green;
            }
            else
            {
                label2.Text = "¬€ À";
                label2.ForeColor = Color.Red;
            }
        }

        public string SavePath { get; set; } = "settings rt.txt";

        private void Form1_Load(object sender, EventArgs e)
        {
            settings = JsonConvert.DeserializeObject<SettingsClass>(File.ReadAllText(SavePath, Encoding.UTF8));
            label1.Text = Directory.GetCurrentDirectory();
            SetError(settings.LogErrors);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetError(!settings.LogErrors);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

    }
}
