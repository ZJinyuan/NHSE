﻿using System;
using System.IO;
using System.Windows.Forms;
using NHSE.Core;
using NHSE.Injection;
using NHSE.WinForms.Properties;

namespace NHSE.WinForms
{
    /// <summary>
    /// Simple launcher for opening a save file.
    /// </summary>
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            // Flash to front
            BringToFront();
            WindowState = FormWindowState.Minimized;
            Show();
            WindowState = FormWindowState.Normal;

            var args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
                Open(args[i]);
        }

        private static void Open(HorizonSave file) => new Editor(file).Show();

        private void Main_DragEnter(object sender, DragEventArgs e)
        {
            if (e.AllowedEffect == (DragDropEffects.Copy | DragDropEffects.Link)) // external file
                e.Effect = DragDropEffects.Copy;
            else if (e.Data != null) // within
                e.Effect = DragDropEffects.Move;
        }

        private void Main_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0)
                return;
            Open(files[0]);
            e.Effect = DragDropEffects.Copy;
        }

        private void Menu_Open(object sender, EventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0)
            {
                // Detect save file from SD cards?
            }
            else if ((ModifierKeys & Keys.Shift) != 0)
            {
                var path = Settings.Default.LastFilePath;
                if (Directory.Exists(path))
                {
                    Open(path);
                    return;
                }
            }

            using var ofd = new OpenFileDialog
            {
                Title = "Open main.dat ...",
                Filter = "New Horizons Save File (main.dat)|main.dat",
                FileName = "main.dat",
            };
            if (ofd.ShowDialog() == DialogResult.OK)
                Open(ofd.FileName);
        }

        private static void Open(string path)
        {
            #if !DEBUG
            try
            #endif
            {
                OpenFileOrPath(path);
            }
            #if !DEBUG
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                WinFormsUtil.Error(ex.Message);
            }
            #endif
        }

        private static void OpenFileOrPath(string path)
        {
            if (Directory.Exists(path))
            {
                OpenSaveFile(path);
                return;
            }

            var dir = Path.GetDirectoryName(path);
            if (dir is null || !Directory.Exists(dir)) // ya never know
            {
                WinFormsUtil.Error("Unable to open the folder that contains the save file.",
                    "Try moving it to another location and opening from there.");
                return;
            }

            OpenSaveFile(dir);
        }

        private static void OpenSaveFile(string path)
        {
            var file = new HorizonSave(path);
            Open(file);

            var settings = Settings.Default;
            settings.LastFilePath = path;
            settings.Save();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys != Keys.Control)
                return;

            switch (e.KeyCode)
            {
                case Keys.O:
                {
                    Menu_Open(sender, e);
                    break;
                }
                case Keys.I:
                {
                    var items = new Item[40];
                    for (int i = 0; i < items.Length; i++)
                        items[i] = new Item(Item.NONE);
                    using var editor = new PlayerItemEditor<Item>(items, 10, 4, true);
                    editor.ShowDialog();
                    break;
                }
                case Keys.H:
                {
                    using var editor = new SysBotRAMEdit(InjectionType.Generic);
                    editor.ShowDialog();
                    break;
                }
            }
        }
    }
}
