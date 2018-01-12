using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;

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
    internal sealed class MSNAuthentication
    {
        #region class variables
        private String username = "";
        private String password = "";
        private String initialFriendlyName = "";

        private MSNController controller = null;
        #endregion

        #region constructors
        internal MSNAuthentication(MSNController controller, String username, String password)
        {
            this.username = username;
            this.password = password;

            this.controller = controller;
        }
        #endregion

        #region notification server authentication
        internal MSNSocketWrapper doAuthentication()
        {
            try
            {
                return doAuthentication("messenger.hotmail.com", 1863);
            }
            catch (Exception)
            {
                Console.WriteLine("Error during authentication in MSNAuthentication.doAuthentication()");
                throw new Exception("Error during authentication in MSNAuthentication.doAuthentication()");
            }
        }

        internal MSNSocketWrapper doAuthentication(String address, int port)
        {
            //messenger.hotmail.com:1863
            MSNSocketWrapper sw = new MSNSocketWrapper(address, port, false);

            if (sw.connected() != true)
            {
                throw new Exception("Not connecting to messenger.hotmail.com");
            }

            #region negotiate protocol version (VER)
            #region VER 1 MSNP8 CVR0\r\n
            MSNMessage outMessage = new MSNMessage("VER " + MSNTrIDGenerator.NextID() + " MSNP9 MSNP8 CVR0\r\n");
            sw.send(outMessage);
            #endregion

            #region<<< VER 1 MSNP8 CVR0\r\n
            MSNMessage inMessage = sw.recieve();

            if (inMessage.getCommand().Equals("VER") != true)
            {
                throw new Exception("VER Expected (" + inMessage.ToString() + ")");
            }

            if (inMessage.getTrID() != outMessage.getTrID())
            {
                throw new Exception("Incorrect TrID reponce in VER (" + inMessage.ToString() + ")");
            }

            String[] messageDataTokens = inMessage.getData();

            if (messageDataTokens.Length == 0)
            {
                throw new Exception("No data recieved in VER (" + inMessage.ToString() + ")");
            }

            if (messageDataTokens[0].ToString().Equals("0"))
            {
                throw new Exception("Protocol MSNP9 MSNP8 CVR0 not supported in VER (" + inMessage.ToString() + ")");
            }
            #endregion
            #endregion

            #region negotiate client version (CVR)
            #region CVR 2 0x0409 win 4.10 i386 MSNMSGR 5.0.0544 MSMSGS example@passport.com\r\n
            outMessage = new MSNMessage("CVR " + MSNTrIDGenerator.NextID() + " 0x0409 win 4.10 i386 MSNMSGR 7.0.0816 MSMSGS " + username + "\r\n");
            sw.send(outMessage);
            #endregion

            #region CVR 2 7.0.0816 7.0.0816 1.0.0000 http://download.microsoft.com/download/8/a/4/8a42bcae-f533-4468-b871-d2bc8dd32e9e/SETUP9x.EXE http://messenger.msn.com\r\n
            inMessage = sw.recieve();

            if (inMessage.getCommand().Equals("CVR") != true)
            {
                throw new Exception("CVR Expected (" + inMessage.ToString() + ")");
            }

            if (inMessage.getTrID() != outMessage.getTrID())
            {
                throw new Exception("Incorrect TrID reponce in CVR (" + inMessage.ToString() + ")");
            }

            messageDataTokens = inMessage.getData();

            if (messageDataTokens.Length == 0)
            {
                throw new Exception("No data recieved in CVR (" + inMessage.ToString() + ")");
            }
            #endregion
            #endregion

            #region get session tpf -> passport session ticket (USR)
            #region USR 3 TWN I alice@passport.com\r\n
            outMessage = new MSNMessage("USR " + MSNTrIDGenerator.NextID() + " TWN I " + username + "\r\n");
            sw.send(outMessage);
            #endregion

            #region get responce
            inMessage = sw.recieve();

            if (!(inMessage.getCommand().Equals("XFR") == true || inMessage.getCommand().Equals("USR") == true))
            {
                throw new Exception("USR/XFR Expected (" + inMessage.ToString() + ")");
            }

            //HANDLE INITIAL USR
            if (inMessage.getCommand().Equals("USR"))
            {
                String[] data = inMessage.getData();
                if (data.Length >= 6)
                {
                    initialFriendlyName = HttpUtility.UrlDecode(data[4]);
                }
            }

            if (inMessage.getTrID() != outMessage.getTrID())
            {
                throw new Exception("Incorrect TrID reponce in CVR (" + inMessage.ToString() + ")");
            }
            #endregion

            #region XFR 3 NS 207.46.106.118:1863 0 207.46.104.20:1863\r\n
            if (inMessage.getCommand().Equals("XFR"))
            {
                messageDataTokens = inMessage.getData();

                if (messageDataTokens.Length != 4)
                {
                    throw new Exception("Incorrect data length in XFR (" + inMessage.ToString() + ")");
                }

                try
                {
                    char[] splitTokens = new char[1];
                    splitTokens[0] = ':';
                    String[] addressDataTokens = messageDataTokens[1].Split(splitTokens);
                    String newAddress = addressDataTokens[0];
                    int newPort = int.Parse(addressDataTokens[1]);

                    return doAuthentication(newAddress, newPort);
                }
                catch (FormatException)
                {
                    throw new Exception("Format error in XFR (" + inMessage.ToString() + ")");
                }
            }
            #endregion

            String sessionTicket = "";

            //First USR
            #region USR 3 TWN S lc=1033,id=507,tw=40,fs=1,ru=http%3A%2F%2Fmessenger%2Emsn%2Ecom,ct=1062764229,kpp=1,kv=5,ver=2.1.0173.1,tpf=43f8a4c8ed940c04e3740be46c4d1619\r\n
            if (inMessage.getCommand().Equals("USR"))
            {
                String messageStr = inMessage.ToString();
                messageStr = messageStr.Replace("\r\n", "");

                char[] splitTokens = new char[1];
                splitTokens[0] = '=';

                messageDataTokens = messageStr.Split(splitTokens);
                String tpf = messageDataTokens[messageDataTokens.Length - 1].ToString();
                String ver = messageDataTokens[messageDataTokens.Length - 3].ToString().Replace(",rn", "");
                sessionTicket = getPassportTicket(tpf, ver);
            }
            #endregion
            #endregion

            #region final authentication (USR)
            #region USR 4 TWN S t=53*1hAu8ADuD3TEwdXoOMi08sD*2!cMrntTwVMTjoB3p6stWTqzbkKZPVQzA5NOt19SLI60PY!b8K4YhC!Ooo5ug$$&p=5eKBBC!yBH6ex5mftp!a9DrSb0B3hU8aqAWpaPn07iCGBw5akemiWSd7t2ot!okPvIR!Wqk!MKvi1IMpxfhkao9wpxlMWYAZ!DqRfACmyQGG112Bp9xrk04!BVBUa9*H9mJLoWw39m63YQRE1yHnYNv08nyz43D3OnMcaCoeSaEHVM7LpR*LWDme29qq2X3j8N\r\n
            outMessage = new MSNMessage("USR " + MSNTrIDGenerator.NextID() + " TWN S " + sessionTicket + "\r\n");
            sw.send(outMessage);
            #endregion

            #region USR 4 OK example@passport.com example%20display%20name 1 0\r\n
            inMessage = sw.recieve();

            if (inMessage.getCommand().Equals("USR"))
            {
                String[] data = inMessage.getData();
                if (data.Length >= 3)
                {
                    initialFriendlyName = HttpUtility.UrlDecode(data[2]);
                }
            }

            if (!inMessage.getCommand().Equals("USR"))
            {
                throw new Exception("Unexpected command responce for USR in doAuthentication(" + address + ", " + port + ")");
            }

            if (inMessage.getTrID() != outMessage.getTrID())
            {
                throw new Exception("Unexpected TrID responce for USR in doAuthentication(" + address + ", " + port + ")");
            }

            messageDataTokens = inMessage.getData();

            if (messageDataTokens.Length != 5)
            {
                throw new Exception("Unexpected length data responce for USR in doAuthentication(" + address + ", " + port + ")");
            }

            if (!messageDataTokens[0].Equals("OK"))
            {
                throw new Exception("Unknown failure to authentication for USR in doAuthentication(" + address + ", " + port + ")");
            }

            if (!messageDataTokens[1].Equals(username))
            {
                throw new Exception("Authenticated as incorrect user for USR in doAuthentication(" + address + ", " + port + ")");
            }

            //#############################################################################################################################
            // AUTHENTICATION PASSED // AUTHENTICATION PASSED // AUTHENTICATION PASSED // AUTHENTICATION PASSED // AUTHENTICATION PASSED //
            //#############################################################################################################################
            return sw;
            #endregion
            #endregion
        }
        #endregion

        #region passport authentication
        private String getPassportTicket(String ticketString, String version)
        {
            MSNSocketWrapper sw = new MSNSocketWrapper("nexus.passport.com", 443, true);

            #region GET /rdr/pprdr.asp HTTP/1.0\r\n<FLUSH HERE>\r\n
            MSNMessage outMessage = new MSNMessage("GET /rdr/pprdr.asp HTTP/1.0\r\n\r\n");
            sw.send(outMessage);
            sw.send(new MSNMessage("\r\n"));
            sw.send(new MSNMessage("\r\n"));

            //outMessage = new Message("\r\n");
            //sw.send(outMessage);
            #endregion

            #region process responce
            MSNMessage inMessage = new MSNMessage("SPOONZ");
            while (!inMessage.ToString().Trim().Equals(""))
            {
                inMessage = sw.recieve();

                //<<< HTTP/1.1 200 OK\r\n
                //<<< Server: Microsoft-IIS/5.0\r\n
                //<<< Date: Mon, 02 Jun 2003 11:57:47 GMT\r\n
                //<<< Connection: close\r\n
                //<<< PassportURLs: DARealm=Passport.Net,DALogin=login.passport.com/login2.srf,DAReg=http://register.passport.net/uixpwiz.srf,Properties=https://register.passport.net/editprof.srf,Privacy=http://www.passport.com/consumer/privacypolicy.asp,GeneralRedir=http://nexusrdr.passport.com/redir.asp,Help=http://memberservices.passport.net/memberservice.srf,ConfigVersion=11\r\n
                //<<< Content-Length: 0\r\n
                //<<< Content-Type: text/html\r\n
                //<<< Cache-control: private\r\n
                //<<< \r\n 

                if (inMessage.ToString().StartsWith("PassportURLs:"))
                {
                    char[] splitTokens = new char[3];
                    splitTokens[0] = ' ';
                    splitTokens[1] = ',';
                    splitTokens[2] = '=';

                    String[] messageDataTokens = inMessage.ToString().Split(splitTokens);
                    int daLoginIndex = 0;

                    while (!messageDataTokens[daLoginIndex].Equals("DALogin") && daLoginIndex < messageDataTokens.Length)
                    {
                        daLoginIndex++;
                    }

                    if (daLoginIndex >= messageDataTokens.Length - 1)
                    {
                        throw new Exception("No DALogin found in PassportURLs from passport nexus");
                    }

                    daLoginIndex++;

                    return getPassportTicket(ticketString, version, messageDataTokens[daLoginIndex].ToString());
                }
            }
            throw new Exception("No redirect given in getPassportTicket(" + ticketString + ")");
            #endregion
        }

        private String getPassportTicket(String ticketString, String version, String address)
        {
            char[] splitTokens = new char[2];
            splitTokens[0] = '\\';
            splitTokens[1] = '/';

            #region process address
            String[] addressTokens = address.Split(splitTokens);
            String pageAddress = "";

            for (int i = 1; i < addressTokens.Length; i++)
            {
                pageAddress += "/" + addressTokens[i];
            }
            #endregion

            MSNSocketWrapper sw = new MSNSocketWrapper(addressTokens[0], 443, true);

            #region GET /login2.srf HTTP/1.1\r\n
            //      Authorization: Passport1.4 OrgVerb=GET,OrgURL=http%3A%2F%2Fmessenger%2Emsn%2Ecom,sign-in=example%40passport.com,pwd=password,lc=1033,id=507,tw=40,fs=1,ru=http%3A%2F%2Fmessenger%2Emsn%2Ecom,ct=1062764229,kpp=1,kv=5,ver=2.1.0173.1,tpf=43f8a4c8ed940c04e3740be46c4d1619\r\n
            //      Host: login.passport.com\r\n\r\n
            MSNMessage outMessage = new MSNMessage("GET " + pageAddress + " HTTP/1.1\r\n");
            sw.send(outMessage);

            //outMessage = new Message("Authorization: Passport1.4 OrgVerb=GET,OrgURL=http%3A%2F%2Fmessenger%2Emsn%2Ecom,sign-in=" + HttpUtility.UrlEncode(username) + ",pwd=" + password + ",lc=1033,id=507,tw=40,fs=1,ru=http%3A%2F%2Fmessenger%2Emsn%2Ecom,ct=1062764229,kpp=1,kv=5,ver=2.1.0173.1,tpf=" + ticketString + "\r\n");
            //outMessage = new Message("Authorization: Passport1.4 OrgVerb=GET,OrgURL=http%3A%2F%2Fmessenger%2Emsn%2Ecom,sign-in=" + HttpUtility.UrlEncode(username) + ",pwd=" + HttpUtility.UrlEncode(password) + ",lc=1033,id=507,tw=40,fs=1,ru=http%3A%2F%2Fmessenger%2Emsn%2Ecom,ct=1062764229,kpp=1,kv=5,ver=" + version + ",tpf=" + ticketString + "\r\n");
            outMessage = new MSNMessage("Authorization: Passport1.4 OrgVerb=GET,OrgURL=http%3A%2F%2Fmessenger%2Emsn%2Ecom,sign-in=" + HttpUtility.UrlEncode(username) + ",pwd=" + HttpUtility.UrlEncode(password) + ",lc=1033,id=507,tw=40,fs=1,ver=" + version + ",tpf=" + ticketString + "\r\n");
            sw.send(outMessage);

            outMessage = new MSNMessage("Host: login.passport.com\r\n\r\n");
            sw.send(outMessage);
            #endregion

            #region process responce
            MSNMessage inMessage = sw.recieve();

            if (inMessage.ToString().Equals("HTTP/1.1 302 Found\r\n") == false && inMessage.ToString().Equals("HTTP/1.1 200 OK\r\n") == false)
            {
                //throw new Exception("Incorrect username / password in getPassportTicket(" + ticketString + ", " + address + ")");
            }

            while (inMessage.ToString().Equals("\r\n") != true)
            {
                inMessage = sw.recieve();

                #region successful authentication
                if (inMessage.ToString().StartsWith("Authentication-Info:") && inMessage.ToString().Contains("da-status=success"))
                {
                    //Authentication-Info: Passport1.4 da-status=success,tname=MSPAuth,tname=MSPProf,tname=MSPSec,from-PP='t=53*1hAu8ADuD3TEwdXoOMi08sD*2!cMrntTwVMTjoB3p6stWTqzbkKZPVQzA5NOt19SLI60PY!b8K4YhC!Ooo5ug$$&p=5eKBBC!yBH6ex5mftp!a9DrSb0B3hU8aqAWpaPn07iCGBw5akemiWSd7t2ot!okPvIR!Wqk!MKvi1IMpxfhkao9wpxlMWYAZ!DqRfACmyQGG112Bp9xrk04!BVBUa9*H9mJLoWw39m63YQRE1yHnYNv08nyz43D3OnMcaCoeSaEHVM7LpR*LWDme29qq2X3j8N',ru=http://messenger.msn.com\r\n

                    splitTokens = new char[1];
                    splitTokens[0] = '\'';

                    String[] messageDataTokens = inMessage.ToString().Split(splitTokens);

                    for (int i = 0; i < messageDataTokens.Length; i++)
                    {
                        if (messageDataTokens[i].StartsWith("t="))
                        {
                            return messageDataTokens[i];
                        }
                    }

                    throw new Exception("Unknown authentication ticket in getPassportTicket(" + ticketString + ", " + address + ")");
                }
                #endregion

                #region redirection
                if (inMessage.ToString().StartsWith("Location:"))
                {
                    //Location: https://loginnet.passport.com/login2.srf?lc=1033\r\n
                    return getPassportTicket(ticketString, version, inMessage.ToString().Replace("Location: ", "").Replace("\r\n", ""));
                }
                #endregion
            }
            #endregion

            throw new Exception("No ticket retrieved from getPassportTicket(" + ticketString + ", " + address);
        }
        #endregion

        #region handle CHL
        internal void handleChallenge(MSNMessage message)
        {
            //<<< CHL 0 15570131571988941333\r\n
            //>>> QRY 1049 msmsgs@msnmsgr.com 32\r\n8f2f5a91b72102cd28355e9fc9000d6e (no newline)
            //<<< QRY 1049\r\n

            String md = HashMD5(message.getData()[0] + "Q1P7W2E4J9R8U3S5");
            controller.sendMessage(new MSNMessage("QRY " + MSNTrIDGenerator.NextID() + " msmsgs@msnmsgr.com 32\r\n" + md));
        }

        internal string HashMD5(string input)
        {
            byte[] result = ((HashAlgorithm)System.Security.Cryptography.CryptoConfig.CreateFromName("MD5")).ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            StringBuilder output = new StringBuilder(16);

            for (int i = 0; i < result.Length; i++)
            {
                // convert from hexa-decimal to character
                output.Append((result[i]).ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
            }
            return output.ToString();
        }
        #endregion

        #region accessors
        internal String Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
            }
        }

        internal String Password
        {
            get
            {
                String output = "";

                for (int i = 0; i < password.Length; i++)
                {
                    output += "*";
                }

                return output;
            }
            set
            {
                password = value;
            }
        }

        internal String InitialFriendlyName
        {
            get
            {
                return initialFriendlyName;
            }
        }
        #endregion
    }
}
