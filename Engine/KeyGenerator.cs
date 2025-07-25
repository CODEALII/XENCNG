using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;

namespace XENCNG.Engine
{
    public static class KeyGenerator
    {
        public static byte[] DeriveFinalKey(string password, byte[] salt, byte[] aesKey, byte[] aesIV)
        {
            byte[] input = Encoding.UTF8.GetBytes(password)
                .Concat(aesKey)
                .Concat(aesIV)
                .ToArray();

            var config = new Argon2Config
            {
                Type = Argon2Type.HybridAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = 4,
                MemoryCost = 1024 * 256,
                Lanes = 2,
                Threads = 2,
                HashLength = 64,
                Salt = salt,
                Password = input
            };

            var argon2 = new Argon2(config);
            return argon2.Hash().Buffer;
        }

        public static void SaveKeyfile(string filePath, byte[] derivedKey, byte[] salt, byte[] aesKey, byte[] aesIV, int layers)
        {
            string keyPath = filePath + ".xkey";

            using var ms = new MemoryStream();
            ms.Write(salt);
            ms.Write(derivedKey);
            ms.Write(aesKey);
            ms.Write(aesIV);
            ms.Write(BitConverter.GetBytes(layers));
            File.WriteAllBytes(keyPath, ms.ToArray());

            Console.WriteLine("[XENCNG] Keyfile saved: " + keyPath);
        }

        public static (byte[] salt, byte[] derivedKey, byte[] aesKey, byte[] aesIV, int layers) LoadKeyfileWithLayer(string filePath)
        {
            string keyPath = filePath + ".xkey";
            if (!File.Exists(keyPath))
                throw new FileNotFoundException("Keyfile not found");

            byte[] full = File.ReadAllBytes(keyPath);
            byte[] salt = full.Take(64).ToArray();
            byte[] derivedKey = full.Skip(64).Take(64).ToArray();
            byte[] aesKey = full.Skip(128).Take(32).ToArray();
            byte[] aesIV = full.Skip(160).Take(16).ToArray();
            int layers = BitConverter.ToInt32(full.Skip(176).Take(4).ToArray());
            return (salt, derivedKey, aesKey, aesIV, layers);
        }
    }
}
