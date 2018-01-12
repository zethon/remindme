<?php 
//-----------------------------------------------------------------------------
// $Workfile: index.php $ $Revision: 1.4 $ $Author: addy $ 
// $Date: 2009/07/17 22:24:29 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');

if (!$userinfo['loggedin'])
{
	header("Location: login.php");	
	exit;
}

function isValidServiceName($service,$name)
{
	if (strlen($name)==0)
		return true;
		
	if ($service == 'aim' || $service == 'yahoo')
		return eregi("^[a-zA-Z][A-Za-z0-9_]{2,16}$",$name);
	else if ($service == 'msn' || $service == 'email')
		return eregi("^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,3})$",$name);
	else if ($service == 'icq')
		return eregi("^[0-9]{5,16}$",$name);
}

function isSelected($uzone,$zone)
{
	if ($uzone == $zone)
		return "selected=true";
	else
		return "";
}

function enteredInfo($one,$two)
{
	if ($_POST['action'] == "")
		return $two;
		
	return $one;	
}


$errorstr = "";

if ($_POST['action'] == 'saveinfo')
{
	if ($userinfo['userid'] != $_POST['userid'])
	{
		// hack attempt!
		header('Location: index.php');
		exit;			
	}
	
	$saveUserRes = UserFactory::SaveUser($_POST);
	if ($saveUserRes->Success === true)
	{
		header("Location: account.php");
		exit;		
	}
	
	
}

// create the error hash from register.php
$errorArray = split(":",$errorstr);
for ($i=0; $i < count($errorArray); $i++)
	$errorHash{$errorArray[$i]} = 1;
	
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

<!-- ACCOUNT MENU -->
<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tbody>
	<tr>
		<td colspan="3" bgcolor="#999999">
		</td>
	</tr>
	<tr height="23">
		<td colspan=2 align=right>
			&nbsp;&nbsp;&nbsp;<a href="editreminder.php?action=new" class="standardlink">create a reminder</a>&nbsp;|&nbsp;<a href="changepass.php" class="standardlink">change password</a>&nbsp;|&nbsp;<a href="allowlist.php" class="standardlink">allow list</a>
		</td>
	</tr>
	<tr>
		<td colspan="3" bgcolor="#999999">
		</td>
	</tr>
</tbody>
</table>
<!-- /ACCOUNT MENU -->

<?
	//$user = GetUserHash($_COOKIE['username']);
	//$ct_hash = getContactHash($_COOKIE['username']);

//	if ($errorstr != "")
//		echo "<font color='red'><b>Please correct the highlighted errors blow and try again.</b></font><br><br>";

	if ($saveUserRes != null && $saveUserRes->Success != true)
	{
		echo "<div class=\"errorinfo\">".$saveUserRes->ErrorMessage."</div><br>\n";
	}
		
?>

<form name="mainform" action="changeinfo.php" method="post">
<input type="hidden" name="action" value="saveinfo" />
<input type="hidden" name="userid" value="<? echo $userinfo['userid']; ?>" />

<!-- USERNAME TABLE -->
<table width="100%">
	<tr>
		<td align="left" colspan="2" class="tcat">
			Account Information
		</td>					
	</tr>	
	
	<tr bgcolor="#c4c4c4" height="1" width="100%">
		<td colspan="2">
		</td>
	</tr>	
	
	<tr>
		<td width=40% class=sampletext>Username:</td>
		<td><input name=user type=text readonly=true value='<? echo $userinfo['username']; ?>'></td>
	</tr>
<!-- /USERNAME TABLE-->

	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>	

<!-- EMAIL FIELD -->
<?
if ($saveUserRes->ErrorType == USER_ERRORTYPE_EMAIL)
{
	echo "<tr style='border: 1px solid rgb(133, 142, 157); background-color: rgb(251, 251, 231);' width=100%>";
	print "<tr><td colspan=2><font color='red'><b>".$saveUserRes->ErrorMessage."</b></font></td></tr>";
}
else
{
	echo "<tr width=100%>";
}
?>	
<td width=40% class=sampletext>E-Mail Address:</td>
<td><input name=email type=text value='<? echo $userinfo['email']; ?>'></td>
</tr>
<!-- /EMAIL FIELD -->

<tr bgcolor="#c4c4c4" height="1">
	<td colspan="2">
	</td>
</tr>	

<!-- TIMEZONE OPTIONS -->
<?
	if ($errorHash{'G'} == 1)
	{
		echo "<tr style='border: 1px solid rgb(133, 142, 157); background-color: rgb(251, 251, 231);' width=58%>";
		echo "<tr><td colspan=2><font color='red'><b>You entered an invalid timezone.</b></font></td></tr>";	
	}
	else
	{
		echo "<tr width=58%>";
	}
?>	
<td class=sampletext width=40%>Timezone:</td>
<td>
	<select name="timezone">
<?

$tzlist = timezone_identifiers_list();
foreach ($tzlist as $tz)
{
	$offset = format_offset(get_timezone_offset($tz,"GMT") / 3600);
	
	$selected = '';
	if ($userinfo['timezone'] == $tz)
		$selected = 'selected';
	
	print ("<option value=\"$tz\" $selected >($offset) $tz</option>\r\n");
}

?>	
		
<!--	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-1200"); ?> value="-1200">(GMT-1200) Eniwetok, Kwajalein</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-1100"); ?> value="-1100">(GMT-1100) Midway Island, Samoa</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-1000"); ?> value="-1000">(GMT-1000) Hawaii</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0900"); ?> value="-0900">(GMT-0900) Alaska</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0800"); ?> value="-0800">(GMT-0800) Pacific Time (US & Canada)</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0700"); ?> value="-0700">(GMT-0700) Mountain Time (US & Canada)</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0600"); ?> value="-0600">(GMT-0600) Central Time (US & Canada)</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0500"); ?> value="-0500">(GMT-0500) Eastern Time (US & Canada)</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0400"); ?> value="-0400">(GMT-0400) La Paz, Atlantic Time (Canada)</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0330"); ?> value="-0330">(GMT-0330) Newfoundland</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0300"); ?> value="-0300">(GMT-0300) Beunos Aires, Greenland</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0200"); ?> value="-0200">(GMT-0200) Mid-Atlantic</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"-0100"); ?> value="-0100">(GMT-0100) Azores</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"0"); ?> value="0">(GMT) Dublin, London, Lisbon </option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0100"); ?> value="+0100">(GMT+0100) Amsterdam, West Central Africa</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0200"); ?> value="+0200">(GMT+0200) Athens, Cairo, Jerusalem</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0300"); ?> value="+0300">(GMT+0300) Moscow, Baghdad, Kuwait</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0330"); ?> value="+0330">(GMT+0330) Tehran</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0400"); ?> value="+0400">(GMT+0400) Abu Dhabi, Baku</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0430"); ?> value="+0430">(GMT+0430) Kabul</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0500"); ?> value="+0500">(GMT+0500) Islamabad, Karachi</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0530"); ?> value="+0530">(GMT+0530) Calcutta, New Dehli</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0545"); ?> value="+0545">(GMT+0545) Kathmandu</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0600"); ?> value="+0600">(GMT+0600) Novosibirsk, Dhaka</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0700"); ?> value="+0700">(GMT+0700) Bangkok, Hanoi, Jakarta</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0800"); ?> value="+0800">(GMT+0800) Beijing, Hong Kong, Perth</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0900"); ?> value="+0900">(GMT+0900) Tokyo, Osaka, Seoul</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+0930"); ?> value="+0930">(GMT+0930) Adelaide</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+1000"); ?> value="+1000">(GMT+1000) Sydney, Canberra, Brisbane</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+1100"); ?> value="+1100">(GMT+1100) Magadan, Solomon Is.</option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+1200"); ?> value="+1200">(GMT+1200) Auckland, Wellington, </option>
	<option <? echo isSelected(enteredInfo($_POST[timezone],$userinfo['timezone']),"+1300"); ?> value="+1300">(GMT+1300) Nuku'alofa</option>-->
	</select>
</td>
</tr>

<!--
<tr bgcolor="#c4c4c4" height="1">
	<td colspan="2">
	</td>
</tr>	

<tr>
	<td width=40% class=sampletext>
		Observe Daylights Savings:</td>
	<td>
		<input type="checkbox" name="DLS" <? if ($userinfo['DLS'] == '1') echo "checked = true"; ?> />
	</td>
</tr>
-->

</table>
<!-- /TIMEZONE OPTIONS-->

<br>

<?

$svclist = $db->query("
			SELECT SUBSTRING( GROUP_CONCAT( DISTINCT serviceid ) , 1, 256 ) AS services
			FROM service
			WHERE (active = 1)
			");
			
$svcarray = split(',',$svclist);




$contacts = $db->query("
			SELECT * 
			FROM system_contacts
			LEFT JOIN service USING (serviceid)
			WHERE (userid = $userinfo[userid])
			ORDER BY description
			");
			
			
// create a services info hash
$svchash = array();	
$services = $db->query("SELECT * FROM service ORDER BY description");
while ($svcinfo = $db->fetch_array($services))
{
	$id = $svcinfo['serviceid'];
	$svchash[$id] = $svcinfo;
}
	
?>
	
<table bgcolor="white" width="100%">	
	<tr>
		<td align="left" colspan="2" class="tcat">
			Contact Information
		</td>					
	</tr>	
		
	
<?	
// PRINT OUT THE USER'S CONTACT INFO	
// TODO: this 3 should not be hard coded			
for ($i = 0; $i < 3; $i++)
{
	$contact = $db->fetch_array($contacts);
	$contserviceid = $contact['serviceid'];
	
?>
	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>	

<tr>
	<td colspan=2 class=box_header1>
		Contact&nbsp;&nbsp;&nbsp;&nbsp;<small><a href="javascript:openPopup(2)" class="smalllink">What's this?</a></small>
	</td>
</tr>
	<?
	if ($errorHash{'J'} == 1 || $errorHash{'K'} == 1)
		echo "<tr style='background-color: rgb(251, 251, 231);'>";
	else
		echo "<tr class=box3>";
?>	
	<td>Service&nbsp;<a href="javascript:openPopup(3)"><b>?</b></a></td><td>Login Name&nbsp;<a href="javascript:openPopup(4)"><b>?</b></a><br></td>
</tr>


<?
	//if ($svchash[$contserviceid]['active'] != '1')
	if ($saveUserRes->ErrorType == USER_ERRORTYPE_CONTACT && $saveUserRes->Data == 4)
	{
		print "<tr style='background-color: rgb(251, 251, 231);'><td colspan=2><font color='red'><b>".$saveUserRes->ErrorMessage."</b></font></td></tr>";
	}
?>

<tr>
	<td>
		<select name="service<? print $i; ?>">
<?
	
// TODO: MOVE THIS QUERY OUT OF THE FOR-LOOP!
$services = $db->query("SELECT * FROM service ORDER BY description");
while ($serviceinfo = $db->fetch_array($services))
{
	$disabled = $serviceinfo['active'] == '1' ? '' : 'disabled="true"';
	$selected = $contact['serviceid'] === $serviceinfo['serviceid'] ? 'selected="true"' : '';

	print "<option value=\"$serviceinfo[serviceid]\" $selected $disabled>$serviceinfo[description]</option>\r\n";	
}	

?>		
		</select>
	</td>
	<td>
		<input type="text" maxlength="256" name="login<? print $i; ?>" style="WIDTH: 250px" value="<? print $contact['login']; ?>">
	</td>
</tr>

<?
		
	//else if ($errorHash{'K'} == 1)
	//	echo "<tr style='background-color: rgb(251, 251, 231);'><td colspan=2><font color='red'><b>Your first contact cannot be blank. Please enter your first method of contact.</b></font></td></tr>";
?>

</td></tr>

<? 

}


?>

</table>

<br>

<input type="submit" value="Save"/>
</form>
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