# 🔐 XENCNG v5.0 – Quantum-Hardened File Encryption
**FREE and Open source**

**XENCNG (eXtended Encryption Next-Generation)** is a powerful, next-gen file encryption system built for **maximum protection** against brute-force, forensic, and even future quantum attacks. It uses layered AES-256 encryption combined with Argon2id key derivation and adaptive hardening.

---

## 🛡 Features

- ✅ **AES-256** encryption with **up to 100,000,000 recursive layers**
- ✅ **Argon2id** password hardening (256MB RAM, multi-threaded)
- ✅ **Keyfile-based** decryption (cannot decrypt without it)
- ✅ Optional **HWID binding** for device-specific protection
- ✅ **Self-contained EXE** (built with .NET 8, no install required)
- ✅ Portable & offline – no external dependencies
- ✅ Supports **file** or **folder** encryption

---

## 🚀 How It Works

1. User selects a file or folder
2. Enters a password
3. XENCNG generates:
   - Random 64-byte salt
   - AES key and IV (via Argon2id)
   - Optional HWID lock
4. File is encrypted **layer by layer** (e.g., 1,000,000× AES)
5. All metadata is stored in `.xkey` file

---

## 🔒 Security Design

| Component            | Description                                   |
|---------------------|-----------------------------------------------|
| AES-256             | Symmetric block cipher, CBC mode              |
| Argon2id            | Key derivation with RAM and CPU load          |
| .xkey file          | Stores all needed metadata (salt, key, IV)    |
| Layered encryption  | Recursively encrypts data for resistance      |
| HWID binding        | Optional per-device encryption                |
| Portable runtime    | Hardened .NET 8 single file executable        |

---

## 📦 Usage

```bash
> XENCNG.exe
```

### Input example:

```
=== XENCNG v5.0 - Quantum Hard AES-Stack Encryption ===
Enter file or folder path:
C:\Sensitive\file.txt

Select mode: [encrypt / decrypt]
encrypt

Enter password:
••••••••

Save keyfile? [y/n]: y
Use HWID lock? [y/n]: y

Select AES-layer strength:
  1 = 100x AES (fast, secure)
  2 = 10,000x AES (strong)
  3 = 100,000,000x AES (ultra secure, very slow)

Choice: 3
```

---

## ⚠ Important

- If you lose your `.xkey` file or password, **decryption is impossible**
- Decryption time scales with layer count — 100M layers can take **hours**
- `.xkey` files should be stored **securely and separately**

---

## 📄 License

MIT License (Free for personal and commercial use)

---

## 🧠 Author

**Ali Faruk**  
Germany  
Project: XENCNG v5.0  
Contributions welcome!

---

> “No brute-force, no quantum trick, no forensic tool can break a key that doesn't exist.”
