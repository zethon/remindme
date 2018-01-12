<?php 
//-----------------------------------------------------------------------------
// $Workfile: suggestions.php $ $Revision: 1.2 $ $Author: addy $ 
// $Date: 2008/12/27 03:21:19 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);
include "vars.inc";

if (!VerifyUser($_COOKIE['username'],$_COOKIE['password']))
{
	header("Location: login.php");	
}

if ($_POST['action'] == "save" && strlen($_COOKIE['username']) > 0)
{
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(createReminder.php() Unable to select database [$dbname])");
	
	$date = date("Y-m-d G:i:s");
	$newquery = "INSERT INTO system_suggestions (uid,subject,text,datetime) ".
	$newquery.= " VALUES ('".$_COOKIE['username']."','".$_POST['subject']."','".SafeQueryData($_POST['text'])."','$date')";

	mysql_query($newquery);
	
	mysql_close();
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
      
<? include "inc/topmenu_secure.inc"; ?> 

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
                  <tr class=box_header><td>Suggestion Box</td></tr>
                    <tr>
                      <td align=top class=box_header_tab>
<!-- begin main info -->

<b>Please leave any comments, questions or suggestions about RemindMe here.</b>
<br><br>

<!-- LIST SUGGESTIONS -->
<?
	mysql_connect($dbhost,$dbuser,$dbpw);
	@mysql_select_db($dbname) or die("(reminder.php() Unable to select database [$dbname])");
	
	$query = "SELECT * FROM system_suggestions ORDER BY datetime DESC";
	$result=mysql_query($query);
	$num=mysql_numrows($result);
	
	$i = 0; $col = 0;
	while ($i < $num)
	{
		$reponse = "";
		
		$datetime = mysql_result($result,$i,"datetime");
		$subject = mysql_result($result,$i,"subject");
		$text = mysql_result($result,$i,"text");
		$response = mysql_result($result,$i,"response");
		
		if ($subject == "0")
		{
			$subject = "I wish this service had...";
		}
		else if ($subject == "1")
		{
			$subject = "Bug or unexpected reaction";
		}
		else
		{
			$subject = "Other...";
		}
		
		if (strlen($response)>1)
		{
			$reponse= "<tr class=box><td align=center><hr></td></tr><tr class=box><td><b>Response<br></b>$response</td></tr>";
		}
		
		echo "
<table border=0 cellspacing=1 cellpadding=1 width=60%>
	<tr>
		<td bgcolor=D0D0D0>    
			<table cellspacing=0 width=100%>
				<tr class=box><td><b>Date:</b> $datetime</td></tr>
				<tr class=box><td><b>Subject:</b> $subject</td></tr>
				<tr class=box><td align=center><hr></td></tr>
				<tr class=box><td>$text<br><br></td></tr>
				$reponse
			</table>
		</td>
	</tr>
</table><br>";

		
		
		$i++;	
	}
?>


<!--/LIST SUGGESTIONS -->


<!--Suggestion Form-->
<br><br>
<form name=suggest method=post action=suggestions.php>
<input type=hidden name=action value='save'>
<table border=0 cellspacing=1 cellpadding=1 width=25%>
	<tr>
		<td bgcolor=D0D0D0>    
			<table cellspacing=0 width=100%>
				<tr class=box_header>
					<td width=20%>Subject:</td>
					<td align=right>
						<select name="subject">
							<option value=0>I wish this service had...</option>
							<option value=1>Bug or unexpected reaction</option>
							<option value=2>Other...</option>
						</select>
					</td>
				</tr>
				<tr class=box><td colspan=2 class=plaintextlarge><textarea name="text" cols="50" rows="7"></textarea></td></tr>
				<tr class=box><td colspan=2 align=right><a class=link href="Javascript:document.suggest.submit()">save</a></td></tr>				
			</table>
		</td>
	</tr>
</table>
</form>
<br>
<!--/Suggestion Form-->



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

</div>
</body>
</html>


