<?
include "vars.inc";

if (VerifyUser($_COOKIE[username],$_COOKIE[password]))
{
	$loggedin = TRUE;
}
else
{
	$loggedin = FALSE;
}
?>
LogEvent("help.php: Loaded");

<html>
<head>
<META content="remind me, reminders, instant message, im, aim, aol, msn, icq, adalid claure" name=description>
<title>RemindMe</title>
</head>
<link rel="stylesheet" href="style.css" type="text/css">
<body>
<div style="position:absolute; z-index:1; left: 0px; top: 0px">

<table align=left border=0 cellspacing=0 cellpadding=0 width=800px>
  <tr>
    <? include "inc/banner_gif.inc"; ?>
  </tr>
  <tr>
    <!--<td class=title2 colspan=3 background=../image/blockTopMenu.jpg><img src=../image/blockTopMenu.jpg></td>-->
    <td class=title2 colspan=3>&nbsp;</td>
  </tr>
  <tr>
    <td width=14px><img src=curveTopMenu.jpg></td>
    <td width=786px class=title> 
      
<? 
if ($loggedin)
{
	include "inc/topmenu_secure.inc"; 
} 
else
{
	include "inc/topmenu_public.inc"; 
}
?> 

    </td>
  </tr>
  <tr><td colspan=3></td></tr>
  <tr><td colspan=3></td></tr>
  <tr><td colspan=3></td></tr>
  <tr><td colspan=3></td></tr>
  <tr><td colspan=3></td></tr>
  <tr><td colspan=3></td></tr>
  <tr>
    <td></td> 
    <td colspan=3> 
      <table border=0 cellpadding=0 cellspacing=0 width=100%>
        <tr>
          <td bgcolor=white align=right>
            <table border=0 cellspacing=0 cellpadding=0>
              <tr>
                <td bgcolor=D0D0D0>
                  <table border=0 cellspacing=1 cellpadding=3 width=783px>
                  <tr class=box_header><td>Help / FAQ</td></tr>
                    <tr>
                      <td align=top class=box_header_tab>
<!-- begin main content -->

<div class=plaintextlarge>
<b>Q. How do I create a Reminder?</b>

<p>A. Users create reminders by sending an instant message to a username on AIM, MSN or ICQ. This username is not a person but a RemindMe bot
specifically designed to store and deliver reminders. To create a reminder simply tell the bot when you want your reminder delivered
and the message you want sent.

<p>Some examples are below:
	<table width=100%>
		<tr><td class=box_header1><div class=sampletext>
			<UL>
				<li>remind me tomorrow at 8am to look for a new job
				<li>remind me to call mom in 45 minutes
				<li>in 2 hours and 15 minutes remind me to go to class
				<li>remind me at 3pm to pick up jane
				<li>remind me to call mom on 16 Jun 04 08:00
				<li>remind me 03/24/2004 3:00pm to call steve and wish him a happy birthday
				<li>remind me jan 1, 2004 09:55am to go to meeting with frank and bob
				<li>on 23-mar-04 at 9:00pm remind me to pick up jimmy from school
			</UL>
		</div></td></tr>
	</table>

<p>Every reminder must contain the phrase "<b>remind me</b>", which alerts the RemindMe bot that the
you are creating a reminder.
	
	
<p><b>Q. I registered but the bot keeps acting like I haven't!</b>

<p>A. RemindMe is currently a Beta product, as a result some functionality still has not been
perfected. While RemindMe remains in Beta new user registrations can take up to 60 minutes 
to sync across our system. The estimate of 60 minutes is very high, most likely your bot
will recognize you within 15-20 minutes. 

<p>You can tell that your RemindMe bot recognizes you if you see the following message when
you send it an IM: "hello user, please type 'help' for help." 


<p><b>Q. What does "Invalid time specification" mean?</b>

<p>A. This error occurs when the RemindMe bot does not understand when you want your reminder
delivered. Usually this is caused by a typo in your message. For example: "remind me in 20 mintes
to check the dryer". The bot will return an error because the word "minutes" has been spelled
incorrectly.

<p><b>Q. I tried to set a reminder for 2:00 but the bot says that time has already past and it's only noon, why?</b>

<p>When notating specific times in your reminders, you must either use 24-hour time notation (ie. 14:00)
or you must specify "am" or "pm" (ie. "2:00pm"). If "am" or "pm" are not used and the time notation is
not in 24-hour notation, the RemindMe bot assumes "am".

<p><b>


</div>
<br>
<!-- end main content -->
						</td>
                    </tr>
                  </table>
                </td>
              </tr>
         	
          </td>
        </tr>
<!-- insert bottom tags here -->        

<? include "inc/bottom_tag.inc"; ?>

<!-- end insert of bottom tags --> 
      </table>
    </td>
  </tr>
</table>

</body>
</html>
</div>

