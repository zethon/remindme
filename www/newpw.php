<?
include "vars.inc";

$success = FALSE;
$error = FALSE;
$error_msg = "";

if ($_POST[action] == "newpass")
{
	if ($_POST[userid] == "" || $_POST[email] == "")
	{
		$error_msg = "Must enter a username and email";
		$error = TRUE;	
	}
	else
	{
		$userinf = GetUserHash($_POST[userid]);
		if ($userinf{'id'} == "")
			
		{
			$error_msg = "Unknown username";	
			$error = TRUE;		
		}
		else
		{
			if ($userinf{'email'} != $_POST[email])
			{
				$error_msg = "Email does not match username";
				$error = TRUE;
			}
			else
			{
				$newpass = makeRandomPassword();
				$encpass = md5($newpass);
				mysql_connect($dbhost,$dbuser,$dbpw);
				@mysql_select_db($dbname) or die("(GetUserHash() Unable to select database [$dbname])");
				$query="UPDATE system_users SET password = '$encpass' WHERE (id='$_POST[userid]')";
				mysql_query($query);
				mysql_close();

				$content = ReplaceInFile("emails/newpass.txt",array(
											'%userid%' => $_POST[user],
											'%newpass%' => $newpass,
												));
				mail($userinf{'email'},"New RemindMe Password",$content,"From: $support_email");	
				$success = TRUE;
			}
		}
	}
}

?>
<html>
<head>
<META content="remind me, reminders, instant message, im, aim, aol, msn, icq, adalid claure" name=description>
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
    <!--<td class=title2 colspan=3 background=../image/blockTopMenu.jpg><img src=../image/blockTopMenu.jpg></td>-->
    <td class=title2 colspan=3>&nbsp;</td>
  </tr>
  <tr>
    <td width=14px><img src=curveTopMenu.jpg></td>
    <td width=786px class=title> 
		<? include "inc/topmenu_public.inc"; ?>      
    </td>
  </tr>
  <!--<tr><td align=center colspan=3><div class=warn>NOTE: "Ciqo" will be down during the evening hours through the weekend. (Feb 6th, 2004)</div></td></tr>-->
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
                  <tr class=box_header><td>New Password</td></tr>
                    <tr>
                      <td align=top class=box_header_tab>
<!-- begin main content -->

<? if ($_POST[action] == "" || $error == TRUE) { ?>
<form name="forgotpass" method="post" action="newpw.php">
<font class="plaintext">Enter your username and email address and a new password will be emailed to you</font>
<table align="left" border="0" cellspacing="0" cellpadding="0" width="40%">
  <tbody><tr>
    <td>
      <table border="0" cellspacing="1" cellpadding="0">
        <tbody>
        <tr><td>&nbsp;</td></tr>
<? if ($error) { echo "<tr><td colspan=3><div class=warn>$error_msg</div></td></tr>"; } ?>        
        
        <tr>
          <td width="5"></td>
          <td width="40%" class="plaintext"><b>username&nbsp;</b></td>
          <td class="box">
            <input type="text" class="box" name="userid" size="25">
          </td>
        </tr>
        <tr>
          <td width="5"></td>
          <td width="40%" class="plaintext"><b>email&nbsp;</b></td>
          <td class="box">
            <input type="text" class="box" name="email" size="25">
          </td>
        </tr>
      </tbody></table>
    </td>
  </tr>

  <tr><td>&nbsp;</td></tr>
  <tr>
    <td align="right"> 
      <a class="link" href="Javascript:history.go(-1)">back</a>
      <font class="plaintext"><b>::</b></font>
      <a class="link" href="Javascript:document.forgotpass.submit()">submit</a> 
    </td>
  </tr>
</tbody></table>
<input type="hidden" name="action" value="newpass">
</form>

<? } elseif ($success) { ?>
<br>
<div class=plaintextlarge>Your new password has been sent to <? echo $userinf{'email'}; ?>
<br><br>
	
<a href="index.php" class=link>home</a>	
<? } ?>  

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

