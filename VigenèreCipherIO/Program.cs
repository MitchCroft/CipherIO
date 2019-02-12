/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: Program                                                      //////////
//////////  Created: 10/02/2019                                                //////////
//////////  Modified: 12/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Manage the user input that is supplied to the application to       //////////
//////////  execute the desired functionality                                  //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using CipherIO.Attributes;

namespace CipherIO {
    /// <summary>
    /// Handle the processing of user input to determine how the application should progress
    /// </summary>
    internal class Program {
        /// <summary>
        /// Entry point for the application program
        /// </summary>
        /// <param name="args">The command line arguments that were supplied to the application</param>
        private static void Main(string[] args) {
            //Store a queue of the commands to be processed by the application
            Queue<string> processQueue = new Queue<string>(); {
                //Store a string buffer that will be added to the process queue
                string buffer = string.Empty;

                //Process the command line arguments that were passed
                for (int i = 0; i < args.Length; i++) {
                    //If this is a command
                    if (args[i].StartsWith("-")) {
                        //Add any previous value to the process queue
                        if (!string.IsNullOrEmpty(buffer)) processQueue.Enqueue(buffer);

                        //Start again with this value
                        buffer = args[i];
                    }

                    //Otherwise, add this option to the command
                    else buffer += " " + args[i];
                }

                //Check if there is any text in the buffer to add
                if (!string.IsNullOrEmpty(buffer)) processQueue.Enqueue(buffer);
            }

            //Create a settings object to use for the operation
            EncryptionOperation operation = new EncryptionOperation();

            //Setup the string operational functionality
            Dictionary<string, Tuple<ACallableAttribute, MemberInfo>> stringFunctions = new Dictionary<string, Tuple<ACallableAttribute, MemberInfo>>(); {
                //Extract the string functionality from the operation object
                IEnumerable<MemberInfo> members = operation.GetType().GetMembers().Where(mem => mem.IsDefined(typeof(ACallableAttribute), true));

                //Loop through and setup the functionality dictionary
                foreach (var member in members) {
                    //Retrieve the function attribute from the member
                    IEnumerable<ACallableAttribute> functions = member.GetCustomAttributes<ACallableAttribute>(true);

                    //Process each of the function objects found
                    foreach (var func in functions) {
                        //Check if the map already has an operation with this name
                        if (stringFunctions.ContainsKey(func.Identifier)) {
                            Console.WriteLine($"Can't setup the StringFunction object with identifier '{func.Identifier}', as that identifier has already been used");
                            continue;
                        }

                        //Add the option to the dictionary
                        stringFunctions.Add(
                            func.Identifier.ToLower(),
                            new Tuple<ACallableAttribute, MemberInfo>(
                                func,
                                member
                            )
                        );
                    }
                }
            }

            //Loop for the duration of the operation
            string toProcess = string.Empty;
            do {
                //Start the next command processing option
                Console.Write("CipherIO-> ");

                //If there are queued commands to process, grab the next
                if (processQueue.Count > 0) {
                    //Grab the option from the queue
                    toProcess = processQueue.Dequeue();

                    //Log it the option to the console for transparency
                    Console.WriteLine(toProcess);
                }

                //Grab the option from the user input
                else toProcess = Console.ReadLine();

                //Split the received text into the sections for processing
                string[] split = toProcess.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                //Check there are options to process
                if (split.Length > 0) {
                    //Trim all split elements
                    for (int i = 0; i < split.Length; i++)
                        split[i] = split[i].Trim();

                    //To lower the first (command) option for ease of use
                    split[0] = split[0].ToLower();

                    //If there is an option with the supplied text, raise the function
                    if (stringFunctions.ContainsKey(split[0]))
                        stringFunctions[split[0]].Item1.Execute(
                            stringFunctions[split[0]].Item2,
                            operation,
                            split.Length > 1 ? split[1] : string.Empty
                        );

                    //Otherwise, output a help message
                    else Console.WriteLine($"Unknown command '{toProcess}', use '-help' to see available options");
                }

                //Add a line break between command operations
                Console.WriteLine();
            } while (true);
        }
    }
}
