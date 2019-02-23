/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: VigenèreCipherKey                                            //////////
//////////  Created: 10/02/2019                                                //////////
//////////  Modified: 23/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Manage a collection of byte data that is used to provide           //////////
//////////  encryption/decryption via a Vigenère Cipher method of modifying    //////////
//////////  byte data via a consistent amount                                  //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace CipherIO.Encryption {
    /// <summary>
    /// Offset supplied byte data based on a supplied encryption key
    /// </summary>
    public sealed class VigenèreCipherKey {
        /*----------Variables----------*/
        //PRIVATE

        /// <summary>
        /// The byte values that will be used to encrypt/decrypt the supplied data values
        /// </summary>
        private byte[] encryptionKey;

        /// <summary>
        /// Store the current progress through the encryption key values to be able to consistently modify values in buffered chunks
        /// </summary>
        private uint progress;

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Retrieve the next value in the encryption sequence
        /// </summary>
        private byte Next() {
            //Check there are key values to use
            if (encryptionKey.Length == 0) return 0;

            //Use the next byte value
            byte val = encryptionKey[Progress++];

            //Return the value
            return val;
        }

        //PUBLIC

        /// <summary>
        /// Get and set the current progress through the encryption key
        /// </summary>
        public uint Progress {
            get { return progress; }
            set { progress = value % (uint)encryptionKey.Length; }
        }

        //PUBLIC

        /// <summary>
        /// The phrase that will be used to encrypt/decrypt data with this key
        /// </summary>
        /// <param name="phrase">A Unicode line of text that will be used to encrypt/decrypt the supplied data</param>
        public VigenèreCipherKey(string phrase) {
            //Null check the phrase
            if (phrase == null) phrase = string.Empty;

            //Store a buffer for the character conversion
            byte[] buffer;

            //Create a list for the data converted key values
            List<byte> keyBasis = new List<byte>(sizeof(char) * phrase.Length);

            //Convert the phrase to byte data that can be used for operations
            for (int c = 0; c < phrase.Length; c++) {
                //Convert the character to the base byte value
                buffer = BitConverter.GetBytes(phrase[c]);

                //Add the data to the basis buffer if it's a valid value
                for (int b = 0; b < buffer.Length; b++) {
                    //Check that the byte has a value
                    if (buffer[b] != 0) keyBasis.Add(buffer[b]);
                }
            }

            //Copy the byte data to the key buffer
            encryptionKey = keyBasis.ToArray();
        }

        /// <summary>
        /// Return the Key back to it's initial state
        /// </summary>
        public void Reset() { progress = 0; }

        /// <summary>
        /// Modify the supplied data values in the forward direction 
        /// </summary>
        /// <param name="data">The data that is to be modified by the operation</param>
        /// <param name="count">The number of bytes to be processed, measured from the start of the buffer</param>
        public void Encrypt(ref byte[] data, int count) {
            unchecked {
                //Loop through the byte data and apply the offset values
                for (int i = 0; i < count; i++)
                    data[i] += Next();
            }
        }

        /// <summary>
        /// Modify the supplied data values in the reverse direction
        /// </summary>
        /// <param name="data">The data that is to be modified by the operation</param>
        /// <param name="count">The number of bytes to be processed, measured from the start of the buffer</param>
        public void Decrypt(ref byte[] data, int count) {
            unchecked {
                //Loop through the byte data and apply the offset values
                for (int i = 0; i < count; i++)
                    data[i] -= Next();
            }
        }
    }
}
