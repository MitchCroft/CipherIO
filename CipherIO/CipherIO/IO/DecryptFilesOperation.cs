﻿/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: DecryptFilesOperation                                        //////////
//////////  Created: 17/02/2019                                                //////////
//////////  Modified: 23/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Manage the decryption of the identified file by the initial        //////////
//////////  settings                                                           //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;

using CipherIO.Async;
using CipherIO.Encryption;

namespace CipherIO.IO {
    /// <summary>
    /// Process an encrypted file to extract the contained file data
    /// </summary>
    public sealed class DecryptFilesOperation : AIOOperation {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Initialise with the default values
        /// </summary>
        /// <param name="key">The key to use for the encryption operation</param>
        /// <param name="target">The target filepath for the operation</param>
        /// <param name="destination">The destination point to output the result of the operation</param>
        /// <param name="includeSub">Flags if sub-directory objects should be included in the operation</param>
        /// <param name="filterExtensions">Identifies the types of files to extract from directories for an operation</param>
        public DecryptFilesOperation(string key, string target, string destination, bool includeSub, string filterExtensions) : 
            base(key, target, destination, includeSub, filterExtensions) 
        {}

        /// <summary>
        /// Handle the decryption of the identified filepath to an output directory
        /// </summary>
        /// <param name="monitor">The monitor that will be updated with information during the operation</param>
        /// <param name="logger">Used to output specific messages to throughout the operation execution</param>
        public override async void StartOperation(AsyncMonitor monitor, LogQueue logger) {
            //Check that there is a single file to process
            if (identifiedFiles.Count != 1) {
                logger.Log($"Can't start decryption operation, there is an invalid number ({identifiedFiles.Count}) of files set as the target. Expected only 1");
                monitor.IsComplete = true;
                return;
            }

            //Store the buffers that will be used for the operation
            FileStream fStream = null;
            GZipStream reader = null;
            FileStream writer = null;

            //Create the encryption key that will be used for this operation
            VigenèreCipherKey key = new VigenèreCipherKey(Key);

            //Flag if the operation was completed successfully
            bool successful = true;

            //Store a collection of the files that are generated by this operation
            //<Temp Path, Final Path>
            List<Tuple<string, string>> generatedFiles = new List<Tuple<string, string>>();

            //Attempt to decrypt the entire file
            try {
                //Create the initial buffer objects
                fStream = new FileStream(TargetPath, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE);
                reader = new GZipStream(fStream, CompressionMode.Decompress);

                //Create the byte buffers that will be used for the operation
                byte[] dataBuffer = new byte[BUFFER_SIZE];
                byte[] intBuffer = new byte[sizeof(int)];
                byte[] longBuffer = new byte[sizeof(long)];

                //Read the number of files that are included in this collection
                await reader.ReadAsync(intBuffer, 0, sizeof(int));
                key.Decrypt(ref intBuffer, sizeof(int));

                //Decrypt the count
                if (BitConverter.IsLittleEndian) Array.Reverse(intBuffer, 0, sizeof(int));
                int fileCount = BitConverter.ToInt32(intBuffer, 0);

                //Calculate the percentage to use for each file processed
                float percentageUsage = .75f / fileCount;

                //Loop through each file to be processed
                for (int i = 0; i < fileCount; i++) {
                    //Read the bytes for the number of characters in the filepath
                    await reader.ReadAsync(intBuffer, 0, sizeof(int));
                    key.Decrypt(ref intBuffer, sizeof(int));

                    //Get the number of characters that are in the 
                    if (BitConverter.IsLittleEndian) Array.Reverse(intBuffer, 0, sizeof(int));
                    int characterCount = BitConverter.ToInt32(intBuffer, 0);

                    //Construct the relative filepath back from the data
                    StringBuilder relativePath = new StringBuilder(characterCount);

                    //Loop through and read the filepath
                    long processedCount = 0;
                    do {
                        //Retrieve the next chunk of characters from datafile
                        int readCount = await reader.ReadAsync(dataBuffer, 0, 
                            Math.Min(
                                (BUFFER_SIZE / sizeof(char)) * sizeof(char),
                                (characterCount - (int)processedCount) * sizeof(char)
                            )
                        );

                        //Decrypt the character bytes
                        key.Decrypt(ref dataBuffer, readCount);

                        //Half the count for final processing
                        readCount /= sizeof(char);

                        //Extract the characters from the buffer
                        byte[] charBuffer = new byte[sizeof(char)];
                        for (int c = 0; c < readCount; c++) {
                            //Get the character bytes from the array
                            Array.Copy(dataBuffer, c * sizeof(char), charBuffer, 0, sizeof(char));

                            //Convert the byte data back to a character
                            if (BitConverter.IsLittleEndian) Array.Reverse(charBuffer, 0, sizeof(char));
                            relativePath.Append(BitConverter.ToChar(charBuffer, 0));
                        }

                        //Increase the counter
                        processedCount += readCount;
                    } while (processedCount < characterCount);

                    //Get the amount of data to evaluated by this process
                    await reader.ReadAsync(longBuffer, 0, sizeof(long));
                    key.Decrypt(ref longBuffer, sizeof(long));

                    //Get the amount of data to be processed
                    if (BitConverter.IsLittleEndian) Array.Reverse(longBuffer, 0, sizeof(long));
                    long dataCount = BitConverter.ToInt64(longBuffer, 0);

                    //Get a temp file to store the data at
                    string tempPath = Path.GetTempFileName();

                    //Add the entry to the monitor list
                    generatedFiles.Add(new Tuple<string, string>(
                        tempPath,
                        Path.Combine(DestinationPath, relativePath.ToString())
                    ));

                    //Try to create process the contained file
                    try {
                        //Open the temporary file for writing
                        writer = new FileStream(tempPath, FileMode.Append, FileAccess.Write, FileShare.Read, BUFFER_SIZE);

                        //Process all of the data within the file
                        processedCount = 0;
                        do {
                            //Retrieve the next chunk of data to process
                            int readCount = await reader.ReadAsync(dataBuffer, 0, 
                                (int)Math.Min(
                                    BUFFER_SIZE,
                                    dataCount - processedCount
                                )
                            );

                            //If there is data to be read but nothing was read from the file, then something has gone wrong
                            if (readCount == 0 && dataCount - processedCount > 0)
                                throw new OperationCanceledException($"OperationCanceledException: {dataCount - processedCount} bytes are left to be read but 0 bytes were read. Reached EOF");

                            //Increase the counters
                            processedCount += readCount;

                            //Decrypt the buffer and write it to the file
                            key.Decrypt(ref dataBuffer, readCount);
                            await writer.WriteAsync(dataBuffer, 0, readCount);
                        } while (processedCount < dataCount);
                    }
#if DEBUG
                    //Log the exception thrown for debugging purposes
                    catch (Exception exec) {
                        logger.Log($"Decryption failed to process internal file. Writing of included file '{relativePath.ToString()}' failed. ERROR: {exec.Message}");
                        successful = false;
                        break;
                    }
#else
                    //Log general failure on exception. Assume key is wrong
                    catch {
                        logger.Log("Decryption failed to process an internal file. Is the Cipher Key correct?");
                        successful = false;
                        break;
                    }
#endif
                    //Cleanup the file writing
                    finally { if (writer != null) { writer.Dispose(); writer = null; } }

                    //Increment the operation percentage
                    monitor.Progress += percentageUsage;
                }
            }

#if DEBUG
            //Log the exception thrown for debugging purposes
            catch (Exception exec) {
                logger.Log($"Decryption failed to process internal file. Is the Cipher Key correct? ERROR: {exec.Message}");
                successful = false;
            }
#else
            //Log general failure on exception. Assume key is wrong
            catch {
                logger.Log("Decryption failed to process an internal file. Is the Cipher Key correct?");
                successful = false;
            }
#endif

            finally {
                //Clear out the file streams
                if (writer != null) { writer.Dispose(); writer = null; }
                if (reader != null) { reader.Dispose(); reader = null; }
                if (fStream != null) { fStream.Dispose(); fStream = null; }

                //If the operation failed, delete all of the temp files
                if (!successful) {
                    for (int i = 0; i < generatedFiles.Count; i++) {
                        try { File.Delete(generatedFiles[i].Item1); } 
                        catch {}
                    }
                }

                //Otherwise, shift the files to the valid destination
                else {
                    //Calculate a percentage requirement for each file to shift
                    float percentageUsage = .25f / generatedFiles.Count;

                    //Process all of the files
                    for (int i = 0; i < generatedFiles.Count; i++) {
                        //Ensure the directory exists
                        Directory.CreateDirectory(Path.GetDirectoryName(generatedFiles[i].Item2));

                        //Delete the file if it already exists
                        try { File.Delete(generatedFiles[i].Item2); } 
                        catch {}

                        //Try to move the decrypted file to the final location
                        try { File.Move(generatedFiles[i].Item1, generatedFiles[i].Item2); }
                        catch (Exception exec) {
                            logger.Log($"Decryption operation failed to relocate decrypted file to '{generatedFiles[i].Item2}'. ERROR: {exec.Message}");
                            successful = false;
                        }

                        //Mark off another file complete
                        monitor.Progress += percentageUsage;
                    }
                }

                //Mark the end of the operation
                monitor.Success = successful;
                monitor.IsComplete = true;
            }
        }
    }
}
