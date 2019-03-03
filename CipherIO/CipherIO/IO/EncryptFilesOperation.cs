/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: EncryptFilesOperation                                        //////////
//////////  Created: 17/02/2019                                                //////////
//////////  Modified: 03/03/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Manage the encryption of the identified filepaths by the initial   //////////
//////////  settings                                                           //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.IO.Compression;

using CipherIO.Async;
using CipherIO.Encryption;

namespace CipherIO.IO {
    /// <summary>
    /// Manage the encryption of the identified filepaths identified by the initial settings
    /// </summary>
    public sealed class EncryptFilesOperation : AIOOperation {
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
        public EncryptFilesOperation(string key, string target, string destination, bool includeSub, string filterExtensions) : 
            base(key, target, destination, includeSub, filterExtensions) 
        {}

        /// <summary>
        /// Handle the encryption of the identified filepaths to an output file
        /// </summary>
        /// <param name="monitor">The monitor that will be updated with information during the operation</param>
        /// <param name="logger">Used to output specific messages to throughout the operation execution</param>
        public override async void StartOperation(AsyncMonitor monitor, LogQueue logger) {
            //Check that there are files to process
            if (identifiedFiles.Count == 0) {
                logger.Log("Can't start encryption operation. No files were identified for encryption");
                monitor.IsComplete = true;
                return;
            }

            //Store the buffers that will be used for the operation
            FileStream fStream = null;
            GZipStream writer = null;
            FileStream reader = null;

            //Create the encryption key that will be used for this operation
            VigenèreCipherKey key = new VigenèreCipherKey(Key);

            //Flag if the operation was completed successfully
            bool successful = true; 

            //Attempt to parse all of the supplied data
            try {
                //Create the initial buffer objects
                fStream = new FileStream(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE);
                writer = new GZipStream(fStream, CompressionMode.Compress);

                //Create the byte buffers that will be used for the operation
                byte[] dataBuffer = new byte[BUFFER_SIZE];
                byte[] intBuffer = null;
                byte[] longBuffer = null;

                //Get the root directory of the target path
                string rootDir = (Path.HasExtension(TargetPath) ?
                    Path.GetDirectoryName(TargetPath) :
                    TargetPath
                );

                //Determine the percentage to use per file that is encrypted
                float percentageUsage = 1f / identifiedFiles.Count;

                //Convert the number of files to process into bytes
                intBuffer = BitConverter.GetBytes(identifiedFiles.Count);
                if (BitConverter.IsLittleEndian) Array.Reverse(intBuffer, 0, sizeof(int));
                key.Encrypt(ref intBuffer, sizeof(int));

                //Write the count to the file
                await writer.WriteAsync(intBuffer, 0, sizeof(int));

                //Process each of the files that are to be included
                foreach (FileInfo file in identifiedFiles) {
                    //Remove the root directory from the filepath
                    string relativePath = file.FullName.Substring(rootDir.Length);

                    //Get the number of characters to be processed in the relative path
                    intBuffer = BitConverter.GetBytes(relativePath.Length);
                    if (BitConverter.IsLittleEndian) Array.Reverse(intBuffer, 0, sizeof(int));
                    key.Encrypt(ref intBuffer, sizeof(int));

                    //Write the character count to the file
                    await writer.WriteAsync(intBuffer, 0, sizeof(int));

                    //Write the filepath to the buffer
                    long processedCount = 0;
                    do {
                        //Track the number of characters that have been added to the buffer
                        int dataBufferUsage = 0;

                        //Copy the available characters to the buffer
                        for (; processedCount < relativePath.Length && dataBufferUsage + sizeof(char) < BUFFER_SIZE; processedCount++, dataBufferUsage += sizeof(char)) {
                            //Get the bytes for the characters
                            byte[] charBuffer = BitConverter.GetBytes(relativePath[(int)processedCount]);

                            //Copy the characters to the buffer
                            if (BitConverter.IsLittleEndian) Array.Reverse(charBuffer, 0, sizeof(char));
                            Array.Copy(charBuffer, 0, dataBuffer, dataBufferUsage, sizeof(char));
                        }

                        //Encrypt the buffer and write it to the file
                        key.Encrypt(ref dataBuffer, dataBufferUsage);
                        await writer.WriteAsync(dataBuffer, 0, dataBufferUsage);
                    } while (processedCount < relativePath.Length);

                    //Get the amount of data in the file
                    longBuffer = BitConverter.GetBytes(file.Length);
                    if (BitConverter.IsLittleEndian) Array.Reverse(longBuffer, 0, sizeof(long));
                    key.Encrypt(ref longBuffer, sizeof(long));

                    //Write the data count to the buffer
                    await writer.WriteAsync(longBuffer, 0, sizeof(long));

                    //Try to open and process the file that is being processed
                    try {
                        //Open the file for reading
                        reader = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE);

                        //Process all of the data in the file
                        processedCount = 0;
                        do {
                            //Retrieve the chunk of data from the file
                            int readCount = await reader.ReadAsync(dataBuffer, 0, BUFFER_SIZE);

                            //Add to the counter
                            processedCount += readCount;

                            //Encrypt and write the data
                            key.Encrypt(ref dataBuffer, readCount);
                            await writer.WriteAsync(dataBuffer, 0, readCount);
                        } while (processedCount < file.Length);
                    }

                    //Log any errors that occur
                    catch (Exception exec) {
                        logger.Log($"Encryption failed to process the file '{file.FullName}'. ERROR: {exec.Message}");
                        successful = false;
                        break;
                    }

                    //Cleanup the file elements
                    finally { if (reader != null) { reader.Dispose(); reader = null; } }

                    //Increment the operation percentage
                    monitor.Progress += percentageUsage;
                }
            }

            //Catch anything unexpected that happens
            catch (Exception exec) {
                logger.Log($"Unexpected error occurred, unable to complete encryption operation. ERROR: {exec.Message}");
                successful = false;
            }

            finally {
                //Clear out the file streams
                if (reader != null) { reader.Dispose(); reader = null; }
                if (writer != null) { writer.Dispose(); writer = null; }
                if (fStream != null) { fStream.Dispose(); fStream = null; }

                //If the operation wasn't successful, delete the file if it exists
                if (!successful) File.Delete(DestinationPath);

                //Mark the end of the operation
                monitor.Success = successful;
                monitor.IsComplete = true;
            }
        }
    }
}
