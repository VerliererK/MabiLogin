using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace MabiLogin
{
    class BeanfunAccount
    {
        private const string SaveFile = "user.dat";
        private const int entropyLength = 20;
        public HashSet<AccountInfo> list;

        public BeanfunAccount()
        {
            list = new HashSet<AccountInfo>();
        }

        public void AddAccount(AccountInfo info)
        {
            if (info.loginMethod == BeanfunLogin.LoginMethod.QRCode) return;
            if (string.IsNullOrEmpty(info.username)) return;
            if (string.IsNullOrEmpty(info.password)) return;

            if (list != null && !list.Contains(info))
            {
                list.Add(info);
            }
        }

        public void LoadAccount()
        {
            if (!File.Exists(SaveFile)) return;

            try
            {
                byte[] data = File.ReadAllBytes(SaveFile);
                byte[] entropy = new byte[entropyLength];
                byte[] cipher = new byte[data.Length - entropy.Length];
                Array.Copy(data, 0, entropy, 0, entropy.Length);
                Array.Copy(data, entropy.Length, cipher, 0, cipher.Length);

                byte[] plain = ProtectedData.Unprotect(cipher, entropy, DataProtectionScope.CurrentUser);
                HashSet<AccountInfo> list = null;
                using (MemoryStream ms = new MemoryStream(plain))
                {
                    var br = new BinaryFormatter();
                    list = br.Deserialize(ms) as HashSet<AccountInfo>;
                }

                if (list != null)
                    this.list = list;
            }
            catch
            {
                File.Delete(SaveFile);
                throw;
            }
        }

        public void SaveAccount()
        {
            if (list == null) return;
            // Data to protect. Convert a string to a byte[] using Encoding.UTF8.GetBytes().
            byte[] plaintext = SerializeToByteArray(list);

            // Generate additional entropy (will be used as the Initialization vector)
            byte[] entropy = new byte[entropyLength];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                rng.GetBytes(entropy);

            byte[] ciphertext = ProtectedData.Protect(plaintext, entropy, DataProtectionScope.CurrentUser);

            byte[] data = new byte[entropy.Length + ciphertext.Length];
            Array.Copy(entropy, data, entropy.Length);
            Array.Copy(ciphertext, 0, data, entropy.Length, ciphertext.Length);
            SaveByteToFile(data);
        }

        private byte[] SerializeToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private void SaveByteToFile(byte[] data)
        {
            using (var stream = new FileStream(SaveFile, FileMode.Create))
            {
                for (int i = 0; i < data.Length; i++)
                    stream.WriteByte(data[i]);

                // Set the stream position to the beginning of the file.
                stream.Seek(0, SeekOrigin.Begin);

                // Read and verify the data.
                for (int i = 0; i < stream.Length; i++)
                    if (data[i] != stream.ReadByte())
                        throw new System.Exception("Save File Failed");
            }
        }
    }

    [Serializable]
    public struct AccountInfo
    {
        public string username;
        public string password;
        public BeanfunLogin.LoginMethod loginMethod;
    }
}