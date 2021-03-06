﻿/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: AIOOperation                                                 //////////
//////////  Created: 16/02/2019                                                //////////
//////////  Modified: 23/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Provide a base point for Cipher IO operations to inherit from for  //////////
//////////  simplified application at the base level                           //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;

namespace CipherIO.IO {
    /// <summary>
    /// Base point for Cipher IO operations to inherit from for use
    /// </summary>
    public abstract class AIOOperation {
        /*----------Variables----------*/
        //CONSTANT

        /// <summary>
        /// Store the minimum size of the data buffer that is created
        /// </summary>
        public const int BUFFER_SIZE = 268435456;

        //PROTECTED

        /// <summary>
        /// Store a list of the filepaths that are to be included in the operation
        /// </summary>
        protected List<FileInfo> identifiedFiles;

        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// Store the cipher key that will be used to process the information
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Store the path targeted by the operation
        /// </summary>
        public string TargetPath { get; private set; }

        /// <summary>
        /// Store the filepath that will be used as the destination
        /// </summary>
        public string DestinationPath { get; private set; }

        /// <summary>
        /// Flags if the sub-directories should be included (If TargetPath is a directory)
        /// </summary>
        public bool IncludeSubDirectories { get; private set; }

        /// <summary>
        /// Identifies the types of files should be included in the identification process when scanning a directory
        /// </summary>
        public string FilterExtensions { get; private set; }

        /// <summary>
        /// Retrieve the number of files that have been identified at the target path
        /// </summary>
        public int FileCount { get { return identifiedFiles.Count; } }

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
        public AIOOperation(string key, string target, string destination, bool includeSub, string filterExtensions) {
            //Stash the values for the operation
            Key = key;
            TargetPath = target;
            DestinationPath = destination;
            IncludeSubDirectories = includeSub;
            FilterExtensions = filterExtensions;
        }

        /// <summary>
        /// Populate the identified files list with the determined values
        /// </summary>
        /// <returns>Returns true if there were files found for processing</returns>
        public bool IdentifiyFiles() {
            //Create the list of files to be processed
            identifiedFiles = new List<FileInfo>();

            //Try to identify the files that are being included
            try {
                //Check if the target path is a directory
                if ((File.GetAttributes(TargetPath) & FileAttributes.Directory) != 0)
                    identifiedFiles.AddRange(new DirectoryInfo(TargetPath).GetFiles(FilterExtensions, IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));

                //Otherwise, use the supplied path
                else identifiedFiles.Add(new FileInfo(TargetPath));
            }

            //Log the error and progress
            catch (Exception exec) {
                Console.WriteLine($"Failed to identify files with the path '{TargetPath}'. ERROR: {exec.Message}");
                return false;
            }

            //Return true if there are files to process
            return (identifiedFiles.Count > 0);
        }

        /// <summary>
        /// Provide the access point for the operation to occur
        /// </summary>
        /// <returns>Returns true of the operation was successful</returns>
        public abstract bool StartOperation();
    }
}
