using System;
using System.Collections.Generic;
using System.Collections;
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
    public sealed class MSNSwitchboardController //: Form, IDisposable
    {
        #region class variables
        private MSNController controller = null;

        private Thread incommingMessageThread = null;

        private Dictionary<String, List<String>> newConversationContacts = new Dictionary<string, List<string>>();

        private List<MSNSwitchboard> activeSwitchboards = new List<MSNSwitchboard>();

        private Queue<MSNMessage> incommingMessageQueue = new Queue<MSNMessage>();
        private Queue<MSNMessage> incommingXFRQueue = new Queue<MSNMessage>();
        private Queue<MSNMessage> incommingRNGQueue = new Queue<MSNMessage>();

        private delegate void RefreshDelegate();

        public event MSNEventDelegates.SwitchboardCreated SwitchboardCreated;
        public event MSNEventDelegates.SwitchboardReCreated SwitchboardReCreated;
        #endregion

        #region constructors / dispose
        internal MSNSwitchboardController(MSNController controller)
        {
            this.controller = controller;

            incommingMessageThread = new Thread(new ThreadStart(processMessageLoop));
            incommingMessageThread.Name = "MSNSwitchboard incomming message processor thread";
            incommingMessageThread.Start();
        }
        #endregion

        internal void startConversation(List<String> usernames)
        {
            //XFR 15 SB\r\n

            try
            {
                int id = MSNTrIDGenerator.NextID();
                newConversationContacts.Add(id.ToString(), usernames);
                controller.sendMessage(new MSNMessage("XFR " + id + " SB\r\n"));
            }
            catch (Exception)
            {
                Console.WriteLine("Error in MSNSwitchboardController.startConversation");
            }
        }

        #region incomming message processing
        private void processIncommingXFR()
        {
            while (incommingXFRQueue.Count > 0)
            {
                MSNMessage message = incommingXFRQueue.Dequeue();

                try
                {
                    #region process command
                    //XFR 12 SB 207.46.26.161:1863 CKI 1790215149.6727142.1184104\r\n
                    String address = message.getData()[1].Replace(":1863", "");
                    int port = 1863;
                    String authCode = message.getData()[3].ToString();
                    String trID = message.getTrID().ToString();
                    List<String> initialUsers = newConversationContacts[trID.ToString()];
                    #endregion

                    #region try to find existing switchboard to connect to
                    MSNSwitchboard existingAddTo = null;
                    for (int i = 0; i < activeSwitchboards.Count; i++)
                    {
                        MSNSwitchboard testSwitchboard = (MSNSwitchboard)activeSwitchboards[i];
                        List<String> switchboardUsers = testSwitchboard.getConnectedUsers();

                        #region test initial users = switchboardUsers
                        int checkCounter = 0;
                        for (int x = 0; x < initialUsers.Count; x++)
                        {
                            for (int y = 0; y < switchboardUsers.Count; y++)
                            {
                                if (initialUsers[x].Equals(switchboardUsers[y].ToString()))
                                {
                                    checkCounter++;
                                }
                            }
                        }

                        bool usersEqual = false;
                        if (initialUsers.Count == switchboardUsers.Count && checkCounter == initialUsers.Count)
                        {
                            usersEqual = true;
                        }
                        #endregion

                        if (usersEqual)
                        {
                            existingAddTo = testSwitchboard;
                            existingAddTo.reconnect(address, port, authCode, initialUsers);

                            if (SwitchboardReCreated != null)
                            {
                                SwitchboardReCreated(existingAddTo);
                            }
                        }
                    }
                    #endregion

                    #region build switchboard (and load plugins) if no existing switchboard
                    if (existingAddTo == null)
                    {
                        MSNSwitchboard s = new MSNSwitchboard(controller, address, port, authCode, initialUsers);

                        activeSwitchboards.Add(s);

                        if (SwitchboardCreated != null)
                        {
                            SwitchboardCreated(s);
                        }
                    }
                    #endregion
                }
                catch (Exception)
                {
                    Console.WriteLine("Error processing XFR message in MSNSwitchboardController.processXFR(), message = " + message.ToString());
                }
            }
        }

        private void processIncommingRNG()
        {
            while (incommingRNGQueue.Count > 0)
            {
                MSNMessage message = incommingRNGQueue.Dequeue();

                try
                {
                    #region process command
                    //RNG 11752013 207.46.108.38:1863 CKI 849102291.520491113 example@passport.com Example%20Name\r\n
                    String address = message.getData()[0].Replace(":1863", "");
                    int port = 1863;
                    String authCode = message.getData()[2];
                    String rngTrID = message.getTrID().ToString();
                    String[] initialUsers = new String[] { message.getData()[3] };
                    #endregion

                    #region try to find existing switchboard to connect to
                    MSNSwitchboard existingAddTo = null;
                    for (int i = 0; i < activeSwitchboards.Count; i++)
                    {
                        MSNSwitchboard testSwitchboard = (MSNSwitchboard)activeSwitchboards[i];
                        List<String> switchboardUsers = testSwitchboard.getConnectedUsers();

                        #region test initial users = switchboardUsers
                        int checkCounter = 0;
                        for (int x = 0; x < initialUsers.Length; x++)
                        {
                            for (int y = 0; y < switchboardUsers.Count; y++)
                            {
                                if (initialUsers[x].Equals(switchboardUsers[y].ToString()))
                                {
                                    checkCounter++;
                                }
                            }
                        }

                        bool usersEqual = false;
                        if (initialUsers.Length == switchboardUsers.Count && checkCounter == initialUsers.Length)
                        {
                            usersEqual = true;
                        }
                        #endregion

                        if (usersEqual)
                        {
                            existingAddTo = testSwitchboard;
                            existingAddTo.reconnect(address, port, authCode, rngTrID);

                            if (SwitchboardReCreated != null)
                            {
                                SwitchboardReCreated(existingAddTo);
                            }
                        }
                    }
                    #endregion

                    #region build switchboard (and load plugins) if no existing switchboard found
                    if (existingAddTo == null)
                    {
                        MSNSwitchboard s = new MSNSwitchboard(controller, address, port, authCode, rngTrID);

                        activeSwitchboards.Add(s);

                        if (SwitchboardCreated != null)
                        {
                            SwitchboardCreated(s);
                        }
                    }
                    #endregion
                }
                catch (Exception)
                {
                    Console.WriteLine("Error processing RNG message in MSNSwitchboardController.processRNG(), message = " + message.ToString());
                }
            }
        }

        private void processMessageLoop()
        {
            while (true)
            {
                if (incommingMessageQueue.Count > 0)
                {
                    MSNMessage message = incommingMessageQueue.Dequeue();

                    try
                    {
                        String command = message.getCommand();

                        #region process XFR
                        if (command.StartsWith("XFR"))
                        {
                            incommingXFRQueue.Enqueue(message);

                            Thread t = new Thread(new ThreadStart(processIncommingXFR));
                            t.Start();
                            //Invoke(new RefreshDelegate(processIncommingXFR));
                        }
                        #endregion
                        #region process RNG
                        else if (command.StartsWith("RNG"))
                        {
                            incommingRNGQueue.Enqueue(message);

                            Thread t = new Thread(new ThreadStart(processIncommingRNG));
                            t.Start();
                            //Invoke(new RefreshDelegate(processIncommingRNG));
                        }
                        #endregion
                    }
                    catch (Exception)
                    {
                        if (message != null)
                        {
                            Console.WriteLine("ERROR processing message in MSNSwitchboardController.processMessageLoop(), message = " + message.ToString());
                        }
                        else
                        {
                            Console.WriteLine("ERROR processing message in MSNSwitchboardController.processMessageLoop(), message = null");
                        }
                    }
                }
                else
                {
                    try
                    {
                        Thread.Sleep(50);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        internal void processMessage(MSNMessage message)
        {
            incommingMessageQueue.Enqueue(message);
        }
        #endregion
    }
}
