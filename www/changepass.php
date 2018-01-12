<?php 
//-----------------------------------------------------------------------------
// $Workfile: index.php $ $Revision: 1.2 $ $Author: addy $ 
// $Date: 2009/07/04 21:31:19 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');

if (!$userinfo['loggedin'])
{
	header("Location: login.php");	
	exit;
}

if ($_POST[action] == "change" && ($_POST[newpw1] == $_POST[newpw2]) && VerifyUser($_COOKIE[username],md5($_POST['oldpw'])))
{
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(changepass.php) Unable to select database [$dbname])");
	$query = "UPDATE system_users SET password = '".md5($_POST[newpw2])."' WHERE (id='$_COOKIE[username]')";
	mysql_query($query);
	
	$time=time(); 
	setcookie ("username", $_COOKIE[username], $time+3200); 
	setcookie ("password", md5($_POST[newpw2]), $time+3200); 	
	$changedpw = TRUE;
}
else
{
	$chnagedpw = FALSE;
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
<!-- begin main content -->

<?
#if ($_POST[action] == "change" && ($_POST[newpw1] == $_POST[newpw2]) && VerifyUser($_COOKIE[username],md5($_POST['oldpw'])))
#{
#	mysql_connect($dbhost,$dbuser,$dbpw);
#	@mysql_select_db($dbname) or die("(changepass.php) Unable to select database [$dbname])");
#	$query = "UPDATE system_users SET password = '".md5($_POST[newpw2])."' WHERE (id='$_COOKIE[username]')";
#	mysql_query($query);
if ($changedpw == TRUE)
{
?>
	<font color=green><b>Password changed successfully</b></font><br><br>
	<a href="account.php" class=link>go back to account settings</a>
<?	
}
elseif ($_POST[action] == "change")
{
	echo "<font color=red><b>Error changing your password</b></font>";
	$changerror = TRUE;
}

if ($_POST[action] == "" || $changerror == TRUE)
{ ?>

<form name=changepass action=changepass.php method=post>
<table align=left style="font-family:arial; font-size:12; border:1 solid #000000;">
<tr><td align=right class=box3>Old Password: </td><td><input type=password name=oldpw size=15></td></tr>
<tr><td align=right class=box3>New Password: </td><td><input type=password name=newpw1 size=15></td></tr>
<tr><td align=right class=box3>Confirm: </td><td><input type=password name=newpw2 size=15></td></tr>
<tr><td colspan=2 align=right><a href="Javascript:document.changepass.submit()" class=link>save</a></td></tr>
<input type=hidden name=action value=change>
</form>
</table>

<? } ?>










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

