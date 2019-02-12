/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: CallableMethod                                               //////////
//////////  Created: 10/02/2019                                                //////////
//////////  Modified: 10/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Allow for the running of a method based on the stored string       //////////
//////////  identifier                                                         //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace CipherIO.Attributes {
    /// <summary>
    /// Handle the activation of a method based on an identifier string
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class CallableMethod : ACallableAttribute {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Assign the identifier that this object will use to represent it
        /// </summary>
        /// <param name="identifier">The identifer that will be used represent the object. NOTE: This should be unique for all values</param>
        /// <param name="description">The description to provide in association with the attached element</param>
        /// <param name="example">An example of how to use the associated element</param>
        public CallableMethod(string identifier, string description, string example) : 
            base(identifier, description, example) 
        {}

        /// <summary>
        /// Execute the specified function on the supplied object
        /// </summary>
        /// <param name="info">Information about the Member that this attribute is attached to</param>
        /// <param name="obj">The object that owns the MemberInfo</param>
        /// <param name="additional">Additional information that can be parsed</param>
        public override void Execute(MemberInfo info, object obj, string additional) {
            //Try to convert the member info object to method information
            MethodInfo method = info as MethodInfo;

            //Check that the method was obtained successfully
            if (method == null) {
                Console.WriteLine($"CallableMethod '{Identifier}' was unable to process the Member Info '{info.Name}' for the object '{obj}' as it was expecting it to be a method");
                return;
            }

            //Check there are no parameters
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length > 0) {
                //Check that the length isn't greater then 1
                if (parameters.Length != 0) {
                    Console.WriteLine($"CallableMethod '{Identifier}' is unable to process the Method '{info.Name}' as it can't handle more then one parameter. Use a function with no parameters or a string only");
                    return;
                }

                //Check that the parameter is a string
                else if (parameters[0].ParameterType != typeof(string)) {
                    Console.WriteLine($"CallableMethod '{Identifier}' is unable to process the Method '{info.Name}' as it can't handle a non-string parameter. Use a function with no parameters or a string only");
                    return;
                }
            }

            //Store the parameters to supply
            object[] parms = (parameters.Length == 0 ?
                new object[0] :
                new object[] { additional }
            );

            //Start the function
            method.Invoke(obj, parms);
        }
    }
}
