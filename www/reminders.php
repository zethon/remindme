<?php 
//-----------------------------------------------------------------------------
// $Workfile: reminders.php $ $Revision: 1.4 $ $Author: addy $ 
// $Date: 2009/07/18 22:01:20 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');

if (!$userinfo['loggedin'])
{
	header("Location: login.php");	
	exit;
}

if ($_REQUEST['action'] == "uncancel" && $_REQUEST['idlist'] != "")
{
	
	$ids = split(" ",$_REQUEST['idlist']);
	
	for ($i=0; $i < count($ids); $i++)
	{
		if ($ids[$i] != "")
		{
			$reminf = getReminder($ids[$i]);
			if ($reminf{'user'} == $_COOKIE['username'])
			{
				mysql_connect($dbhost,$dbuser,$dbpw);
				@mysql_select_db($dbname) or die("(reminder.php() Unable to select database [$dbname])");
				$delquery = "UPDATE system_reminders SET delivered = '0' WHERE (id='".$ids[$i]."')";		
				mysql_query($delquery);	
				mysql_close();	
				LogEvent("reminders.php: reactivating reminder (".$ids[$i].")");
			}			
		}
	}
	

}
elseif ($_REQUEST['action'] == "cancel" && $_REQUEST['idlist'] != "")
{
	$ids = split(" ",$_REQUEST['idlist']);
	
	for ($i=0; $i < count($ids); $i++)
	{
		if ($ids[$i] != "")
		{
			$reminfo = ReminderDB::GetReminder($ids[$i]);
			
			if ($reminfo['userid'] == $userinfo['userid'] || $reminfo['creatorid'] == $userinfo['userid'])
			{
				ReminderDB::DeleteReminder($ids[$i]);
			}
		}
	}
}
elseif ($_REQUEST['action'] == "del" && $_REQUEST['idlist'] != "")
{
	$ids = split(" ",$_REQUEST['idlist']);
	
	for ($i=0; $i < count($ids); $i++)
	{
		if ($ids[$i] != "")
		{
			$tempInfo = getReminder($ids[$i]);
			if ($tempInfo{'user'} == $_COOKIE['username'])
			{
				mysql_connect($dbhost,$dbuser,$dbpw);
				@mysql_select_db($dbname) or die("(reminder.php() Unable to select database [$dbname])");
				$delquery = "DELETE FROM system_reminders WHERE (id='".$ids[$i]."')";		
				mysql_query($delquery);	
				mysql_close();			
				LogEvent("reminders.php: deleted reminder (".$ids[$i].")");
			}
		}
	}
}

require_once('templates/header.php');
?>

<script LANGUAGE="JavaScript">
<!--
// Nannette Thacker http://www.shiningstar.net
function confirmSubmit()
{
	return confirm("You cannot undo this action, are you sure you want to continue?");
}

function stubClick(form)
{
	// there has to be a better way to do this
	var newval = false;
	
	for (i=0;i<form.elements.length;i++) 
	{
		var obj = form.elements[i];
		
		if (obj.type == 'checkbox' && obj.name == 'stub')
		{
			newval = obj.checked;
		}
	}
		
	for (i=0;i<form.elements.length;i++) 
	{
		var obj = form.elements[i];
		
		if (obj.type == 'checkbox' && obj.name != 'stub') 
		{
			obj.checked = newval;		
		}
	}	

}

function confirmAction(form)
{
	var retVal = "";
 	var isChecked=0;
	var field = null;

	for (i=0;i<form.elements.length;i++) 
	{
		var obj = form.elements[i];
		
		if (obj.type == 'checkbox') 
		{
			if (obj.checked == true && obj.name != 'stub') 
			{ 
				retVal += obj.value +" ";
				isChecked++; 
			}
		}
		
	}

	if (isChecked > 0)
	{
		for (i=0;i<form.elements.length;i++) 
		{
			var obj = form.elements[i];
		
			if (obj.type == 'hidden' && obj.name == 'idlist') 
				obj.value = retVal;
		}
		
		return confirm("Are you sure you want to continue?");
	}
	
	return false;
}
// -->
</script>

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
	if (strlen($dbug)>0)
		echo "<h1>[$dbug]</h1>";
?>

<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tbody>
	<tr>
		<td colspan="3" bgcolor="#999999">
		</td>
	</tr>
	<tr height="23">
		<td colspan=2 align=right>
<?	
	// active reminders
	if ($_REQUEST['view'] == 'old')
	{		
			print('<a href="reminders.php?view=pending" class="standardlink">pending reminders</a>');
	}
	else
	{
			print('<a href="reminders.php?view=old" class="standardlink">old reminders</a>');
	}
?>
		&nbsp;|&nbsp;<a href="editreminder.php?action=new" class="standardlink">create a reminder</a>
		</td>
	</tr>
	<tr>
		<td colspan="3" bgcolor="#999999">
		</td>
	</tr>
</tbody>
</table>

<br>
 
<? 
	switch ($_REQEUST['sort'])
	{
		case '0' : $sortfield = 'system_reminders.msg'; break;
		case '1' : $sortfield = 'system_reminders.servertime'; break;
		case '2' : $sortfield = 'system_users.username'; break;
		case '3' : $sortfield = 'system_repeaters.remid'; break;		
		default: $sortfield = 'system_reminders.servertime'; break;
	}
	
	$sortorder = mysql_real_escape_string($_REQUEST['order']);
	if (strlen($sortorder) <= 0)
		$sortorder = 1;
	
	if ($sortorder == 1)
	{
		$sortorderstr = 'DESC';
		$sortorder = 0;
	}
	else
	{
		$sortorder = 1;
		$sortorderstr = 'ASC';
	}
	
	
	
	// active reminders
	if ($_REQUEST['view'] == 'old')
	{
		$reminderquery = sprintf("SELECT * FROM system_reminders 
					LEFT JOIN system_repeaters ON (system_reminders.id = system_repeaters.remid)		
					LEFT JOIN system_users ON (system_users.userid = system_reminders.creatorid)
					WHERE 
						(system_reminders.userid = %d) AND
						(system_reminders.userid = system_reminders.creatorid) AND 
						(delivered = 1) 
					ORDER BY
						%s %s",
						mysql_real_escape_string($userinfo['userid']),
						mysql_real_escape_string($sortfield),
						mysql_real_escape_string($sortorderstr)
					);
	}
	else
	{
		$reminderquery = sprintf("SELECT * FROM system_reminders
					LEFT JOIN system_repeaters ON (system_reminders.id = system_repeaters.remid)		
					LEFT JOIN system_users ON (system_users.userid = system_reminders.creatorid)
					WHERE
						(system_reminders.userid = '%s') AND 
						(system_reminders.delivered = 0)
					ORDER BY 
						%s %s",
						mysql_real_escape_string($userinfo['userid']),
						mysql_real_escape_string($sortfield),
						mysql_real_escape_string($sortorderstr)
						);					
	}
	
	$reminders = $db->query($reminderquery);
	if ($db->num_rows($reminders) > 0)
	{
		if ($_REQUEST['action'] == 'viewold')
				$viewstr = '<input type="hidden" name="view" value="old">';
			//$viewstr = '<input type="hidden" name="view" value="old">';
		
		print "
		
		<form name=personpending method=post action=reminders.php>
			$viewstr
			<input type=hidden name=action value=cancel>
			<input type=hidden name=idlist>		
		<table cellspacing=0 cellpadding=2 width=100%>
			<tr>
				<td align=\"left\" class=\"tcat\" colspan=\"4\">
					Pending Reminders
				</td>					
			</tr>
			
			<tr>
				<td>&nbsp;</td>
				<td><a href='reminders.php?view=$_REQUEST[view]&sort=1&order=$sortorder' class=\"standardlink\">Scheduled Devlivery</a></td>				
				<td><a href='reminders.php?view=$_REQUEST[view]&sort=0&order=$sortorder' class=\"standardlink\">Message</a></td>
				<td><a href='reminders.php?view=$_REQUEST[view]&sort=2&order=$sortorder' class=\"standardlink\">Creator</a></td>
				<td><a href='reminders.php?sort=3&order=$sortorder' class=\"standardlink\">Repeats?</a></td>
				<td>&nbsp;</td>
			</tr>
			";
	
		$i = 0; $col = 0;
		while ($reminder = $db->fetch_array($reminders))	
		{
			if ($col == 0) { $box = "box4"; $col = 1; }
			else { $box = "box3"; $col = 0; }
		
?>		
			<tr class="<? echo "$box"; ?>" onmouseover="this.className='rowY'" onmouseout="this.className='<? echo "$box"; ?>'">
				<!--<td>
					<a href="editreminder.php?action=edit&remid=<? echo $reminder['id']; ?>" class="standardlink"><? echo $reminder['id']; ?></a>
				</td>-->
				
				<td>
					<a href="editreminder.php?action=modify&remid=<? echo $reminder['id']; ?>" class="button">
						<img src="<? print $config['Site']['url']; ?>/images/edit-button-small.png" alt="Edit"/>
					</a>
				</td>
				<td>
					<? echo $reminder['datetime']; ?>
				</td>				
				<td>
					<? echo $reminder['msg']; ?>
				</td>
				<td>
					<? echo $reminder['username']; ?>
				</td>
				<td>
<?
		if ($reminder['repid'] > 0)
		{
			print ("Repeats");
		}
		else
		{
			print ("Does not repeat");
		}					
?>					
				</td>
				<td width=2% align="right"><input type=checkbox name="idnum" value='<? echo $reminder['id']; ?>'></td>
			</tr>
<?
			++$i;	
		}
		echo "
	<tr class=box_header>
		<td colspan=5 align=right>
			<a onclick='return confirmAction(document.personpending)' href='Javascript:document.personpending.submit()' class=link><img src=\"".$config['Site']['url']."/images/delete-button.png\" alt=\"Delete\" /></a>
		</td>
	</tr>";
		echo "</table></form><br><br>";
	}
?>

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