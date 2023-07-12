/*
 * CometClient的事件监听类和广播类。
 * **/

using System;

namespace Commen.ChatCore
{
    /// <summary>
    /// Delegate used in events that reference a CometClient
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public delegate void CometClientEventHandler(object sender, CometClientEventArgs args);

    /// <summary>
    /// Class CometClientEventArgs
    /// 
    /// Used as an EventArgs parameter to CometClientEventHandler
    /// </summary> 
    public class CometClientEventArgs : EventArgs
    {
        private CometClient cometClient;

        /// <summary>
        /// Construct a new instance of a CometClientEventArgs class
        /// </summary>
        /// <param name="cometClient"></param>
        public CometClientEventArgs(CometClient cometClient)
        {
            //  setup the member
            this.cometClient = cometClient;
        }

        /// <summary>
        /// Gets the CometClient referenced in these EventArgs
        /// </summary>
        public CometClient CometClient
        {
            get { return this.cometClient; }
        }
    }

}
