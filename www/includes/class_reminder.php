<?php
//-----------------------------------------------------------------------------
// $Workfile: class_reminder.php $ $Revision: 1.3 $ $Author: addy $ 
// $Date: 2009/07/18 22:01:20 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);

class ReminderDB
{
	public static function CreateReminder($creatoruserid,$touserid,$message,$userdate,$usertime,$repeaterinfo)
	{
		global $db;
		$touser = null;
		$fromuser = null;
		$retval = new Result();
		$tempres = UserFactory::GetUser($touserid);

		if (!$tempres->success)
		{
			$retval->errormessage = $tempres->errormessage;
			return $retval;			
		}
		else
		{
			$touser = $tempres->data;
		}
		
		if ($creatoruserid != $touserid)
		{
			$tempres = UserFactory::GetUser($creatoruserid);
			if (!$tempres->success)
			{
			$retval->errormessage = $tempres->errormessage;
				return $retval;			
			}
			else
			{
				$fromuser = $tempres->data;				
			}
		}
		else
		{
			$fromuser = $touser;
		}
		
		if (empty($userdate))
		{
			$retval->errormessage = "Invalid date ($userdate)";
			return $retval;			
		}

		if (empty($usertime))
		{
			$retval->errormessage = "Invalid time ($usertime)";
			return $retval;			
		}		
		
		if (empty($message))
		{
			$retval->errormessage = 'No message entered';
			return $retval;			
		}		

		$datetimestring = sprintf("%s %s",
						mysql_real_escape_string($userdate),
						mysql_real_escape_string($usertime));
						
		$servertime = get_server_timestamp("$datetimestring",$touser[timezone]);
		
		$insertquery = sprintf("INSERT INTO system_reminders
					(bot,userid,creatorid,msg,servertime,datetime)
					VALUES 
					('%s',%d,%d,'%s','%s','%s')",
					mysql_real_escape_string('ciqo'),
					mysql_real_escape_string($touserid),
					mysql_real_escape_string($creatoruserid),
					mysql_real_escape_string($message),
					mysql_real_escape_string($servertime),
					mysql_real_escape_string($datetimestring));
					
		$db->query($insertquery);			
				
		if ($db->geterrno() != 0)
		{
			$retval->errormessage = $db->geterrdesc();	
			return $retval;		
		}						
		
		if ($db->affected_rows() == 0)
		{
			$retval->errormessage = 'No rows affected';	
			return $retval;		
		}
		
		$result->data = $newreminderid = $db->insert_id();
		
		if (strlen($repeaterinfo['repeaterstr']) > 0)
		{
			if (strlen($repeaterinfo['expiration']) <= 0)
			{
				$retval->errormessage = 'Invalid Reapter Expiration';
				return $retval;		
			}
			
			
			
			$repeaterinsert = sprintf("INSERT INTO system_repeaters
					(remid,pattern,expiration)
					VALUES
					(%d,'%s','%s')",
					mysql_real_escape_string($newreminderid),
					mysql_real_escape_string($repeaterinfo['repeaterstr']),
					mysql_real_escape_string($repeaterinfo['expiration']));
					
			$db->query($repeaterinsert);
						
			if ($db->geterrno() != 0)
			{
				$retval->errormessage = 'New Repeater: '.$db->geterrdesc();	
				return $retval;		
			}						
			
			if ($db->affected_rows() == 0)
			{
				$retval->errormessage = 'New Repeater: No rows affected';	
				return $retval;		
			}			
		}
		
		$result->success = true;		
		return $result;		
	}
	
	
	public static function SaveReminder($reminderinfo,$repeaterstr)
	{
		global $db;
		$retval = new Result();
		
		return $retval;
	}
	
	public static function DeleteRepeaters($reminderid)
	{
		
		return true;
	}	
		
	public static function GetReminder($reminderid)
	{
		global $db;
		
		$query = sprintf("SELECT * 
											FROM system_reminders
											WHERE (id = %d)",
											mysql_real_escape_string($reminderid));

		$reminderinfo = $db->query_first($query);
		
		if ($reminderinfo['id'] > 0)
			return $reminderinfo;

		return false;		
	}
	

	
	public static function DeleteReminder($reminderid)
	{
		global $db;
		
		$query = sprintf("DELETE 
											FROM system_reminders
											WHERE (id = %d)",
											mysql_real_escape_string($reminderid));

		$db->query($query);
		return $db->affected_rows();		
	}
}

class RepeaterDB
{
	static public function GetReapeaterString($posthash)
	{
		if ($posthash["remtype"] == "daily")
		{
			if ($posthash["daily"] == "w" || $posthash["daily"] == "we")
			{
				$retVal = "d{".$posthash["daily"]."}";
			}
			else if ($posthash["daily"] == "dx" && $posthash["xdays"] < 365 && $posthash["xdays"] > 0)
			{
				$retVal = "d{".$posthash["xdays"]."}";
			}
		}
		else if ($posthash["remtype"] == "weekly")
		{
			// build the comma list
			if ($posthash["sunday"] == "on") { $cstring = $cstring . "0,"; }
			if ($posthash["monday"] == "on") { $cstring = $cstring . "1,"; }
			if ($posthash["tuesday"] == "on") { $cstring = $cstring . "2,"; }
			if ($posthash["wednesday"] == "on") { $cstring = $cstring . "3,"; }
			if ($posthash["thursday"] == "on") { $cstring = $cstring . "4,"; }
			if ($posthash["friday"] == "on") { $cstring = $cstring . "5,"; }
			if ($posthash["saturday"] == "on") { $cstring = $cstring . "6,"; }
			$cstring = substr("$cstring", 0, -1);
			
			if ($posthash["xweeks"] > 0 && strlen($cstring) > 0)
			{
				$retVal = "w{".$posthash["xweeks"].":$cstring}";
			}
		}
		else if ($posthash["remtype"] == "monthly")
		{
			if ($posthash["monthly"] == "a" && $posthash["daynum"] < 32 && $posthash["daynum"] > 0
					&& $posthash["monthnum"] < 13 && $posthash["monthnum"] > 0)
			{
				$retVal = "m{a:".$posthash["daynum"].":".$posthash["monthnum"]."}";
			}
			else if ($posthash["monthly"] == "b" && $posthash["xmonths"] > 0 && $posthash["xmonths"] < 99)
			{
				$retVal = "m{b:".$posthash["occurence"].":".$posthash["whichday"].":".$posthash["xmonths"]."}";	
			}
		}
	
		return $retVal;			
	}
	
}

?>