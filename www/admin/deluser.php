<?php 
//-----------------------------------------------------------------------------
// $Workfile: deluser.php $ $Revision: 1.2 $ $Author: addy $ 
// $Date: 2009/07/04 21:31:19 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
include "../vars.inc";

echo "<!--action::[".$_REQUEST["action"]."]-->\n";
echo "<!--password::[".$_REQUEST["pwd"]."]-->\n";

if ($_REQUEST["action"] == "del" && $_REQUEST["pwd"] == "")
{
	//$username = strtolower($_REQUEST["usern"]);
	
	$users = split(",",strtolower($_REQUEST["usern"]));

	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die( "Unable to select database ($dbname)");

  foreach ($users as $username)
  {
		$query = "DELETE FROM system_users WHERE (id = '$username')";
		mysql_query($query);
		$users_count += mysql_affected_rows();
		
		$query = "DELETE FROM system_contacts WHERE (userid = '$username')";
		mysql_query($query);
		$contacts_count += mysql_affected_rows();
		
		$query = "DELETE FROM system_allow_lists WHERE (buddy = '$username') OR (owner = '$username')";
		mysql_query($query);
		$allow_count += mysql_affected_rows();
		
		$query = "DELETE FROM system_reminders WHERE (user = '$username')";
		mysql_query($query);
		$reminders_count += mysql_affected_rows();
	}		

	mysql_close();
	
	if ($users_count == 0)
	{
		$errortxt = 'Unknown user';
	}
}


?>

<H2>Delete User</H2>
<br>
<form name=userdel action=deluser.php method=post>
<b>Password:</b><input type=password name=pwd><br>
<b>Username:</b><input type=text name=usern><br><br>

<input type=hidden name=action value=del>
<input type=submit>
</form>

<? 
	if ($users_count > 0)
	{
		echo"
<hr>
Username: (".$_REQUEST["usern"].")<br>
system_user rows affected: $users_count<br>
system_contacts rows affected: $contacts_count<br>
system_allow_lists rows affected: $allow_count<br>
system_reminders rows affected: $reminders_count<br>
<br>
<b>Repeaters table not cleaned!</b>
		";	
	}
	else if (strlen($errortxt) > 0)
	{
		echo "<b><font color='red'>$errortxt</font></b>";
	}
?>