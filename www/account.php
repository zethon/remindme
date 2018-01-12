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

$botinfo = $db->query_first(sprintf(
							 "SELECT * 
								FROM system_bots 
								WHERE (system_bots.name = '%s')",
								mysql_real_escape_string($userinfo['bot'])));

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

<br>

<!-- MAIN USER INFO -->
<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tbody>
	<tr>
		<td align="left" width="40%" class="tcat">
			Account Information
			&nbsp;&nbsp;
			<a href="changeinfo.php" class="button"><img src="<? print $config['Site']['url']; ?>/images/edit-info.gif" alt="Edit" class="button"/></a>&nbsp;
		</td>					
	</tr>

	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>
	
	<tr height="23">
		<td>
			Username
		</td>

		<td>
			&nbsp;<? echo $userinfo['username'] ?>
		</td>
	</tr>
	
	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>
	
<!--	<tr height="23">
		<td class=box3>
			&nbsp;&nbsp;&nbsp;Name
		</td>
		<td class=box4>
			&nbsp;<? echo $userinfo['firstname']." ".$userinfo['surname']; ?>
		</td>
	</tr>
	
	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>-->
	
	<tr height="23">
		<td>
			Email
		</td>
		<td>
			&nbsp;<? echo $userinfo['email']; ?>
		</td>
	</tr>
	
	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>
	
	<tr height="23">
		<td>
			Timezone
		</td>
		<td>
			&nbsp;
<?
	$offset = format_offset(get_timezone_offset($userinfo['timezone'],"GMT") / 3600);
	echo "($offset) $userinfo[timezone]";
			 
?>
		</td>
	</tr>
	
	<tr>
		<td colspan="3" bgcolor="#999999">
		</td>
	</tr>
</tbody>
</table>
<!-- /MAIN USER INFO -->

<hr/>

<!-- CONTACT INFO -->
<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tbody>
	<tr height="25" class=box_header>
		<td align="left" width="40%" class="tcat">
			Contact Information
			&nbsp;&nbsp;
			<a href="changeinfo.php" class="button"><img src="<? print $config['Site']['url']; ?>/images/edit-info.gif" alt="Edit" class="button"/></a>&nbsp;
		</td>					
	</tr>

	<tr>
		<td colspan="3" bgcolor="#999999">
		</td>
	</tr>	
	
<? 
	
$contacts = $db->query("
			SELECT * 
			FROM system_contacts
			LEFT JOIN service USING (serviceid)
			WHERE (userid = $userinfo[userid])
			ORDER BY description
			");
			
while ($contact = $db->fetch_array($contacts))
{
?>

	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>	

<?	

	print "
		<tr height=\"23\">
			<td>
				$contact[description]
			</td>
	
			<td>
				$contact[login] 
			</td>
		</tr>
		";
}
				
print '	
	<tr>
		<td colspan="2" bgcolor="#999999">
		</td>
	</tr>
</tbody>
</table>
';
?>
<!-- /CONTACT INFO -->

<hr/>

<!-- REMINDME BOT INFO -->

<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tbody>
	<tr height="25" class=box_header>
		<td align="left" width="100%" colspan=2 class="tcat">
			RemindMe Bot Contact Information
		</td>
	</tr>
	
<?

// get a comma seperated list of active services
$activeservices = $db->query_first("
				SELECT SUBSTRING( GROUP_CONCAT( DISTINCT serviceid ) , 1, 256 ) AS services
				FROM service
				WHERE (active=1)
				");
				
					
$botnames = $db->query("						
				SELECT * FROM botscreenname 
				LEFT JOIN service USING (serviceid) 
				WHERE (serviceid IN ($activeservices[services]))
				AND (botid = $botinfo[botid])
				AND (botscreenname.active = 1)
				ORDER BY description			
				");
				
while ($bot = $db->fetch_array($botnames))					
{
?>

	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>	

<?	
	print "
	<tr height=\"23\">
		<td width=\"40%\">
			$bot[description]
		</td>

		<td>
			$bot[screenname]
		</td>
	</tr>
	";
}
			

print '
</tbody>
</table>
';	

?>
<!-- /REMINDME BOT INFO -->

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

