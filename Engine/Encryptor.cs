using System.Security.Cryptography;
using System.Text;

namespace XENCNG.Engine
{
    public static class Encryptor
    {
        public static void EncryptFile(string path, string password, bool saveKey, bool useHWID, int layers)
        {
            Console.WriteLine($"[XENCNG] Encryption started: {path}");

            byte[] random64 = RandomNumberGenerator.GetBytes(64);
            byte[] aesKey = RandomNumberGenerator.GetBytes(32);
            byte[] aesIV = RandomNumberGenerator.GetBytes(16);

            // Argon2 is only used to harden the user password and protect AES Key/IV
            byte[] fullKey = KeyGenerator.DeriveFinalKey(password, random64, aesKey, aesIV);

            if (saveKey)
                KeyGenerator.SaveKeyfile(path, fullKey, random64, aesKey, aesIV, layers);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] data = File.ReadAllBytes(path);
            var mem = new MemoryStream(data);

            for (int i = 0; i < layers; i++)
            {
                mem.Position = 0;
                var tmp = new MemoryStream();
                using (var cs = new CryptoStream(tmp, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    mem.CopyTo(cs);
                mem = new MemoryStream(tmp.ToArray());

                if (i % (layers / 10 + 1) == 0)
                    Console.WriteLine($"[DEBUG] AES Layer: {i}/{layers}");
            }

            File.WriteAllBytes(path + ".xenc", mem.ToArray());
            Console.WriteLine("[XENCNG] Encryption completed.");
        }

        public static void DecryptFile(string path, string password)
        {
            Console.WriteLine($"[XENCNG] Decryption started: {path}");

            string originalPath = path.Replace(".xenc", "");
            var (random64, encryptedKey, aesKey, aesIV, layers) = KeyGenerator.LoadKeyfileWithLayer(originalPath);

            byte[] derived = KeyGenerator.DeriveFinalKey(password, random64, aesKey, aesIV);
            if (!derived.SequenceEqual(encryptedKey))
                throw new UnauthorizedAccessException("Invalid password.");

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] data = File.ReadAllBytes(path);
            var mem = new MemoryStream(data);

            for (int i = 0; i < layers; i++)
            {
                mem.Position = 0;
                var tmp = new MemoryStream();
                using (var cs = new CryptoStream(tmp, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    mem.CopyTo(cs);
                mem = new MemoryStream(tmp.ToArray());

                if (i % (layers / 10 + 1) == 0)
                    Console.WriteLine($"[DEBUG] AES Unlayer: {i}/{layers}");
            }

            File.WriteAllBytes(originalPath + ".xdec", mem.ToArray());
            Console.WriteLine("[XENCNG] Decryption completed.");
        }
    }
}
