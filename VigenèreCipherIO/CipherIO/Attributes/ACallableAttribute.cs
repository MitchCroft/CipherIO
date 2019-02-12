/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: ACallableAttribute                                           //////////
//////////  Created: 10/02/2019                                                //////////
//////////  Modified: 10/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Provide a base point for string-accessible operations to be        //////////
//////////  started from                                                       //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace CipherIO.Attributes {
    /// <summary>
    /// Provide a base point for operational execution based on input string values
    /// </summary>
    public abstract class ACallableAttribute : Attribute {
        /*----------Properties----------*/
        //PUBLIC
        
        /// <summary>
        /// The identifier that this object will use when being processed 
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// A description of what the associated element is/what it is used for
        /// </summary>
        public string Desciption { get; private set; }

        /// <summary>
        /// An example of how the associated information should be used in project execution
        /// </summary>
        public string Example { get; private set; }

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Assign the identifier that this object will use to represent it
        /// </summary>
        /// <param name="identifier">The identifer that will be used represent the object. NOTE: This should be unique for all values</param>
        /// <param name="description">The description to provide in association with the attached element</param>
        /// <param name="example">An example of how to use the associated element</param>
        public ACallableAttribute(string identifier, string description, string example) {
            Identifier = identifier;
            Desciption = description;
            Example = example;
        }

        /// <summary>
        /// Execute the functionality of this attribute 
        /// </summary>
        /// <param name="info">Information about the member object that this attribute is attached to</param>
        /// <param name="obj">The object that the information belongs to</param>
        /// <param name="additional">Additional text that was provided with the operation</param>
        public abstract void Execute(MemberInfo info, object obj, string additional);
    }

    /// <summary>
    /// Simple delegate that can be used to customise the log output location
    /// </summary>
    /// <param name="message">A single log line message that is to be output</param>
    public delegate void LogMessageLineDel(string message);

    /// <summary>
    /// Log the help display information to the desired element
    /// </summary>
    public static class CallableInformationBreakdown {
        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Retrieve all fields with a <see cref="HelpDisplayAttribute"/> attribute attached
        /// </summary>
        /// <param name="type">The type that is to have its fields processed</param>
        /// <returns>Returns an enumerable collection of fields to process</returns>
        private static IEnumerable<FieldInfo> RetrieveFields(Type type) {
            return type.GetFields().Where(field => field.IsDefined(typeof(ACallableAttribute), false));
        }

        /// <summary>
        /// Retrieve all properties with a <see cref="HelpDisplayAttribute"/> attribute attached
        /// </summary>
        /// <param name="type">The type that is to have its properties processed</param>
        /// <returns>Returns an enumerable collection of properties to process</returns>
        private static IEnumerable<PropertyInfo> RetrieveProperties(Type type) {
            return type.GetProperties().Where(prop => prop.IsDefined(typeof(ACallableAttribute), false));
        }

        /// <summary>
        /// Retrieve all methods with a <see cref="HelpDisplayAttribute"/> attribute attached
        /// </summary>
        /// <param name="type">The type that is to have its methods processed</param>
        /// <returns>Returns an enumerable collection of methods to process</returns>
        private static IEnumerable<MethodInfo> RetrieveMethods(Type type) {
            return type.GetMethods().Where(method => method.IsDefined(typeof(ACallableAttribute), false));
        }

        /// <summary>
        /// Output the Help Display information that is attached to the supplied member object
        /// </summary>
        /// <param name="member">The member object that is to be processed</param>
        /// <param name="log">The output method that is being used to log the messages to the output</param>
        private static void OutputHelpMessage(MemberInfo member, LogMessageLineDel log, Action onComplete = null) {
            //Retrieve the HelpDisplay attributes from the member object
            IEnumerable<ACallableAttribute> helpers = member.GetCustomAttributes<ACallableAttribute>(false);

            //Iterate through the list of options
            foreach (ACallableAttribute att in helpers) {
                //Output the values of the object
                log($"Identifier: {att.Identifier}");
                log($"\t|-> Description: {att.Desciption}");
                log($"\t|-> Example: {att.Example}");

                //Handle the on complete behaviour
                onComplete?.Invoke();
            }
        }

        //PUBLIC

        /// <summary>
        /// Output the <see cref="HelpDisplayAttribute"/> attribute elements for the supplied object with their current values
        /// </summary>
        /// <param name="obj">The object that is to have the HelpDisplayElements extracted from it</param>
        /// <param name="log">The output method that is being used to log the messages to the output. Will use <see cref="Console.WriteLine"/> by default</param>
        public static void Log(object obj, LogMessageLineDel log = null) {
            //Check there is an output location
            if (log == null) log = Console.WriteLine;

            //Store the type that is going to be used for the operation
            Type type = obj.GetType();

            //Retrieve all of the values to check for output
            var fields = RetrieveFields(type);
            var props = RetrieveProperties(type);
            var methods = RetrieveMethods(type);

            //Output the fields with help information
            if (fields.Count() > 0) {
                foreach (var field in fields) {
                    //Output the basic information
                    OutputHelpMessage(field, log, () => {
                        log($"\t|-> Value: {field.GetValue(obj).ToString()}");
                    });

                    //Output a blank line for the next section
                    log(string.Empty);
                }
            }

            //Output the properties with help information
            if (props.Count() > 0) {
                foreach (var prop in props) {
                    //Output the basic information
                    OutputHelpMessage(prop, log, () => {
                        //Check if the property can be read
                        if (prop.CanRead) log($"\t|-> Value: {prop.GetValue(obj)}");
                    });

                    //Output a blank line for the next section
                    log(string.Empty);
                }
            }

            //Output the methods with help information
            if (methods.Count() > 0) {
                foreach (var method in methods) {
                    //Output the basic information
                    OutputHelpMessage(method, log);

                    //Output a blank line for the next section
                    log(string.Empty);
                }
            }
        }

        /// <summary>
        /// Output the <see cref="HelpDisplayAttribute"/> attribute elements for the supplied types
        /// </summary>
        /// <typeparam name="T">The type of object that is to be displayed for help information</typeparam>
        /// <param name="log">The output method that is being used to log the messages to the output</param>
        public static void Log<T>(LogMessageLineDel log = null) {
            //Check there is an output location
            if (log == null) log = Console.WriteLine;

            //Store the type that is going to be used for the operation
            Type type = typeof(T);

            //Retrieve all of the values to check for output
            var fields = RetrieveFields(type);
            var props = RetrieveProperties(type);
            var methods = RetrieveMethods(type);

            //Output the fields with help information
            if (fields.Count() > 0) {
                foreach (var field in fields) {
                    //Output the basic information
                    OutputHelpMessage(field, log);

                    //Output a blank line for the next section
                    log(string.Empty);
                }
            }

            //Output the properties with help information
            if (props.Count() > 0) {
                foreach (var prop in props) {
                    //Output the basic information
                    OutputHelpMessage(prop, log);

                    //Output a blank line for the next section
                    log(string.Empty);
                }
            }

            //Output the methods with help information
            if (methods.Count() > 0) {
                foreach (var method in methods) {
                    //Output the basic information
                    OutputHelpMessage(method, log);

                    //Output a blank line for the next section
                    log(string.Empty);
                }
            }
        }
    }
}
