using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SchemaApplication
{
    public partial class mainForm : Form
    {
        string appPath = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        string basePath = ConfigurationManager.AppSettings["MigrationScriptDirectory"];

        OpenFileDialog ofd = new OpenFileDialog();

        public mainForm()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            ofd.InitialDirectory = basePath;
            //ofd.CustomPlaces.Add(@"C:\");
            ofd.Title = "Pick a sql script file";
            ofd.Filter = "SQL Scripts|*.sql";

            txtDestination.Text = appPath;
        }
        public string selectedFilePath
        {
            get
            {
                return tbFilePath.Text;
            }
            set
            {
                tbFilePath.Text = value;
            }
        }
        private void ChooseFile()
        {
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK && ofd.FileName.Length > 0)
            {
                selectedFilePath = ofd.FileName;
            }
        }
        private void ChooseFolder()
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtDestination.Text = folderBrowserDialog.SelectedPath;
            }
        }
        private string ReadFileIntoStringCollection()
        {
            const int MaxBytes = 65536;
            StreamReader sr = new StreamReader(selectedFilePath);
            StringBuilder result = new StringBuilder();
            int nBytesRead = 0;
            string nextLine;
            while ((nextLine = sr.ReadLine()) != null)
            {
                nBytesRead += nextLine.Length;
                if (nBytesRead > MaxBytes)
                    break;
                result.AppendLine(nextLine);
            }
            sr.Close();
            return result.ToString();
        }
        private void CreateFileFromStream(string filePath, Stream stream)
        {
            int bufferSize = (int)stream.Length;
            if (bufferSize == 0) return;
            using (FileStream fileStream = System.IO.File.Create(filePath, bufferSize))
            {
                byte[] buffer = new byte[bufferSize];
                stream.Read(buffer, 0, Convert.ToInt32(buffer.Length));
                fileStream.Write(buffer, 0, buffer.Length);
            }
        }
        private void TransformSchema()
        {
            try
            {
                if (string.IsNullOrEmpty(selectedFilePath))
                {
                    MessageBox.Show("Please select a file", "Warning");
                }

                var fileInfo = new FileInfo(selectedFilePath);
                if (!fileInfo.Exists)
                {
                    MessageBox.Show("File is not existed", "Error");
                }

                var args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"M3_Dev1.", "M3_Test1."},
                {"M3_Dev2.", "M3_Test2."},
                {"[M3_Dev1].", "[M3_Test1]."},
                {"[M3_Dev2].", "[M3_Test2]."}
            };

                string source = ReadFileIntoStringCollection();
                string output = args.Aggregate(source, (current, value) => current.Replace(value.Key, value.Value));

                string targetPath = txtDestination.Text;
                var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                var newFilePath = Path.Combine(targetPath, string.Format("{0}-edited{1}", fileName, fileInfo.Extension));

                FileStream fileStream = new FileStream(newFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    // write a line of text to the file
                    sw.WriteLine(output);
                    sw.Close();

                    MessageBox.Show("Transform Successfully");
                    //this.Close();

                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return;
            }
        }

        private void btnFileBrowser_Click(object sender, EventArgs e)
        {
            ChooseFile();
        }
        private void btnFolderBrowser_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }
        private void btnTransform_Click(object sender, EventArgs e)
        {
            TransformSchema();
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("This will close down the whole application. Confirm?", "Close Application", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            
            txtDestination.Text = "";
            openFileDialog.Reset();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
