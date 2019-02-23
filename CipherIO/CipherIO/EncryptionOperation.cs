/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: EncryptionOperation                                          //////////
//////////  Created: 10/02/2019                                                //////////
//////////  Modified: 23/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Store a collection of settings that are used to encrypt/decrypt    //////////
//////////  data that is identified by the application                         //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Threading;

using CipherIO.IO;
using CipherIO.Async;
using CipherIO.Attributes;

namespace CipherIO {
    /// <summary>
    /// Store a collection of settings values that can be modified for encryption operations
    /// </summary>
    public sealed class EncryptionOperation {
        /*----------Variables----------*/
        //PUBLIC

        [CallableStringSetter("-cipherKey", "Set the phrase key that will be used to encrypt/decrypt the file data. WARNING: If this is lost or forgotten, encrypted files can't be recovered", "-cipherKey phrase")]
        public string cipherKey = string.Empty;

        [CallableStringSetter("-targetPath", "The file/directory that is being targeted by the encryption/decryption operation", "-targetPath filepath", "ProcessFilepathString")]
        public string targetPath = string.Empty;

        [CallableStringSetter("-destinationPath", "The destination file for an encryption operation or directory to store the decrypted files within", "-destinationPath filepath", "ProcessFilepathString")]
        public string destinationPath = string.Empty;

        [CallableBoolSetter("-removeOriginals", "Flags if the original files should be deleted at the end of a encryption/decryption operation. Defaults to false", "-removeOriginals (true/false)")]
        public bool removeOriginals = false;

        [CallableBoolSetter("-includeSubDirectories", "Flags if sub directories should be encrypted along along the root files if targetPath is assigned to a directory", "-includeSubDirectories (true/false)")]
        public bool includeSubDirectories = true;

        [CallableStringSetter("-filterExtension", "When scanning directories/sub-directories, indicates the types of files that should be identified for encryption. Use '*' to include all", "-filterExtension filetype", "ProcessFileExtensionString")]
        public string filterExtensions = "*.*";

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Executes the monitoring of a supplied operation, executing the values and displaying updates as required
        /// </summary>
        /// <param name="operation">The operation that is to be processed</param>
        /// <returns>Returns true if the operation was successful</returns>
        private bool MonitorOperation(AIOOperation operation) {
            //Create the async objects for the operation progress
            AsyncMonitor monitor = new AsyncMonitor();
            LogQueue logger = new LogQueue();

            //Output log information
            Console.WriteLine($"Identifying Files at '{operation.TargetPath}'...");

            //Determine if the file identification was successful
            if (!operation.IdentifiyFiles(monitor, logger)) {
                Console.WriteLine("Operation failed to identify any files for processing");
                return false;
            }

            //Output how many files where found at the path
            Console.WriteLine($"Found {operation.FileCount} files to process. Starting operation...");

            //Start the operation
            operation.StartOperation(monitor, logger);

            //Loop for the duration of the operation
            float prev = 0f, current = 0f;
            do {
                //Sleep the current thread for a short period
                Thread.Sleep(100);

                //Get the next progress value
                current = monitor.Progress;

                //If the progress is different, log it
                if (current > prev) {
                    //Add the message to the queue
                    logger.Log($"\tProgress: {(current * 100f).ToString("F2")}%");

                    //Save the new value
                    prev = current;
                }

                //Process all of the logger messages
                while (logger.HasMessages) Console.WriteLine(logger.NextMessage);
            } while (!monitor.IsComplete || logger.HasMessages);

            //Check if the original files should be removed by the operation
            if (removeOriginals) {
                //If the operation was successful, try to remove the originals
                if (monitor.Success) {
                    //Try to remove the file/directory 
                    try {
                        //Check to see if the supplied is a file or directory
                        if ((File.GetAttributes(targetPath) & FileAttributes.Directory) != 0)
                            Directory.Delete(targetPath, true);
                        else File.Delete(targetPath);
                    }

                    //If anything goes wrong, just log it
                    catch (Exception exec) {
                        Console.WriteLine($"Failed to remove the original files after encryption. ERROR: {exec.Message}");
                    }
                }

                //Otherwise, log basic message
                else Console.WriteLine("Operation failed, not removing original files");
            }

            //Return the success state of the operation
            return monitor.Success;
        }

        //PUBLIC

        /// <summary>
        /// Start an encryption operation with the current settings
        /// </summary>
        [CallableMethod("-encrypt", "Begin an encryption operation with the current settings", "-encrypt")]
        public void Encrypt() {
            //Check if the target path is valid
            if (!File.Exists(targetPath) && !Directory.Exists(targetPath)) {
                Console.WriteLine($"The supplied filepath '{targetPath}' is an invalid target file or directory path to encrypt. Use '-targetPath' to assign a valid filepath");
                return;
            }

            //Check that the destination is a valid single file location
            if (!Path.HasExtension(destinationPath)) {
                Console.WriteLine($"The supplied filepath '{destinationPath}' is an invalid destination filepath. Ensure that the filepath is valid with a file extension. Use '-destinationPath' to assign a valid filepath");
                return;
            }

            //Ensure that the directory exists for the destination
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

            //Start an encryption operation
            MonitorOperation(new EncryptFilesOperation(
                cipherKey,
                targetPath,
                destinationPath,
                includeSubDirectories,
                filterExtensions
            ));
        }

        /// <summary>
        /// Start an encryption operation with the current settings
        /// </summary>
        [CallableMethod("-decrypt", "Begin a decryption operation with the current settings", "-decrypt")]
        public void Decrypt() {
            //Check the target file is valid for decryption
            if (!File.Exists(targetPath)) { 
                Console.WriteLine($"The supplied filepath '{targetPath}' is an invalid file path for decryption. Use '-targetPath' to assign a valid filepath");
                return;
            }

            //Ensure the destination is a valid directory
            if (Path.HasExtension(destinationPath)) {
                Console.WriteLine($"The supplied filepath '{destinationPath}' is an invalid file directory to output decrypted files. Use '-destinationPath' to assign a valid output directory");
                return;
            }

            //Ensure the output directory is valid
            Directory.CreateDirectory(destinationPath);

            //Start the decryption operation
            MonitorOperation(new DecryptFilesOperation(
                cipherKey,
                targetPath,
                destinationPath,
                includeSubDirectories,
                filterExtensions
            ));
        }

        /// <summary>
        /// Clear the current console text from the screen
        /// </summary>
        [CallableMethod("-clear", "Clear the console screen of all currently entered text", "-clear")] 
        public void Clear() { Console.Clear(); }

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

        //STATIC PROCESSING

        /// <summary>
        /// Extract the text between the " characters and return that as the filepath value
        /// </summary>
        /// <param name="path">The text that is to be prcessed for the quotation marks</param>
        /// <returns>Returns the extracted text value</returns>
        public static string ProcessFilepathString(string path) {
            //Store the index of the first " character
            int quoteOpen = path.IndexOf('"');

            //If no character was found, then use the original
            if (quoteOpen == -1) return path;

            //Find the closing quote in the path
            int quoteClose = path.LastIndexOf('"');

            //If there isn't a closing option, return the original
            if (quoteClose == quoteOpen) return path;

            //Return the extracted text values
            return path.Substring(quoteOpen + 1, quoteClose - (quoteOpen + 1));
        }

        /// <summary>
        /// Clean the supplied extension type to a format that can be used for the encryption operations
        /// </summary>
        /// <param name="extension">The text that is to be process for the filter extension</param>
        /// <returns>Returns the extracted value</returns>
        public static string ProcessFileExtensionString(string extension) {
            //Clean off the quotation marks
            extension = ProcessFilepathString(extension);

            //Find the last index of the '.' character to extract from
            int extractPoint = extension.LastIndexOf('.');

            //Extract the extension of the text
            return "*." + extension.Substring(extractPoint + 1);
        } 
    }
}
