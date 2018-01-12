<?
include "vars.inc";

$loggedin = VerifyUser($_COOKIE[username],$_COOKIE[password]);

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
                    <tr>
                      <td align=top class=box_header_tab>


<table align=left border=0 cellspacing=0 cellpadding=2 width=100%>
  <tr>
   <td bgcolor=white valign=top>
      <table border=0 cellspacing=1 cellpadding=0 width=100%>
        <tr>
          <td bgcolor=D0D0D0>
            <table border=0 cellspacing=1 cellpadding=3 width=100%>
              <tr>
                <td class=box_header>
					<b>Upgrade Account</b>
                </td>
              </tr>
              <tr>
<td bgcolor=white class=plaintextlarge>
<!-- begin main info -->
<h2>Under Construction</h2>
<a class="link" href="Javascript:history.go(-1)">back</a>
<!-- end main content -->
				</td>
              </tr>
            </table>
          </td>
        </tr>
      </table>
    </td>
  </tr>
</table>
            
						</td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
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


