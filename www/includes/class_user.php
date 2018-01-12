<?php
//-----------------------------------------------------------------------------
// $Workfile: class_user.php $ $Revision: 1.5 $ $Author: addy $ 
// $Date: 2009/07/17 22:24:29 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);

define('USER_ERRORTYPE_SUCCESS',0);
define('USER_ERRORTYPE_USERNAME',1);
define('USER_ERRORTYPE_PASSWORD',2);
define('USER_ERRORTYPE_EMAIL',3);
define('USER_ERRORTYPE_TIMEZONE',4);
define('USER_ERRORTYPE_LICENSE',5);
define('USER_ERRORTYPE_GLOBAL',6);
define('USER_ERRORTYPE_CONTACT',7);
define('USER_ERRORTYPE_UNKNOWN_USER',8);
define('USER_ERRORTYPE_REGISTRATION_PASSWORD',8);

define('SYSTEM_ADMIN',255);
define('SYSTEM_MODERATOR',100);
define('SYSTEM_USER',1);

class User
{
	public static function ValidateByUsername($username,$password,$rawpassword = false)
	{
		global $db,$userinfo;

		if (strlen($username) == 0 || strlen($password) == 0)
			return false;
			
		$query = sprintf(
					"SELECT * 
						FROM system_users 
						WHERE (username = '%s')",
					mysql_real_escape_string($username));
			
		$userinfo = $db->query_first($query);
		
		if ($rawpassword)
			$password = md5($password);
		
		$userinfo['loggedin'] = true;
		return $userinfo['password'] == $password;
	}
	
	public static function ValidateUser($userid,$password)
	{
		global $db,$userinfo;
		
		if (strlen($userid) == 0 || strlen($password) == 0)
			return false;
		
		$userinfo = $db->query_first(sprintf(
					"SELECT * 
						FROM system_users 
						WHERE (userid = '%s')",
					mysql_real_escape_string($userid)));		
		
		if ($userinfo['password'] == $password)
		{
			$userinfo['loggedin'] = true;
			return $userinfo;
		}
			
		return false;					
	}

}


class UserWriteResult
{
	public $Success = false;
	//public $ErrorHash = array();
	//public $User = null;	
	
	public $UserID = 0;
	public $ErrorType = 0;
	public $ErrorMessage = "";
}


class UserFactory
{
	
	// TODO: check for duplicate screen names entered
	// TODO: rework the validation altogether
	public static function SaveUser($userinfo)
	{
		global $db; 
		$retVal = new UserWriteResult();		
		
		// assume failure
		$retVal->Success = false;
		
		// verify email
		if (strlen($userinfo['email']) == 0 || !eregi("^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,5})$", $userinfo["email"]))
		{
			$retVal->ErrorType = USER_ERRORTYPE_EMAIL;
			$retVal->ErrorMessage = 'Invalid email address.';			
			return $retVal;			
		}
		
		// test for duplicate email		
		$userid = $db->query_first(sprintf(
					"SELECT userid 
						FROM system_users 
						WHERE (email = '%s') AND (userid != %d)",
					mysql_real_escape_string($userinfo['email']),
					$userinfo['userid']
					));		
		
		if ($userid > 0)
		{
			$retVal->ErrorType = USER_ERRORTYPE_EMAIL;
			$retVal->ErrorMessage = 'This email is already in use.';			
			return $retVal;
		}					
		
		// verify user timezone	
		if (strlen($userinfo['timezone']) <= 0)
		{
			$retVal->ErrorType = USER_ERRORTYPE_TIMEZONE;
			$retVal->ErrorMessage = 'Invalid timezone.';			
			return $retVal;			
		}		
		
		// fix DLS
		$dls = $userinfo['DLS'] == 'on' ? '1' : '0';
		
		// ok, let's check the contacts
		
		// create a services info hash
		$svchash = array();	
		$services = $db->query("SELECT * FROM service ORDER BY description");
		while ($svcinfo = $db->fetch_array($services))
		{
			$id = $svcinfo['serviceid'];
			$svchash[$id] = $svcinfo;
		}
		
		
		// TODO: this 3 should not be hard coded			
		$lastoneblank = false;
		for ($i = 0; $i < 3; $i++)
		{
			$loginidx = 'login'.$i;
			$serviceidx = 'service'.$i;
			$serviceid = $userinfo[$serviceidx];
			
			if ($userinfo[$loginidx] == '')
			{
				$lastoneblank = true;		
				next;
			}
			
			// see if someone left contact info blank and filled out info after it
			if (strlen($userinfo[$loginidx]) > 0 && $lastoneblank)
			{
				$retVal->ErrorType = USER_ERRORTYPE_CONTACT;
				$retVal->ErrorMessage = 'You are missing contact info.';			
				$retVal->Data = $i-1;
				return $retVal;					
			}
			
			// see if the service id is supproted
			if ($svchash[$serviceid]['active'] != '1')
			{
				$retVal->ErrorType = USER_ERRORTYPE_CONTACT;
				$retVal->ErrorMessage = 'This service is no longer supported by RemindMe';			
				$retVal->Data = $i;
				return $retVal;						
			}
			
			if (strlen($userinfo[$loginidx]) > 0 && !eregi($svchash[$serviceid]['usernameregex'],$userinfo[$loginidx]))
			{
				$retVal->ErrorType = USER_ERRORTYPE_CONTACT;
				$retVal->ErrorMessage = 'Invalid Screen Name';			
				$retVal->Data = $i;
				return $retVal;										
			}
		}

		// we're here, so write the damn info
		if ($userinfo['userid'] > 0)
		{
			$db->query(sprintf("
				UPDATE system_users
				SET 
					email = '%s',
					timezone = '%s',
					DLS = $dls
				WHERE (userid = %d)",
				mysql_real_escape_string($userinfo['email']),
				mysql_real_escape_string($userinfo['timezone']),
				mysql_real_escape_string($userinfo['userid'])
			));
			
			if ($db->geterrno() != 0)
			{
				$retVal->ErrorType = USER_ERRORTYPE_GLOBAL;
				$retVal->ErrorMessage = $db->geterrdesc();			
				return $retVal;							
			}
			
			// delete the user's current contacts
			$db->query(sprintf("
				DELETE FROM system_contacts 
				WHERE 
					(userid = %d)",
				mysql_real_escape_string($userinfo[userid])
			));
			
			// write the contact info we're given
			for ($i = 0; $i < 3; $i++)
			{
				$loginidx = 'login'.$i;
				$serviceidx = 'service'.$i;	
				$serviceid = $userinfo[$serviceidx];			
				
				if (strlen($userinfo[$loginidx]) > 0)
				{
					$db->query(sprintf("
						INSERT INTO system_contacts
							(userid,serviceid,login)
						VALUES
							(%d,%d,'%s')",
						mysql_real_escape_string($userinfo['userid']),
						mysql_real_escape_string($serviceid),
						mysql_real_escape_string($userinfo[$loginidx])						
						));
						
					if ($db->geterrno() != 0)
					{
						$retVal->ErrorType = USER_ERRORTYPE_GLOBAL;
						$retVal->ErrorMessage = $db->geterrdesc();			
						return $retVal;							
					}						
				}
			}
			
			$retVal->UserID  = $userinfo['userid'];
			$retVal->Success = true;
			return $retVal;
		}	

		return $retVal;
	}
	
	public static function CreateUser($userinfo)
	{
		global $db; 
		$retVal = new UserWriteResult();
		
		// assume failure
		$retVal->Success = false;

		// TODO: do more username checks here
		if (strlen($userinfo['username']) < 3 || strlen($userinfo['username']) > 16)
		{
			$retVal->ErrorType = USER_ERRORTYPE_USERNAME;
			$retVal->ErrorMessage = 'Username must be between 3 and 16 characters long.';			
			return $retVal;
		}
		
		// test for duplicate username		
		$userid = $db->query_first(sprintf(
			"SELECT userid 
			 FROM system_users 
			 WHERE (username = '%s')",
				mysql_real_escape_string($userinfo['username'])));
				
		if ($userid > 0)
		{
			$retVal->ErrorType = USER_ERRORTYPE_USERNAME;
			$retVal->ErrorMessage = 'This username is already in use.';			
			return $retVal;
		}
		
		
		// verify email
		if (strlen($userinfo['email']) == 0 || !eregi("^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,5})$", $userinfo["email"]))
		{
			$retVal->ErrorType = USER_ERRORTYPE_EMAIL;
			$retVal->ErrorMessage = 'Invalid email address.';			
			return $retVal;			
		}
		
		// test for duplicate email		
		$userid = $db->query_first(sprintf(
					"SELECT userid 
						FROM system_users 
						WHERE (email = '%s')",
					mysql_real_escape_string($userinfo['email'])));		
					
		if ($userid > 0)
		{
			$retVal->ErrorType = USER_ERRORTYPE_EMAIL;
			$retVal->ErrorMessage = 'This email is already in use.';			
			return $retVal;
		}					
		
		// verify passwords
		if (strlen($_POST["pass1"]) < 3)
		{
			$retVal->ErrorType = USER_ERRORTYPE_PASSWORD;
			$retVal->ErrorMessage = 'Passwords must be greater than 3 characters long.';			
			return $retVal;
		}
		
		if ($_POST["pass1"] != $_POST["pass2"])
		{
			$retVal->ErrorType = USER_ERRORTYPE_PASSWORD;
			$retVal->ErrorMessage = 'Passwords did not match.';			
			return $retVal;
		}		
		
		if (!ereg("^[-+]{1}[0-9]{4}",$userinfo["timezone"]) && $userinfo["timezone"] != "0")
		{
			$retVal->ErrorType = USER_ERRORTYPE_TIMEZONE;
			$retVal->ErrorMessage = 'Invalid timezone.';			
			return $retVal;			
		}			
		
		// did they agree to the user agreement?
//		if ($userinfo['agree'] != 'on')	
//		{
//			$retVal->ErrorType = USER_ERRORTYPE_LICENSE;
//			$retVal->ErrorMessage = 'You must accept the user license to join.';			
//			return $retVal;						
//		}
		
		
		// TODO: RE-ENGINEER THE DAMN BOT-NAME FIASCO
		// TODO: insert DLS
		$createquery = sprintf("INSERT INTO system_users 
			(username,email,password,timezone,bot)
			VALUES
			('%s','%s','%s','%s','%s')",
			mysql_real_escape_string($userinfo['username']),
			mysql_real_escape_string($userinfo['email']),
			md5(mysql_real_escape_string($userinfo['pass1'])),
			mysql_real_escape_string($userinfo['timezone']),
			'ciqo');
		
		$insert = $db->query($createquery);
			
		if ($db->geterrno() == 0)
		{
			// TODO: create user object?
			$retVal->UserID  = $db->insert_id();
			$retVal->Success = true;
		}		
		else
		{
			$retVal->ErrorType = USER_ERRORTYPE_GLOBAL;
			$retVal->ErrorMessage = $db->geterrdesc();			
			return $retVal;				
		}
		
		return $retVal;
	}	
	
	public static function GetUser($userid)
	{
		global $db; 
		$retVal = new Result();		
		
		if ($userid > 0)
		{
			$query = sprintf("SELECT * FROM system_users
												WHERE (userid = %d)",
							mysql_real_escape_string($userid)
							);
			
			$userinfo = $db->query_first($query);
			
			if ($db->geterrno() != 0)
			{
				$retVal->errortype = RESULT_GETUSER_ERROR;
				$retVal->errormessage = "Database Error (".$db->geterrdesc().")";	
				return $retVal;						
			}												

			
			if ($userinfo['userid'] <= 0)
			{
				$retVal->errortype = RESULT_GETUSER_ERROR;
				$retVal->errormessage = "Unknown user ($userinfo[userid])";			
				return $retVal;						
			}										
			
			$retVal->data = $userinfo;
			$retVal->success = true;
		}
				
		return $retVal;
	}
	
}



?>