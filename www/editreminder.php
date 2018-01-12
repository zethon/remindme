<?php 
//-----------------------------------------------------------------------------
// $Workfile: editreminder.php $ $Revision: 1.7 $ $Author: addy $ 
// $Date: 2009/07/18 22:01:20 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');

if (!$userinfo['loggedin'])
{
	header("Location: login.php");	
	exit;
}

// init this fucker to null, we use it later
$saveReminderResult = null;

if ($_REQUEST['docreate'] == 'true')
{
	$repeaterinfo['repeaterstr'] = RepeaterDB::GetReapeaterString($_POST);
	$repeaterinfo['expiration'] = $_POST['input_expiration']. " 00:00:00";
	
	$saveReminderResult = ReminderDB::CreateReminder($userinfo['userid'],$_POST['touser'],$_POST['message'],$_POST['input_date'],$_POST['input_time'],$repeaterinfo);	
		
	if ($saveReminderResult->success)
	{
		// just to be safe, delete any reminders with this reminderid
		ReminderDB::DeleteRepeaters($saveReminderResult->data);
		
		// redirect us to the reminder list page
		header('Location: reminders.php');
		exit;			
	}
}
elseif ($_REQUEST['domodify'] == 'true')
{
	$repeaterstr = RepeaterDB::GetReapeaterString($_POST);
	$saveReminderResult = ReminderDB::SaveReminder($_POST,$repeaterinfo);
	
	if ($saveReminderResult->success)
	{
		// redirect us to the reminder list page
		header('Location: reminders.php');
		exit;			
	}
}

//$errormsg = $_REQUEST['errormsg'];
//$confmsg = $_REQUEST['confirm'];
//
//// hack check
//if (($_GET['action'] == 'edit' || $_POST['action'] == 'modify') && $_REQUEST['remid'] > 0)
//{
//	$reminderinfo = $db->query_first(sprintf(
//						"SELECT * 
//						FROM system_reminders 
//						WHERE (id = %d);",
//					mysql_real_escape_string($_REQUEST['remid'])
//						));
//	
//	if ($reminderinfo['userid'] != $userinfo['userid'] && $reminderinfo['creator'] != $userinfo['userid'])
//	{
//		//LogEvent("editreminder.php: HACK WARNING 1:".$_REQUEST['remid'].":".$reminderinfo['user'].":".$userinfo['username']);
//		header("Location: index.php");
//		exit;
//	}
//}
//
//
//if ($_POST['action'] == 'create' || $_POST['action'] == 'modify')
//{
//	if (empty($_POST['input_date']))
//	{
//		$errormsg = "You did not enter a date.";	
//		setcookie("ReminderError",'Invalid date/time',time()+10);
//		header("Location: editreminder.php?action=edit&remid=".$_POST['remid']."&errormsg=".$errormsg);	
//		exit;
//	}
//	else if ($_POST['input_time'] == "")
//	{
//		$errormsg = "You did not enter a time.";	
//		header("Location: editreminder.php?action=edit&remid=".$_POST['remid']."&errormsg=".$errormsg);	
//		exit;
//	}
//	else if ($_POST['message'] =="")
//	{
//		$errormsg = "You did not enter a message.";
//		header("Location: editreminder.php?action=edit&remid=".$_POST['remid']."&errormsg=".$errormsg);	
//		exit;
//	}
//
//
//	$datestring = $_POST['input_date']." ".$_POST['input_time'];
//	$datestring = str_replace("/","-",$datestring);
//	
//	if ($_POST['action'] == 'create')
//	{
//		// add field to accept the touser name of the reminder
//		// createreminder will look for the touser's bot
//		if (($_POST['remtype'] == 'once') || ($_POST['remtype'] != 'once' && strlen(getReapterString($_POST)) > 0))
//		{
//			$result = ReminderDB::CreateReminder($userinfo['username'],$datestring,$_POST['message'],$touser);	
//			
//			if ($result->success)
//				ReminderDB::DeleteRepeaters($remid);
//		}
//		
//		if ($_POST['remtype'] == "daily" || $_POST['remtype'] == "monthly" || $_POST['remtype'] == "weekly")
//		{
//
//			$repeaterstring = getReapterString($_POST);
//			if (strlen($repeaterstring) > 0)
//			{
//				CreateRepeater($remid,$repeaterstring,FALSE,$_POST["input_expiration"]);				
//			}
//			else
//			{
//				$errormsg = "Your 'Frequency' info was invalid.";
//				header("Location: editreminder.php?action=edit&remid=".$_POST['remid']."&errormsg=".$errormsg);	
//				exit;				
//			}
//		}
//		
//		//LogEvent("editreminder.php: Created Reminder ($remid)");
//		header("Location: editreminder.php?action=edit&remid=$remid&confirm=Reminder created.");	
//		exit;
//	}
//	elseif ($_POST['action'] == 'modify')
//	{
//		$remid = $_POST['remid'];
//		
//		if ($_POST['remtype'] == "once")
//		{
//			DeleteRepeaters($remid);	
//		}
//		else if ($_POST['remtype'] == "daily" || $_POST['remtype'] == "monthly" || $_POST['remtype'] == "weekly")
//		{
//			$repeaterstring = getReapterString($_POST);
//			$dbug = "[".$repeaterstring."]";
//			
//			if (strlen($repeaterstring) > 0)
//			{
//				if (strlen($_POST['repid']) > 0)
//				{
//					ModifyRepeater($_POST['repid'],$repeaterstring,FALSE,$_POST["input_expiration"]);
//				}
//				else
//				{
//					CreateRepeater($remid,$repeaterstring,FALSE,$_POST["input_expiration"]);		
//				}
//			}
//			else
//			{
//				$errormsg = "Your 'Frequency' info was invalid.";
//				header("Location: editreminder.php?action=edit&remid=".$_POST['remid']."&errormsg=".$errormsg);	
//				exit;
//			}
//		}
//			
//		//LogEvent("editreminder.php: Modified Reminder ($remid)");
//		editReminder($touser,$userinfo['username'],$_POST['remid'],$datestring,$_POST['message']);	
//		header("Location: editreminder.php?action=edit&remid=".$_POST['remid']."&confirm=Reminder saved");
//		exit;
//	}
//}

//$userinf = GetUserHash($_COOKIE['username']);
//$planinf = getPlanHash($userinf{'plan']);

$isnew = ($_GET['action'] == 'new');

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

<table width=100% boder=1>

<?
if ($_REQUEST['action'] == 'new')
{
?>
	<tr>
		<td class="tcat">
			Create Reminder
		</td>
	</tr>
	<tr>
		<td>
			
		</td>
	</tr>
	<tr>
		<td sytle="border:>&nbsp;</td>
	</tr>
	
<tr>
	<td align="left" width="100%">	
	
<div id="wrap">

<div id="grey1" class="tpop">
	<div>
		Quick Add
	</div>
</div>

</div>

<div id="pop1" class="apop">
	<div>
		<input style="width:90%; align:left;" name="quickAddString" />&nbsp;<img src="<? print $config['Site']['url']; ?>/images/add-icon.png"/>
		<br/>
		Example: remind me at 12pm tomorrow to have lunch with Frank
	</div>
</div>

	</td>
</tr>

	
<?
}
else if ($_REQUEST['action'] == 'edit')
{
	echo "
	<tr>
		<td class=\"tcat\">
			Edit Reminder
		</td>
	</tr>";
}

if ($saveReminderResult != null && !$saveReminderResult->success)
{
	echo "
	<tr>
		<td class=\"errorinfo\">
			".$saveReminderResult->errormessage."
		</td>
	</tr>\n";
}
?>

<form name="reminder" method="post" action="editreminder.php">
<? 
	$datestring = null;

	if ($_REQUEST['action'] == 'new')
	{
		echo "<input type=\"hidden\" name=\"docreate\" value=\"true\">\n";
		echo "<input type=\"hidden\" name=\"action\" value=\"new\">\n";
		$remtype_once = "checked";		
	}
	else if ($_REQUEST['modify'] == 'edit')
	{
		echo "<input type=\"hidden\" name=\"domodify\" value=\"true\">\n";
		echo "<input type=\"hidden\" name=\"action\" value=\"modify\">\n"; 
		echo "<input type=\"hidden\" name=\"remid\" value=\"$reminderinfo[id]\">\n"; 		
		
		//$datestring = $reminderinfo['datetime'];
	}
	
	if ($doCreateResult != null && $doCreateResult->success === false)
	{
		
	}

//	if ($_GET['action']=='new') 
//	{
//		echo "<input type=hidden name=action value=create>\n";
//		$remtype_once = "checked";
//	} 
//	else 
//	{ 
//		echo "<input type=hidden name=action value=modify>\n"; 
//		echo "<input type=hidden name=remid value=".$_GET['remid'].">\n"; 
//		
//		
//		list($date,$time) = split(" ",$reminderinfo['datetime']);
//		
//		$date = str_replace("-","/",$date);
//		$dateval = "value='$date'";
//		//$timeval = "value='$time'";
//		
//		// calculate int time val
//		list($hour,$min,$seconds) = split(":",$time);
//		$timeval = "value=\"$hour:$min\"";
//		$inttimeval = $hour * 100 + $min;		
//				
//		// TODO (ALPHA): BAD BAD BAD!!! GET THAT $_GET OUT OF THE QUERY!
//		$repeatinfo = $db->query_first("SELECT * FROM system_repeaters WHERE (remid = '$_GET[remid]');");
//		if (strlen($repeatinfo['repid']) > 0)
//		{
//			echo "<input type=hidden name=repid value=".$repeatinfo['repid'].">\n"; 
//			$patternarray = split(":", $repeatinfo['pattern'], 5);
//			
//			switch (substr($repeatinfo['pattern'],0,1))
//			{
//				case "d":
//					$remtype_daily = "checked";
//					$temparray = split("{", $repeatinfo['pattern'], 5);
//					switch ($temparray[1])
//					{
//						case "w}":
//							$daily_w = "checked";
//						break;
//						case "we}":
//							$daily_we = "checked";
//						break;
//						default:
//							$daily_dx = "checked";
//							$xdays_text = $temparray[1];
//							$xdays_text = substr($xdays_text,0,-1);
//						break;	
//					}
//				break;
//				
//				case "w":
//					$remtype_weekly = "checked";
//					$xweeks_text = substr($patternarray[0],2);
//					$daystr = substr($patternarray[1],0,-1); 
//					
//					for ($i=0;$i<7;$i++)
//					{
//						$number = strstr($daystr,"$i");
//						if ($number)
//						{
//							$daysarray[$i] = "checked";
//						}
//					}
//				break;
//				
//				case "m":
//					$remtype_monthly = "checked";
//					
//					if ($patternarray[0] == "m{a")
//					{
//						$monthly_a = "checked";
//						$daynum_text = $patternarray[1];
//						$monthnum_text = substr($patternarray[2],0,-1);
//					}
//					else if ($patterns[0] = "m{b")
//					{
//						$monthly_b = "checked";
//						$xmonths_text = substr($patternarray[3],0,-1);
//						$occurence{"$patternarray[1]"} = "selected";
//						$whichday{"$patternarray[2]"} = "selected"; 
//					}
//				break;	
//			}
//		}
//		else // no repeater
//		{
//			$remtype_once = "checked";
//		}
//	} 
	


	if (strlen($confmsg) > 0)
	{
		echo "<font color=green><b>$confmsg</b></font><br><br>\n";
	}

	
	if (strlen($dbug) > 0)
	{
		echo "<h1>[$dbug]</h1>";
	}	



	if (strlen($reminderinfo['creator']) > 0 && $reminderinfo['creator'] != $userinfo['username'])
	{
		echo "<div class=plaintext><b>Reminder created by:</b> ".$reminderinfo['creator']."</div><br>";
	}
?>

</table>


<table bgcolor=white width=100%>
<?
	// ** get buddies from database and print them
	//mysql_connect($dbhost,$dbuser,$dbpw);
	//@mysql_select_db($dbname) or die("(editreminder.php() Unable to select database [$dbname])");
	
	//$result = mysql_query("SELECT * FROM `system_allow_lists` WHERE (buddy = '".SafeQueryData($_COOKIE['username'])."')");
	//$num=mysql_numrows($result);
	//$buddies = $db->query("SELECT * FROM `system_allow_lists` WHERE (buddy = '$userinfo[username]')");
	$buddies = $db->query("SELECT *
												FROM system_allow_lists
												LEFT JOIN system_users ON (buddyid = userid)
												WHERE (ownerid = $userinfo[userid]);
							");
							

	print "	
	<tr>
		<td class=plaintext>
			Send this reminder to:<br>
			<select name=\"touser\" style='WIDTH: 200px'>
			<option value=\"$userinfo[userid]\">myself</option>";
			
	while ($buddy = $db->fetch_array($buddies))				
	{
		if (($_REQUEST['action'] == 'edit' && $reminderinfo['userid'] == $buddy['userid'])
					|| ($_REQUEST['action'] == 'new' && $_REQUEST['touserid'] == $buddy['userid']))
		{
			$checked = 'selected';
		}
		else
		{
			$checked = '';
		}
		
		print "<option value=\"$buddy[userid]\" $checked>$buddy[username]</options>";
	}
	
print "			
			</select><br><br>
		</td>
	</tr>";
?>				

	
	
	<tr>
		<td class=plaintext width=15%>Start Date:</td>
		<td class=plaintext>Start Time:</td>
	</tr>
	<tr>
		<!-- Start Date Input -->
		<td>
			<input id="input_date" name="input_date" value="<? echo $_POST['input_date']; ?>" />
			<script>
			    Calendar.setup({
			        trigger    	: "input_date",
			        inputField 	: "input_date",
			        showTime   	: 24,
<? 
	if (!empty($inttimeval)) 
	{
		print "			        time 				: $inttimeval,";
	}
?>
			        fdow 				: 0,
			        onSelect   : function(cal) { 
			        		var date = Calendar.intToDate(this.selection.get());
			            date = Calendar.intToDate(date);
			            date = Calendar.printDate(date, "%Y-%m-%d");
			        		document.getElementById("input_date").value = date;
			        		
					        var h = cal.getHours(), m = cal.getMinutes();
					        // zero-pad them
					        if (h < 10) h = "0" + h;
					        if (m < 10) m = "0" + m;
					        document.getElementById("input_time").value = h + ":" + m + ":00";
					        			        		
			        		this.hide() 
			        	},
			        	
			        	onTimeChange : function(cal) {
					        var h = cal.getHours(), m = cal.getMinutes();
					        // zero-pad them
					        if (h < 10) h = "0" + h;
					        if (m < 10) m = "0" + m;
					        document.getElementById("input_time").value = h + ":" + m + ":00";
			        	}
			    });
			</script>
		</td>
		<!-- /Start Date Input -->
		
		
		<!-- Time Input -->
			<td>
				<input value="<? echo $_POST['input_time']; ?>" type="text" name="input_time" id="input_time" size="12" readonly id="input_date">
			</td>
		<!-- /Time Input -->
	</tr>
	
	<!-- Message Box -->
	<tr>
		<td class="plaintext" colspan="2">
			Message (max 1024 characters):
			<br>
			<TEXTAREA name="message" cols="50"><?php echo $reminderinfo['msg']; ?></TEXTAREA>
		</td>
	</tr>
	<!-- /Message Box -->
	
	<!-- NOTIFICATION BOX -->	
<?

	if ($reminderinfo['delivered'] != "1")
	{
?>
		<tr>
			<td colspan="2">
				<b>NOTE:</b> Reminders created and modified on this page can take up to <b>30 minutes</b> to be received by the RemindMe bot. 
				If you need a reminder delivered within 30 minutes of creating it, it is best to create the reminder through IM.
			</td>
		</tr>
<?
	}
	else
	{
?>
<p class=plaintextlarge><b>NOTE:</b> This reminder is marked as delivered. You cannot save any changes</p>
<?
	}
?>
	<!-- /NOTIFICATION BOX -->	
	
</table>

<br>

<table cellspacing=0 cellpadding=0 width=100%>
	<tr bgcolor="#E4E2E2">
		<td width=100% colspan=4 class=plaintextlarge>&nbsp;Frequency<td>
	</tr>
	<tr>
		<td class=plaintext width=25%><input <? echo $remtype_once; ?> type=radio name="remtype" value="once" checked />&nbsp;One Time Only<br><br></td>
		<td class=plaintext align=right>Expiration Date:&nbsp;
		
			<!--<input <? echo "value='".$repeatinfo['expiration']."'"; ?> type="text" name="input_expiration" id="input_expiration" size="8" readonly onClick="getDate(this);">-->

			<input id="input_expiration" name="input_expiration" value="<? echo $_POST['input_expiration']; ?>" disabled />		
			<script>
				 Calendar.setup({
							trigger    	: "input_expiration",
			        inputField 	: "input_expiration",				 	
					});
			</script>	
		
		</td>
	</tr>
	<tr>
		<td colspan=4>
		<table cellpadding=0 cellspacing=0 bgcolor=white width="100%">
				<tr class=plaintext>
					<td  class=plaintext><input <? echo $remtype_daily; ?> type=radio name="remtype" value="daily" disabled>&nbsp;Daily</td>
					<td  class=plaintext><input <? echo $remtype_weekly; ?> type=radio name="remtype" value="weekly" disabled>&nbsp;Weekly</td>
					<td  class=plaintext><input <? echo $remtype_monthly; ?> type=radio name="remtype" value="monthly" disabled>&nbsp;Monthly</td>
				</tr>
				
				<tr class=plaintext valign=top>
					<!-- daily -->
					<td>
						<table border=0 cellspacing=1 cellpadding=1 width=100%><tr><td bgcolor=D0D0D0>
						<table bgcolor=white width=100% style="HEIGHT: 75px"><tr><td class=plaintext>					
							<input <? echo $daily_dx; ?> type=radio name=daily value=dx disabled> Every <input <? echo "value='$xdays_text'"; ?> style="WIDTH: 28px;" maxlength=3 type=text name=xdays disabled /> day(s)<br>
							<input <? echo $daily_w; ?> type=radio name=daily value=w disabled> Every weekday<br>
							<input <? echo $daily_we; ?> type=radio name=daily value=we disabled> Every weekend day
					    </td></tr></table>
					    </td></tr></table>
					</td>
					<!-- weekly -->
					<td>
						<table border=0 cellspacing=1 cellpadding=1 width=100%><tr><td bgcolor=D0D0D0>
						<table bgcolor=white width=100% style="HEIGHT: 75px"><tr><td class=plaintext>
							Every <input style="WIDTH: 28px;" maxlength=3 type=text name=xweeks <? echo "value='$xweeks_text'"; ?> disabled /> week(s) on:<br>
					    	<input <? echo $daysarray[0]; ?> type=checkbox name=sunday disabled />Sunday&nbsp;
					    	<input <? echo $daysarray[1]; ?> type=checkbox name=monday disabled />Monday&nbsp;
					    	<input <? echo $daysarray[2]; ?> type=checkbox name=tuesday disabled />Tuesday&nbsp;
					    	<input <? echo $daysarray[3]; ?> type=checkbox name=wednesday disabled />Wednesday<br>
					    	<input <? echo $daysarray[4]; ?> type=checkbox name=thursday disabled />Thursday&nbsp;
					    	<input <? echo $daysarray[5]; ?> type=checkbox name=friday disabled />Friday&nbsp;
					    	<input <? echo $daysarray[6]; ?> type=checkbox name=saturday disabled />Saturday
					    </td></tr></table>
					    </td></tr></table>
					</td>
					<!-- monthly -->
					<td>
						<table border=0 cellspacing=1 cellpadding=1 width=100%><tr><td bgcolor=D0D0D0>
						<table bgcolor=white width=100% style="HEIGHT: 75px"><tr><td class=plaintext>
					
							<input <? echo $monthly_a; ?> type=radio name=monthly value=a disabled /> Day <input <? echo "value='$daynum_text'"; ?> style="WIDTH: 22px;" maxlength=2 type=text name=daynum disabled /> of every <input <? echo "value='$monthnum_text'"; ?> style="WIDTH: 22px;" maxlength=2 type=text name=monthnum disabled /> month(s)<br>
							<input <? echo $monthly_b; ?> type=radio name=monthly value=b disabled /> The 
							<select name=occurence disabled>
								<option <? echo $occurence{'1'}; ?> value="1">first</option>
								<option <? echo $occurence{'2'}; ?> value="2">second</option>
								<option <? echo $occurence{'3'}; ?> value="3">third</option>
								<option <? echo $occurence{'4'}; ?> value="4">fourth</option>
								<option <? echo $occurence{'l'}; ?> value="l"/>last</option>
							</select> 
							<select name=whichday disabled>
								<option <? echo $whichday{'d'}; ?> value="d">day</option>
								<option <? echo $whichday{'w'}; ?> value="w">weekday</option>
								<option <? echo $whichday{'we'}; ?> value="we">weekendday</option>
								<option <? echo $whichday{'0'}; ?> value="0">Sunday</option>
								<option <? echo $whichday{'1'}; ?> value="1">Monday</option>
								<option <? echo $whichday{'2'}; ?> value="2">Tuesday</option>
								<option <? echo $whichday{'3'}; ?> value="3">Wednesday</option>
								<option <? echo $whichday{'4'}; ?> value="4">Thursday</option>
								<option <? echo $whichday{'5'}; ?> value="5">Friday</option>
								<option <? echo $whichday{'6'}; ?> value="6">Saturday</option>
							</select>
							<br>of every <input <? echo "value='$xmonths_text'"; ?> style="WIDTH: 22px;" maxlength=2 type=text name=xmonths disabled /> month(s)
					    </td></tr></table>
					    </td></tr></table>
					</td> 
				</tr>
			</table>
		</td>
	</tr>
</table>
</form>

<table width=100%>
	<tr>
<?
	if ($reminderinfo['delivered'] != "1")
	{
?>

		<td align=right><a href="Javascript:document.reminder.submit()" class="button"><img src="<? print $config['Site']['url']; ?>/images/save-button.gif" alt="Save" class="button"/></a></td>
<?
	}
?>
		
	<tr>
</table>

<!-- CONTACT INFO -->
<?
	if ($reminderinfo['deliveredname'] != "")
	{
?>	
<table border="0" cellpadding="0" cellspacing="0" width="100%">
<tbody>
	
	<tr height="23">
		<td colspan="2" class=box4>&nbsp;&nbsp;<b>Delivery Info</b></td>
	</tr>

	
	<tr bgcolor="#c4c4c4" height="1">
		<td colspan="2">
		</td>
	</tr>

	<tr height="23">
		<td class=box3>
			&nbsp;&nbsp;&nbsp;Most recent date of delivery:
		</td>
		<td class=box4>
			&nbsp;<? echo $reminderinfo['deliveredtime']." (-0500)" ?>
		</td>
	</tr>
	
	<tr>
		<td colspan="2" bgcolor="#999999">
		</td>
	</tr>

	<tr height="23">
		<td class=box3>
			&nbsp;&nbsp;&nbsp;Most recently delivered to:
		</td>
		<td class=box4>
			&nbsp;<? echo $reminderinfo['deliveredname']." via ".strtoupper($reminderinfo['deliveredconn'])?>
		</td>
	</tr>
</tbody>
</table>
<?
	}
?>

<!-- /CONTACT INFO -->



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

<!--
//-----------------------------------------------------------------------------
// $Workfile: editreminder.php $ $Revision: 1.7 $ $Author: addy $ 
// $Date: 2009/07/18 22:01:20 $
//-----------------------------------------------------------------------------
-->