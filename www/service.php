<?php
//-----------------------------------------------------------------------------
// $Workfile: service.php $ $Revision: 1.15 $ $Author: addy $ 
// $Date: 2008/12/21 01:47:08 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL);
require_once('./global.php');
require_once(DIR.'/includes/httprequest.php');
require_once(DIR.'/includes/class_xml.php');

class UserContact
{
	var $servicename = '';
	var $userlogin = '';
	
	function UserContact($strServ,$strLogin)
	{
		$this->servicename = strtolower($strServ);
		$this->userlogin = strtolower($strLogin);
	}
}

class WebService
{
	public $lastquery = "";
	
	// 0 - reminderid
	// 1 - server time
	// 2 - user time string
	// 3 - delivered (boolean)
	function SaveReminderDeliveryInfo($params)
	{
		global $db;
		
		$query = "
			UPDATE system_reminders 
			SET
				servertime = '$params[1]',
				datetime = '$params[2]',
				delivered  = $params[3]
			WHERE (id = $params[0])
		";
		
		$db->query($query);
		if ($db->geterrno() != 0)
		{
			// TODO: throw exception
			return;
		}
		
		$retXml = new XML_Builder();	
		$retXml->add_tag('reminderid',$params[0]);
		return $retXml->doc;
	}	
	
	
	// 0 - reminder id 
	// 1 - server time string
	// 2 - user time string
	// 3 - message
	// 4 - delivered (boolean)
	// 5 - delivered time
	// 6 - deliveredname
	// 7 - deliveredconn
	function SaveReminder($params)
	{
		global $db;
		
		$query = "
			UPDATE system_reminders 
			SET
				servertime = '$params[1]',
				datetime = '$params[2]',
				msg  = '$params[3]',
				delivered  = $params[4],
				deliveredtime  = '$params[5]',
				deliveredname  = '$params[6]',
				deliveredconn  = '$params[7]'								
			WHERE (id = $params[0])
		";
		
		//print($query);
		$db->query($query);
		if ($db->geterrno() != 0)
		{
			// TODO: throw exception
			return;
		}
		
		$retXml = new XML_Builder();	
		$retXml->add_tag('reminderid',$params[0]);
		return $retXml->doc;
	}
	
	// returns the reminders ID number
	// params index information
	// 0 - user
	// 1 - creator
	// 2 - server delivery time
	// 3 - usertimestring
	// 4 - message
	// 5 - delivered (boolean)
	// 6 - rem bot
	function CreateReminder($params)
	{
		global $db;
				
		$this->lastquery = sprintf("INSERT INTO system_reminders
				(userid,creatorid,servertime,datetime,msg,delivered,bot)
			VALUES
				($params[0],$params[1],'$params[2]','$params[3]',
				 '%s',$params[5],'$params[6]')",
				 mysql_real_escape_string($params[4]));
		
		$db->query($this->lastquery);
		if ($db->geterrno() != 0)
		{
			// throw new Exception(->geterrdesc());
			// throw an exception! (once we're on php5)
			//print "(".$db->geterrdesc().")";
			return;
		}		
		
		$retXml = new XML_Builder();	
		$retXml->add_tag('reminderid',$db->insert_id());
		return $retXml->doc;
	}
	
	function GetLastDeliveredReminder($params)
	{
		global $db;
		$strUserId = $params[0];		
		
		$query = "
			SELECT * 
			FROM system_reminders 
			LEFT JOIN system_repeaters ON system_reminders.id = system_repeaters.remid 
			WHERE (userid = $strUserId) 
			GROUP BY id 
			ORDER BY deliveredtime DESC 
			LIMIT 1
		";		
		
		$reminderInfo = $db->query_first($query);

		$retXml = new XML_Builder();	
		$retXml->add_group('reminder');			
		foreach (array_keys($reminderInfo) as $key)
		{
			if (is_string($reminderInfo[$key]) && strlen($reminderInfo[$key]) > 0)
				$retXml->add_tag($key,$reminderInfo[$key]);
		}
		$retXml->close_group();
		
		return $retXml->doc;
	}
	
	
	// the UNDELIVERED reminders for a given user
	function GetUsersReminders($params)
	{
		global $db;
		$strUserId = $params[0];		
		
		$query = "
			SELECT * 
			FROM system_reminders 
			WHERE 
				(userid = $strUserId) 
				AND (delivered = 0)
			";
			
		$retXml = new XML_Builder();	
		$retXml->add_group('reminders');
		$reminders = $db->query($query);
		while ($reminder = $db->fetch_array($reminders))
		{
			$retXml->add_group('reminder');
			foreach (array_keys($reminder) as $key)
			{
				if (is_string($reminder[$key]) && strlen($reminder[$key]) > 0)
					$retXml->add_tag($key,$reminder[$key]);				
			}
			$retXml->close_group();	
		}
		$retXml->close_group();
					
		return $retXml->doc;					
	}
	
	function GetUser($params)
	{
		global $db;
		$strUserId = $params[0];
		
		$query = "
			SELECT 
				system_users.userid,
				system_users.username,
				class,
				email,
				bot,
				timezone,
				DLS,
				plan,
				plan_name,
				num_reminders,
				num_ptp_reminders 
			FROM system_users 
			LEFT JOIN system_subscribe_plans ON system_subscribe_plans.id = system_users.plan 
			WHERE system_users.userid = $strUserId
		";
		$userinfo = $db->query_first($query);
		
		$retXml = new XML_Builder();	
		$retXml->add_group('user');
		
		foreach (array_keys($userinfo) as $key)
		{
			if (is_string($userinfo[$key]) && strlen($userinfo[$key]) > 0)
				$retXml->add_tag($key,$userinfo[$key]);			
		}
				
		$query = "
			SELECT system_contacts.*,service.name as service
			FROM system_contacts
			LEFT JOIN service USING (serviceid)
			WHERE (userid = $strUserId)		
		";		

		$retXml->add_group('contacts');
		$contactsq = $db->query($query);
		while ($contactinfo = $db->fetch_array($contactsq))
		{
			$retXml->add_group('contact');
			foreach (array_keys($contactinfo) as $key)
			{
				if (is_string($contactinfo[$key]) && strlen($contactinfo[$key]) > 0)
					$retXml->add_tag($key,$contactinfo[$key]);			
			}
			$retXml->close_group();
		}
		$retXml->close_group();
		

		$query = "
			SELECT * 
			FROM system_allow_lists 
			WHERE (ownerid = $strUserId)	
		";
		
		$retXml->add_group('buddies');
		$buddiesq = $db->query($query);
		while ($buddyinfo = $db->fetch_array($buddiesq))
		{
			$retXml->add_group('buddy');
			foreach (array_keys($buddyinfo) as $key)
			{
				if (is_string($buddyinfo[$key]) && strlen($buddyinfo[$key]) > 0)
					$retXml->add_tag($key,$buddyinfo[$key]);			
			}
			$retXml->close_group();
		}
		$retXml->close_group();				
		
		$retXml->close_group();
		return $retXml->doc;

	}
	
	function GetUserByUsername($params)
	{
		global $db;
		$strUsername = $params[0];
				
		$query = "SELECT userid FROM system_users WHERE (username = '$strUsername')";
		$userinfo = $db->query_first($query);
		
		return $this->GetUser(array($userinfo['userid']));
	}
		
	function GetReminder($params)
	{
		global $db;
		$strRemID = $params[0];
		
		$query = "
			SELECT * 
			FROM system_reminders 
			WHERE (id = $strRemID)
		";
		
		$reminderInfo = $db->query_first($query);

		$retXml = new XML_Builder();	
		$retXml->add_group('reminder');			
		foreach (array_keys($reminderInfo) as $key)
		{
			if (is_string($reminderInfo[$key]) && strlen($reminderInfo[$key]) > 0)
				$retXml->add_tag($key,$reminderInfo[$key]);
		}
		$retXml->close_group();
		
		return $retXml->doc;
	}
	
	// total number of reminders created by the given user
	function GetUserReminderCount($params)
	{
		global $db;
		//$strUserName = strtolower($params[0]);
		$strUserID = strtolower($params[0]);

		$query = "
			SELECT COUNT(*) AS count
			FROM system_reminders 
			WHERE (userid = $strUserID) 
				AND (userid = creator)
		";
		
		$count = $db->query_first($query);
		
		$retXml = new XML_Builder();	
		$retXml->add_tag('count',$count['count']);
		
		return $retXml->doc;
	}
	
	function GetUserPTPReminderCount($params)
	{
		global $db;
		$strUserID = strtolower($params[0]);

		$query = "
			SELECT COUNT(*) AS count
			FROM system_reminders 
			WHERE (userid = strUserID) 
				AND (userid != creator)
		";
		
		
		$count = $db->query_first($query);
		
		$retXml = new XML_Builder();	
		$retXml->add_tag('count',$count['count']);
		
		return $retXml->doc;
	}	

	// TODO: this function should accept a second param specifying the time (60 * 60 * 2)	
	// which should be configurable in the bot
	function GetReminders($params)
	{
		global $db;
		$strBotName = $params[0];
			
		$this->lastquery = "		
			SELECT *
			FROM system_reminders
			LEFT JOIN system_repeaters ON system_reminders.id = system_repeaters.remid
			WHERE (
				bot = '$strBotName'
				AND delivered =0
					) AND (
				UNIX_TIMESTAMP( servertime ) - UNIX_TIMESTAMP( NOW( ) ) < ( 60 * 60 * 2 )
				)";
				
		if ($reminderlist = $db->query($this->lastquery))
		{
			$retXml = new XML_Builder();	
			$retXml->add_group('reminders');			
			while ($reminfo = $db->fetch_array($reminderlist))
			{
				$retXml->add_group('reminder');	
				foreach(array_keys($reminfo) as $key)
				{
					if (strlen($reminfo[$key]) > 0)
						$retXml->add_tag($key,$reminfo[$key]);
				}				
				//print_r($reminfo);
				$retXml->close_group();
			}
			$retXml->close_group();
			
			return $retXml->doc;
		}
		
		return false;				
	}
	
	function LoadUsers($params)
	{
		global $db;
		$strBotName = $params[0];
		
		// TODO: get rid of the system_users.id
		$this->lastquery = "
			SELECT 
				system_users.userid,
				system_users.username,
				class,
				email,
				bot,
				timezone,
				DLS,
				plan,
				plan_name,
				num_reminders,
				num_ptp_reminders,
				system_contacts.login as service_login,
				service.name as service_name
			FROM system_users 
				LEFT JOIN system_subscribe_plans ON system_subscribe_plans.id = system_users.plan 
				LEFT JOIN system_contacts ON system_users.userid = system_contacts.userid
				LEFT JOIN service USING(serviceid)
			WHERE bot = '$strBotName'
				AND system_contacts.login != \"\"
			ORDER BY username ASC, system_contacts.priority ASC
		";
		
		$userlist = $db->query($this->lastquery);

		if ($db->geterrno() === 0)
		{
			// build the user hash before we convert it to xml
			$users = array();
			while ($userinfo = $db->fetch_array($userlist))
			{
				$userid = $userinfo['userid'];
				if ($users[$userid] == null)
				{
					foreach(array_keys($userinfo) as $key)
					{
						if (substr($key,0,8) != 'service_')
							$users[$userid][$key] = $userinfo[$key];
					}

					$users[$userid]['contacts'] = array();
				}
				
				if ($users[$userid] != null && strlen($userinfo['service_login']) > 0 && strlen($userinfo['service_name']) > 0)
				{
					$service = new UserContact($userinfo['service_name'],$userinfo['service_login']);
					array_push($users[$userid]['contacts'],$service);
				}
			}
			
			// convert the user hash to xml
			$retXml = new XML_Builder();	
			$retXml->add_group('users');
			foreach (array_keys($users) as $userid)
			{
				if (count($users[$userid]['contacts']) === 0)
					next;
				
				$retXml->add_group('user');
				foreach (array_keys($users[$userid]) as $key)
				{
					if (is_string($users[$userid][$key]))
						$retXml->add_tag($key,$users[$userid][$key]);					
				}				
				
				foreach ($users[$userid]['contacts'] as $contact)
				{
					$retXml->add_group('contact');
					$retXml->add_tag('service',$contact->servicename);
					$retXml->add_tag('login',$contact->userlogin);
					$retXml->close_group();		
				}
				$retXml->close_group();	
			}
			$retXml->close_group();
			
			return $retXml->doc;			
		}
		else 
		{
			print "(".$db->geterrdesc().")";				
			// throw an exception, bitch!
		}
		
		// should never happen
		return false;
	}
}

// we answer in nothing but xml
header ("content-type: text/plain"); 

// define important globals
$DEBUG = ($_REQUEST['debug'] == '1');

// create the response XML object
$xmlResponse = new XML_Builder();
$xmlResponse->add_group('remindme');

// get the http request info
$http_request = new http_request();
$content = $http_request->body();

// parse the body
$xmlBodyObj = new XML_Parser($content);
$xmlBodyData = $xmlBodyObj->parse();

// was the body of the request valid XML?
if (!$xmlBodyData)
{
	$errornum = $xmlBodyObj->xml_parser_error_no;
	$errortxt = xml_error_string($errornum);
	$xmlResponse->add_tag('error',"Failed to parse body ($errornum): ".$errortxt);
}
else
{
	// TODO: add more security checks

	$funcName = $xmlBodyData['function']['name'];
	$webService = new WebService();
	if (!method_exists($webService,$funcName))
	{
		$xmlResponse->add_tag('error',"Unknown function: $funcName");
	}
	else
	{
		$arguments = array();
		if (is_array($xmlBodyData['function']['argument']))
			$arguments = $xmlBodyData['function']['argument'];
		else
			$arguments[0] = $xmlBodyData['function']['argument'];
		
		$data = $webService->$funcName($arguments);
		if ($data === false)
		{
			$xmlResponse->add_tag('error',"Null return data from function");		
		}
		else
		{
			$xmlResponse->add_tag('data',$data);
		}
	}
}

if ($DEBUG)
{
	$xmlResponse->add_group('DEBUGINFO');
	
	$xmlResponse->add_group('GET');
	foreach (array_keys($_GET) as $key)
	{
		$xmlResponse->add_tag($key,$_GET[$key]);		
	}
	$xmlResponse->close_group();	
	
	$xmlResponse->add_tag('BODY',$content);
	
	$xmlResponse->add_tag('lastquery',$webService->lastquery);
	$xmlResponse->close_group();	
	
	
}

$xmlResponse->close_group();	
$xmlResponse->print_xml();

?>
