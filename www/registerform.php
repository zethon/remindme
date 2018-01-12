<?php 
//-----------------------------------------------------------------------------
// $Workfile: index.php $ $Revision: 1.3 $ $Author: addy $ 
// $Date: 2009/07/04 21:31:19 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');
require_once(DIR.'/includes/class_user.php');
session_start();

// TODO: get this out of here
//require_once(DIR.'/vars.inc');

if (User::ValidateUser($_SESSION['userid'],$_SESSION['password']))
{
	$loggedin = TRUE;
}
else
{
	$loggedin = FALSE;
}

if ($loggedin)
{
	header('Location:index.php');
	exit;	
}

// move this the begining of the file at some point
function isSelected($uzone,$zone)
{
	if ($uzone == $zone)
		return "selected=true";
	else
		return "";
}

if ($_POST['register'] == 'true')
{
	if ($_POST['regpw'] != 'chapman')
	{
		$createUserRes = new UserWriteResult();
		$createUserRes->ErrorType = USER_ERRORTYPE_REGISTRATION;
		$createUserRes->ErrorMessage = "Invalid registration password. Registraion is currently invite only.";
	}
	else
	{
		$createUserRes = UserFactory::CreateUser($_POST);
	
		if ($createUserRes->Success === true)
		{
			session_register('userid');
			session_register('password');
			
			$userinfo = $db->query_first("SELECT * FROM system_users WHERE (userid = ".$createUserRes->UserID.")");
			$_SESSION['userid'] = $userinfo['userid'];
			$_SESSION['password'] = $userinfo['password'];			
	
			header("Location: index.php");
			exit;
		}
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

<?

	//echo "<div class=plaintextlarge>Registration for RemindMe is currently closed. Please check back soon.</div>";
	//exit;

	if ($createUserRes->ErrorType == USER_ERRORTYPE_GLOBAL)
		echo "<font color='red'><b>".$createUserRes->ErrorMessage."</b></font><br><br>";
		
/*	$errorHash{'A'} = 1;		
	$errorHash{'H'} = 1;		
	$errorHash{'I'} = 1;*/		
?>


<form name="mainform" action="registerform.php" method="post">
<input type="hidden" name="register"/ value="true">

<!-- USERNAME FIELD -->
<?
	//if ($errorHash{'A'} == 1 || $errorHash{'B'} == 1 || $errorHash{'C'} == 1)
	if ($createUserRes->ErrorType == USER_ERRORTYPE_USERNAME)
		echo "<table style='border: 1px solid rgb(133, 142, 157); background-color: rgb(251, 251, 231);' width=90%>";
	else
		echo "<table width=90%>";
?>	
	<tr>
		<td width=40% class=sampletext>Desired Username:</td>
		<td><input name="username" type=text value='<? echo $_POST['username']; ?>'></td>
	</tr>
<?

	if ($createUserRes->ErrorType == USER_ERRORTYPE_USERNAME)
		print "<tr><td colspan=2><font color='red'><b>".$createUserRes->ErrorMessage."</b></font></td></tr>";

//	if ($errorHash{'A'} == 1)
//		echo "<tr><td colspan=2><font color='red'><b>You did not enter a username.</b></font></td></tr>";	
//	else if ($errorHash{'B'} == 1)
//		echo "<tr><td colspan=2><font color='red'><b>The username you entered is invalid. Your username must begin with a letter and be between 3-20 letters or numbers long.</b></font></td></tr>";	
//	else if ($errorHash{'C'} == 1)
//		echo "<tr><td colspan=2><font color='red'><b>That username is already in use. Please select another username.</b></font></td></tr>";	
?>			
</table>
<!-- /USERNAME FIELD -->

<br/>

<!-- EMAIL FIELD -->
<?
if ($createUserRes->ErrorType == USER_ERRORTYPE_EMAIL)
	echo "<table style='border: 1px solid rgb(133, 142, 157); background-color: rgb(251, 251, 231);' width=50%>";
else
	echo "<table width=50%>";
?>	
<tr><td width=40% class=sampletext>E-Mail Address:</td><td><input name="email" type=text value='<? echo $_POST["email"]; ?>'></td></tr>
<?
if ($createUserRes->ErrorType == USER_ERRORTYPE_EMAIL)
	print "<tr><td colspan=2><font color='red'><b>".$createUserRes->ErrorMessage."</b></font></td></tr>";

?>
</table>
<!-- /EMAIL FIELD -->


<br>

<!-- PASSWORD FIELDS -->
<?

	if ($createUserRes->ErrorType == USER_ERRORTYPE_PASSWORD)
		echo "<table style='border: 1px solid rgb(133, 142, 157); background-color: rgb(251, 251, 231);' width=50%>";
	else
		echo "<table width=50%>";
?>	
	
<tr><td width=40% class=sampletext>Password:</td><td><input name=pass1 type=password value='<? echo $_POST['pass1']; ?>'></td></tr>
<tr><td width=40% class=sampletext>Re-Type Password:</td><td><input name=pass2 type=password value='<? echo $_POST['pass2']; ?>'></td></tr>
<?
	if ($createUserRes->ErrorType == USER_ERRORTYPE_PASSWORD)
		print "<tr><td colspan=2><font color='red'><b>".$createUserRes->ErrorMessage."</b></font></td></tr>";
?>
</table>
<!-- /PASSWORD FIELDS -->

<br>

<!-- TIMEZONE OPTIONS -->
<?
	if ($errorHash{'G'} == 1)
		echo "<table style='border: 1px solid rgb(133, 142, 157); background-color: rgb(251, 251, 231);' width=58%>";
	else
		echo "<table width=58%>";
?>	
<tr><td class=sampletext width=40%>Timezone:</td>
	<td>
		<select name="timezone">
		<option <? echo isSelected($_POST['timezone'],"-1200"); ?> value="-1200">(GMT-1200) Eniwetok, Kwajalein</option>
		<option <? echo isSelected($_POST['timezone'],"-1100"); ?> value="-1100">(GMT-1100) Midway Island, Samoa</option>
		<option <? echo isSelected($_POST['timezone'],"-1000"); ?> value="-1000">(GMT-1000) Hawaii</option>
		<option <? echo isSelected($_POST['timezone'],"-0900"); ?> value="-0900">(GMT-0900) Alaska</option>
		<option <? echo isSelected($_POST['timezone'],"-0800"); ?> value="-0800">(GMT-0800) Pacific Time (US & Canada)</option>
		<option <? echo isSelected($_POST['timezone'],"-0700"); ?> value="-0700">(GMT-0700) Mountain Time (US & Canada)</option>
		<option <? echo isSelected($_POST['timezone'],"-0600"); ?> value="-0600">(GMT-0600) Central Time (US & Canada)</option>
		<option <? echo isSelected($_POST['timezone'],"-0500"); if ($createUserRes == null) echo "selected=true";  ?> value="-0500">(GMT-0500) Eastern Time (US & Canada)</option>
		<option <? echo isSelected($_POST['timezone'],"-0400"); ?> value="-0400">(GMT-0400) La Paz, Atlantic Time (Canada)</option>
		<option <? echo isSelected($_POST['timezone'],"-0330"); ?> value="-0330">(GMT-0330) Newfoundland</option>
		<option <? echo isSelected($_POST['timezone'],"-0300"); ?> value="-0300">(GMT-0300) Beunos Aires, Greenland</option>
		<option <? echo isSelected($_POST['timezone'],"-0200"); ?> value="-0200">(GMT-0200) Mid-Atlantic</option>
		<option <? echo isSelected($_POST['timezone'],"-0100"); ?> value="-0100">(GMT-0100) Azores</option>
		<option <? echo isSelected($_POST['timezone'],"0"); ?> value="0">(GMT) Dublin, London, Lisbon </option>
		<option <? echo isSelected($_POST['timezone'],"+0100"); ?> value="+0100">(GMT+0100) Amsterdam, West Central Africa</option>
		<option <? echo isSelected($_POST['timezone'],"+0200"); ?> value="+0200">(GMT+0200) Athens, Cairo, Jerusalem</option>
		<option <? echo isSelected($_POST['timezone'],"+0300"); ?> value="+0300">(GMT+0300) Moscow, Baghdad, Kuwait</option>
		<option <? echo isSelected($_POST['timezone'],"+0330"); ?> value="+0330">(GMT+0330) Tehran</option>
		<option <? echo isSelected($_POST['timezone'],"+0400"); ?> value="+0400">(GMT+0400) Abu Dhabi, Baku</option>
		<option <? echo isSelected($_POST['timezone'],"+0430"); ?> value="+0430">(GMT+0430) Kabul</option>
		<option <? echo isSelected($_POST['timezone'],"+0500"); ?> value="+0500">(GMT+0500) Islamabad, Karachi</option>
		<option <? echo isSelected($_POST['timezone'],"+0530"); ?> value="+0530">(GMT+0530) Calcutta, New Dehli</option>
		<option <? echo isSelected($_POST['timezone'],"+0545"); ?> value="+0545">(GMT+0545) Kathmandu</option>
		<option <? echo isSelected($_POST['timezone'],"+0600"); ?> value="+0600">(GMT+0600) Novosibirsk, Dhaka</option>
		<option <? echo isSelected($_POST['timezone'],"+0700"); ?> value="+0700">(GMT+0700) Bangkok, Hanoi, Jakarta</option>
		<option <? echo isSelected($_POST['timezone'],"+0800"); ?> value="+0800">(GMT+0800) Beijing, Hong Kong, Perth</option>
		<option <? echo isSelected($_POST['timezone'],"+0900"); ?> value="+0900">(GMT+0900) Tokyo, Osaka, Seoul</option>
		<option <? echo isSelected($_POST['timezone'],"+0930"); ?> value="+0930">(GMT+0930) Adelaide</option>
		<option <? echo isSelected($_POST['timezone'],"+1000"); ?> value="+1000">(GMT+1000) Sydney, Canberra, Brisbane</option>
		<option <? echo isSelected($_POST['timezone'],"+1100"); ?> value="+1100">(GMT+1100) Magadan, Solomon Is.</option>
		<option <? echo isSelected($_POST['timezone'],"+1200"); ?> value="+1200">(GMT+1200) Auckland, Wellington, </option>
		<option <? echo isSelected($_POST['timezone'],"+1300"); ?> value="+1300">(GMT+1300) Nuku'alofa</option>
		</select>
	</td>
</tr>

<?
	if ($errorHash{'G'} == 1)
		echo "<tr><td colspan=2><font color='red'><b>You entered an invalid timezone.</b></font></td></tr>";	
?>

<tr>
	<td width=40% class=sampletext>
		Observe Daylights Savings:</td>
	<td>
		<input type=checkbox name=dls <? if ($_POST['dls'] == "on" || $createUserRes == null) echo "checked=true"; ?>>
	</td>
</tr>

</table>
<!-- /TIMEZONE OPTIONS-->

<br>

<br>

<!-- USER AGREEMANE -->
<?
if ($createUserRes->ErrorType == USER_ERRORTYPE_REGISTRATION)
		echo "<table style='border: 1px solid rgb(133, 142, 157); background-color: rgb(251, 251, 231);' width=100%>";
	else
		echo "<table width=100%>";
?>	
<tr>
<td>
<!-- <div class=plaintextlarge><input name=agree type=checkbox>I have read and agree to the <a href="javascript:openPopup(1)">RemindMe Terms and Conditions of Use</a></div>-->
Registration Password: <input type="password" name="regpw" />
</td>
</tr>
<?
if ($createUserRes->ErrorType == USER_ERRORTYPE_REGISTRATION)
		print "<tr><td colspan=2><font color='red'><b>".$createUserRes->ErrorMessage."</b></font></td></tr>";?>
</table>
<!-- /USER AGREEMENT -->


<br><br>
<input type="submit" value="Register"/>
</form>

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


<script type="text/javascript">
//at_attach("login_form_parent", "login_form_child", "click", "y", "pointer");
</script>


</body>
</html>

