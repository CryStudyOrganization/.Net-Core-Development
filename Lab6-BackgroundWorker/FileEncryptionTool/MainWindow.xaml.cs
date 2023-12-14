﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Windows.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

namespace FileEncryptionTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //private List<User> _users = new List<User>();

        Aes aesHelper = Aes.Create();

        public MainWindow()
        {
            InitializeComponent();
            recipientsListBox.SelectionMode = SelectionMode.Multiple;
            FileEncryption.pu = (
                (int i) => encryptionProgressBar.Dispatcher.Invoke(
                    () => encryptionProgressBar.Value = i,
                    DispatcherPriority.Background
                )
            );
            
        }

        private void Update_RNG(List<Point> coords)
        {

            //use coordinates entered by user
            List<byte> bytes = new List<byte>();
            foreach (var p in coords)
            {
                bytes.Add(Convert.ToByte(p.X));
                bytes.Add(Convert.ToByte(p.Y));
            }
            

            //get system uptime
            using (var uptime = new PerformanceCounter("System", "System Up Time"))
            {
                uptime.NextValue();       //Call this an extra time before reading its value
                bytes.AddRange(BitConverter.GetBytes(uptime.NextValue()));
            }

            //get random number from Australian National University's Quantum RNG Server

            FileEncryption.key = GetAnuBytes(Int32.Parse(keySize_TextBox.Text) / 8);

            //FileEncryption.key = bytes.ToArray();

            encryptFile_Button.IsEnabled = true;
        }

        public static byte[] GetAnuBytes(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
                bytes[i] = (byte)((i + 1) % 10);
            return bytes;
            using (var wc = new WebClient())
            {
                string result = wc.DownloadString(string.Format("https://qrng.anu.edu.au/API/jsonI.php?length={0}&type=uint8", length));
                var m = Regex.Match(result, "\"data\":\\[(?<rnd>[0-9,]*?)\\]", RegexOptions.Singleline); //parse JSON with regex

                if (m.Success)
                {
                    var g = m.Groups["rnd"];
                    if (g != null && g.Success)
                    {
                        string[] values = g.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < values.Length; i++)
                            bytes[i] = Byte.Parse(values[i]);
                    }
                }
                return bytes;
            }
        }

        private CipherMode GetSelectedCipherMode()
        {
            if (modeECB.IsChecked == true)
                return CipherMode.ECB;
            if (modeCBC.IsChecked == true)
                return CipherMode.CBC;
            if (modeCFB.IsChecked == true)
                return CipherMode.CFB;
            if (modeOFB.IsChecked == true)
                return CipherMode.OFB;
            return CipherMode.ECB;
        }

        private string ValidateInputPath(string path)
        {
            if (String.IsNullOrEmpty(path)) return "Input file was not chosen!";
            if (!File.Exists(path)) return "Input file doesn't exist!";
            return null;
        }

        private string validateOutputPath(string path)
        {
            if (String.IsNullOrEmpty(path)) return "Destination file was not chosen!";
            if (File.Exists(path)) return "Destination file already exists!";
            if (!Path.IsPathRooted(path)) return "Incorrect path for destination file!";

            try { Path.GetFullPath(path); }
            catch { return "Destination directory doesn't exist!"; }

            string pathAndFileName = path;

            string path2 = pathAndFileName.Substring(0, pathAndFileName.LastIndexOf("\\"));
            try
            {
                if (!Directory.Exists(path2))
                    Directory.CreateDirectory(path2);
            }
            catch
            {
                return "Destination directory was not chosen!";
            }

            return null;
        }

        private void inputFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                inputFile_TextBox.Text = openFileDialog.FileName;
        }

        private void outputFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                outputFile_TextBox.Text = openFileDialog.FileName;
        }

        private void decryptionInputButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                decryptionInputFileBox.Text = openFileDialog.FileName;
                FileEncryption.loadPossibleRecipientsAndFileType(openFileDialog.FileName, decryptionRecipientsList, extension_Label);
            }
        }

        private void decryptionOutputButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                decryptionOutputFileBox.Text = openFileDialog.FileName;
        }

        private void generateRandomNumber_Button_Click(object sender, RoutedEventArgs e)
        {
            RNG_Window win2 = new RNG_Window(Update_RNG);
            win2.ShowDialog();
        }
        
        private void encryptFile_Button_Click(object sender, RoutedEventArgs e)
        {
            string inputFileError = ValidateInputPath(inputFile_TextBox.Text);
            string outputFileError = validateOutputPath(outputFile_TextBox.Text);

            if(inputFileError != null)
            {
                MessageBox.Show(inputFileError);
                return;
            }

            if(outputFileError != null)
            {
                MessageBox.Show(outputFileError);
                return;
            }

            try
            {
                FileEncryption.targetUsers = recipientsListBox.Items.Cast<User>().ToList();
                FileEncryption.mode = GetSelectedCipherMode();
                FileEncryption.keySize = Int32.Parse(keySize_TextBox.Text);
                //FileEncryption.key = GetAnuBytes(FileEncryption.keySize >> 3);
                FileEncryption.bufferSize = 1 << 15;
                FileEncryption.blockSize = Int32.Parse(blockSize_TextBox.Text);
                FileEncryption.iv = GetAnuBytes(16);

                FileEncryption.InitializeEncryption(inputFile_TextBox.Text, outputFile_TextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during decryption: " + ex);
            }
        }

        private void modeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (validBlockSize_Label == null)
                return;
            aesHelper.Mode = (CipherMode)GetSelectedCipherMode();

            String text = String.Format("{0} - {1}", aesHelper.LegalBlockSizes[0].MinSize, aesHelper.LegalBlockSizes[0].MaxSize);
            if (aesHelper.LegalBlockSizes[0].MinSize == aesHelper.LegalBlockSizes[0].MaxSize)
                text = String.Format("{0}", aesHelper.LegalBlockSizes[0].MinSize.ToString());

            //validBlockSize_Label.Content = text;
        }

        private void DecryptFile_Button_Click(object sender, RoutedEventArgs e)
        {
            string inputFileError = ValidateInputPath(decryptionInputFileBox.Text);
            string outputFileError = validateOutputPath(decryptionOutputFileBox.Text);

            if (inputFileError != null)
            {
                MessageBox.Show(inputFileError);
                return;
            }

            if (outputFileError != null)
            {
                MessageBox.Show(outputFileError);
                return;
            }

            try
            {
                FileEncryption.bufferSize = 1 << 15;
                //FileEncryption.key = GetAnuBytes(32);


                User selectedUser = (User)decryptionRecipientsList.SelectedItem;
                if(selectedUser == null)
                {
                    MessageBox.Show("A user was not chosen");
                    return;
                }
                string password = decryptionPassword.Password;
                
                FileEncryption.InitializeDecryption(decryptionInputFileBox.Text, decryptionOutputFileBox.Text, selectedUser, password);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error encountered during decryption");
            }
        }



       
       
        private void AddUser_Button_Click(object sender, RoutedEventArgs e)
        {
            string _email = email.Text;
            string _password = passwordBox.Password;
            string _passwordRepeat = passwordBoxRepeat.Password;

            string passwordError = User.validatePassword(_password);
            string repeatError = validateRepeatedPassoword();

            if(passwordError == null && repeatError == null && !String.IsNullOrEmpty(_email))
            {
                new User(_email, _password);
                MessageBox.Show("Added new user: " + _email);
            }

            
        }


        private string validateRepeatedPassoword()
        {
            if (passwordBoxRepeat.Password != passwordBox.Password)
            {
                return "Passwords must match!";
            }
            return null;
        }



        private void passwordBoxRepeat_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string error = validateRepeatedPassoword();

            if (error != null) passwordReapetError.Content = error;
            else passwordReapetError.Content = "";
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string error = User.validatePassword(passwordBox.Password);

            if (error != null) passwordError.Content = error;
            else passwordError.Content = "";
        }

        private void addRecipient_Click(object sender, RoutedEventArgs e)
        {
            new Recipients_Window(recipientsListBox).Show();
        }

        private void removeRecipient_Click(object sender, RoutedEventArgs e)
        {
            List<User> selectedItems = recipientsListBox.SelectedItems.Cast<User>().ToList();

            foreach (User item in selectedItems)
            {
                recipientsListBox.Items.Remove(item);
            }
        }

        private static bool IsTextAllowed(string text) {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void blockSize_TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            e.Handled = !IsTextAllowed(e.Text) || ((TextBox)sender).Text.Length >= 3;
        }

        private void keySize_TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            e.Handled = !IsTextAllowed(e.Text) || ((TextBox)sender).Text.Length >= 3;
        }
    }
}
