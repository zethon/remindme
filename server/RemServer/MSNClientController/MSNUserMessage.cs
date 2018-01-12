using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public abstract class MSNUserMessage
    {
        protected MSNEnumerations.UserMessageType messageType;
        protected String userPayload;
        protected String font;
        protected String username;
        protected int id;
        protected bool send;
        protected bool display;
        private bool processByPlugins = true;

        public MSNUserMessage()
        {

        }


        public bool getSend()
        {
            return send;
        }

        public bool getDisplay()
        {
            return display;
        }

        public void setSend(bool set)
        {
            send = set;
        }

        public void setDisplay(bool set)
        {
            display = set;
        }

        public String getUserPayload()
        {
            return userPayload;
        }

        public void setUserPayload(String payload)
        {
            userPayload = payload;
        }

        public override string ToString()
        {
            if (messageType.Equals(MSNEnumerations.UserMessageType.outgoing_text_message))
            {
                //MSG 4 N 133\r\n
                //MIME-Version: 1.0\r\n
                //Content-Type: text/plain; charset=UTF-8\r\n
                //X-MMS-IM-Format: FN=Arial; EF=I; CO=0; CS=0; PF=22\r\n
                //\r\n
                //Hello! How are you? 

                String output = "MSG " + id + " N ";
                String payload = "MIME-Version: 1.0\r\nContent-Type: text/plain; charset=UTF-8\r\nX-MMS-IM-Format: FN=" + font + "; EF=I; CO=0; CS=0; PF=22\r\n\r\n";
                payload += userPayload;
                output += payload.Length + "\r\n" + payload;

                return output;
            }
            else if (messageType.Equals(MSNEnumerations.UserMessageType.outgoing_typing_message))
            {
                //MSG 4 U 91\r\n
                //MIME-Version: 1.0\r\n
                //Content-Type: text/x-msmsgscontrol\r\n
                //TypingUser: alice@passport.com\r\n

                String output = "MSG " + id + " U ";
                String payload = "MIME-Version: 1.0\r\nContent-Type: text/x-msmsgscontrol\r\nTypingUser: " + username + "\r\n";
                output += payload.Length + "\r\n" + payload;
                return output;
            }
            else if (messageType.Equals(MSNEnumerations.UserMessageType.outgoing_emoticon_message))
            {
                //MSG derek_bartram@hotmail.com derek_bartram 238
                //MIME-Version: 1.0
                //Content-Type: text/x-mms-emoticon

                //(log)	<msnobj Creator="derek_bartram@hotmail.com" Size="602" Type="2" Location="iin6.dat" Friendly="AAA=" SHA1D="QTy7engLwhy9Iz6gHhC993l8Rhg=" SHA1C="ST5jW80e8bGERWmQvVRDQ27Y9lw="/>	

                String output = "MSG " + username + " eggs ";
                String payload = "MIME-Version: 1.0\r\nContent-Type: text/x-mms-emoticon\r\n\r\n" + userPayload + "";
                output += payload.Length + "\r\n" + payload;
                return output;
            }
            else if (messageType.Equals(MSNEnumerations.UserMessageType.incomming_emoticon_message))
            {
                return "NOT YET IMPLEMENTED";
            }
            else
            {
                return "UNKNOWN MESSAGE TYPE";
            }
        }

        public MSNEnumerations.UserMessageType getMessageType()
        {
            return messageType;
        }

        public String getUsername()
        {
            return username;
        }

        public void setUsername(String username)
        {
            this.username = username;
        }

        public bool ProcessByPlugins
        {
            get
            {
                return processByPlugins;
            }
            set
            {
                processByPlugins = value;
            }
        }
    }
}
