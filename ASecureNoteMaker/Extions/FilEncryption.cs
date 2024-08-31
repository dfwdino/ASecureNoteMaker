using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class FilEncryption
{
    public static void EncryptFile(string text, string outputFilePath, string passphrase)
    {
        // Generate key and IV from the passphrase
        using (var aes = Aes.Create())
        {
            byte[] salt = GenerateSalt();
            var key = DeriveKeyAndIV(passphrase, salt, aes.KeySize / 8, aes.BlockSize / 8);

            aes.Key = key.Key;
            aes.IV = key.IV;

            using (var fsInput = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            using (var fsEncrypted = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                // Write the salt to the output file
                fsEncrypted.Write(salt, 0, salt.Length);

                using (var cryptoStream = new CryptoStream(fsEncrypted, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    fsInput.CopyTo(cryptoStream);
                }
            }
        }
    }

    public static string DecryptFile(string inputFilePath, string passphrase)
    {
        using (var aes = Aes.Create())
        {
            using (var fsEncrypted = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the salt from the input file
                byte[] salt = new byte[16];
                fsEncrypted.Read(salt, 0, salt.Length);

                var key = DeriveKeyAndIV(passphrase, salt, aes.KeySize / 8, aes.BlockSize / 8);

                aes.Key = key.Key;
                aes.IV = key.IV;

                using (var cryptoStream = new CryptoStream(fsEncrypted, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var msDecrypted = new MemoryStream())
                {
                    cryptoStream.CopyTo(msDecrypted);
                    return Encoding.UTF8.GetString(msDecrypted.ToArray());
                }
            }
        }
    }



    private static (byte[] Key, byte[] IV) DeriveKeyAndIV(string passphrase, byte[] salt, int keySize, int ivSize)
    {
        using (var rfc2898 = new Rfc2898DeriveBytes(passphrase, salt, 100000))
        {
            byte[] key = rfc2898.GetBytes(keySize);
            byte[] iv = rfc2898.GetBytes(ivSize);
            return (key, iv);
        }
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }
}
