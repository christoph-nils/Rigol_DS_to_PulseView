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
using System.IO.Compression;
namespace CSV_Converter {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        private void btn_load_Click(object sender, EventArgs e) {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "CSV & Text|*.txt; *.csv|Alles|*";
            if(fileDialog.ShowDialog() == DialogResult.OK) {
                txt_load.Text = fileDialog.FileName;
            }
        }
        private void btn_save_Click(object sender, EventArgs e) {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "SR|*.sr|ZIP|*.zip";
            if (fileDialog.ShowDialog() == DialogResult.OK) {
                txt_save.Text = fileDialog.FileName;
            }
        }
        private void btn_start_Click(object sender, EventArgs e) {
            string filepath_input = txt_load.Text;
            string filepath_output = txt_save.Text;
            if (File.Exists(filepath_input)) {
                if (File.Exists(filepath_output))
                    File.Delete(filepath_output);
                ZipArchive zipArchive = ZipFile.Open(filepath_output, ZipArchiveMode.Create);
                int file_lenght = File.ReadAllLines(filepath_input).Length;
                StreamReader reader = new StreamReader(filepath_input);
                string[] parts = reader.ReadLine().Split(',');
                int channels = parts.Length - 3;
                string[] channel_names = new string[channels];
                for(int i = 0; i < channels; i++)
                    channel_names[i] = parts[i + 1];
                parts = reader.ReadLine().Split(',');
                double frequency;
                frequency = 1/ Convert.ToDouble(parts[parts.Length - 1].Replace('.', ','));
                string samplerate = Convert.ToInt32(frequency).ToString() + " Hz";
                ZipArchiveEntry archiveEntry;
                StreamWriter streamWriter; // generate some metadata
                archiveEntry = zipArchive.CreateEntry("version");
                streamWriter = new StreamWriter(archiveEntry.Open());
                streamWriter.WriteLine("2");
                streamWriter.Close();
                streamWriter.Dispose();
                archiveEntry = zipArchive.CreateEntry("metadata");
                streamWriter = new StreamWriter(archiveEntry.Open());
                streamWriter.WriteLine("[global]");
                streamWriter.WriteLine("[device 1]");
                streamWriter.WriteLine("samplerate = " + samplerate);
                streamWriter.WriteLine("total analog = " + channels);
                int channel_num = 1;
                foreach(string channel_name in channel_names)
                    streamWriter.WriteLine("analog" + channel_num++ + " = " + channel_name);
                streamWriter.WriteLine("unitsize=1");
                streamWriter.Close();
                streamWriter.Dispose();
                int byte_count = 4 * (file_lenght - 2); // generate buffer and other variables
                byte[][] analog_output = new byte[channels][];
                for (int i = 0; i < channels; i++)
                    analog_output[i] = new byte[byte_count];                
                int index = 0;
                float value;
                byte[] b;
                while (!reader.EndOfStream) { // convert line by line to float and store values in byte arrays (easy to save)
                    parts = reader.ReadLine().Split(',');
                    for (int i = 0; i < channels; i++) {
                        value = Convert.ToSingle(parts[i + 1].Replace('.', ','));
                        b = BitConverter.GetBytes(value);
                        analog_output[i][index +0] = b[0];
                        analog_output[i][index +1] = b[1];
                        analog_output[i][index +2] = b[2];
                        analog_output[i][index +3] = b[3];
                    }
                    index += 4;
                }
                Stream stream_analog; // save compressed streams
                for (int i = 0; i < channels; i++) {
                    archiveEntry = zipArchive.CreateEntry("analog-1-" + (i + 1) + "-1");
                    stream_analog = archiveEntry.Open();
                    stream_analog.Write(analog_output[i], 0, byte_count);
                    stream_analog.Close();
                }
                zipArchive.Dispose();
            }
        }
    }
}
// in under 100 lines ;-)