using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace server
{
	public enum FrequencyType {Daily,Weekly,Monthly};
	
	public class RepeaterClass
	{
		private string m_iID;
		public string ID { get { return m_iID; }}

		private string m_strPattern;
		public string Pattern { get { return m_strPattern; }}

		private int m_iCount;
		public int Count { get { return m_iCount; }}

		private bool m_bDisabled;
		public bool Disabled { get { return m_bDisabled; }}

		private string m_strExpiration;

		public RepeaterClass (string id, string pattern, int iCount, bool disabled, string dt)
		{
			m_iID = id;
			m_strPattern = pattern;
			m_iCount = iCount;
			m_bDisabled = disabled;
			m_strExpiration = dt;
		}

		public bool HasExpired()
		{
			bool bRetVal = false;

			try
			{
				string strPattern = "yyyy-MM-dd HH:mm:ss"; 
				DateTime dt = DateTime.ParseExact(m_strExpiration, strPattern, null); 
				if (dt < System.DateTime.Now)
					bRetVal = true;
			}
			catch(Exception e) 
			{
				
			}
			
			return bRetVal;
		}

	}
	/// <summary>
	/// Summary description for RepeatPattern.
	/// </summary>
	public class RepeatPattern
	{
		private bool m_IsValidPattern = false;
		public bool IsValidPattern
		{
			get { return m_IsValidPattern; }
			set { m_IsValidPattern = value; }
		}

		private FrequencyType m_FreqType;
		public FrequencyType Frequency
		{
			get { return m_FreqType; }
			set { m_FreqType = value; }
		}

		private string m_strPattern;
		public string PatternString
		{	
			get { return m_strPattern; }
			set { m_strPattern = value; }
		}

		private ArrayList m_values = new ArrayList();
		public string GetValue(int i)
		{
			if (i <= m_values.Count)
				return (string)m_values[i];

			return null;
		}

		public RepeatPattern(string strPattern)
		{
			PatternString = strPattern;
			ParsePatternString();
		}

		public DateTime GetNextDate(DateTime dt)
		{
			DateTime RetVal = new DateTime();
			RetVal = dt;
			
			switch (Frequency)
			{
				case FrequencyType.Daily:
					string strVal = (string)m_values[0];
					if (strVal == "w")
					{
						int iAddFactor = 0;

						if ((int)dt.DayOfWeek >= 0 && (int)dt.DayOfWeek <= 4)
							iAddFactor = 1;
						else if ((int)dt.DayOfWeek == 5)
							iAddFactor = 3;
						else if ((int)dt.DayOfWeek == 6)
							iAddFactor = 2;
						
						RetVal = RetVal.AddDays(iAddFactor);
					}
					else if (strVal == "we")
					{
						int iAddFactor = 0;
						switch ((int)dt.DayOfWeek)
						{
							case 0: iAddFactor = 6; break;
							case 1: iAddFactor = 5; break;
							case 2: iAddFactor = 4; break;
							case 3: iAddFactor = 3; break;
							case 4: iAddFactor = 2; break;
							case 5: case 6: iAddFactor = 1; break;
						}

						RetVal = RetVal.AddDays(iAddFactor);
					}
					else
					{
						int i = Int32.Parse(strVal);
						RetVal = RetVal.AddDays(i);
					}
				break;

				case FrequencyType.Weekly:
					int iDayFactor = 0;
					bool bIsThisWeek = false;

					foreach (string day in m_values[1].ToString().Split(','))
					{
						int i = int.Parse(day);
						if (i > (int)dt.DayOfWeek)
						{
							iDayFactor = i - (int)dt.DayOfWeek;
							bIsThisWeek = true;
							break;
						}
					}
					
					if (bIsThisWeek)
						RetVal = RetVal.AddDays(iDayFactor);
					else
					{
						int iWeekFactor = int.Parse(m_values[0].ToString());
						string[] days = m_values[1].ToString().Split(',');
						int iFirstDay = int.Parse(days[0]);

						int iDays = (iFirstDay) - (int)dt.DayOfWeek;
						RetVal = RetVal.AddDays((7*iWeekFactor)+iDays);
					}
				break;

				case FrequencyType.Monthly:
					if ((string)m_values[0] == "a")
					{
						DateTime tempDate = dt.AddMonths(int.Parse((string)m_values[2]));

						int iDate = 0;
						if ((string)m_values[1] == "l")
						{
							iDate = GetMonthDayCount(tempDate.Month,tempDate.Year);
						}
						else
						{
							iDate = int.Parse((string)m_values[1]);
							if (iDate > GetMonthDayCount(tempDate.Month,tempDate.Year))
								iDate = GetMonthDayCount(tempDate.Month,tempDate.Year);
						}

						RetVal = new DateTime(tempDate.Year,tempDate.Month,iDate,tempDate.Hour,tempDate.Minute,tempDate.Second);
					}
					else if ((string)m_values[0] == "b")
					{
						// m_values[0] - a/b switch
						// m_values[1] - occurence ie. 1,2,3,4,L
						// m_values[2] - d (day), we (weekend day), w (weekday), 0 (Sunday), 1,2,3 etc..
						// m_values[3] - X in "of every X month"

						Hashtable events = new Hashtable();
						Hashtable lasts = new Hashtable();
						DateTime tempDate = dt.AddMonths(int.Parse((string)m_values[3]));
						
						int iOccurence = 0;

						if ((string)m_values[1] == "l")
						{
							iOccurence = -1;
						}
						else
						{
							iOccurence = int.Parse((string)m_values[1]);
							if (iOccurence > 4)
								iOccurence = 4;
						}

						// initialize the hash
						lasts["d"] = 0; lasts["we"] = 0; lasts["w"] = 0;
						for (int i = 0; i < 7; i++)
							lasts[i.ToString()] = 0;

						events["d"] = 0; events["we"] = 0; events["w"] = 0;
						for (int i = 0; i < 7; i++)
							events[i.ToString()] = 0;

						for (int iDay = 1; iDay <= GetMonthDayCount(tempDate.Month,tempDate.Year); iDay++)
						{	
							DateTime loopTemp = new DateTime(tempDate.Year,tempDate.Month,iDay,tempDate.Hour,tempDate.Minute,tempDate.Second);
						
							// update the events
							events["d"] = (int)events["d"] + 1;
							lasts["d"] = iDay;
							
						
							if ((int)loopTemp.DayOfWeek == 0 || (int)loopTemp.DayOfWeek == 6)
							{
								events["we"] = (int)events["we"] + 1; // weekend day
								lasts["we"] = iDay;
							}
							else
							{
								lasts["w"] = iDay;
								events["w"] = (int)events["w"] + 1; // weekday
							}
                            
							events[((int)loopTemp.DayOfWeek).ToString()] = (int)events[((int)loopTemp.DayOfWeek).ToString()] + 1;
							lasts[((int)loopTemp.DayOfWeek).ToString()] = (int)lasts[((int)loopTemp.DayOfWeek).ToString()] + 1;
						
							// check ourselves out
							if ((string)m_values[1] != "l" && (int)events[((string)m_values[2])] == iOccurence)
							{
								RetVal = loopTemp;
								break;
							}
						}
						
						if ((string)m_values[1] == "l")
						{
							int iLastDay = (int)lasts[((string)m_values[2])];
							RetVal = new DateTime(tempDate.Year,tempDate.Month,iLastDay,tempDate.Hour,tempDate.Minute,tempDate.Second);
						}
					}
					break;
			}


			return RetVal;
		}

		private int GetMonthDayCount(int iMonth, int iYear)
		{
			int iRetVal = 0;

			switch (iMonth)
			{
				case 1:	case 3: case 5:	case 7:	
				case 8:	case 10: case 12:
					iRetVal = 31;
				break;

				case 4:	case 6:
				case 9:	case 11:
					iRetVal = 30;
				break;
			}

			if (iMonth == 2)
			{
				if ((iYear % 4)==0)
					iRetVal = 29;
				else
					iRetVal = 28;
			}

			return iRetVal;
		}


		private bool ParsePatternString()
		{
			bool bRetVal = true;
			string strPattern = PatternString;
			
			// clean up the string
			strPattern = strPattern.Trim();
			strPattern = strPattern.ToLower();

			// grab the values between the { and }
			Regex reg = new Regex(@"\{(.*)\}",RegexOptions.IgnoreCase);
			Match m = reg.Match(strPattern);

			// parse the values into an array
			if (m.Success)
			{
				string strInside = m.Groups[0].Value.ToString();
				strInside = strInside.Replace("{",null);
				strInside = strInside.Replace("}",null);

				foreach (string str in strInside.Split(':'))
					m_values.Add(str);
			}

			// patterns are at a min 4 chars long
			if (strPattern.Length >= 4)
			{
				// lazy parsing checking, we're not planning to send this corrupt data
				switch (strPattern.Substring(0,1))
				{
					case "d":
						Frequency = FrequencyType.Daily;
						bRetVal = true;
						break;

					case "w":
						Frequency = FrequencyType.Weekly;
						bRetVal = true;
						break;

					case "m":
						Frequency = FrequencyType.Monthly;
						bRetVal = true;
						break;

					default:
					break;
				}
			}

			return bRetVal;
		}
	}
}
