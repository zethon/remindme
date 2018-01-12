using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace dotEmail
{
	public class Pop3Exception : System. ApplicationException
	{
		public Pop3Exception( string str)
			: base( str)
		{
		}
	}

	public class Pop3Message
	{
		public long number;
		public long bytes;
		public bool retrieved;
		public string message;

		public string From
		{
			get 
			{
				if (message.Length == 0)
					return "";
				
				Regex r = new Regex("^from:\\s",RegexOptions.IgnoreCase);
				Match m;

				string [] lines = Regex.Split(message,"\r\n");
				foreach (string line in lines)
				{
					m = r.Match(line);
					if (m.Success)
					{
						Regex r1 = new Regex(@"([\w._]+@[\w.\-_]+)",RegexOptions.IgnoreCase);
						Match m1 = r1.Match(line);
						if (m1.Success)
						{
							return m1.Groups[0].Value.ToString();
						}
						else
							return  "";
					}
				}

				return "";
			}
		}

		public string Subject
		{
			get 
			{
				if (message.Length == 0)
					return "";
				
				Regex r = new Regex("^subject: (.*)$",RegexOptions.IgnoreCase);
				Match m;

				string [] lines = Regex.Split(message,"\r\n");
				foreach (string line in lines)
				{
					m = r.Match(line);
					if (m.Success)
						return Regex.Replace(line,@"^Subject: (.*)$","$1",RegexOptions.IgnoreCase);
				}

				return "";
			}
		}
	
	}

	public class Pop3 : System.Net.Sockets.TcpClient 
	{
		public void Connect(string server, string username, string password)
		{
			string message;
			string response;

			Connect(server, 110);
			response = Response();
			if (response.Substring(0, 3) != "+OK")
			{
				throw new Pop3Exception(response);
			}

			message = "USER " + username + "\r\n";
			Write(message);
			response = Response();
			if (response.Substring(0, 3) != "+OK")
			{
				throw new Pop3Exception(response);
			}

			message = "PASS " + password + "\r\n";
			Write(message);
			response = Response();
			if (response.Substring(0, 3) != "+OK")
			{
				throw new Pop3Exception(response);
			}
		}

		public void Disconnect()
		{
			string message;
			string response;
			message = "QUIT\r\n";
			Write(message);
			response = Response();
			if (response.Substring(0, 3) != "+OK")
			{
				throw new Pop3Exception(response);
			}
		}

		public ArrayList List()
		{
			string message;
			string response;

			ArrayList retval = new ArrayList();
			message = "LIST\r\n";
			Write(message);
			response = Response();
			if (response.Substring(0, 3) != "+OK")
			{
				throw new Pop3Exception(response);
			}

			while (true)
			{
				response = Response();
				if (response == ".\r\n")
				{
					return retval;
				}
				else
				{
					Pop3Message msg = new Pop3Message();
					char[] seps = { ' ' };
					string[] values = response.Split(seps);
					msg.number = Int32.Parse(values[0]);
					msg.bytes = Int32.Parse(values[1]);
					msg.retrieved = false;
					retval.Add(msg);
					continue;
				}
			}
		}

		public Pop3Message Retrieve(Pop3Message rhs)
		{
			string message;
			string response;

			Pop3Message msg = new Pop3Message();
			msg.bytes = rhs.bytes;
			msg.number = rhs.number;

			message = "RETR " + rhs.number + "\r\n";
			Write(message);
			response = Response();
			if (response.Substring(0, 3) != "+OK")
			{
				throw new Pop3Exception(response);
			}

			msg.retrieved = true;
			while (true)
			{
				response = Response();
				if (response == ".\r\n")
				{
					break;
				}
				else
				{
					msg.message += response;
				}
			}

			return msg;
		}

		public void Delete(Pop3Message rhs)
		{
			string message;
			string response;

			message = "DELE " + rhs.number + "\r\n";
			Write(message);
			response = Response();
			if (response.Substring(0, 3) != "+OK")
			{
				throw new Pop3Exception(response);
			}
		}

		private void Write(string message)
		{
			System.Text.ASCIIEncoding en = new System.Text.ASCIIEncoding() ;

			byte[] WriteBuffer = new byte[1024] ;
			WriteBuffer = en.GetBytes(message) ;

			NetworkStream stream = GetStream() ;
			stream.Write(WriteBuffer, 0, WriteBuffer.Length);

			//Debug.WriteLine("WRITE:" + message);
		}

		private string Response()
		{
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
			byte[] serverbuff = new Byte[1024];
			NetworkStream stream = GetStream();
			int count = 0;
			while (true)
			{
				byte[] buff = new Byte[2];
				int bytes = stream.Read(buff, 0, 1 );
				if (bytes == 1)
				{
					serverbuff[count] = buff[0];
					count++;

					if (buff[0] == '\n')
					{
						break;
					}
				}
				else
				{
					break;
				};
			};

			string retval = enc.GetString(serverbuff, 0, count );
			//Debug.WriteLine("READ:" + retval);
			return retval;
		}

	}

	/// <summary>
	/// Summary description for dotPOP3.
	/// </summary>
	public class dotPOP3
	{
		public dotPOP3()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
