using XENCNG.Engine;

Console.WriteLine("=== XENCNG v5.0 – Quantum Hard AES-Stack Encryption ===");

Console.Write("Enter file or folder path: ");
string inputPath = Console.ReadLine();

if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
{
    Console.WriteLine("[ERROR] Path not found.");
    return;
}

Console.Write("Select mode: [encrypt / decrypt]: ");
string mode = Console.ReadLine()?.Trim().ToLower();

if (mode != "encrypt" && mode != "decrypt")
{
    Console.WriteLine("[ERROR] Invalid mode.");
    return;
}

Console.Write("Enter password: ");
string password = Console.ReadLine();

if (mode == "encrypt")
{
    Console.Write("Save keyfile? [y/n]: ");
    bool saveKey = Console.ReadLine()?.ToLower() == "y";

    Console.Write("Use HWID binding? [y/n]: ");
    bool useHWID = Console.ReadLine()?.ToLower() == "y";

    Console.WriteLine("[XENCNG] Select AES-layer strength:");
    Console.WriteLine("  1 = 100x AES (fast, secure)");
    Console.WriteLine("  2 = 10,000x AES (strong)");
    Console.WriteLine("  3 = 100,000x AES (ultra secure, slower)");
    Console.Write("Choice [1–3]: ");
    int layerMode = int.Parse(Console.ReadLine() ?? "1");

    int layers = layerMode switch
    {
        1 => 100,
        2 => 10_000,
        3 => 100_000,
        _ => 100
    };

    if (Directory.Exists(inputPath))
    {
        foreach (var file in FileWalker.GetAllFiles(inputPath))
            Encryptor.EncryptFile(file, password, saveKey, useHWID, layers);
    }
    else
    {
        Encryptor.EncryptFile(inputPath, password, saveKey, useHWID, layers);
    }
}
else
{
    if (Directory.Exists(inputPath))
    {
        foreach (var file in FileWalker.GetAllFiles(inputPath))
            Encryptor.DecryptFile(file, password);
    }
    else
    {
        Encryptor.DecryptFile(inputPath, password);
    }
}
