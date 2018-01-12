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
    internal sealed class MSNMessage
    {
        private String message;

        internal MSNMessage(String message, int trid, String[] data, bool endNewLine)
        {
            this.message = message + " " + trid;

            for (int i = 0; i < data.Length; i++)
            {
                this.message += " " + data[i].ToString();
            }

            if (endNewLine)
            {
                this.message += "\r\n";
            }
        }

        internal MSNMessage(String message)
        {
            this.message = message;
        }

        internal String getWholeMessage()
        {
            return message;
        }

        internal String getCommand()
        {
            try
            {
                char[] tokenSplit = new char[1];
                tokenSplit[0] = ' ';

                String[] tokens = message.Split(tokenSplit);

                if (tokens.Length > 0)
                {
                    return tokens[0];
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                throw new Exception("Malformed message : Cannot get command (" + message + ")");
            }
        }

        internal int getTrID()
        {
            char[] tokenSplit = new char[1];
            tokenSplit[0] = ' ';

            String[] tokens = message.Split(tokenSplit);

            if (tokens.Length > 1)
            {
                try
                {
                    return int.Parse(tokens[1].ToString());
                }
                catch (Exception)
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }

        internal String[] getData()
        {
            char[] tokenSplit = new char[1];
            tokenSplit[0] = ' ';

            String[] tokens = message.Split(tokenSplit);

            try
            {
                if (tokens.Length >= 2)
                {
                    String data = message.Substring(tokens[0].ToString().Length + 1 + tokens[1].ToString().Length + 1);
                    return data.Split(tokenSplit);
                }
                else
                {
                    return new String[0];
                }
            }
            catch (Exception)
            {
                return new String[0];
            }
        }

        internal String[] getTokens()
        {
            char[] tokenSplit = new char[1];
            tokenSplit[0] = ' ';

            return message.Split(tokenSplit);
        }

        public override string ToString()
        {
            return message;
        }
    }
}
