using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public class MSNContactsList //: MSNLoginLogoutListenerInterface, IDisposable
        {
            #region class variables
            private MSNController controller = null;
            private MSNContact lastContact = null;

            private int versionID;

            private Dictionary<String, MSNContact> contacts = new Dictionary<string,MSNContact>();
            private Dictionary<String, MSNGroup> groupsByName = new Dictionary<string,MSNGroup>();
            private Dictionary<int, MSNGroup> groupsByNumber = new Dictionary<int,MSNGroup>();

            private Queue<MSNMessage> processingQueue = new Queue<MSNMessage>(); //queue of messages from server to process
            private Thread processingQueueThread = null; //thread to process incomming messages

            private MSNEnumerations.NewContact handleNewContact = MSNEnumerations.NewContact.manual_add; //how to handle a new contact (auto add to allow list or not)
            #endregion

            #region constructor / dispose
            internal MSNContactsList(MSNController controller)
            {
                this.controller = controller;
                controller.LoginStatusChanged +=new MSNEventDelegates.LoginStatusChangedEventDelegate(controller_LoginStatusChanged);

                processingQueueThread = new Thread(new ThreadStart(processMessageLoop));
                processingQueueThread.Name = "Contacts List Message Processing Thread";
                processingQueueThread.Start();
            }
            #endregion

            public void AddBuddy(string strBuddyName)
            {
                if (controller != null)
                {
                    
                    //controller.MasterSocketWrapper.send(new MSNMessage("ADD 18 AL " + strBuddyName + " " + strBuddyName + "\r\n"));
                }
            }

            private void reset()
            {
                handleNewContact = MSNEnumerations.NewContact.manual_add;
                versionID = 0;
                contacts.Clear();
                groupsByName.Clear();
                groupsByNumber.Clear();
                processingQueue.Clear();
            }

            #region event handlers

            private void controller_LoginStatusChanged(MSNEnumerations.LoginStatus newStatus)
            {
 	            if (newStatus == MSNEnumerations.LoginStatus.LOGGED_OUT)
                {
                    reset();
                }
                else if (newStatus == MSNEnumerations.LoginStatus.LOGGED_IN)
                {
                    //send initial sync command
                    controller.sendMessage(new MSNMessage("SYN " + MSNTrIDGenerator.NextID() + " " + versionID + "\r\n"));
                }
            }

#endregion

            #region incomming message handlers
            internal void processMessage(MSNMessage message)
            {
                //Don't process, just queue it up for processing
                processingQueue.Enqueue(message);
            }

            private MSNContact getContact(String username)
            {
                if (contacts.ContainsKey(username))
                {
                    return contacts[username];
                }
                else
                {
                    MSNContact newContact = new MSNContact(controller, username);
                    contacts.Add(username, newContact);

                    controller.sendAddRemoveContactMessage(username, true);
                    return newContact;
                }
            }

            private void processMessageLoop()
            {
                MSNMessage message = new MSNMessage("NULL");

                while (true)
                {
                    if (processingQueue.Count > 0)
                    {
                        try
                        {
                            #region get message from queue
                            message = processingQueue.Dequeue();

                            String command = message.getCommand();
                            String[] data = message.getTokens();
                            #endregion
                            #region handle SYN
                            if (command.Equals("SYN"))
                            {
                                //SYN 9 1 18 7
                                versionID = int.Parse(data[2]);
                            }
                            #endregion
                            #region handle GTC
                            else if (command.Equals("GTC"))
                            {
                                //GTC A
                                if (data[1].Equals("A")) //manual
                                {
                                    handleNewContact = MSNEnumerations.NewContact.manual_add;
                                }
                                else if (data[1].Equals("N")) //auto
                                {
                                    handleNewContact = MSNEnumerations.NewContact.auto_add;
                                }
                            }
                            #endregion
                            #region handle LSG
                            else if (command.Equals("LSG"))
                            {
                                //LSG 0 Individuals 0

                                if (!data[2].Equals("~"))
                                {
                                    String groupName = HttpUtility.UrlDecode(data[2]);
                                    MSNGroup g = new MSNGroup(controller, groupName, int.Parse(data[1]));

                                    if (!groupsByName.ContainsKey(groupName))
                                    {
                                        groupsByName.Add(groupName, g);
                                        groupsByNumber.Add(int.Parse(data[1]), g);
                                    }
                                }
                            }
                            #endregion
                            #region handle LST
                            else if (command.Equals("LST"))
                            {
                                //LST derek_bartram@hotmail.com derek_bartram 11 3 
                                //all contacts send 11!?!

                                String username = data[1];

                                #region get / make contact
                                MSNContact c = getContact(username);

                                lastContact = c;
                                #endregion

                                #region process friendly name
                                c.FriendlyName = HttpUtility.UrlDecode(data[2]);
                                #endregion

                                #region process groups
                                if (data.Length == 5)
                                {
                                    char[] splitChars = new char[1];
                                    splitChars[0] = ',';

                                    String[] groupIDs = data[4].Split(splitChars);

                                    for (int i = 0; i < groupIDs.Length; i++)
                                    {
                                        int groupID = int.Parse(groupIDs[i]);
                                        if (groupsByNumber.ContainsKey(int.Parse(groupIDs[i])))
                                        {
                                            MSNGroup g = groupsByNumber[int.Parse(groupIDs[i])];
                                            c.Groups.Add(g);
                                            g.Contacts.Add(c);
                                        }
                                    }
                                }
                                #endregion
                            }
                            #endregion
                            #region handle BPR
                            else if (command.Equals("BPR"))
                            {
                                //BPR 12182 myname@msn.com PHH 555%20555%204321
                                //BPR 12182 myname@msn.com PHW I%20AM%20DUMB\r\n
                                //BPR 12182 myname@msn.com PHM I%20Dont%20Have%20One\r\n
                                //BPR 12182 myname@msn.com MOB Y\r\n

                                //BPR MOB Y

                                MSNContact c = lastContact;
                                String phoneString = "";
                                if (data.Length == 3)
                                {
                                    phoneString = HttpUtility.UrlDecode(data[2]);
                                }

                                c.setPhone(MSNStaticHelperFunctions.toPhoneTypes(data[1]), phoneString);
                            }
                            #endregion
                            #region handle ILN
                            else if (command.Equals("ILN"))
                            {
                                //ILN 10 NLN webmaster@norbosoft.zzn.com 80%25%20Andy%20::%205.5%20ml%20Marky 1347272748 %3Cmsnobj%20Creator%3D%22webmaster%40norbosoft.zzn.com%22%20Size%3D%2216657%22%20Type%3D%223%22%20Location%3D%22TFR100.dat%22%20Friendly%3D%22UwBpAGQAAAA%3D%22%20SHA1D%3D%22LCJswyrzD1bDGR41HJ3xmfoBbqM%3D%22%20SHA1C%3D%22bYULHZIyQUxMB%2BFM1FNO%2BYAiP6M%3D%22%20contenttype%3D%22M%22%20contentid%3D%22U3097935%22%2F%3E

                                MSNContact c = getContact(data[3]);
                                c.Status = MSNStaticHelperFunctions.toUserStatus(data[2]);
                                if (data.Length >= 5)
                                {
                                    c.FriendlyName = HttpUtility.UrlDecode(data[4]);
                                }
                            }
                            #endregion
                            #region handle NLN
                            else if (command.Equals("NLN"))
                            {
                                //NLN AWY derek_bartram@hotmail.com derek_bartram 1347272812 %3Cmsnobj%20Creator%3D%22derek_bartram%40hotmail.com%22%20Size%3D%2229106%22%20Type%3D%223%22%20Location%3D%22xpp539.dat%22%20Friendly%3D%22AAA%3D%22%20SHA1D%3D%22YeWGuRSbOGhJ1Wbk3JY5vCzoqT8%3D%22%20SHA1C%3D%22nj0278TZP%2FP4yIGc6qxIXrY2olQ%3D%22%2F%3E
                                MSNContact c = getContact(data[2]);
                                c.Status = MSNStaticHelperFunctions.toUserStatus(data[1]);
                                if (data.Length >= 4)
                                {
                                    c.FriendlyName = HttpUtility.UrlDecode(data[3]);
                                }
                            }
                            #endregion

                            else if (command.Equals("FLN"))
                            {
                                MSNContact c = getContact(data[1]);
                                c.Status = MSNStaticHelperFunctions.toUserStatus("FLN");
                            }

                            #region unknown
                            else
                            {
                                throw new Exception("Unknown message type in MSNContactsList.processMessageLoop() <" + message.ToString() + ">");
                            }
                            #endregion
                        }
                        catch (Exception)
                        {
                            Console.Error.WriteLine("Error processing command in MSNContactsList.processMessageLoop()\r\n" + message.ToString());
                        }
                    }
                    else
                    {
                        Thread.Sleep(20);
                    }
                }
            }
            #endregion

            #region accessors
            public Dictionary<String, MSNContact> Contacts
            {
                get
                {
                    return contacts;
                }
            }

            public Dictionary<String, MSNGroup> Groups
            {
                get
                {
                    return groupsByName;
                }
            }

            public MSNEnumerations.NewContact HandleNewContact
            {
                get
                {
                    return handleNewContact;
                }
            }
            #endregion

        }

            #region internal classes
            public class MSNGroup
            {
                #region class variables
                private MSNController controller = null;
                private String name = null;
                private int groupNumber = 0;
                private MSNListenableList<MSNContact> contacts = new MSNListenableList<MSNContact>();
                #endregion

                #region constructors
                public MSNGroup(MSNController controller, String name, int groupNumber)
                {
                    this.controller = controller;
                    this.name = name;
                    this.groupNumber = groupNumber;

                    controller.sendAddRemoveGroupMessage(name, true);

                    contacts.ElementAdded +=new MSNListenableList<MSNContact>.ElementAddedDelegate(contacts_ElementAdded);
                    contacts.ElementRemoved +=new MSNListenableList<MSNContact>.ElementRemovedDelegate(contacts_ElementRemoved);
                }

void  contacts_ElementRemoved(MSNListenableList<MSNContact> sender, MSNListenableList<MSNContact>.ElementRemovedEventArgs<MSNContact> args)
{
 	controller.sendAddRemoveContactFromGroupMessage(null, args.Item.Username, false);
}

private void  contacts_ElementAdded(MSNListenableList<MSNContact> sender, MSNListenableList<MSNContact>.ElementAddedEventArgs<MSNContact> args)
{
                    controller.sendAddRemoveContactFromGroupMessage(name, args.Item.Username, true);
}
                #endregion

                #region accessors
                public String GroupName
                {
                    get
                    {
                    return name;
                    }
                }

                public int GroupNumber
                {
                    get
                    {
                    return groupNumber;
                    }
                }

                public MSNListenableList<MSNContact> Contacts
                {
                    get
                    {
                        return contacts;
                    }
                }
                #endregion
            }

            public class MSNContact
            {
                #region class variables
                private MSNController controller = null;

                private String username = null;
                private String friendlyName = "";
                private MSNListenableList<MSNGroup> groups = new MSNListenableList<MSNGroup>();

                private bool onAllowList = false;
                private bool onBlockList = false;
                private bool onForwardList = false;
                private bool onReverseList = false;

                private String phh = "";
                private String phw = "";
                private String phm = "";
                private String mob = "N";

                private MSNEnumerations.UserStatus status = MSNEnumerations.UserStatus.offline;
                #endregion

                #region constructor
                public MSNContact(MSNController controller, String username)
                {
                    this.controller = controller;
                    this.username = username;
                }
                #endregion

                #region accessors
                public void setListMember(MSNEnumerations.ContactLists list, bool member)
                {
                    if (list.Equals(MSNEnumerations.ContactLists.allow_list))
                    {
                        onAllowList = member;
                    }
                    else if (list.Equals(MSNEnumerations.ContactLists.block_list))
                    {
                        onBlockList = member;
                    }
                    else if (list.Equals(MSNEnumerations.ContactLists.forward_list))
                    {
                        onForwardList = member;
                    }
                    else if (list.Equals(MSNEnumerations.ContactLists.reverse_list))
                    {
                        onReverseList = member;
                    }

                    controller.sendAddRemoveContactMasterListMessage(username, list, true);
                }

                public bool getListMember(MSNEnumerations.ContactLists list)
                {
                    if (list.Equals(MSNEnumerations.ContactLists.allow_list))
                    {
                        return onAllowList;
                    }
                    else if (list.Equals(MSNEnumerations.ContactLists.block_list))
                    {
                        return onBlockList;
                    }
                    else if (list.Equals(MSNEnumerations.ContactLists.forward_list))
                    {
                        return onForwardList;
                    }
                    else if (list.Equals(MSNEnumerations.ContactLists.reverse_list))
                    {
                        return onReverseList;
                    }
                    else
                    {
                        return false;
                    }
                }

                public String FriendlyName
                {
                    get
                    {
                        return friendlyName;
                    }
                    set
                    {
                        friendlyName = value;
                        controller.sendContactFriendlyNameChangeMessage(username, friendlyName);
                    }
                }

                public void setPhone(MSNEnumerations.PhoneTypes phoneType, String value)
                {
                    if (phoneType == MSNEnumerations.PhoneTypes.mob)
                    {
                        mob = value;
                    }
                    else if (phoneType == MSNEnumerations.PhoneTypes.phh)
                    {
                        phh = value;
                    }
                    else if (phoneType == MSNEnumerations.PhoneTypes.phm)
                    {
                        phm = value;
                    }
                    else if (phoneType == MSNEnumerations.PhoneTypes.phw)
                    {
                        phw = value;
                    }
                    else
                    {
                        throw new Exception("Unknown PhoneType in MSNContactsList.Contact.setPhone(" + phoneType + ", " + value + ")");
                    }

                    controller.sendContactPhoneChangeMessage(username, phoneType, value);
                }

                public String getPhone(MSNEnumerations.PhoneTypes phoneType)
                {
                    if (phoneType == MSNEnumerations.PhoneTypes.mob)
                    {
                        return mob;
                    }
                    else if (phoneType == MSNEnumerations.PhoneTypes.phh)
                    {
                        return phh;
                    }
                    else if (phoneType == MSNEnumerations.PhoneTypes.phm)
                    {
                        return phm;
                    }
                    else if (phoneType == MSNEnumerations.PhoneTypes.phw)
                    {
                        return phw;
                    }
                    else
                    {
                        throw new Exception("Unknown PhoneType in MSNContactsList.Contact.getPhone(" + phoneType + ")");
                    }
                }

                public String Username
                {
                    get
                    {
                        return username;
                    }
                }

                public MSNListenableList<MSNGroup> Groups
                {
                    get
                    {
                        return groups;
                    }
                }

                public MSNEnumerations.UserStatus Status
                {
                    get
                    {
                        return status;
                    }
                    set
                    {
                        status = value;
                        controller.sendContactStatusChangeMessage(username, status);
                    }
                }
                #endregion
            }
            #endregion
    
}
