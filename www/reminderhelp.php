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

LogEvent("reminderhelp.php: Loaded");

?>



<html>
<head>
<META content="remind me, reminders, instant message, im, aim, aol, msn, icq, adalid claure" name=description>
<script language="javascript" src="scripts/datetimepicker.js"></script>
<? include "inc/title.inc"; ?>
</head>
<link rel="stylesheet" href="style.css" type="text/css">
<body>
<div style="position:absolute; z-index:1; left: 0px; top: 0px">

<table align=left border=0 cellspacing=0 cellpadding=0 width=800px>
  <tr>
  	<? include "inc/banner_gif.inc"; ?>
  </tr>
  <tr>
    <!--<td class=title2 colspan=3 background=images/banner.jpg><img src=images/banner.jpg></td>-->
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
                  <tr class=box_header><td>How It Works</td></tr>
                    <tr>
                      <td align=top class=box_header_tab>
<!-- begin main content -->
<p><b>Add RemindMe Bot to Your Buddy List</b></p>

<table>
<tr>
<td><a href="help/buddylist.jpg"><img src="help/buddylist.jpg" height=313 width=133></a></td>

<td valign=top>
<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;Add the screen name of your RemindMe bot to your Buddy List(s) as you would add any other 
buddy. Check your IM software for help adding a buddy to your buddy list. You can find the
screen name of your bot by logging into RemindMe's website and viewing your account information once
you've registered.

<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;Once you've added the bot to your buddy list you should see it sign on. If you do not see 
the bot's screen name on your buddy list, check the privacy settings of your IM software. 

<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;If you believe you have added the RemindMe bot to your buddy list and you do
not see him sign on, please contact <i>support-at-remindme.cc</i>

<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;You can start sending instant messages to the RemindMe bot almost immediatly, though it may take up to 30
minutes after registration for the bot to recognize your IMs. 


</td>
</tr>

</table>

<hr>

<p><b>Unknown User Error</b></p>

<table>
<tr>
<td valign=top>
<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;RemindMe bots will respond to any IMs. However only registered users can create and receive
reminders. Uknown users will see the error message on the right. New users may also receive this error message for 
up to 30 minutes after registration. 
</td>
<td><a href="help/unknownuser.jpg"><img src="help/unknownuser.jpg" height=263 width=278></a></td>
</tr>

</table>

<hr>

<p><b>Registered Users</b></p>

<table>
<tr>
<td><a href="help/knownuser.jpg"><img src="help/knownuser.jpg" height=263 width=278></a></td>
<td valign=top>
<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;Once your RemindMe bot receives your registration you will see a message
like the one on the left. It can take up to 30 minutes for the bot to receive your registration.
If it has been longer than 30 minutes please contact <i>support -at- remindme.cc</i>.
</td>
</tr>
</table>

<hr>

<a name="create"></a><p><b>Creating a Reminder Using the IM Bot</b></p>

<table>
<tr>
<td valign=top>
<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;You can create reminders by sending an instant message to the RemindMe bot. The RemindMe bot
can understand several different date and time formats and you can word your reminders in a variety of formats. 
Below are some examples of messages sent to the IM bot to create reminders:
<UL>
	<li class=plaintextlarge>remind me tomorrow at 8am to look for a new job
	<li class=plaintextlarge>remind me that i have to call mom in 45 minutes
	<li class=plaintextlarge>in 2 hours and 15 minutes remind me to go to class
	<li class=plaintextlarge>remind me at 3pm to pick up jane
	<li class=plaintextlarge>remind me to call mom on 16 Jun 04 08:00
	<li class=plaintextlarge>remind me 03/24/2004 3:00pm to which steve a happy birthday
	<li class=plaintextlarge>remind me jan 1, 2004 09:55am about the meeting with frank
	<li class=plaintextlarge>on 23-mar-04 at 9:00pm remind me to pick up jimmy from school
</UL>
<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;The examples above are in three basic formats:
<UL>
	<li class=plaintextlarge><b>remind me</b> <i>date and time</i> <b>(seperator)</b> <i>reminder text</i>
	<li class=plaintextlarge><b>remind me</b> <b>(seperator)</b> <i>reminder text</i> <b>(preposition)</b> <i>date and time</i>
	<li class=plaintextlarge><b>(preposition)</b> <i>date and time</i> <b>remind me</b> <b>(seperator)</b> <i>reminder text</i>
</UL>
<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;Seperators can be either 'to', 'that' or 'about'. When creating a reminder you must use one of them. 
Prepositions can be 'in', 'on' or 'at'. When using the second or third format mentioned above, a preposition
must precede the date/time. 

<p class=plaintextlarge>
&nbsp;&nbsp;&nbsp;The RemindMe bot will send you a confirmation message telling you the date and time that your
reminder will be delivered.


</td>
<td><a href="help/setreminder1.jpg"><img src="help/setreminder1.jpg" height=263 width=278></a></td>
</tr>
</table>

<hr>



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