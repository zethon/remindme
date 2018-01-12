using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
    public class MSNController
    {
        #region class variables
        #region msn objects
        private MSNAuthentication authentication;
        private MSNLocalClient localClient;
        private MSNContactsList contactsList;
        private MSNSwitchboardController switchboardController;
        #endregion

        #region master socket handler stuff
        private Thread masterReadThread = null;
        private MSNSocketWrapper masterSocketWrapper = null;
        public MSNSocketWrapper MasterSocketWrapper
        {
            get { return masterSocketWrapper; }
        }
        #endregion

        #region events
        public event MSNEventDelegates.StatusChangedEventDelegate LocalClientStatusChanged;
        public event MSNEventDelegates.FriendlyNameChangedEventDelegate LocalFriendlyNameChanged;
        public event MSNEventDelegates.LoginStatusChangedEventDelegate LoginStatusChanged;
        public event MSNEventDelegates.LoginSettingsChangedEventDelegate LoginSettingsChanged;
        public event MSNEventDelegates.FriendlyNameChangedEventDelegate FriendlyNameChanged;
        public event MSNEventDelegates.PhoneNumberChangedEventDelegate PhoneNumberChanged;
        public event MSNEventDelegates.GroupModifiedEventDelegate GroupModified;
        public event MSNEventDelegates.GroupMemberChangedEventDelegate GroupMemberChanged;
        public event MSNEventDelegates.ContactStatusChangedEventDelegate ContactStatusChanged;
        public event MSNEventDelegates.ContactAddedEventDelegate ContactAdded;
        public event MSNEventDelegates.MasterListContactAdded MasterListContactAdded;
        #endregion

        #region other
        private MSNEnumerations.LoginStatus currentStatus = MSNEnumerations.LoginStatus.LOGGED_OUT;
        private Thread loginThread;
        #endregion
        #endregion

        #region constructors / dispose
        public MSNController()
        {
            #region initialise msn components
            authentication = new MSNAuthentication(this, "", "");
            localClient = new MSNLocalClient(this);
            contactsList = new MSNContactsList(this);
            switchboardController = new MSNSwitchboardController(this);
            #endregion

            #region initialise incomming message processor
            masterReadThread = new Thread(new ThreadStart(masterReadRedirectLoop));
            masterReadThread.Name = "Master Read Thread";
            masterReadThread.Start();
            #endregion

            this.LoginStatusChanged += new MSNEventDelegates.LoginStatusChangedEventDelegate(MSNController_LoginStatusChanged);
        }

        public void Dispose()
        {
            logoutMSNClient();

            while (currentStatus.Equals(MSNEnumerations.LoginStatus.LOGGED_IN))
            {
                try
                {
                    Thread.Sleep(20);
                }
                catch (Exception err1)
                {
                    Console.Error.WriteLine("Error waiting for disconnect in MSNController.Dispose()\r\n" + err1.ToString());
                }
            }

            try
            {
                //switchboardController.Dispose();
            }
            catch (Exception err5)
            {
                Console.Error.WriteLine("Error disposing switchboard in MSNController.Dispose()\r\n" + err5.ToString());
            }

            try
            {
                if (loginThread != null && !(loginThread.ThreadState == ThreadState.Aborted || loginThread.ThreadState == ThreadState.Stopped))
                {
                    loginThread.Abort();
                }
            }
            catch (Exception err4)
            {
                Console.Error.WriteLine("Error aborting login thread in MSNController.Dispose()\r\n" + err4.ToString());
            }

            try
            {
                masterReadThread.Abort();
            }
            catch (Exception err2)
            {
                Console.Error.WriteLine("Error aborting master read thread in MSNController.Dispose()\r\n" + err2.ToString());
            }

            try
            {
                //contactsList.Dispose();
            }
            catch (Exception err3)
            {
                Console.Error.WriteLine("Error disposing MSNContactsList in MSNController.Dispose()\r\n" + err3.ToString());
            }
        }
        #endregion

        #region message redirection handler
        private void masterReadRedirectLoop()
        {
            MSNMessage inMessage = new MSNMessage("NULL");
            String inMessageString = "NULL";

            while (true)
            {
                if (masterSocketWrapper != null && masterSocketWrapper.connected() == true)
                {

                    try
                    {
                        inMessage = masterSocketWrapper.recieve();
                        if (inMessage == null)
                        {
                            if (LoginStatusChanged != null)
                            {
                                LoginStatusChanged(MSNEnumerations.LoginStatus.LOGGED_OUT);
                            }
                            continue;
                        }

                        inMessageString = inMessage.ToString();

                        if (inMessageString.StartsWith("CHG"))
                        {
                            localClient.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("CHL"))
                        {
                            authentication.handleChallenge(inMessage);
                        }
                        else if (inMessageString.StartsWith("QRY"))
                        {
                            //don't need to do anything... just confirming successful CHL QRY responce
                        }
                        else if (inMessageString.StartsWith("BLP"))
                        {
                            localClient.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("PRP"))
                        {
                            localClient.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("REA"))
                        {
                            localClient.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("USR"))
                        {
                            localClient.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("LSG"))
                        {
                            contactsList.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("LST"))
                        {
                            contactsList.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("SYN"))
                        {
                            contactsList.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("ILN"))
                        {
                            contactsList.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("NLN"))
                        {
                            contactsList.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("BPR"))
                        {
                            contactsList.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("XFR"))
                        {
                            switchboardController.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("RNG"))
                        {
                            switchboardController.processMessage(inMessage);
                        }
                        else if (inMessageString.StartsWith("FLN"))
                        {
                            contactsList.processMessage(inMessage);
                        }
                        else
                        {
                            Console.Error.WriteLine("Unknown Message (" + inMessageString + ")");
                        }

                    }
                    catch (Exception)
                    {
                        Console.Error.WriteLine("Error in MSNController.masterReadRedirectLoop()");
                    }

                    Thread.Sleep(5);
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
        }
        #endregion

        #region accessors
        public void loginMSNClient()
        {
            try
            {
                if (currentStatus == MSNEnumerations.LoginStatus.LOGGED_OUT && loginThread == null)
                {
                    loginThread = new Thread(new ThreadStart(loginMSNClientPrivate));
                    loginThread.Name = "Login MSN Client Thread";
                    loginThread.Start();
                }
            }
            catch (Exception)
            {
            }
        }

        private void loginMSNClientPrivate()
        {
            if (LoginStatusChanged != null)
            {
                LoginStatusChanged(MSNEnumerations.LoginStatus.LOGGING_IN);
            }

            MSNSocketWrapper sw = null;
            try
            {
                sw = authentication.doAuthentication();
                masterSocketWrapper = sw;

                if (LoginStatusChanged != null)
                {
                    LoginStatusChanged(MSNEnumerations.LoginStatus.LOGGED_IN);
                }

                loginThread = null;

                localClient.FriendlyName = authentication.InitialFriendlyName;
            }
            catch (Exception)
            {
                if (LoginStatusChanged != null)
                {
                    LoginStatusChanged(MSNEnumerations.LoginStatus.LOGGED_OUT);
                }

                sw = null;
                loginThread = null;
            }
        }

        public void logoutMSNClient()
        {
            if (currentStatus.Equals(MSNEnumerations.LoginStatus.LOGGED_IN))
            {
                MSNSocketWrapper sw = masterSocketWrapper;
                masterSocketWrapper = null;

                if (LoginStatusChanged != null)
                {
                    LoginStatusChanged(MSNEnumerations.LoginStatus.LOGGED_OUT);
                }

                sw.send(new MSNMessage("OUT"));
            }
        }

        public void startConversation(List<String> initialUsers)
        {
            if (currentStatus == MSNEnumerations.LoginStatus.LOGGED_IN)
            {
                switchboardController.startConversation(initialUsers);
            }
        }

        public String Username
        {
            set
            {
                if (currentStatus == MSNEnumerations.LoginStatus.LOGGED_OUT)
                {
                    authentication.Username = value;

                    if (LoginSettingsChanged != null)
                    {
                        LoginSettingsChanged(authentication.Username, authentication.Password);
                    }
                }
            }
            get
            {
                return authentication.Username;
            }
        }

        public String Password
        {
            set
            {
                if (currentStatus == MSNEnumerations.LoginStatus.LOGGED_OUT)
                {
                    authentication.Password = value;

                    if (LoginSettingsChanged != null)
                    {
                        LoginSettingsChanged(authentication.Username, authentication.Password);
                    }
                }
            }
            get
            {
                return authentication.Password;
            }
        }

        public MSNEnumerations.UserStatus Status
        {
            get
            {
                return localClient.CurrentStatus;
            }
            set
            {
                localClient.CurrentStatus = value;
            }
        }

        internal void sendMessage(MSNMessage message)
        {
            if (masterSocketWrapper != null && masterSocketWrapper.connected() == true)
            {
                masterSocketWrapper.send(message);
            }
        }

        public MSNContactsList ContactsList
        {
            get
            {
                return contactsList;
            }
        }

        public String FriendlyName
        {
            get
            {
                return localClient.FriendlyName;
            }
            set
            {
                localClient.FriendlyName = value;
            }
        }

        public MSNContactsList ContactLists
        {
            get
            {
                return contactsList;
            }
        }

        public MSNSwitchboardController SwitchboardController
        {
            get
            {
                return switchboardController;
            }
        }

        public MSNEnumerations.LoginStatus LoginStatus
        {
            get
            {
                return currentStatus;
            }
            set
            {
                if (currentStatus == MSNEnumerations.LoginStatus.LOGGING_IN)
                {
                    throw new Exception("Currently connection, try again once either connected or disconnected");
                }
                else if (currentStatus == MSNEnumerations.LoginStatus.LOGGED_IN && value == MSNEnumerations.LoginStatus.LOGGED_IN)
                {
                    throw new Exception("Already logged in");
                }
                else if (currentStatus == MSNEnumerations.LoginStatus.LOGGED_IN && value == MSNEnumerations.LoginStatus.LOGGED_OUT)
                {
                    logoutMSNClient();
                }
                else if (currentStatus == MSNEnumerations.LoginStatus.LOGGED_OUT && value == MSNEnumerations.LoginStatus.LOGGED_OUT)
                {
                    throw new Exception("Already logged out");
                }
                else if (currentStatus == MSNEnumerations.LoginStatus.LOGGED_OUT && value == MSNEnumerations.LoginStatus.LOGGED_IN)
                {
                    loginMSNClient();
                }
                else
                {
                    throw new Exception("Cannot set status to logging in");
                }
            }
        }
        #endregion

        #region send messages to listeners
        internal void sendLocalClientStatusChangeMessage()
        {
            if (LocalClientStatusChanged != null)
            {
                LocalClientStatusChanged(localClient.CurrentStatus);
            }
        }

        internal void sendLocalClientFriendlyNameChangeMessage()
        {
            if (LocalFriendlyNameChanged != null)
            {
                LocalFriendlyNameChanged(this.Username, localClient.FriendlyName);
            }
        }

        internal void sendAddRemoveContactMessage(String username, bool isAdd)
        {
            if (ContactAdded != null)
            {
                ContactAdded(username, isAdd);
            }
        }

        internal void sendAddRemoveContactMasterListMessage(String username, MSNEnumerations.ContactLists list, bool isAdd)
        {
            if (MasterListContactAdded != null)
            {
                MasterListContactAdded(username, isAdd, list);
            }
        }

        internal void sendContactFriendlyNameChangeMessage(String username, String friendlyName)
        {
            if (FriendlyNameChanged != null)
            {
                FriendlyNameChanged(username, friendlyName);
            }
        }

        internal void sendContactPhoneChangeMessage(String username, MSNEnumerations.PhoneTypes type, String newNumber)
        {
            if (PhoneNumberChanged != null)
            {
                PhoneNumberChanged(username, type, newNumber);
            }
        }

        internal void sendAddRemoveGroupMessage(String groupName, bool isAdd)
        {
            if (GroupModified != null)
            {
                GroupModified(groupName, isAdd);
            }
        }

        internal void sendAddRemoveContactFromGroupMessage(String groupName, String username, bool isAdd)
        {
            if (GroupMemberChanged != null)
            {
                GroupMemberChanged(username, groupName, isAdd);
            }
        }

        internal void sendContactStatusChangeMessage(String username, MSNEnumerations.UserStatus status)
        {
            if (ContactStatusChanged != null)
            {
                ContactStatusChanged(username, status);
            }
        }
        #endregion

        #region message handlers (listener)
        void MSNController_LoginStatusChanged(MSNEnumerations.LoginStatus newStatus)
        {
            this.currentStatus = newStatus;
        }
        #endregion
    }
}
