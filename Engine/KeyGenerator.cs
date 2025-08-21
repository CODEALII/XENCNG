using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;

namespace XENCNG.Engine
{
    /// <summary>
    /// Handles key derivation, keyfile creation, and keyfile loading for XENCNG.
    /// Uses Argon2id for password hardening and secure key derivation.
    /// </summary>
    public static class KeyGenerator
    {
        /// <summary>
        /// Derives a final verification key from user password and encryption parameters using Argon2id.
        /// This key is used to verify password correctness during decryption.
        /// Argon2id provides memory-hard and time-hard key stretching for quantum resistance.
        /// </summary>
        /// <param name="password">User-provided password</param>
        /// <param name="salt">64-byte cryptographically secure salt</param>
        /// <param name="aesKey">32-byte AES encryption key</param>
        /// <param name="aesIV">16-byte AES initialization vector</param>
        /// <returns>64-byte derived key for password verification</returns>
        public static byte[] DeriveFinalKey(string password, byte[] salt, byte[] aesKey, byte[] aesIV)
        {
            // Combine password with AES key and IV to create input for Argon2
            // This ensures the derived key depends on both password and encryption parameters
            byte[] input = Encoding.UTF8.GetBytes(password)
                .Concat(aesKey)
                .Concat(aesIV)
                .ToArray();

            // Configure Argon2id with strong parameters for quantum resistance
            var config = new Argon2Config
            {
                Type = Argon2Type.HybridAddressing, // Argon2id: hybrid of Argon2i and Argon2d
                Version = Argon2Version.Nineteen,   // Latest Argon2 version
                TimeCost = 4,                       // 4 iterations (time complexity)
                MemoryCost = 1024 * 256,           // 256MB memory usage (memory complexity)
                Lanes = 2,                         // 2 parallel lanes
                Threads = 2,                       // 2 threads for computation
                HashLength = 64,                   // 64-byte output hash
                Salt = salt,                       // 64-byte salt for uniqueness
                Password = input                   // Combined password + AES parameters
            };

            // Perform Argon2id key derivation
            var argon2 = new Argon2(config);
            return argon2.Hash().Buffer;
        }

        /// <summary>
        /// Saves encryption metadata to a keyfile for later decryption.
        /// The keyfile contains all necessary information to decrypt the file,
        /// except for the user password which must be provided separately.
        /// </summary>
        /// <param name="filePath">Original file path (keyfile will be filePath + ".xkey")</param>
        /// <param name="derivedKey">64-byte Argon2id-derived verification key</param>
        /// <param name="salt">64-byte salt used for key derivation</param>
        /// <param name="aesKey">32-byte AES encryption key</param>
        /// <param name="aesIV">16-byte AES initialization vector</param>
        /// <param name="layers">Number of encryption layers applied</param>
        public static void SaveKeyfile(string filePath, byte[] derivedKey, byte[] salt, byte[] aesKey, byte[] aesIV, int layers)
        {
            string keyPath = filePath + ".xkey";

            // Create keyfile with binary structure:
            // Bytes 0-63:   64-byte salt
            // Bytes 64-127: 64-byte derived verification key
            // Bytes 128-159: 32-byte AES key
            // Bytes 160-175: 16-byte AES IV
            // Bytes 176-179: 4-byte layer count (little-endian)
            using var ms = new MemoryStream();
            ms.Write(salt);                           // 64 bytes: salt
            ms.Write(derivedKey);                     // 64 bytes: verification key  
            ms.Write(aesKey);                         // 32 bytes: AES key
            ms.Write(aesIV);                          // 16 bytes: AES IV
            ms.Write(BitConverter.GetBytes(layers));  // 4 bytes: layer count
            
            File.WriteAllBytes(keyPath, ms.ToArray());
            Console.WriteLine("[XENCNG] Keyfile saved: " + keyPath);
        }

        /// <summary>
        /// Loads encryption metadata from a keyfile for decryption.
        /// Parses the binary keyfile structure to extract all necessary decryption parameters.
        /// </summary>
        /// <param name="filePath">Original file path (keyfile expected at filePath + ".xkey")</param>
        /// <returns>Tuple containing: (salt, derivedKey, aesKey, aesIV, layers)</returns>
        /// <exception cref="FileNotFoundException">Thrown when keyfile doesn't exist</exception>
        public static (byte[] salt, byte[] derivedKey, byte[] aesKey, byte[] aesIV, int layers) LoadKeyfileWithLayer(string filePath)
        {
            string keyPath = filePath + ".xkey";
            if (!File.Exists(keyPath))
                throw new FileNotFoundException("Keyfile not found");

            // Read and parse keyfile binary structure:
            byte[] full = File.ReadAllBytes(keyPath);
            byte[] salt = full.Take(64).ToArray();              // Bytes 0-63: salt
            byte[] derivedKey = full.Skip(64).Take(64).ToArray(); // Bytes 64-127: verification key
            byte[] aesKey = full.Skip(128).Take(32).ToArray();   // Bytes 128-159: AES key
            byte[] aesIV = full.Skip(160).Take(16).ToArray();    // Bytes 160-175: AES IV
            int layers = BitConverter.ToInt32(full.Skip(176).Take(4).ToArray()); // Bytes 176-179: layer count
            
            return (salt, derivedKey, aesKey, aesIV, layers);
        }
    }
}
