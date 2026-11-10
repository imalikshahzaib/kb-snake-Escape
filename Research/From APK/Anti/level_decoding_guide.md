# Level Decoding Guide: Snake Escape

The level files in `Addressables/asset-game-level/*.json` are encrypted using **AES-128** encryption and then encoded in **Base64**. To decode them for educational study, follow this technical breakdown.

## 1. Identified Encryption Keys
Based on the `SandBoxIDConfig.asset` found in the project's Resources, the following 16-character strings are highly likely to be the **AES Key** and **IV (Initialization Vector)**:

- **Potential Key/IV 1**: `c3629b9207ffe243`
- **Potential Key/IV 2**: `041e4bc5fecbe593`
- **Potential Key/IV 3**: `f7cc066e04b9ae38`
- **Potential Key/IV 4**: `ca65f43573ec1236`

In most Unity implementations, one of these is the Key and another is the IV. Common combinations to try are (1 & 2) or (3 & 4).

## 2. Decryption Strategy
The decryption process in the game's code likely follows this flow:
1.  Read the content of `lv[X].json`.
2.  Convert the string from **Base64** to a byte array.
3.  Initialize an AES-128-CBC or AES-128-ECB decipher with the correct Key/IV.
4.  Decrypt the byte array.
5.  Convert the resulting bytes back to a UTF-8 string (which will be the actual JSON).

## 3. Deep Analysis Tools
To find the *exact* decryption logic and confirmed keys, you should perform a static analysis on the following DLL files located in `AuxiliaryFiles/GameAssemblies/`:

- `Game.MainGame.dll`
- `Game.MainGame.Gameplay.dll`
- `GDK.SaveLoadData.dll` (A specialized library for data persistence)

### Recommended Steps for you:
1.  **Use dnSpy or ILSpy**: Open the DLLs mentioned above in one of these tools (dnSpy is recommended as it handles obfuscation better).
2.  **Search for Level Loading**: Search for classes named `LevelManager`, `LevelLoader`, or `LevelService`.
3.  **Find the Decryption Method**: Look for methods like `Decrypt`, `Decode`, or `LoadFromAddressables`.
4.  **Extract the Key**: The keys are often hardcoded as strings in a class like `SecurityUtils`, `CryptoSettings`, or even `Config`.

## 4. Why it's Encrypted
The developer has encrypted these files to:
- Prevent players from modifying levels to cheat.
- Protect their level design intellectual property (over 1400 levels is a significant asset).

## 5. Sample Decryption Script (Node.js)
If you find the correct Key and IV, you can use a script like this to batch-process the levels:

```javascript
const crypto = require('crypto');
const fs = require('fs');

const KEY = '...'; // Insert confirmed key
const IV = '...';  // Insert confirmed IV

function decryptLevel(filePath) {
    const encryptedB64 = fs.readFileSync(filePath, 'utf8');
    const encryptedData = Buffer.from(encryptedB64, 'base64');
    
    const decipher = crypto.createDecipheriv('aes-128-cbc', KEY, IV);
    let decrypted = decipher.update(encryptedData);
    decrypted = Buffer.concat([decrypted, decipher.final()]);
    
    return decrypted.toString('utf8');
}
```

---
*Created for ArrowMaze Research.*
