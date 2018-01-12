<?php 
//-----------------------------------------------------------------------------
// $Workfile: index.php $ $Revision: 1.7 $ $Author: addy $ 
// $Date: 2009/07/04 21:31:19 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');


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
<? if ($userinfo['loggedin']) { ?>
						<div class="largeText">Welcome <? echo $userinfo['username'] ?>!</div>
						<br/>
<? } ?>	                
	                
                        <div class="largeText">What is RemindMe?</div>
                        <p>RemindMe is a reminder service that sends and receives reminders through IMs or email. Users add a screen-name to their buddy list and interact with the service through an
                        IM window. Users add reminders with simple language commands, for example: "remind me tomorrow at 2pm
                        to call mom". In this case, RemindMe would send an IM reminder the following day at 2pm with the text "call mom". 
                        Reminders can be as short or as long as you want!</p>
                        
						<div class="largeText">Features</div>
						<ul style="list-style-position:inside;">
						<li>Receive reminders on AOL Instant Messenger or Email! (GoogleTalk and Yahoo Instant Messenger support coming soon!)</li>
						<li>Easy to understand real language interface</li>
						<li>Can be used from any computer that has instant messaging</li>
						<li>Reoccuring reminders can remind you daily, weekly or monthly!</li>
						<li>100% free of charge!</li>
						</ul>   
						
                        <div class="largeText">How do I create reminders?</div>
                        <p>Users create reminders by sending an instant message to a specified username. This username is not a person but a RemindMe bot
                        specifically designed to store and deliver reminders. Users can also send reminders to the bot through email, simply enter the reminder
                        in the subject of the email and the bot will do the rest! The only things users must do is tell the bot when you want your reminder delivered
                        and the message you want sent.</p>
                        
  					    <b>Some examples are below:</b>
					    <table width="100%">
						    <tr>
							    <td id="sampleBox">
							        <ul>
							        <li>remind me tomorrow at 8am to look for a new job</li>
							        <li>remind me to call mom in 45 minutes</li>
							        <li>in 2 hours and 15 minutes remind me to go to class</li>
							        <li>remind me at 3pm to pick up jane</li>
							        <li>remind me to call mom on 16 Jun 04 08:00</li>
							        <li>remind me 03/24/2004 3:00pm to call steve and wish him a happy birthday</li>
							        <li>remind me jan 1, 2004 09:55am to go to meeting with frank and bob</li>
							        <li>on 23-mar-04 at 9:00pm remind me to pick up jimmy from school</li>
							        </ul>
							    </td>
						    </tr>
					    </table>                        
	                
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

<? if (!$loggedin) { ?>
	<script type="text/javascript">
	//at_attach("login_form_parent", "login_form_child", "click", "y", "pointer");
	</script>
<? } ?>

</body>
</html>