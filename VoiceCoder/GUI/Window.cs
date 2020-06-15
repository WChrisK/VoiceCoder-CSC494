using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Speech.Recognition;
using VoiceCoder.Parser;
using VoiceCoder.Util;

namespace VoiceCoder.GUI
{
    public partial class Window : Form
    {
        private RecognitionEngine engine;

        public Window()
        {
            InitializeComponent();
            FormClosing += Window_FormClosing;
            engine = new RecognitionEngine();
        }

        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            engine.Dispose();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Search for the root folder with .vcg files";
            fbd.ShowDialog();
            if (fbd.SelectedPath != null)
            {
                pathTextBox.Text = fbd.SelectedPath;
            }
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            if (pathTextBox.Text == null || pathTextBox.Text.Length <= 0)
            {
                statusLabel.Text = "ERROR: No folder path selected";
            }
            else if (!Directory.Exists(pathTextBox.Text))
            {
                statusLabel.Text = "ERROR: No folder path selected";
            }
            else
            {
                engine.LoadFolder(pathTextBox.Text);
                statusLabel.Text = "Loaded successfully!";
            }
        }
    }
}
