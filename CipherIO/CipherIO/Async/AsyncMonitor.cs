/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////
//////////                                                                     //////////
//////////  Copyright (C) 2019 Mitchell Croft (http://www.mitchcroft.games/)   //////////
//////////  All rights reserved                                                //////////
//////////                                                                     //////////
//////////  Name: AsyncMonitor                                                 //////////
//////////  Created: 15/02/2019                                                //////////
//////////  Modified: 15/02/2019                                               //////////
//////////                                                                     //////////
//////////  Purpose:                                                           //////////
//////////  Provide a basic interface to allow for the monitoring of an async  //////////
//////////  operation from the main thread                                     //////////
//////////                                                                     //////////
/////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////

namespace CipherIO.Async {
    /// <summary>
    /// Provide an interface that can be shared between processes to allow for the monitoring
    /// of async operations across a thread
    /// </summary>
    public sealed class AsyncMonitor {
        /*----------Variables----------*/
        //PRIVATE

        /// <summary>
        /// Store a counter for the progress of the operations (0-1)
        /// </summary>
        private float progress;

        /// <summary>
        /// Flag if this operation has completed 
        /// </summary>
        private bool isComplete;

        /// <summary>
        /// Store a flag that indicates the success of the operation
        /// </summary>
        private bool success;

        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// Get and set the progress scale with lock protection
        /// </summary>
        public float Progress {
            get { lock (this) return progress; }
            set { lock (this) progress = value; }
        }

        /// <summary>
        /// Returns a flag that returns true when the progress is >= 1
        /// </summary>
        public bool IsComplete {
            get { lock (this) return isComplete; }
            set { lock (this) isComplete = value; }
        }

        /// <summary>
        /// Store a simple flag that can be set and tested 
        /// </summary>
        public bool Success {
            get { lock (this) return success; }
            set { lock (this) success = value; }
        }
    }
}
