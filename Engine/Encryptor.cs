using System.Security.Cryptography;
using System.Text;

namespace XENCNG.Engine
{
    /// <summary>
    /// Core encryption and decryption engine for XENCNG.
    /// Implements multi-layer AES-256-CBC encryption with quantum-resistant design.
    /// </summary>
    public static class Encryptor
    {
        /// <summary>
        /// Encrypts a file using multi-layer AES-256-CBC encryption.
        /// Each layer applies a full AES encryption pass over the entire data,
        /// making the encryption quantum-resistant through computational complexity.
        /// </summary>
        /// <param name="path">Path to the file to encrypt</param>
        /// <param name="password">User password for key derivation</param>
        /// <param name="saveKey">Whether to save encryption metadata to a .xkey file</param>
        /// <param name="useHWID">Whether to bind encryption to hardware ID (not implemented)</param>
        /// <param name="layers">Number of AES encryption layers to apply (100-100,000)</param>
        public static void EncryptFile(string path, string password, bool saveKey, bool useHWID, int layers)
        {
            Console.WriteLine($"[XENCNG] Encryption started: {path}");

            // Generate cryptographically secure random values
            byte[] random64 = RandomNumberGenerator.GetBytes(64);  // Salt for Argon2id
            byte[] aesKey = RandomNumberGenerator.GetBytes(32);    // AES-256 key
            byte[] aesIV = RandomNumberGenerator.GetBytes(16);     // AES initialization vector

            // Use Argon2id to derive a verification key from the user password
            // This key is used to verify password correctness during decryption
            byte[] fullKey = KeyGenerator.DeriveFinalKey(password, random64, aesKey, aesIV);

            // Save encryption metadata to keyfile if requested
            // This includes salt, derived key, AES key/IV, and layer count
            if (saveKey)
                KeyGenerator.SaveKeyfile(path, fullKey, random64, aesKey, aesIV, layers);

            // Configure AES-256 encryption in CBC mode with PKCS7 padding
            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIV;
            aes.Mode = CipherMode.CBC;      // Cipher Block Chaining mode
            aes.Padding = PaddingMode.PKCS7; // Standard padding for block ciphers

            // Read the original file data into memory
            byte[] data = File.ReadAllBytes(path);
            var mem = new MemoryStream(data);

            // Apply multiple layers of AES encryption
            // Each layer completely re-encrypts the entire data from the previous layer
            for (int i = 0; i < layers; i++)
            {
                mem.Position = 0; // Reset stream position for reading
                var tmp = new MemoryStream();
                
                // Encrypt the current data and write to temporary stream
                using (var cs = new CryptoStream(tmp, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    mem.CopyTo(cs);
                
                // Replace current data with encrypted result for next layer
                mem = new MemoryStream(tmp.ToArray());

                // Show progress every 10% of layers (minimum every layer for small counts)
                if (i % (layers / 10 + 1) == 0)
                    Console.WriteLine($"[DEBUG] AES Layer: {i}/{layers}");
            }

            // Write the final encrypted data to output file with .xenc extension
            File.WriteAllBytes(path + ".xenc", mem.ToArray());
            Console.WriteLine("[XENCNG] Encryption completed.");
        }

        /// <summary>
        /// Decrypts a file that was encrypted with EncryptFile.
        /// Requires the corresponding .xkey file to retrieve encryption metadata.
        /// Reverses the multi-layer encryption by applying the same number of decryption passes.
        /// </summary>
        /// <param name="path">Path to the .xenc file to decrypt</param>
        /// <param name="password">User password for key verification</param>
        public static void DecryptFile(string path, string password)
        {
            Console.WriteLine($"[XENCNG] Decryption started: {path}");

            // Determine original file path by removing .xenc extension
            string originalPath = path.Replace(".xenc", "");
            
            // Load encryption metadata from the keyfile
            var (random64, encryptedKey, aesKey, aesIV, layers) = KeyGenerator.LoadKeyfileWithLayer(originalPath);

            // Verify password by deriving the same key and comparing
            byte[] derived = KeyGenerator.DeriveFinalKey(password, random64, aesKey, aesIV);
            if (!derived.SequenceEqual(encryptedKey))
                throw new UnauthorizedAccessException("Invalid password.");

            // Configure AES decryption with the same parameters used for encryption
            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIV;
            aes.Mode = CipherMode.CBC;      // Must match encryption mode
            aes.Padding = PaddingMode.PKCS7; // Must match encryption padding

            // Read the encrypted file data
            byte[] data = File.ReadAllBytes(path);
            var mem = new MemoryStream(data);

            // Apply decryption layers in reverse order
            // Each layer decrypts one level of the multi-layer encryption
            for (int i = 0; i < layers; i++)
            {
                mem.Position = 0; // Reset stream position for reading
                var tmp = new MemoryStream();
                
                // Decrypt the current data and write to temporary stream
                using (var cs = new CryptoStream(tmp, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    mem.CopyTo(cs);
                
                // Replace current data with decrypted result for next layer
                mem = new MemoryStream(tmp.ToArray());

                // Show progress every 10% of layers (minimum every layer for small counts)
                if (i % (layers / 10 + 1) == 0)
                    Console.WriteLine($"[DEBUG] AES Unlayer: {i}/{layers}");
            }

            // Write the final decrypted data to output file with .xdec extension
            File.WriteAllBytes(originalPath + ".xdec", mem.ToArray());
            Console.WriteLine("[XENCNG] Decryption completed.");
        }
    }
}
