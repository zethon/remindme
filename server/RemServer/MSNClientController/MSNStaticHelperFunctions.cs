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
    internal sealed class MSNStaticHelperFunctions
    {
        internal static MSNEnumerations.UserStatus toUserStatus(String msnStatusCode)
        {
            //NLN - Available 
            //BSY - Busy 
            //IDL - Idle 
            //BRB - Be Right Back 
            //AWY - Away 
            //PHN - On the Phone 
            //LUN - Out to Lunch 

            if (msnStatusCode.Equals("NLN"))
            {
                return MSNEnumerations.UserStatus.online;
            }
            else if (msnStatusCode.Equals("BSY"))
            {
                return MSNEnumerations.UserStatus.busy;
            }
            else if (msnStatusCode.Equals("IDL"))
            {
                return MSNEnumerations.UserStatus.away;
            }
            else if (msnStatusCode.Equals("BRB"))
            {
                return MSNEnumerations.UserStatus.be_right_back;
            }
            else if (msnStatusCode.Equals("AWY"))
            {
                return MSNEnumerations.UserStatus.away;
            }
            else if (msnStatusCode.Equals("PHN"))
            {
                return MSNEnumerations.UserStatus.on_the_phone;
            }
            else if (msnStatusCode.Equals("LUN"))
            {
                return MSNEnumerations.UserStatus.out_to_lunch;
            }
            else if (msnStatusCode.Equals("FLN"))
            {
                return MSNEnumerations.UserStatus.offline;
            }
            else if (msnStatusCode.Equals("HDN"))
            {
                return MSNEnumerations.UserStatus.offline;
            }
            else
            {
                throw new Exception("Unknown status code in MSNLocalClient.processMessage");
            }
        }

        internal static String fromUserStatus(MSNEnumerations.UserStatus userStatusCode)
        {
            //NLN - Available 
            //BSY - Busy 
            //IDL - Idle 
            //BRB - Be Right Back 
            //AWY - Away 
            //PHN - On the Phone 
            //LUN - Out to Lunch 

            if (userStatusCode == MSNEnumerations.UserStatus.online)
            {
                return "NLN";
            }
            else if (userStatusCode == MSNEnumerations.UserStatus.busy)
            {
                return "BSY";
            }
            else if (userStatusCode == MSNEnumerations.UserStatus.offline)
            {
                return "HDN";
            }
            else if (userStatusCode == MSNEnumerations.UserStatus.be_right_back)
            {
                return "BRB";
            }
            else if (userStatusCode == MSNEnumerations.UserStatus.away)
            {
                return "AWY";
            }
            else if (userStatusCode == MSNEnumerations.UserStatus.on_the_phone)
            {
                return "PHN";
            }
            else if (userStatusCode == MSNEnumerations.UserStatus.out_to_lunch)
            {
                return "LUN";
            }
            else
            {
                throw new Exception("Unknown status code in MSNLocalClient.processMessage");
            }
        }

        internal static MSNEnumerations.ContactLists toContactLists(String contactListCode)
        {
            if (contactListCode.Equals("FL"))
            {
                return MSNEnumerations.ContactLists.forward_list;
            }
            else if (contactListCode.Equals("RL"))
            {
                return MSNEnumerations.ContactLists.reverse_list;
            }
            else if (contactListCode.Equals("AL"))
            {
                return MSNEnumerations.ContactLists.allow_list;
            }
            else if (contactListCode.Equals("BL"))
            {
                return MSNEnumerations.ContactLists.block_list;
            }
            else
            {
                throw new Exception("Unknown contactListCode in StaticHelperFunctions.toContactLists(" + contactListCode + ")");
            }
        }

        internal static MSNEnumerations.PhoneTypes toPhoneTypes(String phoneTypeCode)
        {
            if (phoneTypeCode.Equals("MOB"))
            {
                return MSNEnumerations.PhoneTypes.mob;
            }
            else if (phoneTypeCode.Equals("PHH"))
            {
                return MSNEnumerations.PhoneTypes.phh;
            }
            else if (phoneTypeCode.Equals("PHM"))
            {
                return MSNEnumerations.PhoneTypes.phm;
            }
            else if (phoneTypeCode.Equals("PHW"))
            {
                return MSNEnumerations.PhoneTypes.phw;
            }
            else
            {
                throw new Exception("Unknown phoneTypeCode in StaticHelperFunctions.toPhoneTypes(" + phoneTypeCode + ")");
            }
        }
    }
}
