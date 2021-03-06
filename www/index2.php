<?php 
//-----------------------------------------------------------------------------
// $Workfile: index.php $ $Revision: 1.10 $ $Author: addy $ 
// $Date: 2008/03/07 02:30:23 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
require_once('./global.php');
//require_once(DIR.'/includes/class_user.php');
require_once(DIR.'/vars.inc');
session_start();

if (VerifyUser($_COOKIE[username],$_COOKIE[password]))
{
	$loggedin = TRUE;
}
else
{
	$loggedin = FALSE;
}

?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" lang="en" xml:lang="en">

<head>
<title>RemindMe | Instant Message Reminders | Email Reminders</title>

<link rel="ICON" type="image/gif" href="images/remindme-favicon.gif" />
<link rel="stylesheet" href="style/remindme-style.css" type="text/css" />
<script type="text/javascript" src="scripts/dropdown.js"></script>

</head>

<body >

<form class="login_form" id="login_form_child" name="login" method="post" action="login.php">
    <table>
        <tr><td align="right"><b>User:</b>&nbsp;</td><td><input type="text" name="username" /></td></tr>
        <tr><td align="right"><b>Password:</b>&nbsp;</td><td><input type="password" name="password" /></td></tr>
        <tr><td colspan="2" align="right"><input type="submit" value="Go" /></td></tr>
    </table>
</form>

<div id="outerContainer">
	
<table width="100%" id="innerContainer" cellpadding="0" cellspacing="0">
	<tr>
		<td id="toptable" align="center" valign="top">
			<img alt="RemindMe Logo" align="left" src="images/logo.gif" style="padding-left:20px; padding-top: 10px;"/>
		</td>
	</tr>
	
	<tr>
	    <td id="middletable">
            <table width="100%" cellpadding="0" cellspacing="0">
                <tr>
                    <td id="menuItems">
                        <a href="index.php">home</a> | <a href="register.php">register</a> | <a href="hiw.php">how it works</a> | <a href="contact.php">contact</a>
                    </td>
                    <td id="menuLoginButton">
<? if ($loggedin) { ?>                    
                    <a href="#" id="login_form_parent">login</a>&nbsp;
<? } else { ?>
					<a href="#" id="login_form_parent">login</a>&nbsp;
<? } ?>					
                    </td>
                </tr>
            </table>
            	    
	    
	        <table cellpadding="0" cellspacing="0">
	            <tr>
                    <td id="sideMenu">
                        <form action="https://www.paypal.com/cgi-bin/webscr" method="post">
                        <input type="hidden" name="cmd" value="_s-xclick"/>
                        <input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-but21.gif" border="0" name="submit" alt="Make payments with PayPal - it's fast, free and secure!"/>
                        <input type="hidden" name="encrypted" value="-----BEGIN PKCS7-----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-----END PKCS7-----"/>
                        </form>
                        
                        <br />
                        
			            <script type="text/javascript">
			            <!--
			            google_ad_client = "pub-3215275432766543";
			            google_ad_width = 120;
			            google_ad_height = 240;
			            google_ad_format = "120x240_as";
			            google_ad_channel ="";
			            google_color_border = "009999";
			            google_color_bg = "FFFFCC";
			            google_color_link = "CC9900";
			            google_color_url = "008000";
			            google_color_text = "000000";
			            //-->
			            </script>
			            <script type="text/javascript" src="http://pagead2.googlesyndication.com/pagead/show_ads.js"></script>                        
                    </td>	  
                    
                    <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>          
	            
	                <td id="mainContent">
<? if ($loggedin) { ?>
						<div class="largeText">Welcome <? echo $_COOKIE[username] ?>!</div>
<? } ?>	                
	                
                        <div class="largeText">What is RemindMe?</div>
                        <p>RemindMe is a reminder service that sends and receives reminders through IMs or email. Users add a screen-name to their buddy list and interact with the service through an
                        IM window. Users add reminders with simple language commands, for example: "remind me tomorrow at 2pm
                        to call mom". In this case, RemindMe would send an IM reminder the following day at 2pm with the text "call mom". 
                        Reminders can be as short or as long as you want!</p>
                        
						<div class="largeText">Features</div>
						<ul style="list-style-position:inside;">
						<li>Receive reminders on GoogleTalk, AOL Instant Messenger or Email! (Yahoo Instant Messenger support coming soon!)</li>
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

<script type="text/javascript">
at_attach("login_form_parent", "login_form_child", "click", "y", "pointer");
</script>

</body>
</html>