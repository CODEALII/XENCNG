using XENCNG.Engine;

// Check for help arguments
if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h" || args[0] == "help" || args[0] == "/?"))
{
    ShowHelp();
    return;
}

Console.WriteLine("=== XENCNG v5.0 – Quantum Hard AES-Stack Encryption ===");
Console.WriteLine("🔐 Multi-layer AES-256 encryption with Argon2id key derivation");
Console.WriteLine("📖 For help and usage information, run: XENCNG --help");
Console.WriteLine();

Console.Write("Enter file or folder path: ");
string? inputPathInput = Console.ReadLine();
if (string.IsNullOrWhiteSpace(inputPathInput))
{
    Console.WriteLine("[ERROR] No path provided.");
    return;
}
string inputPath = inputPathInput.Trim();

if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
{
    Console.WriteLine("[ERROR] Path not found.");
    return;
}

Console.Write("Select mode: [encrypt / decrypt]: ");
string? modeInput = Console.ReadLine();
if (string.IsNullOrWhiteSpace(modeInput))
{
    Console.WriteLine("[ERROR] No mode selected.");
    return;
}
string mode = modeInput.Trim().ToLower();

if (mode != "encrypt" && mode != "decrypt")
{
    Console.WriteLine("[ERROR] Invalid mode. Please enter 'encrypt' or 'decrypt'.");
    return;
}

Console.Write("Enter password: ");
string? passwordInput = Console.ReadLine();
if (string.IsNullOrWhiteSpace(passwordInput))
{
    Console.WriteLine("[ERROR] No password provided.");
    return;
}
string password = passwordInput;

if (mode == "encrypt")
{
    Console.Write("Save keyfile? [y/n]: ");
    string? saveKeyInput = Console.ReadLine();
    bool saveKey = saveKeyInput?.ToLower() == "y";

    Console.Write("Use HWID binding? [y/n]: ");
    string? useHWIDInput = Console.ReadLine();
    bool useHWID = useHWIDInput?.ToLower() == "y";

    Console.WriteLine("[XENCNG] Select AES-layer strength:");
    Console.WriteLine("  1 = 100x AES (fast, secure)");
    Console.WriteLine("  2 = 10,000x AES (strong)");
    Console.WriteLine("  3 = 100,000x AES (ultra secure, slower)");
    Console.Write("Choice [1–3]: ");
    
    string? layerModeInput = Console.ReadLine();
    if (!int.TryParse(layerModeInput ?? "1", out int layerMode))
    {
        layerMode = 1; // Default to option 1 if parsing fails
    }

    int layers = layerMode switch
    {
        1 => 100,
        2 => 10_000,
        3 => 100_000,
        _ => 100 // Default to 100 layers for invalid input
    };

    // Process encryption for file or directory
    if (Directory.Exists(inputPath))
    {
        Console.WriteLine($"[XENCNG] Encrypting all files in directory: {inputPath}");
        foreach (var file in FileWalker.GetAllFiles(inputPath))
            Encryptor.EncryptFile(file, password, saveKey, useHWID, layers);
    }
    else
    {
        Encryptor.EncryptFile(inputPath, password, saveKey, useHWID, layers);
    }
}
else // decrypt mode
{
    // Process decryption for file or directory
    if (Directory.Exists(inputPath))
    {
        Console.WriteLine($"[XENCNG] Decrypting all .xenc files in directory: {inputPath}");
        foreach (var file in FileWalker.GetAllFiles(inputPath))
        {
            if (file.EndsWith(".xenc"))
                Encryptor.DecryptFile(file, password);
        }
    }
    else
    {
        Encryptor.DecryptFile(inputPath, password);
    }
}

/// <summary>
/// Displays help information about XENCNG usage and features
/// </summary>
static void ShowHelp()
{
    Console.WriteLine("=== XENCNG v5.0 – Quantum Hard AES-Stack Encryption ===");
    Console.WriteLine();
    Console.WriteLine("🔐 WHAT IS XENCNG?");
    Console.WriteLine("XENCNG (eXtended Encryption Next-Generation) is a powerful file encryption tool");
    Console.WriteLine("that provides quantum-resistant security through multiple layers of AES-256 encryption");
    Console.WriteLine("combined with Argon2id password hardening.");
    Console.WriteLine();
    Console.WriteLine("🛡️  KEY FEATURES:");
    Console.WriteLine("• Multi-layer AES-256 encryption (100 to 100,000 layers)");
    Console.WriteLine("• Argon2id password hardening (256MB RAM, multi-threaded)");
    Console.WriteLine("• Keyfile-based decryption (.xkey files)");
    Console.WriteLine("• Optional HWID binding for device-specific protection");
    Console.WriteLine("• Supports both individual files and entire folders");
    Console.WriteLine("• Quantum-resistant security design");
    Console.WriteLine();
    Console.WriteLine("📖 USAGE:");
    Console.WriteLine("  XENCNG.exe                 Start interactive mode");
    Console.WriteLine("  XENCNG.exe --help          Show this help information");
    Console.WriteLine("  XENCNG.exe -h              Show this help information");
    Console.WriteLine();
    Console.WriteLine("🚀 HOW IT WORKS:");
    Console.WriteLine("1. Select a file or folder to encrypt/decrypt");
    Console.WriteLine("2. Choose encryption or decryption mode");
    Console.WriteLine("3. Enter your password");
    Console.WriteLine("4. For encryption: choose security level (100-100,000 AES layers)");
    Console.WriteLine("5. Optionally save a keyfile (.xkey) for easier decryption");
    Console.WriteLine("6. Optionally bind to hardware ID for device-specific protection");
    Console.WriteLine();
    Console.WriteLine("🔒 SECURITY LEVELS:");
    Console.WriteLine("• Level 1: 100x AES layers (fast, secure)");
    Console.WriteLine("• Level 2: 10,000x AES layers (strong, slower)");
    Console.WriteLine("• Level 3: 100,000x AES layers (ultra secure, much slower)");
    Console.WriteLine();
    Console.WriteLine("📁 FILE OUTPUTS:");
    Console.WriteLine("• Encrypted files: originalname.xenc");
    Console.WriteLine("• Key files: originalname.xkey (if enabled)");
    Console.WriteLine("• Decrypted files: originalname.xdec");
    Console.WriteLine();
    Console.WriteLine("⚠️  IMPORTANT WARNINGS:");
    Console.WriteLine("• If you lose your .xkey file or password, decryption is IMPOSSIBLE");
    Console.WriteLine("• Higher security levels take exponentially longer to process");
    Console.WriteLine("• Store .xkey files securely and separately from encrypted data");
    Console.WriteLine("• Level 3 (100,000 layers) can take hours for large files");
    Console.WriteLine();
    Console.WriteLine("🔧 TECHNICAL DETAILS:");
    Console.WriteLine("• Encryption: AES-256-CBC with PKCS7 padding");
    Console.WriteLine("• Key Derivation: Argon2id (256MB RAM, 4 iterations, 2 threads)");
    Console.WriteLine("• Salt: 64-byte cryptographically secure random");
    Console.WriteLine("• IV: 16-byte cryptographically secure random per layer");
    Console.WriteLine("• Key Length: 32 bytes (256-bit)");
    Console.WriteLine();
    Console.WriteLine("📄 LICENSE: GNU General Public License v3.0");
    Console.WriteLine("🧠 AUTHOR: Ali Faruk (Germany)");
    Console.WriteLine();
    Console.WriteLine("For more information, visit: https://github.com/CODEALII/XENCNG");
}
