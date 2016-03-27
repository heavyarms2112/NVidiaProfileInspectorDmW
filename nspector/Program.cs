﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using nspector.Common;
using nspector.Common.Import;
using nspector.Native.WINAPI;

namespace nspector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Remove Zone.Identifier from Alternate Data Stream
                SafeNativeMethods.DeleteFile(Application.ExecutablePath + ":Zone.Identifier");
            }
            catch { }

            try
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Length == 1 && File.Exists(args[0]))
                {

                    if (new FileInfo(args[0]).Extension.ToLower() == ".nip")
                    {
                        try
                        {
                            var import = DrsServiceLocator.ImportService;
                            import.ImportProfiles(args[0]);
                            GC.Collect();
                            Process current = Process.GetCurrentProcess();
                            foreach (
                                Process process in
                                    Process.GetProcessesByName(current.ProcessName.Replace(".vshost", "")))
                            {
                                if (process.Id != current.Id && process.MainWindowTitle.Contains("Settings"))
                                {
                                    MessageHelper mh = new MessageHelper();
                                    mh.sendWindowsStringMessage((int)process.MainWindowHandle, 0, "ProfilesImported");
                                }
                            }
                            MessageBox.Show("Profile(s) successfully imported!", Application.ProductName,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Import Error: " + ex.Message, Application.ProductName + " Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                else if (ArgExists(args, "-createCSN"))
                {
                    File.WriteAllText("CustomSettingNames.xml", Properties.Resources.CustomSettingNames);
                }
                else
                {
                    
                    bool createdNew = true;
                    using (Mutex mutex = new Mutex(true, Application.ProductName, out createdNew))
                    {
                        if (createdNew)
                        {
                            Application.Run(new frmDrvSettings(ArgExists(args, "-showOnlyCSN"), ArgExists(args, "-disableScan")));
                        }
                        else
                        {
                            Process current = Process.GetCurrentProcess();
                            foreach (
                                Process process in
                                    Process.GetProcessesByName(current.ProcessName.Replace(".vshost", "")))
                            {
                                if (process.Id != current.Id && process.MainWindowTitle.Contains("Settings"))
                                {
                                    MessageHelper mh = new MessageHelper();
                                    mh.bringAppToFront((int)process.MainWindowHandle);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace ,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        
        static bool ArgExists(string[] args, string arg)
        {
            foreach (string a in args)
            {
                if (a.ToUpper() == arg.ToUpper())
                    return true;
            }
            return false;
        }
    }
}