/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: EncryptionOperation                                          //////////
//////////  Created: 10/02/2019                                                //////////
//////////  Modified: 10/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Store a collection of settings that are used to encrypt/decrypt    //////////
//////////  data that is identified by the application                         //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;

using CipherIO.Attributes;
using CipherIO.Encryption;

namespace CipherIO {
    /// <summary>
    /// Store a collection of settings values that can be modified for encryption operations
    /// </summary>
    public sealed class EncryptionOperation {
        /*----------Variables----------*/
        //PUBLIC

        [CallableStringSetter("-cipherKey", "Set the phrase key that will be used to encrypt/decrypt the file data", "-cipherKey phrase")]
        public string cipherKey = string.Empty;

        [CallableStringSetter("-targetPath", "The file/directory that is being targeted by the encryption/decryption operation", "-targetPath filepath")]
        public string targetPath = string.Empty;

        [CallableStringSetter("-destinationPath", "The destination file for an encryption operation or directory to store the decrypted files within", "-destinationPath filepath")]
        public string destinationPath = string.Empty;

        [CallableBoolSetter("-removeOriginals", "Flags if the original files should be deleted at the end of a encryption/decryption operation. Defaults to false", "-removeOriginals (true/false)")]
        public bool removeOriginals = false;

        [CallableBoolSetter("-includeSubDirectories", "Flags if sub directories should be encrypted along along the root files if targetPath is assigned to a directory", "-includeSubDirectories (true/false)")]
        public bool includeSubDirectories = true;

        [CallableIntSetter("-bufferSize", "Indicates the size of the buffer that will be used to handle the reading/writing of files during the encryption/decryption process. Defaults to 256MB", "-bufferSize (Size in Bytes)")]
        public uint bufferSize = 268435456;

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Start an encryption operation with the current settings
        /// </summary>
        [CallableMethod("-encrypt", "Begin an encryption operation with the current settings", "-encrypt")]
        public void Encrypt() {
            Console.WriteLine("Coming Soon: Encryption Operations!");

            //Testing
            VigenèreCipherKey key = new VigenèreCipherKey(cipherKey);

            //Take the byte data of the supplied target path
            byte[] targetBytes = new byte[sizeof(char) * targetPath.Length];
            for (int i = 0; i < targetPath.Length; i++)
                Array.Copy(BitConverter.GetBytes(targetPath[i]), 0, targetBytes, i * sizeof(char), sizeof(char));

            //Encrypt the byte data
            key.Encrypt(ref targetBytes);

            //Convert the byte data back to a string 
            string encrypted = string.Empty;
            for (int i = 0; i < targetBytes.Length; i += 2)
                encrypted += BitConverter.ToChar(targetBytes, i);

            //Log the encrypted text
            Console.WriteLine($"Encrypted Target Path: {encrypted}");

            //Testing
        }

        /// <summary>
        /// Start an encryption operation with the current settings
        /// </summary>
        [CallableMethod("-decrypt", "Begin a decryption operation with the current settings", "-decrypt")]
        public void Decrypt() {
            Console.WriteLine("Coming Soon: Decryption Operations!");
        }

        /// <summary>
        /// Display all of the help options for this current object
        /// </summary>
        [CallableMethod("-help", "Display the possible options for this application", "-help")]
        public void Help() {
            Console.Write("\n-------------------- Possible Options --------------------\n");
            CallableInformationBreakdown.Log(this);
            Console.Write("----------------------------------------------------------\n\n");
        }

        /// <summary>
        /// Close and exit the current program
        /// </summary>
        [CallableMethod("-exit", "Close and exit the current application", "-exit")]
        public void Exit() { Environment.Exit(0); }
    }
}
