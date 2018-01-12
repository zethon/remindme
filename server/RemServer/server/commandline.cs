/*
* Arguments class: application arguments interpreter
*
* Authors: R. LOPES
* Contributors: R. LOPES, BillyZKid, Hastarin, E. Marcon (VB version)
* Created: 25 October 2002
* Modified: 29 September 2003
* URL: http://www.codeproject.com/csharp/command_line.asp
*
* Version: 1.1
*/

using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace server
{
	public class Arguments : StringDictionary
	{
		private Regex argex = new Regex(@"^/|-(?<name>\w+)(?::(?<value>.+))?$", RegexOptions.Compiled);

		public Arguments(string[] args)
		{
			foreach(string arg in args)
			{
				Match match = argex.Match(arg);

				if (!match.Success)
					throw new ArgumentException("Invalid argument format: " + arg);

				Add(match.Groups["name"].Value, match.Groups["value"].Value);
			}
		}
	} 
}