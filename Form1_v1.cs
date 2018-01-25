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

namespace CSV_Converter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_load_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "CSV & Text|*.txt; *.csv|Alles|*";
            if(fileDialog.ShowDialog() == DialogResult.OK) {
                txt_load.Text = fileDialog.FileName;
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "SR|*.sr|ZIP|*.zip";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txt_save.Text = fileDialog.FileName;
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            string filepath_input = txt_load.Text;
            string filepath_output = txt_save.Text;
            if (File.Exists(filepath_input))
            {
                if (File.Exists(filepath_output))
                    File.Delete(filepath_output);
                ZipArchive zipArchive = ZipFile.Open(filepath_output, ZipArchiveMode.Create);
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
                StreamWriter streamWriter;
                archiveEntry = zipArchive.CreateEntry("version");
                streamWriter = new StreamWriter(archiveEntry.Open());
                streamWriter.WriteLine("2");
                streamWriter.Close();
                streamWriter.Dispose();
                archiveEntry = zipArchive.CreateEntry("metadata");
                streamWriter = new StreamWriter(archiveEntry.Open());
                streamWriter.WriteLine("[global]");
                streamWriter.WriteLine("sigrok version = 0.6.0-git-af2f9a5");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("[device 1]");
                streamWriter.WriteLine("samplerate = " + samplerate);
                streamWriter.WriteLine("total analog = " + channels);
                int channel_num = 1;
                foreach(string channel_name in channel_names)
                    streamWriter.WriteLine("analog" + channel_num++ + " = " + channel_name);
                streamWriter.WriteLine("unitsize=1");
                streamWriter.Close();
                streamWriter.Dispose();
                ZipArchiveEntry[] archiveEntry_analog_channels;
                archiveEntry_analog_channels = new ZipArchiveEntry[channels];
                Stream[] stream_analog_channels;
                stream_analog_channels = new Stream[channels];
                for(int i = 0; i < channels; i++) {
                    archiveEntry_analog_channels[i] = zipArchive.CreateEntry("analog-1-" + (i + 1) + "-1");
                    stream_analog_channels[i] = archiveEntry_analog_channels[i].Open();
                }

                int index;
                float[] value;
                string line;
                byte[] b;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    parts = line.Split(',');
                    index = Convert.ToInt32(parts[0]);
                    value = new float[parts.Length - 1];
                    for (int x = 1; x < parts.Length; x++)
                        value[x - 1] = (float)Convert.ToDouble(parts[x].Replace('.', ','));

                    for (int i = 0; i < channels; i++)
                    {
                        b = BitConverter.GetBytes(value[i]);
                        stream_analog_channels[i].WriteByte(b[0]);
                        stream_analog_channels[i].WriteByte(b[1]);
                        stream_analog_channels[i].WriteByte(b[2]);
                        stream_analog_channels[i].WriteByte(b[3]);
                    }
                }
                    
                for (int i = 0; i < channels; i++) {
                    stream_analog_channels[i].Close();
                    stream_analog_channels[i].Dispose();
                }
                zipArchive.Dispose();
            }
            //ZipArchive zipArchive = ZipFile.Open("", ZipArchiveMode.Update);
            //ZipArchiveEntry archiveEntry = zipArchive.CreateEntry("", CompressionLevel.Fastest);
            //archiveEntry.Open().Write()
        }
    }
}
