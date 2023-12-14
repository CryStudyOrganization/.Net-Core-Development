﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Controls;

namespace FileEncryptionTool
{
    static class FileEncryption
    {
        public delegate void ProgressUpdate(int i);
        static public ProgressUpdate pu;
        static private string algorithmName = "AES";
        static public byte[] key;
        static public byte[] iv;
        static public CipherMode mode;
        static public int bufferSize;
        static public int keySize;
        static public int blockSize;
        static public List<User> targetUsers;
        //static public User currentUser;
        //static public string password;

        static public void InitializeEncryption(string inputFile, string outputFile)
        {
            XDocument xdoc = new XDocument(
                new XElement("EncryptedFileHeader",
                    new XElement("Algorithm", algorithmName),
                    new XElement("KeySize", keySize.ToString()),
                    new XElement("BlockSize", blockSize.ToString()),
                    new XElement("CipherMode", mode.ToString()),
                    new XElement("IV", Convert.ToBase64String(iv)),
                    new XElement("FileExtension", Path.GetExtension(inputFile)),
                    new XElement("ApprovedUsers",
                        from user in targetUsers
                        select new XElement("User",
                            new XElement("Email", user.Email),
                            new XElement("SessionKey", RSA.encryptToString(key, user.getPublicKey()))
                        )
                    )
                )
            );

            using (StreamWriter writer = new StreamWriter(outputFile, false))
            {
                try
                {
                    xdoc.Save(writer);
                    writer.Write("\r\nDATA\r\n");
                }catch(Exception ex)
                {
                    MessageBox.Show("Error: " + ex);
                }
                
            }

            if (EncryptFile(inputFile, outputFile))
                MessageBox.Show("Successfuly written to file");
        }

        static public void loadPossibleRecipientsAndFileType(string inputFile, ListBox recipients, Label extension)
        {
            bool isSupportedFile = false;

            //read the header to memory
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamReader s = File.OpenText(inputFile))
                {
                    while (!s.EndOfStream)
                    {
                        var l = s.ReadLine();
                        if (l.Contains("DATA"))
                        {
                            isSupportedFile = true;
                            break;
                        }
                            

                        ms.Write(Encoding.ASCII.GetBytes(l.ToCharArray()), 0, l.Length);
                    }
                }

                if (!isSupportedFile)
                {
                    MessageBox.Show("Incorrect file");
                    return;
                }

                //read settings from header
                ms.Position = 0;
                XDocument xdoc = XDocument.Load(ms);
                var root = xdoc.Element("EncryptedFileHeader");

                extension.Content = root.Element("FileExtension").Value;

                List<string> emails = root.Element("ApprovedUsers").Elements().Select(element => element.Element("Email").Value).ToList();
                Dictionary<string, User> allUsers = User.loadUsers().ToDictionary(x => x.Email, x => x);

                recipients.Items.Clear();
                foreach (var email in emails)
                {
                    if (allUsers.ContainsKey(email))
                    {
                        recipients.Items.Add(allUsers[email]);
                    }
                }
            }
        }


        static public void InitializeDecryption(string inputFile, string outputFile, User currentUser, string password)
        {
            bool isSupportedFile = false;
            //read the header to memory
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamReader s = File.OpenText(inputFile))
                {
                    while (!s.EndOfStream)
                    {
                        var l = s.ReadLine();
                        if (l.Contains("DATA"))
                        {
                            isSupportedFile = true;
                            break;
                        }
                            

                        ms.Write(Encoding.ASCII.GetBytes(l.ToCharArray()), 0, l.Length);
                    }
                }

                if (!isSupportedFile)
                {
                    MessageBox.Show("Incorrect file");
                    return;
                }

                //write settings from header
                ms.Position = 0;
                XDocument xdoc = XDocument.Load(ms);
                var root = xdoc.Element("EncryptedFileHeader");
                algorithmName = root.Element("Algorithm").Value;
                keySize = Int32.Parse(root.Element("KeySize").Value);
                blockSize = Int32.Parse(root.Element("BlockSize").Value);

                var extension = root.Element("FileExtension").Value;
                var outputExtension = Path.GetExtension(outputFile);
                if (outputExtension != extension)
                {
                    MessageBox.Show("File extension was change to " + extension);
                    if (string.IsNullOrEmpty(outputExtension))
                        outputFile += extension;
                    else
                        outputFile.Replace(outputExtension, extension);
                }

                Enum.TryParse(root.Element("CipherMode").Value, out mode);
                //iv = root.Element("IV").Value.Select(s => Byte.Parse(s.ToString())).ToArray();
                iv = Convert.FromBase64String(root.Element("IV").Value);
                
                var usersAndKeys = root.Element("ApprovedUsers").Elements().Select(element => new Tuple<string, string>(element.Element("Email").Value, element.Element("SessionKey").Value)).ToList();
               
                foreach (var user in usersAndKeys)
                {
                    if (user.Item1 == currentUser.Email)
                    {
                        key = RSA.decryptFromString(user.Item2, currentUser.getPrivateKey(password), keySize);
                        break;
                    }
                }
            }
            

            if (DecryptFile(inputFile, outputFile))
                MessageBox.Show("File successfuly decrypted");
        }

        static private bool EncryptFile(string inputFile, string outputFile)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = keySize;
                    aesAlg.Mode = mode;
                    aesAlg.FeedbackSize = blockSize;
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    aesAlg.Padding = PaddingMode.Zeros;

                    MessageBox.Show(String.Format("Starting encryption, parameters:\nkey size: {0}\nfeedback size: {1}\nmode: {2}", keySize, blockSize, mode.ToString()));

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                    byte[] buffer = new byte[bufferSize];
                    using (Stream output = File.Open(outputFile, FileMode.Append))
                    {
                        using (CryptoStream cs = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                        {
                            using (BinaryWriter bw = new BinaryWriter(cs))
                            {
                                using (Stream input = File.OpenRead(inputFile))
                                {
                                    int count = 0;
                                    double i = 0;
                                    long totalSize = input.Length / bufferSize;
                                    while ((count = input.Read(buffer, 0, bufferSize)) > 0)
                                    {
                                        bw.Write(buffer, 0, count);
                                        i++;
                                        pu((int)(i / totalSize * 100.0)); //calling progress update delegate (progress bar function)
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        static private bool DecryptFile(string inputFile, string outputFile)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = keySize;
                    aesAlg.Mode = mode;
                    aesAlg.FeedbackSize = blockSize;
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    aesAlg.Padding = PaddingMode.Zeros;

                    MessageBox.Show(String.Format("Starting decryption, parameters:\nkey size: {0}\nfeedback size: {1}\nmode: {2}", keySize, blockSize, mode.ToString()));

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    byte[] buffer = new byte[bufferSize];

                    using (Stream output = File.Open(outputFile, FileMode.Create))
                    {
                        using (CryptoStream cs = new CryptoStream(output, decryptor, CryptoStreamMode.Write))
                        {
                            using (BinaryWriter bw = new BinaryWriter(cs))
                            {
                                using (Stream input = File.OpenRead(inputFile))
                                {
                                    //keep reading until we hit data label (we don't want to decrypt header)
                                    bool found = false;
                                    while (!found)
                                    {
                                        if (input.ReadByte() == 'D' &&
                                            input.ReadByte() == 'A' &&
                                            input.ReadByte() == 'T' &&
                                            input.ReadByte() == 'A' &&
                                            input.ReadByte() == '\r' &&
                                            input.ReadByte() == '\n'
                                            )
                                        {
                                            found = true;
                                        }
                                    }
                                    int count = 0;
                                    double i = 0;
                                    long totalSize = input.Length / bufferSize;
                                    while ((count = input.Read(buffer, 0, bufferSize)) > 0)
                                    {
                                        bw.Write(buffer, 0, count);
                                        i++;
                                        pu((int)(i / totalSize * 100.0));
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

     
        static public byte[] encryptPrivateKey(string content, byte[] password)
        {

            byte[] valueBytes = Encoding.UTF8.GetBytes(content);
            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = password;
                aes.GenerateIV();
                aes.Mode = CipherMode.ECB;
                aes.CreateEncryptor(aes.Key, aes.IV);

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (MemoryStream to = new MemoryStream())
                    {
                        to.Write(aes.IV, 0, 16);
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            writer.Write(valueBytes, 0, valueBytes.Length);
                            writer.FlushFinalBlock();
                            encrypted = to.ToArray();
                        }
                    }
                }
                aes.Clear();
            }

            return encrypted;
        }



        static public string decryptPrivateKey(byte[] content, byte[] password)
        {
            byte[] _initialVector = new byte[16];
            Array.Copy(content, 0, _initialVector, 0, 16);
           
            byte[] decrypted;
            int decryptedByteCount = 0;
            using (Aes aes = Aes.Create())
            {
                aes.Key = password;
                aes.IV = _initialVector;
                aes.Mode = CipherMode.ECB;
                aes.CreateEncryptor(aes.Key, aes.IV);

                try
                {
                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        using (MemoryStream from = new MemoryStream(content))
                        {
                            from.Read(_initialVector, 0, 16);
                            using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                            {
                                decrypted = new byte[content.Length];
                                decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                            }
                        }
                    }
                } catch (Exception e)
                {
                    return String.Empty;
                }
                aes.Clear();
            }

            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }
    }
}
