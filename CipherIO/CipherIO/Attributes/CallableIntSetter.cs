/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: CallableIntSetter                                            //////////
//////////  Created: 10/02/2019                                                //////////
//////////  Modified: 23/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Handle the assigning of an int value to the member object when     //////////
//////////  this operation is executed                                         //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace CipherIO.Attributes {
    /// <summary>
    /// Handle the setting of the integer value of a Field/Property based on an identifier string
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CallableIntSetter : ACallableAttribute {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Assign the identifier that this object will use to represent it
        /// </summary>
        /// <param name="identifier">The identifer that will be used represent the object. NOTE: This should be unique for all values</param>
        /// <param name="description">The description to provide in association with the attached element</param>
        /// <param name="example">An example of how to use the associated element</param>
        /// <param name="onProcess">Optional additional function name that can be used to process the parsed values</param>
        public CallableIntSetter(string identifier, string description, string example, string onProcess = null) : 
            base(identifier, description, example, onProcess) 
        {}

        /// <summary>
        /// Apply the supplied additional value to the object value
        /// </summary>
        /// <param name="info">Information about the member object that this attribute is attached to</param>
        /// <param name="obj">The object that the information belongs to</param>
        /// <param name="additional">Text information that must be parsed to retrieve a int value</param>
        public override void Execute(MemberInfo info, object obj, string additional) {
            //Cast the member info into one of the supported types
            FieldInfo field = info as FieldInfo;
            PropertyInfo prop = info as PropertyInfo;

            //Check that the member is valid
            if (field == null && prop == null) {
                Console.WriteLine($"CallableBoolSetter '{Identifier}' was unable to process the Member Info '{info.Name}' for the object '{obj}' as it was expecting it to be a field or property");
                return;
            }

            //Get the bool value out of the additional text
            int val;
            if (!int.TryParse(additional, out val)) {
                Console.WriteLine($"CallableBoolSetter '{Identifier}' was unable to parse the text '{additional}' to an integer value. Unable to assign value");
                return;
            }

            //Check if there is an additional processing function to call
            if (!string.IsNullOrEmpty(OnProcessParsed)) {
                //Try to get the function from the calling object
                MethodInfo parseFunc = obj.GetType().GetMethod(OnProcessParsed, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                //If there was something found, see if it can be used
                if (parseFunc != null) {
                    //Check that the parameters are correct
                    ParameterInfo[] parameters = parseFunc.GetParameters();

                    //There needs to be a single parameter
                    if (parameters.Length == 1) {
                        //Parameter needs to be the correct type
                        if (parameters[0].ParameterType == typeof(int)) {
                            //Process the supplied method
                            val = (int)parseFunc.Invoke(obj, new object[] { val });
                        }
                    }
                }
            }

            //If the value is just a field, assign the value
            if (field != null) field.SetValue(obj, val);

            //Otherwise, need to check write access
            else {
                //Check the property can be set
                if (!prop.CanWrite) {
                    Console.WriteLine($"CallableBoolSetter '{Identifier}' is unable to assign the property '{info.Name}' as it doesn't have a setter");
                    return;
                }

                //Assign the value
                prop.SetValue(obj, val);
            }
        }
    }
}
