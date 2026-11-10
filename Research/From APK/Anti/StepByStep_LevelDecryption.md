# Step-by-Step Guide: Decrypting 1400+ Competitor Levels

This guide provides a clear path to extracting the raw gameplay data from the competitor's encrypted files.

---

## Phase 1: Finding the "Master Key" (The Research)
Before you can run a bulk decryption, you must confirm the exact **AES Key** and **IV**.

1.  **Download dnSpy**: [dnSpy on GitHub](https://github.com/dnSpy/dnSpy/releases). This is the best tool for reading Unity DLLs.
2.  **Open the Gameplay DLL**: 
    - Launch dnSpy.
    - Drag and drop `C:\Users\Muhammad Ali\Downloads\snakeescape\AuxiliaryFiles\GameAssemblies\Game.MainGame.Gameplay.dll` into dnSpy.
3.  **Search for Decryption Code**:
    - Press `Ctrl+Shift+K` to search.
    - Type `Decrypt` or `AES`.
    - Look for a method that takes a `string` (the Base64 from the JSON) and returns a `string` (the raw JSON).
4.  **Note the Hardcoded Strings**: 
    - Once you find the decryption function, you will see two strings (or variables) being used as the Key and IV. 
    - **Expected format**: 16-character strings (e.g., `c3629b9207ffe243`).

---

## Phase 2: Setting up the Decryption Lab
We will use **Node.js** to handle the bulk decryption because it is fast and easy to script.

1.  **Install Node.js**: (You already have this installed based on our previous check).
2.  **Create a Workspace**: Create a folder named `LevelExtractor` on your desktop or in your project.
3.  **Create `decrypt.js`**: Copy the script provided in Phase 3 into this file.

---

## Phase 3: The Decryption Script (Automation)
Once you have the **KEY** and **IV** from Phase 1, use this script to process all 1400+ files in seconds.

```javascript
const crypto = require('crypto');
const fs = require('fs');
const path = require('path');

// --- INSERT KEYS FROM DNSPY HERE ---
const KEY = 'c3629b9207ffe243'; // Example (Replace with confirmed key)
const IV = '041e4bc5fecbe593';  // Example (Replace with confirmed IV)
// ------------------------------------

const inputDir = 'C:/Users/Muhammad Ali/Downloads/snakeescape/ExportedProject/Assets/Addressables/asset-game-level';
const outputDir = 'C:/Users/Muhammad Ali/ArrowMaze/Research/From APK/Anti/DecryptedLevels';

// Create output directory if it doesn't exist
if (!fs.existsSync(outputDir)) fs.mkdirSync(outputDir, { recursive: true });

function decryptFile(fileName) {
    try {
        const filePath = path.join(inputDir, fileName);
        const encryptedB64 = fs.readFileSync(filePath, 'utf8');
        const encryptedData = Buffer.from(encryptedB64, 'base64');
        
        // Setup Decipher (Change 'aes-128-cbc' if dnSpy shows a different mode)
        const decipher = crypto.createDecipheriv('aes-128-cbc', KEY, IV);
        let decrypted = decipher.update(encryptedData);
        decrypted = Buffer.concat([decrypted, decipher.final()]);
        
        const outputFilePath = path.join(outputDir, fileName);
        fs.writeFileSync(outputFilePath, decrypted.toString('utf8'));
        console.log(`[SUCCESS] Decrypted: ${fileName}`);
    } catch (err) {
        console.error(`[ERROR] Failed to decrypt ${fileName}: ${err.message}`);
    }
}

// Read all levels
const files = fs.readdirSync(inputDir).filter(f => f.endsWith('.json'));
console.log(`Found ${files.length} levels. Starting decryption...`);

files.forEach(decryptFile);
console.log('--- Finished! ---');
```

---

## Phase 4: Validating the Data
1.  **Check the Output**: Go to `C:\Users\Muhammad Ali\ArrowMaze\Research\From APK\Anti\DecryptedLevels`.
2.  **Open a File**: Open `lv10.json` (decrypted). It should now look like a proper JSON object:
    ```json
    {
      "width": 10,
      "height": 10,
      "grid": [...],
      "objects": [...]
    }
    ```
3.  **Study the Schema**: Compare this JSON structure with your `LevelData.cs` in `ArrowMaze`. You can now build a parser to load these 1400 levels directly into your game!

---

> [!IMPORTANT]
> If `decipher.final()` fails with "wrong final block length" or "bad decrypt", it means either the Key/IV is wrong or the encryption mode is not `aes-128-cbc`. Most Unity mobile games use `CBC` or `ECB`. Check dnSpy to be 100% sure.
