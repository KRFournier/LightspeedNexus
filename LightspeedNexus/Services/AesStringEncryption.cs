using System.Security.Cryptography;
using System.Text;

namespace LightspeedNexus.Services;

public static class AesStringEncryption
{
    private static readonly byte[] FixedKey = [
        0xf2, 0xd3, 0xe2, 0x0d, 0xd4, 0x5e, 0xe4, 0xb2, 0xdc, 0x02, 0x6c, 0x8e, 0x89, 0x12, 0xbe, 0x77,
        0xe8, 0x18, 0xeb, 0x40, 0x74, 0x88, 0x8a, 0xe1, 0xf3, 0xff, 0x2b, 0x22, 0xef, 0x12, 0x53, 0x23
        ];

    public static string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = FixedKey; // Set the fixed key

        // Generate a new, random IV for each encryption operation
        aesAlg.GenerateIV();
        byte[] iv = aesAlg.IV;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new();

        // Prepend the IV to the encrypted data so it can be used for decryption
        msEncrypt.Write(iv, 0, iv.Length);

        using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            using StreamWriter swEncrypt = new(csEncrypt);
            swEncrypt.Write(plainText);
        }

        // Convert the memory stream to a Base64 string for easy storage/transmission
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public static string DecryptString(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        byte[] fullCipher = Convert.FromBase64String(cipherText);

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = FixedKey; // Set the fixed key

        // Extract the IV from the beginning of the ciphertext
        byte[] iv = new byte[aesAlg.BlockSize / 8]; // IV size is BlockSize / 8 bytes
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aesAlg.IV = iv;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new();

        // Create a memory stream from the ciphertext (excluding the IV part)
        using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Write))
        {
            csDecrypt.Write(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        }

        // Convert the decrypted bytes back to a string
        return Encoding.UTF8.GetString(msDecrypt.ToArray());
    }
}