using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

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
    public sealed class MSNSocketWrapper
    {
        private Socket socket;
        private StreamReader reader;
        private StreamWriter writer;
        private bool isConnected;

        #region constructors
        internal MSNSocketWrapper(Socket socket)
        {
            this.socket = socket;
            NetworkStream ns = new NetworkStream(socket);
            reader = new StreamReader(ns);
            writer = new StreamWriter(ns);
            writer.AutoFlush = true;
            isConnected = socket.Connected;
        }

        internal MSNSocketWrapper(String addressString, int port, bool useSSL)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            IPEndPoint ep = null;

            try
            {
                ep = new IPEndPoint(IPAddress.Parse(addressString), port);
            }
            catch (Exception)
            {
                IPAddress[] addresses = null;

                #region try and resolve addresses
                try
                {
                    addresses = Dns.GetHostAddresses(addressString);
                }
                catch (Exception)
                {
                    try
                    {
                        addresses = Dns.GetHostAddresses(addressString);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            addresses = Dns.GetHostAddresses(addressString);
                        }
                        catch (Exception)
                        {
                            throw new Exception("Could not resolve host (" + addressString + ") in SocketWrapper(" + addressString + ", " + port + ", " + useSSL.ToString() + ")");
                        }
                    }
                }
                #endregion

                IPAddress address = addresses[0];

                ep = new IPEndPoint(address, port);
            }

            if (ep == null)
            {
                throw new Exception("Unable to resove address in SocketWrapper(" + addressString + ", " + port + ")");
            }

            #region connect socket (3 attempts)
            try
            {
                socket.Connect(ep);
            }
            catch (Exception)
            {
                Console.WriteLine("Error connecting socket (attempt 1)");

                try
                {
                    socket.Connect(ep);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error connecting socket (attempt 2)");

                    try
                    {
                        socket.Connect(ep);
                    }
                    catch (Exception err3)
                    {
                        Console.WriteLine("Error connecting socket (attempt 3)\r\n" + err3.ToString());

                        throw new Exception("Error establishing socket connection");
                    }
                }
            }
            #endregion

            isConnected = socket.Connected;
            if (socket.Connected != true)
            {
                throw new Exception("Unable to connect to server in SocketWrapper(" + addressString + ", " + port + ", " + useSSL + ")");
            }

            NetworkStream ns = new NetworkStream(socket);
            if (useSSL)
            {
                try
                {
                    SslStream ssls = new SslStream(ns, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                    ssls.AuthenticateAsClient(addressString);

                    reader = new StreamReader(ssls);
                    writer = new StreamWriter(ssls);
                }
                catch (Exception)
                {
                    throw new Exception("Unable to establish SSL in SocketWrapper" + addressString + ", " + port + ", " + useSSL + ")");
                }
            }
            else
            {
                reader = new StreamReader(ns);
                writer = new StreamWriter(ns);

            }
            writer.AutoFlush = true;
        }
        #endregion

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        internal static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        internal bool connected()
        {
            if (socket.Connected == false || isConnected == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal void send(MSNMessage message)
        {
            Console.WriteLine(">>> " + message.ToString());
            writer.Write(message.ToString());
        }

        public void SendRawMessage(string strData)
        {
            send(new MSNMessage(strData));
        }

        internal MSNMessage recieve()
        {
            try
            {
                String message = reader.ReadLine();

                if (message == null)
                {
                    isConnected = false;
                    return null;
                }

                MSNMessage inMessage = new MSNMessage(message);
                Console.WriteLine("<<< " + inMessage.ToString());
                return inMessage;
            }
            catch (Exception)
            {
                Console.WriteLine("Error recieving message in MSNSocketWrapper.recieve()");
                isConnected = false;
                return null;
            }
        }

        internal MSNMessage recieve(int length)
        {
            try
            {
                String message = "";
                char[] buffer = new char[length];
                message += reader.Read(buffer, 0, length);

                for (int i = 0; i < length; i++)
                {
                    message += buffer[i].ToString();
                }

                if (message == null)
                {
                    isConnected = false;
                    return null;
                }

                MSNMessage inMessage = new MSNMessage(message);
                Console.WriteLine("<<< " + inMessage.ToString());
                return inMessage;
            }
            catch (Exception)
            {
                Console.WriteLine("Error recieving message in MSNSocketWrapper.recieve()");
                isConnected = false;
                return null;
            }
        }

        internal void close()
        {
            try
            {
                writer.Flush();
                writer.Close();
                reader.Close();
                socket.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
