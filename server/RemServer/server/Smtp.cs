/*
 * Copyright 2003-2004, Axosoft, LLC
 * You are free to distribute this code royalty-free as you wish, however, we only ask
 * that you leave this copyright notice at the top of the source file giving Axosoft
 * credit for this source code.  If you find any bugs or want to contribute your
 * enhancements, please send them to support@axosoft.com.
 *
 * Visit Axosoft at http://www.axosoft.com
 */

 

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
 
namespace Axosoft.Common.Utilities
{
	public class SmtpException : ApplicationException
	{
		public SmtpException(string message) : base(message)
		{
		}
	}

 
	///
	/// Indicates the type of message to be sent
	///

	public enum MessageType
	{
		///
		/// The message is plain text
		///

		Text = 0,

		///

		/// The message is HTML

		///

		HTML = 1

	}

 

	///

	/// A mail message that can be sent using the Smtp class

	///

	public class MailMessage
	{
		private string _emailFrom = "";
		public string EmailFrom
		{
			get { return _emailFrom; }
			set { _emailFrom = value; }
		}

		private string _emailSubject = "";
		public string EmailSubject
		{
			get { return _emailSubject; }
			set { _emailSubject = value; }

		}

 

		private ArrayList _emailTo = null;

		public ArrayList EmailTo

		{

			get { return _emailTo; }

		}

		public void AddEmailTo(string email)

		{

			if(_emailTo == null)

				_emailTo = new ArrayList();

			_emailTo.Add(email);

		}

 

		private string _emailMessage = "";

		public string EmailMessage

		{

			get { return _emailMessage; }

			set { _emailMessage = value; }

		}

 

		private MessageType _emailMessageType = MessageType.Text;

		public MessageType EmailMessageType

		{

			get { return _emailMessageType; }

			set { _emailMessageType = value; }

		}

	}

 

	///

	/// This class allows sending of e-mails through Smtp

	/// For help on SMTP, look up http://www.faqs.org/rfcs/rfc821.html

	///

	public class Smtp

	{

		#region Class properties

		private string _serverSmtp = "";

		public string SmtpServer

		{

			get { return _serverSmtp; }

			set { _serverSmtp = value; }

		}

 

		private int _portSmtp = 25;

		public int SmtpPort

		{

			get { return _portSmtp; }

			set { _portSmtp = value; }

		}

 

		private string _userSmtp = "";

		public string SmtpUser

		{

			get { return _userSmtp; }

			set { _userSmtp = value; }

		}

 

		private string _passwordSmtp = "";

		public string SmtpPassword

		{

			get { return _passwordSmtp; }

			set { _passwordSmtp = value; }

		}

 

		#endregion

 

		public Smtp()

		{

		}

 

		#region Public methods

		///

		/// Sends the e-mail based on the properties set for this object

		///

		public void SendEmail(MailMessage msg)

		{

			int code;

 

			if(_serverSmtp == "" || msg.EmailFrom == "" || msg.EmailSubject == "" || msg.EmailTo == null)

			{

				throw new SmtpException("Invalid Smtp or email parameters.");

			}

 

			// open a connection to the Smtp server

			using(TcpClient smtpSocket = new TcpClient(_serverSmtp, _portSmtp))

			using(NetworkStream ns = smtpSocket.GetStream())

			{

				// get response from Smtp server

				code = GetSmtpResponse(ReadBuffer(ns));

				if(code != 220)

				{

					throw new SmtpException("Error connecting to Smtp server. (" + code.ToString() + ")");

				}

 

				// EHLO

				WriteBuffer(ns, "ehlo\r\n");

				// get response from Smtp server

				string buffer = ReadBuffer(ns);

				code = GetSmtpResponse(buffer);

				if(code != 250)

				{

					throw new SmtpException("Error initiating communication with Smtp server. (" + code.ToString() + ")");

				}

				// check for AUTH=LOGIN

				if(buffer.IndexOf("AUTH=LOGIN") >= 0)

				{

					// AUTH LOGIN

					WriteBuffer(ns, "auth login\r\n");

					// get response from Smtp server

					code = GetSmtpResponse(ReadBuffer(ns));

					if(code != 334)

					{

						//throw new SmtpException("Error initiating Auth=Login. (" + code.ToString() + ")");

					}

 

					// username:

					WriteBuffer(ns, System.Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(_userSmtp)) + "\r\n");

					// get response from Smtp server

					code = GetSmtpResponse(ReadBuffer(ns));

					if(code != 334)

					{

						//throw new SmtpException("Error setting Auth user name. (" + code.ToString() + ")");

					}

 

					// password:

					WriteBuffer(ns, System.Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(_passwordSmtp)) + "\r\n");

					// get response from Smtp server

					code = GetSmtpResponse(ReadBuffer(ns));

					if(code != 235)

					{

						//throw new SmtpException("Error setting Auth password. (" + code.ToString() + ")");

					}

				}

 

				// MAIL FROM:

				WriteBuffer(ns, "mail from: <" + msg.EmailFrom + ">\r\n");

				// get response from Smtp server

				code = GetSmtpResponse(ReadBuffer(ns));

				if(code != 250)

				{

					throw new SmtpException("Error setting sender email address. (" + code.ToString() + ")");

				}

 

				// RCPT TO:

				foreach(string sEmailTo in msg.EmailTo)

				{

					WriteBuffer(ns, "rcpt to:<" + sEmailTo + ">\r\n");

					// get response from Smtp server

					code = GetSmtpResponse(ReadBuffer(ns));

					if(code != 250 && code != 251)

					{

						throw new SmtpException("Error setting receipient email address. (" + code.ToString() + ")");

					}

				}

 

				// DATA

				WriteBuffer(ns, "data\r\n");

				// get response from Smtp server

				code = GetSmtpResponse(ReadBuffer(ns));

				if(code != 354)

				{

					throw new SmtpException("Error starting email body. (" + code.ToString() + ")");

				}

 

				// Repeat the from and to addresses in the data section

				WriteBuffer(ns, "from:<" + msg.EmailFrom + ">\r\n");

				foreach(string sEmailTo in msg.EmailTo)

				{

					WriteBuffer(ns, "to:<" + sEmailTo + ">\r\n");

				}

 

				WriteBuffer(ns, "Subject:" + msg.EmailSubject + "\r\n");

				switch(msg.EmailMessageType)

				{

					case MessageType.Text:

						// send text message

						WriteBuffer(ns, "\r\n" + msg.EmailMessage + "\r\n.\r\n");

						break;

 

					case MessageType.HTML:

						// send HTML message

						WriteBuffer(ns, "MIME-Version: 1.0\r\n");

						WriteBuffer(ns, "Content-type: text/html\r\n");

						WriteBuffer(ns, "\r\n" + msg.EmailMessage + "\r\n.\r\n");

						break;

				}

				// get response from Smtp server

				code = GetSmtpResponse(ReadBuffer(ns));

				if(code != 250)
				{
					throw new SmtpException("Error setting email body. (" + code.ToString() + ")");
				}

				// QUIT

				WriteBuffer(ns, "quit\r\n");

			}

		}

		#endregion

 

		#region Private methods

		///

		/// Looks for an Smtp response code inside a repsonse string

		///

		///


								   /// The int value of the Smtp reponse code

								   private int GetSmtpResponse(string sResponse)

								   {

									   int response = 0;

									   int iSpace = sResponse.IndexOf(" ");

									   int iDash = sResponse.IndexOf("-");

									   if(iDash > 0 && iDash < iSpace)

										   iSpace = sResponse.IndexOf("-");

 

									   try

									   {

										   if(iSpace > 0)

											   response = int.Parse(sResponse.Substring(0, iSpace));

									   }

									   catch(Exception)

									   {

										   // error - ignore it

									   }

 

									   return response;

								   }

 

		///

		/// Write a string to the network stream

		///

		///

																	  private void WriteBuffer(NetworkStream ns, string sBuffer)

																	  {

																		  try

																		  {

																			  byte[] buffer = Encoding.ASCII.GetBytes(sBuffer);

																			  ns.Write(buffer, 0, buffer.Length);

																		  }

																		  catch(System.IO.IOException)

																		  {

																			  // error writing to stream

																			  throw new SmtpException("Error sending data to Smtp server.");

																		  }

																	  }

 

		///

		/// Reads a response from the network stream

		///

		///

											 /// A string representing the reponse read

											 private string ReadBuffer(NetworkStream ns)

											 {

												 byte[] buffer = new byte[1024];

												 int i=0;

												 int b;

												 int timeout = System.Environment.TickCount;

 

												 try

												 {

													 // wait for data to show up on the stream

													 while(!ns.DataAvailable && ((System.Environment.TickCount - timeout) < 20000))

													 {

														 System.Threading.Thread.Sleep(100);

													 }

													 if(!ns.DataAvailable)

														 throw new SmtpException("No response received from Smtp server.");

 

													 // read while there's data on the stream

													 while(i < buffer.Length && ns.DataAvailable)

													 {

														 b = ns.ReadByte();

														 buffer[i++] = (byte)b;

													 }

												 }

												 catch(System.IO.IOException)

												 {

													 // error reading from stream

													 throw new SmtpException("Error receiving data from Smtp server.");

												 }

 

												 return Encoding.ASCII.GetString(buffer);

											 }

		#endregion

	}

}