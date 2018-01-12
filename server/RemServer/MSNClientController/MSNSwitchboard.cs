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
    public sealed class MSNSwitchboard
    {
        #region class variables
        private MSNController controller = null;
        private List<String> connectedUsers = new List<string>();

        private MSNSocketWrapper switchboardSocket = null;
        public MSNSocketWrapper SwitchboardSock
        {
            get { return switchboardSocket; }
        }

        private List<String> lastMessageUsers = new List<string>();
        private List<String> activeUsers = new List<string>();

        #region msg stuff
        private Queue<MSNUserMessage> incommingMSGQueue = new Queue<MSNUserMessage>();
        private Thread incommingMSGThread = null;

        private Queue<MSNUserMessage> outgoingMSGQueue = new Queue<MSNUserMessage>();
        private Thread outgoingMSGThread = null;
        #endregion

        private Thread incommingMessageHandlerThread = null;

        public event MSNEventDelegates.SwitchboardUserConnected UserConnected;
        public event MSNEventDelegates.MessageRecieved MessageRecieved;
        public event MSNEventDelegates.MessageSent MessageSent;

        private MSNListenableList<IMSNSwitchboardPlugin> plugins = new MSNListenableList<IMSNSwitchboardPlugin>();
        #endregion

        #region constructors / reconnects
        internal MSNSwitchboard(MSNController controller, String addressString, int addressPort, String authenticateString, List<String> initialUsers) //handles conversations started by local client
        {
            try
            {
                this.controller = controller;

                #region connect to switchboard
                try
                {
                    switchboardSocket = new MSNSocketWrapper(addressString, addressPort, false);

                    if (!switchboardSocket.connected())
                    {
                        Console.WriteLine("Error making switchboard connection in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ", " + initialUsers.ToString() + ")");
                        throw new Exception("Could not connect to switchboard in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ", " + initialUsers.ToString() + ")");
                    }
                }
                catch (Exception)
                {

                    Console.WriteLine("Error making switchboard connection in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ", " + initialUsers.ToString() + ")");
                    return;
                }
                #endregion

                #region authenticate
                //USR 7 alice@passport.com 189597.1056411784.29994\r\n
                switchboardSocket.send(new MSNMessage("USR " + MSNTrIDGenerator.NextID() + " " + controller.Username + " " + authenticateString + "\r\n"));
                #endregion

                #region add initial users
                //CAL 8 bob@passport.com\r\n
                if (initialUsers != null)
                {
                    for (int i = 0; i < initialUsers.Count; i++)
                    {
                        switchboardSocket.send(new MSNMessage("CAL " + MSNTrIDGenerator.NextID() + " " + initialUsers[i] + "\r\n"));
                    }
                }
                #endregion

                #region incommingMessageReadThread
                incommingMessageHandlerThread = new Thread(new ThreadStart(processIncommingMessageLoop));
                incommingMessageHandlerThread.Name = "MSNSwitchboard incomming read thread";
                incommingMessageHandlerThread.Start();
                #endregion

                #region incommingMSGThread
                incommingMSGThread = new Thread(new ThreadStart(incommingMSGLoop));
                incommingMSGThread.Name = "Incomming MSG handler thread";
                incommingMSGThread.Start();
                #endregion

                #region outgoingMSGThread
                outgoingMSGThread = new Thread(new ThreadStart(outgoingMSGLoop));
                outgoingMSGThread.Name = "Outgoing MSG handler thread";
                outgoingMSGThread.Start();
                #endregion

                plugins.ElementAdded += new MSNListenableList<IMSNSwitchboardPlugin>.ElementAddedDelegate(plugins_ElementAdded);
            }
            catch (Exception)
            {
                //Console.WriteLine("Error establishing switchboard session in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ", " + initialUsers.ToString() + ")");
            }
        }

        internal MSNSwitchboard(MSNController controller, String addressString, int addressPort, String authenticateString, String rngTrID) //handles conversations started by local client
        {
            try
            {
                this.controller = controller;

                #region connect to switchboard
                try
                {
                    switchboardSocket = new MSNSocketWrapper(addressString, addressPort, false);

                    if (!switchboardSocket.connected())
                    {
                        Console.WriteLine("Error making switchboard connection in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ")");
                        throw new Exception("Could not connect to switchboard in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ")");
                    }
                }
                catch (Exception)
                {

                    Console.WriteLine("Error making switchboard connection in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ")");
                    return;
                }
                #endregion

                #region authenticate
                //ANS 1 alice@passport.com 1056411141.26158 17342299\r\n
                switchboardSocket.send(new MSNMessage("ANS " + MSNTrIDGenerator.NextID() + " " + controller.Username + " " + authenticateString + " " + rngTrID + "\r\n"));
                #endregion

                #region incommingMessageReadThread
                incommingMessageHandlerThread = new Thread(new ThreadStart(processIncommingMessageLoop));
                incommingMessageHandlerThread.Name = "MSNSwitchboard incomming read thread";
                incommingMessageHandlerThread.Start();
                #endregion

                #region incommingMSGThread
                incommingMSGThread = new Thread(new ThreadStart(incommingMSGLoop));
                incommingMSGThread.Name = "Incomming MSG handler thread";
                incommingMSGThread.Start();
                #endregion

                #region outgoingMSGThread
                outgoingMSGThread = new Thread(new ThreadStart(outgoingMSGLoop));
                outgoingMSGThread.Name = "Outgoing MSG handler thread";
                outgoingMSGThread.Start();
                #endregion

                plugins.ElementAdded += new MSNListenableList<IMSNSwitchboardPlugin>.ElementAddedDelegate(plugins_ElementAdded);
            }
            catch (Exception)
            {
                Console.WriteLine("Error establishing switchboard session in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ")");
            }
        }

        internal void reconnect(String addressString, int addressPort, String authenticateString, List<String> initialUsers)
        {
            try
            {
                activeUsers.Clear();

                #region connect to switchboard
                try
                {
                    switchboardSocket = new MSNSocketWrapper(addressString, addressPort, false);

                    if (!switchboardSocket.connected())
                    {
                        Console.WriteLine("Error making switchboard connection in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ", " + initialUsers.ToString() + ")");
                        throw new Exception("Could not connect to switchboard in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ", " + initialUsers.ToString() + ")");
                    }
                }
                catch (Exception)
                {

                    Console.WriteLine("Error making switchboard connection in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ", " + initialUsers.ToString() + ")");
                    return;
                }
                #endregion

                #region authenticate
                //USR 7 alice@passport.com 189597.1056411784.29994\r\n
                switchboardSocket.send(new MSNMessage("USR " + MSNTrIDGenerator.NextID() + " " + controller.Username + " " + authenticateString + "\r\n"));
                #endregion

                #region add initial users
                //CAL 8 bob@passport.com\r\n
                if (initialUsers != null)
                {
                    for (int i = 0; i < initialUsers.Count; i++)
                    {
                        switchboardSocket.send(new MSNMessage("CAL " + MSNTrIDGenerator.NextID() + " " + initialUsers[i] + "\r\n"));
                    }
                }
                #endregion

                #region incommingMessageReadThread
                try
                {
                    incommingMessageHandlerThread.Abort();
                }
                catch (Exception)
                {
                }

                incommingMessageHandlerThread = new Thread(new ThreadStart(processIncommingMessageLoop));
                incommingMessageHandlerThread.Name = "MSNSwitchboard incomming read thread";
                incommingMessageHandlerThread.Start();
                #endregion

                #region incommingMSGThread
                try
                {
                    incommingMSGThread.Abort();
                }
                catch (Exception)
                {
                }

                incommingMSGThread = new Thread(new ThreadStart(incommingMSGLoop));
                incommingMSGThread.Name = "Incomming MSG handler thread";
                incommingMSGThread.Start();
                #endregion

                #region outgoingMSGThread
                try
                {
                    outgoingMSGThread.Abort();
                }
                catch (Exception)
                {
                }

                outgoingMSGThread = new Thread(new ThreadStart(outgoingMSGLoop));
                outgoingMSGThread.Name = "Outgoing MSG handler thread";
                outgoingMSGThread.Start();
                #endregion
            }
            catch (Exception)
            {
                Console.WriteLine("Error establishing switchboard session in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ", " + initialUsers.ToString() + ")");
            }
        }

        internal void reconnect(String addressString, int addressPort, String authenticateString, String rngTrID)
        {
            try
            {
                activeUsers.Clear();

                #region connect to switchboard
                try
                {
                    switchboardSocket = new MSNSocketWrapper(addressString, addressPort, false);

                    if (!switchboardSocket.connected())
                    {
                        Console.WriteLine("Error making switchboard connection in MSNSwitchboard.reconnect(" + addressString + ", " + addressPort + ", " + authenticateString + ")");
                        throw new Exception("Could not connect to switchboard in MSNSwitchboard.reconnect(" + addressString + ", " + addressPort + ", " + authenticateString + ")");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error making switchboard connection in MSNSwitchboard.reconnect(" + addressString + ", " + addressPort + ", " + authenticateString + ")");
                    return;
                }
                #endregion

                #region authenticate
                //ANS 1 alice@passport.com 1056411141.26158 17342299\r\n
                switchboardSocket.send(new MSNMessage("ANS " + MSNTrIDGenerator.NextID() + " " + controller.Username + " " + authenticateString + " " + rngTrID + "\r\n"));
                #endregion

                #region incommingMessageReadThread
                try
                {
                    incommingMessageHandlerThread.Abort();
                }
                catch (Exception)
                {
                }

                incommingMessageHandlerThread = new Thread(new ThreadStart(processIncommingMessageLoop));
                incommingMessageHandlerThread.Name = "MSNSwitchboard incomming read thread";
                incommingMessageHandlerThread.Start();
                #endregion

                #region incommingMSGThread
                try
                {
                    incommingMSGThread.Abort();
                }
                catch (Exception)
                {
                }

                incommingMSGThread = new Thread(new ThreadStart(incommingMSGLoop));
                incommingMSGThread.Name = "Incomming MSG handler thread";
                incommingMSGThread.Start();
                #endregion

                #region outgoingMSGThread
                try
                {
                    outgoingMSGThread.Abort();
                }
                catch (Exception)
                {
                }

                outgoingMSGThread = new Thread(new ThreadStart(outgoingMSGLoop));
                outgoingMSGThread.Name = "Outgoing MSG handler thread";
                outgoingMSGThread.Start();
                #endregion
            }
            catch (Exception)
            {
                Console.WriteLine("Error establishing switchboard session in MSNSwitchboard(" + controller.ToString() + ", " + addressString + ", " + addressPort + ", " + authenticateString + ")");
            }
        }
        #endregion

        #region recieved messages handlers
        private void incommingMSGLoop()
        {
            while (true)
            {
                MSNUserMessage message = null;

                try
                {
                    #region wait for a message to process
                    while (incommingMSGQueue.Count == 0)
                    {
                        Thread.Sleep(100);
                    }
                    #endregion

                    #region get message
                    message = incommingMSGQueue.Dequeue();

                    if (message == null)
                    {
                        continue;
                    }
                    #endregion

                    #region process message through plugins
                    if (message.ProcessByPlugins)
                    {
                        foreach (IMSNSwitchboardPlugin plugin in plugins)
                        {
                            try
                            {
                                plugin.processIngoingMessage(message);
                            }
                            catch (Exception err)
                            {
                                Console.WriteLine("IMSNSwitchboard plugin error: " + err.ToString());
                            }
                        }
                    }
                    #endregion

                    #region writing message to gui plugins IF message.getShowOrRecieve() == true
                    if (message.getDisplay() && MessageRecieved != null)
                    {
                        MessageRecieved(message);
                    }

                    /**
                    if (message.getDisplay())
                    {
                        for (int i = 0; i < windowPlugins.Count; i++)
                        {
                            MSNSwitchboardGUIPluginListener plugin = null;

                            try
                            {
                                plugin = (MSNSwitchboardGUIPluginListener)windowPlugins[i];
                                plugin.processIngoingMessage(message);
                            }
                            catch (Exception)
                            {
                                if (plugin != null)
                                {
                                    Console.WriteLine("Error sending inbound MSG to gui plugin (" + plugin.getName() + ") in MSNSwitchboard.incommingMSGLoop()");
                                }
                                else
                                {
                                    Console.WriteLine("Error sending inbound MSG to gui plugin (null) in MSNSwitchboard.incommingMSGLoop()");
                                }
                            }
                        }
                    }
                    **/
                    #endregion
                }
                catch (Exception)
                {
                    if (message != null)
                    {
                        Console.WriteLine("Error processing message in MSNSwitchboad.incommingMSGLoop(), message = " + message.ToString());
                    }
                    else
                    {
                        Console.WriteLine("Error processing message in MSNSwitchboad.incommingMSGLoop(), message = null");
                    }
                }
            }
        }

        private void processIncommingMessageLoop()
        {
            while (true)
            {
                MSNMessage message = null;

                try
                {
                    #region wait for connected socket
                    while (switchboardSocket.connected() != true)
                    {
                        try
                        {
                            Thread.Sleep(100);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    #endregion

                    message = switchboardSocket.recieve();

                    #region ignore null messages
                    if (message == null)
                    {
                        continue;
                    }
                    #endregion

                    String messageString = message.ToString();

                    #region handle JOI
                    if (messageString.StartsWith("JOI"))
                    {
                        //JOI dave@passport.com Dave\r\n
                        String username = message.getTokens()[1].ToString();
                        if (connectedUsers.Contains(username) != true)
                        {
                            connectedUsers.Add(username);
                        }

                        if (activeUsers.Contains(username) != true)
                        {
                            activeUsers.Add(username);
                        }

                        sendSwitchboardListenerUserConnectedDisconnected(username, true);
                    }
                    #endregion
                    #region handle CAL
                    else if (messageString.StartsWith("CAL"))
                    {
                        //CAL 8 RINGING 17342299\r\n
                    }
                    #endregion
                    #region handle IRO
                    else if (messageString.StartsWith("IRO"))
                    {
                        //IRO 1 1 2 example@passport.com Mike\r\n
                        String username = message.getTokens()[4].ToString();
                        if (connectedUsers.Contains(username) != true)
                        {
                            connectedUsers.Add(username);
                        }

                        if (activeUsers.Contains(username) != true)
                        {
                            activeUsers.Add(username);
                        }

                        sendSwitchboardListenerUserConnectedDisconnected(username, true);
                    }
                    #endregion
                    #region handle MSG
                    else if (messageString.StartsWith("MSG"))
                    {
                        //MSG example@passport.com Mike 133\r\n
                        //MIME-Version: 1.0\r\n
                        //Content-Type: text/plain; charset=UTF-8\r\n
                        //X-MMS-IM-Format: FN=Arial; EF=I; CO=0; CS=0; PF=22\r\n
                        //\r\n
                        //Hello! How are you? 

                        try
                        {
                            String wholeString = messageString + "\r\n" + switchboardSocket.recieve(int.Parse(message.getData()[1]));
                            String[] wholeStringTokens = wholeString.Split(new String[] { "\r\n" }, StringSplitOptions.None);

                            if (wholeStringTokens.Length >= 4 && wholeStringTokens[3].StartsWith("TypingUser:"))
                            {
                                MSNUserMessage num = new MSNUserTypingMessage(wholeStringTokens[0].Split(new char[] { ' ' })[1], MSNEnumerations.UserMessageType.incomming_typing_message);
                                incommingMSGQueue.Enqueue(num);
                            }
                            else if (wholeStringTokens[2].StartsWith("Content-Type: text/x-mms-emoticon"))
                            {
                                String payload = "";
                                for (int i = 4; i < wholeStringTokens.Length; i++)
                                {
                                    payload += wholeStringTokens[i] + "\r\n";
                                }

                                MSNUserMessage num = new MSNUserIncommingEmoticon(wholeStringTokens[0].Split(new char[] { ' ' })[1], payload);
                                incommingMSGQueue.Enqueue(num);
                            }
                            else
                            {
                                #region process payload
                                String payload = "";
                                for (int i = 5; i < wholeStringTokens.Length; i++)
                                {
                                    payload += wholeStringTokens[i] + "\r\n";
                                }
                                if (payload.EndsWith("\r\n"))
                                {
                                    payload = payload.Substring(0, payload.Length - 2);
                                }
                                #endregion

                                if (wholeStringTokens[3].Equals(""))
                                {
                                    wholeStringTokens[3] = "X-MMS-IM-Format: FN=Arial; EF=I; CO=0; CS=0; PF=22";
                                }
                                MSNUserMessage num = new MSNUserIncommingMessage((wholeStringTokens[0].Split(new char[] { ' ' }))[1], (wholeStringTokens[3].Split(new char[] { ' ' }))[1].Replace("FN=", "").Replace(";", ""), payload);
                                incommingMSGQueue.Enqueue(num);
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Error processing message in MSNSwitchboard.processIncommingMessage(), message = " + messageString);
                        }
                    }
                    #endregion
                    #region handle BYE
                    else if (messageString.StartsWith("BYE"))
                    {
                        //BYE carol@passport.com\r\n
                        String username = messageString.Replace("\r\n", "").Split(new char[] { ' ' })[1];
                        activeUsers.Remove(username);
                        //connectedUsers.Remove(username);
                        sendSwitchboardListenerUserConnectedDisconnected(username, false);
                    }
                    #endregion
                }
                catch (Exception)
                {
                    if (message != null)
                    {
                        Console.WriteLine("Error processing incomming message in MSNSwitchboard.processIncommingMessageLoop(), message = " + message.ToString());
                    }
                    else
                    {
                        Console.WriteLine("Error processing incomming message in MSNSwitchboard.processIncommingMessageLoop(), message = NULL");
                    }

                    if (switchboardSocket.connected() != true)
                    {
                        while (switchboardSocket != null && switchboardSocket.connected() != true)
                        {
                            try
                            {
                                Thread.Sleep(300);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Error waiting for switchboard to be reconnected in MSNSwitchboard.processIncommingMessageLoop()");
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region sending messages handlers
        private void sendMessage(MSNMessage message)
        {
            if (switchboardSocket.connected() == false)
            {
                List<String> initialUsers = new List<string>();
                for (int i = 0; i < lastMessageUsers.Count; i++)
                {
                    initialUsers.Add(lastMessageUsers[i].ToString());
                }
                controller.startConversation(initialUsers);

                while (switchboardSocket.connected() == false)
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

            switchboardSocket.send(message);
        }

        public void sendMessage(MSNUserMessage message)
        {
            #region reconnect socket if needs be
            if (switchboardSocket.connected() != true)
            {
                List<String> initialUsers = new List<String>();
                for (int i = 0; i < lastMessageUsers.Count && i < lastMessageUsers.Count; i++) //added i < lastMessageUsers.Cout to remove exceptions
                {
                    initialUsers[i] = lastMessageUsers[i].ToString();
                    controller.startConversation(initialUsers);
                }
            }
            #endregion

            #region reinvite people if they've all left
            if (activeUsers.Count == 0 && lastMessageUsers.Count > 0)
            {
                for (int i = 0; i < lastMessageUsers.Count; i++)
                {
                    invite(lastMessageUsers[i].ToString());
                }
            }
            else if (activeUsers.Count == 0 && lastMessageUsers.Count == 0)
            {
                for (int i = 0; i < connectedUsers.Count; i++)
                {
                    invite(connectedUsers[i].ToString());
                }
            }
            #endregion

            if (message != null)
            {
                outgoingMSGQueue.Enqueue(message);
            }

            #region update last messages user
            lastMessageUsers.Clear();

            if (activeUsers.Count > 0)
            {
                for (int i = 0; i < activeUsers.Count; i++)
                {
                    lastMessageUsers.Add(activeUsers[i].ToString());
                }
            }
            #endregion
        }

        private void outgoingMSGLoop()
        {
            while (true)
            {
                MSNUserMessage message = null;

                try
                {
                    #region wait for a message to process
                    while (outgoingMSGQueue.Count == 0)
                    {
                        Thread.Sleep(20);
                    }
                    #endregion

                    #region get message
                    message = outgoingMSGQueue.Dequeue();
                    #endregion

                    #region process message through plugins
                    if (message.ProcessByPlugins)
                    {
                        foreach (IMSNSwitchboardPlugin plugin in plugins)
                        {
                            try
                            {
                                plugin.processOutgoingMessage(message);
                            }
                            catch (Exception err)
                            {
                                Console.WriteLine("IMSNSwitchboard plugin error: " + err.ToString());
                            }
                        }
                    }
                    #endregion

                    #region wait for socket / active users to be valid if actually sending the message
                    if (message.getSend())
                    {
                        #region reconnect socket if needs be
                        if (switchboardSocket.connected() != true)
                        {
                            List<String> initialUsers = new List<string>();
                            for (int i = 0; i < lastMessageUsers.Count; i++)
                            {
                                initialUsers.Add(lastMessageUsers[i].ToString());
                                controller.startConversation(initialUsers);
                            }
                        }
                        #endregion

                        #region reinvite people if they've all left
                        if (activeUsers.Count == 0 && lastMessageUsers.Count > 0)
                        {
                            for (int i = 0; i < lastMessageUsers.Count; i++)
                            {
                                invite(lastMessageUsers[i].ToString());
                            }
                        }
                        else if (activeUsers.Count == 0 && lastMessageUsers.Count == 0)
                        {
                            for (int i = 0; i < connectedUsers.Count; i++)
                            {
                                invite(connectedUsers[i].ToString());
                            }
                        }
                        #endregion

                        #region wait for active users
                        while (activeUsers.Count == 0)
                        {
                            try
                            {
                                Thread.Sleep(50);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region writing message to gui plugins IF message.getShowOrRecieve() == true
                    if (message.getDisplay() && MessageSent != null)
                    {
                        MessageSent(message);
                    }
                    /**
                    if (message.getDisplay())
                    {
                        for (int i = 0; i < windowPlugins.Count; i++)
                        {
                            MSNSwitchboardGUIPluginListener plugin = null;

                            try
                            {
                                plugin = (MSNSwitchboardGUIPluginListener)windowPlugins[i];
                                plugin.processOutgoingMessage(message);
                            }
                            catch (Exception)
                            {
                                if (plugin != null)
                                {
                                    Console.WriteLine("Error sending outgoing MSG to gui plugin (" + plugin.getName() + ") in MSNSwitchboard.incommingMSGLoop()");
                                }
                                else
                                {
                                    Console.WriteLine("Error sending outgoing MSG to gui plugin (null) in MSNSwitchboard.incommingMSGLoop()");
                                }
                            }
                        }
                    }
                    **/
                    #endregion

                    #region actually send the message to socket IF message.getSendOrRecieve() == true
                    if (message.getSend())
                    {
                        if (message.getMessageType().Equals(MSNEnumerations.UserMessageType.outgoing_text_message))
                        {
                            sendMessage(new MSNMessage(new MSNUserTypingMessage(controller.Username, MSNEnumerations.UserMessageType.outgoing_typing_message).ToString()));
                        }
                        sendMessage(new MSNMessage(message.ToString()));
                    }
                    #endregion
                }
                catch (Exception)
                {
                    if (message != null)
                    {
                        Console.WriteLine("Error processing message in MSNSwitchboad.incommingMSGLoop(), message = " + message.ToString());
                    }
                    else
                    {
                        Console.WriteLine("Error processing message in MSNSwitchboad.incommingMSGLoop(), message = null");
                    }
                }
            }
        }
        #endregion

        #region invite
        public void invite(String username)
        {
            sendMessage(new MSNMessage("CAL " + MSNTrIDGenerator.NextID() + " " + username + "\r\n"));
        }
        #endregion

        #region leave
        public void closeConversation()
        {
            sendMessage(new MSNMessage("BYE " + controller.Username + "\r\n"));
        }
        #endregion

        #region switchboard listener methods
        private void sendSwitchboardListenerUserConnectedDisconnected(String username, bool connected)
        {
            if (UserConnected != null)
            {
                UserConnected(this, username, connected);
            }
        }
        #endregion

        #region accessors
        public List<String> getConnectedUsers()
        {
            //return connectedUsers;
            return connectedUsers;
        }

        public MSNListenableList<IMSNSwitchboardPlugin> Plugins
        {
            get
            {
                return plugins;
            }
        }
        #endregion

        private void plugins_ElementAdded(MSNListenableList<IMSNSwitchboardPlugin> sender, MSNListenableList<IMSNSwitchboardPlugin>.ElementAddedEventArgs<IMSNSwitchboardPlugin> args)
        {
            args.Item.Switchboard = this;
        }
    }
}
