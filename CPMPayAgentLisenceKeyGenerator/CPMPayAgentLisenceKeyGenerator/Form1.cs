using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace CPMPayAgentLisenceKeyGenerator
{
    public partial class Form1 : Form
    {
        private const string LicenceRegistryPath = @"SOFTWARE\CPM";
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSystemInfo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSystemInfo.Text))
                txtSystemInfo.Text = HardwareID.HDInfo.GetUniqueId();
            else
                MessageBox.Show("You are creating key for another system.", "Another System Key", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ////to delete
            //System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            //string resultingString = "P@$$w0rd"+txtSystemInfo.Text;
            //byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(resultingString);
            //byte[] hash = md5.ComputeHash(inputBytes);

            //// step 2, convert byte array to hex string
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //for (int i = 0; i < hash.Length; i++)
            //{
            //    sb.Append(hash[i].ToString("X2")); //UPPER case hexadecimal conversion
            //}
            //xtxLisenceKey.Text = sb.ToString();

            //btnLisenceKey.Enabled = !string.IsNullOrEmpty(xtxLisenceKey.Text) & true;
            xtxLisenceKey.Text = LicenseManager.LicenseGenerator.GenerateLicence(txtSystemInfo.Text, "P@$$w0rd", dtPickerLisence.Value);
        }

        private void xtxLisenceKey_TextChanged(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(xtxLisenceKey.Text))
                btnLisenceKey.Enabled = true;
            else
                btnLisenceKey.Enabled = false;
        }

        private void btnLisenceKey_Click(object sender, EventArgs e)
        {
            string key = xtxLisenceKey.Text;
            //EncryptorDecryptor.SimpleAES aes = new EncryptorDecryptor.SimpleAES();
            //key = aes.EncryptToString(key);
            
            //var rkey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(LicenceRegistryPath);
            //string licenseKey = rkey.GetValue("lic") != null ? rkey.GetValue("lic").ToString() : string.Empty;

            //rkey.SetValue("lic", key);
            //rkey.Flush();
            //rkey.Close();
            //Console.Write(licenseKey);
            //if (rkey != null)
            //    licenseKey = rkey.GetValue("license") != null ? rkey.GetValue("license").ToString() : string.Empty;

            if (File.Exists("license.dat"))
            {
                if (MessageBox.Show("License file exists. Are you sure you want to overwrite it?", "License Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                {
                    TextWriter tw = new StreamWriter("license.dat");
                    tw.Write(key);
                    tw.Flush();
                    tw.Close();
                    MessageBox.Show("License file was overwritten.", "License Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
                    Application.Exit();
                }
                else
                    MessageBox.Show("License file was not overwritten.", "License Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                TextWriter tw = new StreamWriter("license.dat");
                tw.Write(key);
                tw.Flush();
                tw.Close();
                MessageBox.Show("License file was created.", "License Exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
                Application.Exit();
            }
        }
    }
}
