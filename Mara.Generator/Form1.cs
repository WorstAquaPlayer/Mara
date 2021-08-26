﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Mara.Lib;
using Newtonsoft.Json;

namespace Mara.Generator
{
    public partial class Form1 : Form
    {
        private MaraConfig config;
        private PatchGenerator generator;

        public Form1()
        {
            config = new MaraConfig()
            {
                Platform = MaraPlatform.Generic
            };
            InitializeComponent();
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            if (config == null)
            {
                MessageBox.Show("Please follow all steps for generating the patch.",
                    "Config not generated.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (config.Info == null)
            {
                MessageBox.Show("Please follow step 2 for generating the patch info.",
                    "Config info not generated.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (generator == null)
            {
                MessageBox.Show("Please follow step 3 for generating the patch files directories.",
                    "Config path not generated.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UpdateLog("Generating patch, please wait...");
            config = generator.GeneratePatch(config);

            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText($"{generator.OutPath}{Path.DirectorySeparatorChar}info.json", json);
            UpdateLog("Generating zip file, please wait...");
            GenerateZip();

            MessageBox.Show("Finished, check the out folder",
                "Finished",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateLog("Finished.");
        }

        private void informationButton_Click(object sender, EventArgs e)
        {
            var infoWindow = new FormInfo(config);

            var result = infoWindow.UpdateConfig();
            if (!result.Item1) return;

            config = result.Item2;
            UpdateLog("Updated patch information.");

        }

        private void FilesButton_Click(object sender, EventArgs e)
        {
            var fileWindow = new FileSelectorForm(generator);


            var result = fileWindow.UpdateConfig();
            if (!result.Item1) return;

            generator = result.Item2;
            UpdateLog("Updated directories information.");
        }

        private void patcherButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("WIP, wait for the final version from this generator.",
                "Not finished.",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            /*
            var patcherWindow = new PatcherForm();

            patcherWindow.ShowDialog();*/
        }

        private void Button3ds_CheckedChanged(object sender, EventArgs e)
        {
            if (!Button3ds.Checked) return;
            config.Platform = MaraPlatform.Nintendo3ds;
            UpdateLog("Selected Nintendo 3DS Platform.");
        }

        private void buttonGeneric_CheckedChanged(object sender, EventArgs e)
        {
            if (!buttonGeneric.Checked) return;
            config.Platform = MaraPlatform.Generic;
            UpdateLog("Selected Generic Platform.");
        }

        private void ButtonSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (!ButtonSwitch.Checked) return;
            config.Platform = MaraPlatform.NintendoSwitch;
            UpdateLog("Selected Nintendo Switch Platform.");
        }

        private void ButtonPsvita_CheckedChanged(object sender, EventArgs e)
        {
            if (!ButtonPsvita.Checked) return;
            config.Platform = MaraPlatform.PlaystationVita;
            UpdateLog("Selected PlayStation Vita Platform.");
        }

        private void ButtonPs4_CheckedChanged(object sender, EventArgs e)
        {
            if (!ButtonPs4.Checked) return;
            config.Platform = MaraPlatform.Playstation4;
            UpdateLog("Selected PlayStation 4 Platform.");
        }

        private void UpdateLog(string text)
        {
            logTextBox.AppendText($@"{text}{Environment.NewLine}");
        }

        private void GenerateZip()
        {
            var tempPath = Path.GetTempPath();

            File.WriteAllBytes($"{tempPath}{Path.DirectorySeparatorChar}7za.dll", Properties.Resources.sevenzadll);
            File.WriteAllBytes($"{tempPath}{Path.DirectorySeparatorChar}7za.exe", Properties.Resources.sevenzaexe);
            File.WriteAllBytes($"{tempPath}{Path.DirectorySeparatorChar}7zxa.dll", Properties.Resources.sevenxzadll);

            var files = $"\"{generator.OutPath}{Path.DirectorySeparatorChar}info.json\" ";
            var zip = $"\"{generator.OutPath}{Path.DirectorySeparatorChar}result.zip\" ";

            foreach (var file in config.FilesInfo.ListXdeltaFiles)
            {
                files += $"\"{generator.OutPath}{Path.DirectorySeparatorChar}{file}\" ";
            }

            var generateZip = new ProcessStartInfo();
            {
                string arguments =
                    //$" a -mx9 -tzip {zip}{files}-mx=7 -mm=LZMA";
                    $" a -mx9 -tzip {zip}-r \"{generator.OutPath}\\.\" -mx=7 -mm=LZMA";
                generateZip.FileName = $"{tempPath}{Path.DirectorySeparatorChar}7za.exe";
                generateZip.Arguments = arguments;
                generateZip.UseShellExecute = false;
                generateZip.CreateNoWindow = true;
                generateZip.ErrorDialog = false;
                generateZip.RedirectStandardOutput = true;
                Process x = Process.Start(generateZip);
                x.WaitForExit();
            }

            File.Delete($"{tempPath}{Path.DirectorySeparatorChar}7za.dll");
            File.Delete($"{tempPath}{Path.DirectorySeparatorChar}7za.exe");
            File.Delete($"{tempPath}{Path.DirectorySeparatorChar}7zxa.dll");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            logTextBox.Text = string.Empty;
        }
    }
}
