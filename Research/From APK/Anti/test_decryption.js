const crypto = require('crypto');
const fs = require('fs');

const dataB64 = "408OXC5Ep6yVfaUkAyBUMGDviPCb12Ac1+0q6U4T4ohkEnbXEiW4vFrLUSTmF8MohlmXusLVLy3knofBLfvx3SocBEz8Q+4huQp5hFAvbUcEZPpi0n4kxi5hqy5GDSXW6B3MJG5pP6erQGn1kITUArPcpockCc9CBzN6jt2rpe2IGyqMvQbOfDac01hD3B9SiQ/xox6dsM2ipjxtmTTX9rfaphwSyLV6HyTAvfccpIgkiO76wTaGCvJIlOFOcor5Fo4gEMknVoKG88P/RKjmjpXtrycVM7+WNg74qdLw+ey4//4lvcvQZaPzx34roeSFrMCRcr3sl/uCiZTpeIN2pwwXx9skIHL78o0l9Jok2no0EJdMekg7DksxzsIUxFmZBUZtmqULde2KsiiGNL86i5yRq0uox/7/RJBeAXWDJ/FIDJDN1cKWQk7b2fDFSjJOlooUj/xDsjw3m5MMO07Cn54FiyOK2hbXiLJKZE/hNAltO2hKbHiASijw8/cGuwXgDolZBoHwXL5l0nwBfm6nwIN5snB5o0S7QMuSPQLWCXDMOdb7NdeUoIiLac6a6Vv8x5HPx7RiQn6M+f5ozKAYB5ajxmrfNB/SQMoTdxd8vdHoRzCPj1oH+MRYVcXwvjEErF4EW6o4ijfAFYtzGcmY0jLv5XKEDWLzdON0KR4vGTaXuUT2fpUNjnxWwtZ9V7vKYBfRmDEiab/eH7i8opxy0YPYIThCit/FMMpwmMRvOyyhNXhAZB1OLh4lwPSqG3fXz2tH3X8a9s+7lrijXZzcdSXdSoRWH1HGuk63LQnvRfD2VBzj6J8jDFPHAB2FwdBNIrdJpC9oAFTHLNkkXwYLXWqMMklb+vXrOxkiS/4sB92xxJzK1mbZLSv7rjjIv+iCPbVogQiWRSgSSAsftUEgCmPj+PyXt/CY8pb2qZqQzR953cAUGZ5jR/GxVSQ6xooWiBRLoPRmVvkaAwtnBIDHuJcBVbPeVCirQmjiRSxhBBVkJjWdR+zx3uI/NTPhCHDa4Ejf7p8n6/0ihAAXcNhgG4vfVeR5U3nVL8s69/vwnfAcB1b+d4AUxs+iTLCLRcqO9q6gLkRqp1SuZclW2go4WjKzr2A4MxMeRT+3I/MF+3Byd7dogpRaUOB8lSO6Uu8XVNkIdCMiEyvUB/D4wROK0Z3YECMwN2uCroIP74zm1JzfRhLntXvUygulVIievHvSesOvEwBSZjf2CDgh5cdhyg==";
const data = Buffer.from(dataB64, 'base64');
const candidates = [
    "66D3488293D1E14D",
    "066F1F46827CA7E3",
    "258B92A395D874C3",
    "DEFA620D07FBA633",
    "D7ABA3C227A032BC",
    "D0587E3EBC655679",
    "D3D70B00E26CE0ED",
    "1F1DC29669F2FF53",
    "c3629b9207ffe243",
    "041e4bc5fecbe593",
    "f7cc066e04b9ae38",
    "ca65f43573ec1236"
];

candidates.forEach((keyStr, kIdx) => {
    candidates.forEach((ivStr, vIdx) => {
        try {
            // Try as strings
            let decipher = crypto.createDecipheriv('aes-128-cbc', keyStr, ivStr);
            let decrypted = decipher.update(data);
            decrypted = Buffer.concat([decrypted, decipher.final()]);
            console.log(`FOUND! Key: ${keyStr}, IV: ${ivStr} (String mode)`);
            console.log(decrypted.toString('utf8'));
            process.exit(0);
        } catch (e) {}

        try {
            // Try as hex buffers
            const keyBuf = Buffer.from(keyStr, 'hex');
            const ivBuf = Buffer.from(ivStr, 'hex');
            if (keyBuf.length === 16 && ivBuf.length === 16) {
                let decipher = crypto.createDecipheriv('aes-128-cbc', keyBuf, ivBuf);
                let decrypted = decipher.update(data);
                decrypted = Buffer.concat([decrypted, decipher.final()]);
                console.log(`FOUND! Key: ${keyStr}, IV: ${ivStr} (Hex mode)`);
                console.log(decrypted.toString('utf8'));
                process.exit(0);
            }
        } catch (e) {}
    });
});
console.log("No match found in current candidates.");
