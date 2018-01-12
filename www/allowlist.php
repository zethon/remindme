<?php 
//-----------------------------------------------------------------------------
// $Workfile: allowlist.php $ $Revision: 1.3 $ $Author: addy $ 
// $Date: 2009/07/04 21:31:19 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');

if (!$userinfo['loggedin'])
{
	header("Location: login.php");	
	exit;
}

if ($_POST['action'] == 'sendreminder')
{
	header("Location: editreminder.php?action=new&touser=".$_POST['touser']);	
	exit;
}
else if ($_POST['action'] == 'deluser')
{
	$newbuddy = GetUserHash($_POST['delbuddy']);

	if ($newbuddy == "")
		$errormsg = "Invalid Username";
	else
	{
		mysql_connect($dbhost,$dbuser,$dbpw);
		@mysql_select_db($dbname) or die("(allowlist.php() Unable to select database [$dbname])");
		mysql_query("DELETE from system_allow_lists WHERE (buddy = '".SafeQueryData($_POST['delbuddy'])."' AND owner = '".SafeQueryData($_COOKIE['username'])."')");
		mysql_close();
	}
}
else if ($_POST['action'] == 'adduser')
{
	$newbuddy = GetUserHash($_POST['newbuddy']);

	if ($newbuddy == "")
		$errormsg = "Invalid Username";	
	else
	{
		mysql_connect($dbhost,$dbuser,$dbpw);
		@mysql_select_db($dbname) or die("(allowlist.php() Unable to select database [$dbname])");
	
		// see how many buddies this user already has	
		$buddyq = mysql_query("SELECT * FROM `system_allow_lists` WHERE (owner = '".SafeQueryData($_COOKIE['username'])."')");
		$buddycount = mysql_numrows($buddyq);

		// and see if this buddy is already on the user's allow list
		$result = mysql_query("SELECT * FROM `system_allow_lists` WHERE (owner = '".SafeQueryData($_COOKIE['username'])."' and buddy='".SafeQueryData($_POST['newbuddy'])."')");
		$num=mysql_numrows($result);
		
		if ($num == 0 && $buddycount < 50)
		{
			$newquery = "INSERT INTO system_allow_lists (owner,buddy,num_allowed) ".
			$newquery.= " VALUES ('".SafeQueryData($_COOKIE['username'])."','".SafeQueryData($_POST['newbuddy'])."','5')";
			$result = mysql_query($newquery);			
		}
		mysql_close();				
	}
}

require_once('templates/header.php');
?>

<body>

<div id="outerContainer">
	
<table width="100%" id="innerContainer" cellpadding="0" cellspacing="0">
	<tr>
		<td id="toptable" align="center" valign="top">
<?
require_once('templates/logo.php');
?>
		</td>
	</tr>
	
	<tr>
	    <td id="middletable">
<?
require_once('templates/navbar.php');
?>
            	  
			<br/>            	    
	    
	        <table cellpadding="0" cellspacing="0">
	            <tr>
                    <td id="sideMenu">
<?
require_once('templates/sidebar.php');
?>
                    </td>	  
                    
                    <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>          
	            
	                <td id="mainContent">
<!-- begin main info -->

<p class=plaintext>RemindMe users that are on your "Allow List" are allowed to create reminders
on your behalf.</p>

<?
	if (strlen($errormsg) > 0)
		echo "<div class=warn>$errormsg</div>";
?>

<!-- ALLOW LIST -->
<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tbody>

	<tr width=100%>
		<td class=box4 align=left>
			<div class=plaintextlarge>Users who can remind you&nbsp;&nbsp;</div>
			<form method=post action=allowlist.php name=deluser>
			<input type=hidden name=action value=deluser>
			<select name="delbuddy" size=5 style="WIDTH: 200px">
<?
	// ** get buddies from database and print them
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(createReminder.php() Unable to select database [$dbname])");
	
	$result = mysql_query("SELECT * FROM `system_allow_lists` WHERE (owner = '".SafeQueryData($_COOKIE['username'])."')");
	$num=mysql_numrows($result);
	
	$i = 0; 
	while ($i < $num)
	{	
		$buddy = $datetime = mysql_result($result,$i,"buddy");
		echo "<option value='$buddy'>$buddy</option>";
		$i++;
	}
	
	mysql_close();	
?>
			</select>
			<br>
			<a href="Javascript:document.deluser.submit()" class=link>remove username</a>&nbsp;&nbsp;
			</form>
		</td>
		
		

		<td align=left>
			<div class=plaintextlarge>Your Reminder Contacts&nbsp;&nbsp;</div>
			<form method=post action=allowlist.php name=sendrem>
			<input type=hidden name=action value=sendreminder>	
			<select size=5 style="WIDTH: 200px" name=touser>		
<?
	// ** get buddies from database and print them
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(createReminder.php() Unable to select database [$dbname])");
	
	$result = mysql_query("SELECT * FROM `system_allow_lists` WHERE (buddy = '".SafeQueryData($_COOKIE['username'])."')");
	$num=mysql_numrows($result);
	
	$i = 0; 
	while ($i < $num)
	{	
		$buddy = $datetime = mysql_result($result,$i,"owner");
		echo "<option value='$buddy'>$buddy</option>";
		$i++;
	}
	
	mysql_close();	
?>			
			</select>
			<br>
			<a href="Javascript:document.sendrem.submit()" class=link>send this user a reminder</a>&nbsp;&nbsp;
			</form>
		</td>		
	</tr>

	<tr>
		<td>&nbsp;</td>
	</tr>

	<tr height="23" valign=top>
		<td>
			<form method=post action=allowlist.php name=adduser>
			<input type=hidden name=action value=adduser>
						<input type=text maxlength=20 name=newbuddy>&nbsp;<a href="Javascript:document.adduser.submit()" class=link>add user</a>
			</form>
		</td>
		<td>&nbsp;</td>
	</tr>




</tbody>
</table>
<!-- /ALLOW LIST -->

<br>

<!-- end main content -->
	                </td>
	            </tr>
	        </table>
	    </td>
	</tr>
	
	<tr>
		<td id="bottomtable" align="center" valign="top">
		&nbsp;
		</td>
	</tr>
</table>

</div>

</body>
</html>


