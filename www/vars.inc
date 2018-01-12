<?php 
//-----------------------------------------------------------------------------
// $Workfile: vars.inc $ $Revision: 1.3 $ $Author: addy $ 
// $Date: 2009/07/04 21:31:19 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);

define("PLAN_FREE",0);
define("BASIC",1);
define("BASIC_PLUS",2);
define("PLAN_BETA_USER", 100);
define("PLAN_ADMIN", 255);

$support_email = "support@remindme.cc";
$info_email = "info@remindme.cc";
$reg_notify_email = "info@remindme.cc";
$default_plan = 100;

$prio_array = array( 'first','second', 'third' );

$dbhost = "localhost";
$dbuser="remindme_bob";
$dbpw="";
$dbname="remindme_test";


function Normalize($string)
{
	$string = str_replace(' ','',$string);
	$string = strtolower($string);	
	return $string;
}

//function UserLogin($user,$password)
//{
//	global $key,$dbname,$dbuser,$dbpw,$dbhost;
//	
//	$user = trim(stripslashes($user));
//	$password = trim(stripslashes($password));
//	
//	mysql_connect($dbhost,$dbuser,$dbpw);
//	@mysql_select_db($dbname) or die("UserLogin: (Unable to select database [$dbname])");
//	
//	$query="SELECT * FROM system_users WHERE (username='".$user."')";
//	$result=mysql_query($query);
//	$num=mysql_numrows($result);	
//	
//	if ($num > 0)
//	{
//		$storedpw = mysql_result($result,0,"password");
//		
//		if ($storedpw == md5($password))
//		{
//			mysql_close();
//			return 1;
//		}
//		else
//		{
//			mysql_close();
//			return 0;
//		}
//	}
//	else
//	{
//		mysql_close();
//		return 0;
//	}	
//}

function VerifyUser($user,$encpw)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
	
	$user = trim(stripslashes($user));
	$encpw = trim(stripslashes($encpw));
	
	$link = mysql_connect($dbhost,$dbuser,$dbpw);
	if (!$link) 
	{
    die('Could not connect: ' . mysql_error());
	}	

	@mysql_select_db($dbname) or die("VerifyUser() Unable to select database [$dbname]");
			
	$query="SELECT * FROM system_users WHERE (username='".$user."')";
	$result=mysql_query($query);
	$num=mysql_numrows($result);

	if ($num == 0)
	{
		return 0;
	}
	
	$storedpw = mysql_result($result,0,"password");
	
	mysql_close();
	
	
	return ($storedpw == $encpw);
}

function GetUserHash($user)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
		
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(GetUserHash() Unable to select database [$dbname])");
	
	$query="SELECT * FROM system_users WHERE (id='".$user."')";
	$result=mysql_query($query);
	$num=mysql_numrows($result);	
	
	if ($num == 0)
	{
		mysql_close();
		return 0;
	}
	
	$retval{'id'} = $user;
	$retval{'bot'} = mysql_result($result,0,"bot");
	$retval{'class'} = mysql_result($result,0,"class");
	$retval{'email'} = mysql_result($result,0,"email");
	$retval{'firstname'} = mysql_result($result,0,"firstname");
	$retval{'surname'} = mysql_result($result,0,"surname");
	$retval{'aimname'} = mysql_result($result,0,"aim_name");
	$retval{'msnname'} = mysql_result($result,0,"msn_name");
	$retval{'icqname'} = mysql_result($result,0,"icq_name");
	//$retval{'yahooname'} = mysql_result($result,0,"yahoo_name");
	$retval{'timezone'} = mysql_result($result,0,"timezone");
	$retval{'plan'} = mysql_result($result,0,"plan");
	$retval{'contact-order'} = mysql_result($result,0,"contact_order");
	$retval{'dls'} = mysql_result($result,0,"DLS");
	
	mysql_close();
	return $retval;
}

function GetBotHash($botname)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
		
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(GetBotHash() Unable to select database [$dbname])");
	
	$query="SELECT * FROM system_bots WHERE (name='".$botname."')";
	$result=mysql_query($query);
	$num=mysql_numrows($result);	
	
	if ($num == 0)
	{
		mysql_close();
		return 0;
	}
	
	$retval{'name'} = $botname;
	$retval{'users'} = mysql_result($result,0,"users");
	$retval{'aimname'} = mysql_result($result,0,"aim_name");
	$retval{'msnname'} = mysql_result($result,0,"msn_name");
	$retval{'icqname'} = mysql_result($result,0,"icq_name");
	$retval{'yahooname'} = mysql_result($result,0,"yahoo_name");
	$retval{'emailname'} = mysql_result($result,0,"email_name");

	mysql_close();
	return $retval;	
}

function ReplaceInFile($filename,$hash)
{
	// read the file
	$file_handle = fopen($filename,"r");
	$file_contents = fread($file_handle, filesize($filename));
	fclose($file_handle);  
	
	while (list($key,$value) = each($hash))
	{
		$file_contents = str_replace($key, $value, $file_contents);
	}
	return $file_contents;	
}

function SafeQueryData($query)
{
	$query = preg_replace( "/'/", "\\'", $query );		
	return $query;
}

function SafeFormData($data)
{
	$data = preg_replace( "/'/", "''", $data );		
	return $data;	
}

function makeRandomPassword() {
  $salt = "ABCDEFGHIJKLMNOPQRSTUVWXYZabchefghjkmnpqrstuvwxyz0123456789";
  srand((double)microtime()*1000000); 
  	$i = 0;
  	while ($i <= 7) {
    		$num = rand() % 59;
    		$tmp = substr($salt, $num, 1);
    		$pass = $pass . $tmp;
    		$i++;
  	}
  	return $pass;
}


function LogEvent($event,$user="")
{
	if ($user == "")
	{
		if ($_COOKIE[username] == "")
		{
			$user = "ANONYMOUS";
		}
		else
		{
			$user = $_COOKIE[username];
		}
	}

	$time = time();
	$month = date("m");
	$day = date("d");
	$year = date("Y");
	$filename = "$year-$month-$day.log";
	
	$fp = fopen("/home/remindme/www/logs/$filename","a");
	$result = fwrite($fp,date("H:i:s",time()).":$event:$user:$_SERVER[REMOTE_ADDR]\n");
	fclose($fp);  		
}


// increments the reminder count and returns the value
function pingReminderCount()
{
	global $dbname,$dbuser,$dbpw,$dbhost;
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(pingReminderCount() Unable to select database [$dbname])");
	
	$query="UPDATE system_reminder_count SET count=count+1;";
	$result=mysql_query($query);
	$query="SELECT * FROM system_reminder_count;";
	$result=mysql_query($query);
	$retval = mysql_result($result,0,"count");
	//mysql_close();	
	
	return $retval;
}


function createReminder($user,$datetime,$message,$touser)
{
	global $dbname,$dbuser,$dbpw,$dbhost;	
	
	$userinf = GetUserHash($user);
	//$bot = $userinf{'bot'};
	
	$touserinf = GetUserHash($touser);
	$bot = $touserinf{'bot'};
	
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(createReminder.php() Unable to select database [$dbname])");
	
	$datetime = mysql_escape_string($datetime);
	$message = mysql_escape_string($message);
		
	$cmd = "perl servertime.pl \"$datetime\" ".$userinf{'timezone'}." ".$userinf{'dls'};
	$servertime = exec ($cmd);
	
	if ($servertime == "")
	{
		$servertime = $datetime;
	}

	// will need to update this for the creator field
	$newquery = "INSERT INTO system_reminders (bot,user,creator,datetime,msg,servertime) ".
	$newquery.= " VALUES ('$bot','$touser','$user','$datetime','$message','$servertime')";

	$link = mysql_query($newquery);
	//echo mysql_errno($link) . ": " . mysql_error($link) . "\n";
	
	$result = mysql_query("SELECT LAST_INSERT_ID();");
	$retVal = mysql_result($result,0,0);
	
	mysql_close();		

	return $retVal;
}


function createUser($userid,$password,$email,$botname,$timezone,$dls,$firstname,$lastname,$ct_hash)
{
	global $dbname,$dbuser,$dbpw,$dbhost,$default_plan;
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(createUser.php() Unable to select database [$dbname])");	
	
	if ($dls == 'on')
	{
		$dls = '1';
	}
	else 
	{
		$dls = '0';
	}
	
	$encpass = md5($password);
	$query = "INSERT INTO system_users (id,email,bot,timezone,DLS,password,firstname,surname,plan)".
	" VALUES ('$userid','$email','$botname','$timezone','$dls','$encpass','$firstname','$lastname','$default_plan')";
	$link = mysql_query($query);
	
	$contacts = array( 'first','second', 'third' );
	
	$prio = 0;
	foreach ($contacts as $con)
	{
		$query = "INSERT INTO system_contacts (userid,service,login,priority) ".
		"VALUES ('$userid','".$ct_hash{$con}{service}."','".Normalize($ct_hash{$con}{login})."','$prio'); ";
		$link = mysql_query($query);
		$prio++;
	}
	
	mysql_close();	
	return 0;
}


function updateUser($userid,$email,$timezone,$dls,$firstname,$lastname,$ct_hash)
{
	global $dbname,$dbuser,$dbpw,$dbhost,$prio_array;
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(updateUser.php() Unable to select database [$dbname])");	
	
	
	if ($dls == "on")
	{
		$dls = "1";	
	}
	else 
	{
		$dls = "0";	
	}
	
	$query = "UPDATE system_users SET email='$email',timezone='$timezone',DLS='$dls',".
			"firstname='$firstname',surname='$lastname' WHERE (id = '$userid');";
	$link = mysql_query($query);

	$prio = 0;
	foreach ($prio_array as $con)
	{
		$query = "UPDATE system_contacts SET login='".Normalize($ct_hash{$con}{login})."',service='".
				$ct_hash{$con}{service}."' WHERE (userid = '$userid') AND (priority='$prio');";
				
		$link = mysql_query($query);	
		$prio++;		
	}	

	mysql_close();
}


function cancelReminder($remid)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(cancelReminder.php() Unable to select database [$dbname])");	
	$query="UPDATE system_reminders SET delivered = '1' WHERE (id = '$remid');";	
	$link = mysql_query($query);
	mysql_close();			
}

function getPendingUserReminder($remid)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(getReminder.php() Unable to select database [$dbname])");	
	//$query="SELECT * FROM system_reminders WHERE (delivered = '0') AND (id = '$remid') AND (creator = '".$_COOKIE[username]."');";
	$query="SELECT * FROM system_reminders WHERE (delivered = '0') AND (id = '$remid'));";
	$result = mysql_query($query);
	
	$retval{'id'} = mysql_result($result,0,"id");
	$retval{'user'} = mysql_result($result,0,"user");
	$retval{'datetime'} = mysql_result($result,0,"datetime");
	$retval{'creator'} = mysql_result($result,0,"creator");
	$retval{'msg'} = mysql_result($result,0,"msg");		

	mysql_close();			
	return $retval;
}

function getReminder($remid)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(getReminder.php() Unable to select database [$dbname])");	
	$query="SELECT * FROM system_reminders WHERE (id = '".SafeQueryData($remid)."');";
	$result = mysql_query($query);
	
	$retval{'id'} = mysql_result($result,0,"id");
	$retval{'user'} = mysql_result($result,0,"user");
	$retval{'datetime'} = mysql_result($result,0,"datetime");
	$retval{'msg'} = mysql_result($result,0,"msg");		
	$retval{'delivered'} = mysql_result($result,0,"delivered");		
	//$retval{'id'} = mysql_result($result,0,"id");		
	//$retval{'servertime'} = mysql_result($result,0,"servertime");		
	$retval{'deliveredtime'} = mysql_result($result,0,"deliveredtime");
	$retval{'creator'} = mysql_result($result,0,"creator");
	$retval{'deliveredname'} = mysql_result($result,0,"deliveredname");		
	$retval{'deliveredconn'} = mysql_result($result,0,"deliveredconn");		

	mysql_close();			
	return $retval;
}

function getContactHash($userid)
{
	global $dbname,$dbuser,$dbpw,$dbhost,$prio_array;
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(getContactHash.php() Unable to select database [$dbname])");	
	$query="SELECT * FROM system_contacts WHERE (userid = '$userid');";
	$result = mysql_query($query);

	$retval{'userid'} = mysql_result($result,0,"userid");
	
	$count = 0;
	foreach ($prio_array as $prio)
	{
		$order = mysql_result($result,$count,"priority");
		$retval{$prio_array[$order]}{service} = mysql_result($result,$count,'service');
		$retval{$prio_array[$order]}{login} = mysql_result($result,$count,'login');
		$count++;
	}
	
	mysql_close();			
	return $retval;
}

function getReapeater($remid)
{
	global $dbname,$dbuser,$dbpw,$dbhost,$prio_array;
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(getContactHash.php() Unable to select database [$dbname])");		

	$query="SELECT * FROM system_repeaters WHERE (remid = '$remid');";
	$result = mysql_query($query);
	
	$retval{'repid'} = mysql_result($result,0,"repid");
	$retval{'remid'} = mysql_result($result,0,"remid");
	$retval{'pattern'} = mysql_result($result,0,"pattern");
	$retval{'count'} = mysql_result($result,0,"count");		
	$retval{'disabled'} = mysql_result($result,0,"disabled");	
	$retval{'expiration'} = mysql_result($result,0,"expiration");	
	$retval{'expiration'} = substr($retval{'expiration'},0,10);
	$retval{'expiration'} = str_replace("-","/",$retval{'expiration'});

	mysql_close();	
	return $retval;	
}

function editReminder($touser,$creator,$remid,$datetime,$message)
{
	
	global $dbname,$dbuser,$dbpw,$dbhost;
	
	$touserinf = GetUserHash($touser);
	$bot = $touserinf{'bot'};
	
	$userinf = GetUserHash($creator);
	
	$cmd = "perl servertime.pl \"$datetime\" ".$userinf{'timezone'}." ".$userinf{'dls'};
	$servertime = exec ($cmd);
	
	if ($servertime == "")
	{
		$servertime = $datetime;
	}
	
	$message = mysql_escape_string($message);
	$datetime = mysql_escape_string($datetime);
	
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(saveReminder.php() Unable to select database [$dbname])");	
	
	$query="UPDATE system_reminders SET user = '$touser', bot = '$bot', servertime = '$servertime', datetime = '$datetime', msg = '$message' WHERE (delivered = '0') AND (id = '$remid') AND (creator = '".$_COOKIE[username]."');";
	
	$result = mysql_query($query);
	mysql_close();
	
	return $retval;
}


function getPlanHash($planid)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
		
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(GetBotHash() Unable to select database [$dbname])");
	
	$query="SELECT * FROM system_subscribe_plans WHERE (id='$planid');";
	
	$result=mysql_query($query);
	$num=mysql_numrows($result);	
	
	if ($num == 0)
	{
		mysql_close();
		return 0;
	}
	
	$retval{'id'} = mysql_result($result,0,"id");
	$retval{'allow_repeat_reminders'} = mysql_result($result,0,"allow_repeat_reminders");
	$retval{'num_reminders'} = mysql_result($result,0,"num_reminders");
	//$retval{'num_repeat_reminders'} = mysql_result($result,0,"num_repeat_reminders");
	
	mysql_close();
	return $retval;
}

function getReapterString($posthash)
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

function CreateRepeater($remid,$pattern,$disabled,$expiration)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
	
	
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(createReminder.php() Unable to select database [$dbname])");
//
//	// will need to update this for the creator field
	$expiration = str_replace("/","-",$expiration);
	$expiration .= " 00:00:00";

	$newquery = "INSERT INTO system_repeaters (remid,pattern,expiration) ".
	$newquery.= " VALUES ('$remid','$pattern','$expiration')";

	$link = mysql_query($newquery);
//	//echo mysql_errno($link) . ": " . mysql_error($link) . "\n";
	mysql_close();	
	return $newquery;		
}

function ModifyRepeater($repid,$pattern,$disabled,$expiration)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
	
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(createReminder.php() Unable to select database [$dbname])");	
	
	$expiration = str_replace("/","-",$expiration);
	$expiration .= " 00:00:00";
	
	$newquery =  "UPDATE system_repeaters SET pattern = '$pattern', expiration = '$expiration' ";
	$newquery .= "WHERE (repid = '$repid');";

	$link = mysql_query($newquery);
	
	mysql_close();	
	return $newquery;		
}

function DeleteRepeaters($remid)
{
	global $dbname,$dbuser,$dbpw,$dbhost;
	
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(createReminder.php() Unable to select database [$dbname])");
	
	$link = mysql_query("DELETE from system_repeaters WHERE (remid = '$remid')");
	mysql_close();	
	return $newquery;		
}

?>
