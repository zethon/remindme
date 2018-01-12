using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

//------------------------------------------------------------------------------//
//                                                                              //
// Author:  Derek Bartram                                                       //
// Date:    23/01/2008                                                          //
// Version: 1.000                                                               //
// Website: http://www.derek-bartram.co.uk                                      //
// Email:   webmaster@derek-bartram.co.uk                                       //
//                                                                              //
// This code is provided on a free to use and/or modify basis for personal work //
// provided that this banner remains in each of the source code files that is   //
// found in the original source. For any publicically available work (source    //
// and/or binaries 'Derek Bartram' and 'http://www.derek-bartram.co.uk' must be //
// credited in both the user documentation, source code (where applicable), and //
// in the user interface (typically Help > About would be appropiate). Please   //
// also contact myself via the provided email address to let me know where and  //
// what my code is being used for; this helps me provide better solutions for   //
// all.                                                                         //
//                                                                              //
// THIS SOURCE AND/OR COMPILED LIBRARY MUST NOT BE USED FOR COMMERCIAL WORK,    //
// including not-for-profit work, without prior consent.                        //
//                                                                              //
// This agreement overrides any other agreements made by any other parties. By  //
// using, viewing, linking, or compiling the included source or binaries you    //
// agree to the terms and conditions as set out here and in any included (if    //
// applicable) license.txt. For commercial licensing please see the web address //
// above or contact myself via email. Thank you.                                //
//                                                                              //
// Please contact me at the above email for further help, information,          //
// comments, suggestions, licensing, or feature requests. Thank you.            //
//                                                                              //
//                                                                              //
//------------------------------------------------------------------------------//

namespace DNBSoft.MSN.ClientController
{
    internal sealed class MSNLocalClient
    {
        private MSNController controller = null;
        private MSNEnumerations.UserStatus currentStatus = MSNEnumerations.UserStatus.offline;
        private MSNEnumerations.LoginStatus currentLoginStatus = MSNEnumerations.LoginStatus.LOGGED_OUT;
        private MSNEnumerations.UnknownContact handleUnknownContact = MSNEnumerations.UnknownContact.allow_chat;
        private String friendlyName = "";

        internal MSNLocalClient(MSNController controller)
        {
            this.controller = controller;

            controller.LoginStatusChanged += new MSNEventDelegates.LoginStatusChangedEventDelegate(controller_LoginStatusChanged);
        }

        #region event handlers
        private void controller_LoginStatusChanged(MSNEnumerations.LoginStatus newStatus)
        {
            currentLoginStatus = newStatus;
        }
        #endregion

        #region accessors
        internal MSNEnumerations.UserStatus CurrentStatus
        {
            get
            {
                return currentStatus;
            }
            set
            {
                controller.sendMessage(new MSNMessage("CHG " + MSNTrIDGenerator.NextID() + " " + MSNStaticHelperFunctions.fromUserStatus(value) + " 536870912\r\n"));
            }
        }

        internal MSNEnumerations.UnknownContact HandleUnknownClient
        {
            get
            {
                return handleUnknownContact;
            }
        }

        internal String FriendlyName
        {
            get
            {
                return friendlyName;
            }
            set
            {
                controller.sendMessage(new MSNMessage("REA " + MSNTrIDGenerator.NextID() + " " + controller.Username + " " + HttpUtility.UrlEncode(value) + "\r\n"));
            }
        }
        #endregion

        internal void processMessage(MSNMessage message)
        {
            if (message.getCommand().Equals("CHG"))
            {
                currentStatus = MSNStaticHelperFunctions.toUserStatus(message.getData()[0]);

                controller.sendLocalClientStatusChangeMessage();
            }
            else if (message.getCommand().Equals("PRP"))
            {
                Console.WriteLine("MSNLocalClient.processMessage(" + message.ToString() + ") does not handle PRP YET!");
            }
            else if (message.getCommand().Equals("BLP"))
            {
                String[] data = message.getData();

                if (data[1].Equals("AL")) //allow unknown chat
                {
                    handleUnknownContact = MSNEnumerations.UnknownContact.allow_chat;
                }
                else if (data[1].Equals("BL")) //block unknown chat
                {
                    handleUnknownContact = MSNEnumerations.UnknownContact.disallow_chat;
                }
            }
            else if (message.getCommand().Equals("REA"))
            {
                String[] data = message.getData();
                if (data.Length >= 3)
                {
                    friendlyName = HttpUtility.UrlDecode(data[2]);
                    controller.sendLocalClientFriendlyNameChangeMessage();
                }
            }
            else if (message.getCommand().Equals("USR"))
            {
                String[] data = message.getData();
                if (data.Length >= 6)
                {
                    friendlyName = HttpUtility.UrlDecode(data[3]);
                    controller.sendLocalClientFriendlyNameChangeMessage();
                }
            }
        }
    }
}
