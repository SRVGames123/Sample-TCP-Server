
using System.Security.Cryptography;
using System.Text;

namespace ModalStrikeServer.RpcServer.Utilities {
    public class EncryptUtility {
        public static string MD5(string text) => MD5_2(new UTF8Encoding().GetBytes(text));

        public static string MD5_2(byte[] bytes) {
            byte[] array = new MD5CryptoServiceProvider().ComputeHash(bytes);
            string text = string.Empty;
            for(int i = 0; i < array.Length; i++) {
                text += Convert.ToString(array[i], 16).PadLeft(2, '0');
            }
            return text.PadLeft(32, '0');
        }

        public static byte[] Encrypt(string plainText, byte[] Key, byte[] IV) {
            byte[] encrypted;
            using(AesManaged aes = new AesManaged()) {
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                using(MemoryStream ms = new MemoryStream()) {
                    using(CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                        using(StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            return encrypted;
        }

        public static byte[] EncryptByte(byte[] plainText, byte[] Key, byte[] IV) {
            byte[] encrypted;
            using(AesManaged aes = new AesManaged()) {
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                using(MemoryStream ms = new MemoryStream()) {
                    using(CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                        cs.Write(plainText, 0, plainText.Length);
                        cs.FlushFinalBlock();
                        encrypted = ms.ToArray();
                    }
                }
            }
            return encrypted;
        }

        public static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV) {
            string plaintext = null;
            using(AesManaged aes = new AesManaged()) {
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                using(MemoryStream ms = new MemoryStream(cipherText)) {
                    using(CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                        using(StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }

        public static byte[] DecryptByte(byte[] cipherText, byte[] Key, byte[] IV) {
            byte[] plaintext = null;
            using(AesManaged aes = new AesManaged()) {
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                using(MemoryStream ms = new MemoryStream()) {
                    using(CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write)) {
                        cs.Write(cipherText, 0, cipherText.Length);
                        cs.FlushFinalBlock();
                        plaintext = ms.ToArray();
                    }
                }
            }
            return plaintext;
        }

        public static async Task<byte[]> DecryptByteAsync(byte[] cipherText, byte[] Key, byte[] IV) {
            if(Key.Length != 16 && Key.Length != 24 && Key.Length != 32) {
                throw new ArgumentException($"Invalid key size: {Key.Length}.  AES key must be 16, 24, or 32 bytes.");
            }

            if(IV.Length != 16) {
                throw new ArgumentException($"Invalid IV size: {IV.Length}.  AES IV must be 16 bytes.");
            }

            byte[] plaintext = null;

            using(AesManaged aes = new AesManaged()) {
                aes.Padding = PaddingMode.None;

                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);

                using(MemoryStream ms = new MemoryStream()) {
                    using(CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write)) {
                        try {
                            await cs.WriteAsync(cipherText, 0, cipherText.Length);
                            cs.FlushFinalBlock();
                            plaintext = ms.ToArray();
                        }
                        catch(CryptographicException ex) {
                            Console.WriteLine($"DecryptByteAsync: CryptographicException: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
