/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: LogQueue                                                     //////////
//////////  Created: 15/02/2019                                                //////////
//////////  Modified: 15/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Wrap a queue of string log messages that can be added to from      //////////
//////////  any thread and processed from a single location for consistent     //////////
//////////  output behaviour                                                   //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace CipherIO.Async {
    /// <summary>
    /// Provide asynchronous access to a queue of messages that can be logged in a 
    /// consistent manner
    /// </summary>
    public sealed class LogQueue {
        /*----------Variables----------*/
        //PRIVATE

        /// <summary>
        /// Store a queue of messages that can be output to a location
        /// </summary>
        private Queue<string> messageQueue;
        
        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// Return a bool that indicates if there are messages within the queue to be processed
        /// </summary>
        public bool HasMessages { get { lock (this) return messageQueue.Count > 0; } }

        /// <summary>
        /// Retrieve the next message that is stored within the queue
        /// </summary>
        public string NextMessage { get { lock (this) return messageQueue.Dequeue(); } }

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Initialise the internal queue
        /// </summary>
        public LogQueue() { messageQueue = new Queue<string>(); }

        /// <summary>
        /// Add a new message to the queue
        /// </summary>
        /// <param name="msg">The message to be added to the queue</param>
        public void Log(string msg) { lock (this) messageQueue.Enqueue(msg); }
    }
}
