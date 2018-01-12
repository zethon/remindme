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
    public abstract class MSNEnumerations
    {
        public enum LoginStatus
        {
            LOGGING_IN, LOGGED_OUT, LOGGED_IN
        }

        public enum UserStatus
        {
            online, offline, busy, out_to_lunch, on_the_phone, be_right_back, away, unknown
        }

        public enum NewContact
        {
            auto_add, manual_add
        }

        public enum UnknownContact
        {
            allow_chat, disallow_chat
        }

        public enum ContactLists
        {
            forward_list, reverse_list, allow_list, block_list
        }

        public enum PhoneTypes
        {
            phh, phw, phm, mob
        }

        public enum UserMessageType
        {
            incomming_text_message, outgoing_text_message, incomming_typing_message, outgoing_typing_message, dummy_message, incomming_emoticon_message, outgoing_emoticon_message
        }
    }
}
