<?php 
//-----------------------------------------------------------------------------
// $Workfile: contact.php $ $Revision: 1.2 $ $Author: addy $ 
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
<!-- begin main content -->

<TABLE id="Table4" cellSpacing="0" cellPadding="0" align="left" border="0">
	
<? if ($userinfo['loggedin']) { ?>	
	<TR>
		<TD bgColor="white"><FONT class="plaintextlarge"><B>Live Tech Support!</B><BR>
				If you're having problems using RemindMe, or have a general question you can send an IM to screen name "RemindMeHelp" on AOL Instant Messenger. (Signed on most weekdays 9am - 6pm)
		</TD>
	</TR>
	
	<TR><TD><br></TD></TR>
<? } ?>
	
	<TR>
		<TD bgColor="white"><FONT class="plaintextlarge"><B>Support:</B><BR>
				Please check out <a class=link href="//wiki.remindme.cc">the wiki page</a> as your question may be answered there. <BR></FONT>
		</TD>
	</TR>
	
	<TR><TD><br></TD></TR>
	
	<TR>
		<TD bgColor="white"><FONT class="plaintextlarge"><B>Or you can email your question to:</B><BR>
				<FONT class="link">support -AT- remindme.cc</FONT></FONT>
		</TD>
	</TR>
	
	<TR><TD><br></TD></TR>
	
	<tr>
		<td>
			<FONT class="plaintextlarge">
			<B>For advertising or general inquires please email us at:</B><BR>
			<FONT class="link">info -AT- remindme.cc</FONT>
			
		</td>
	</tr>
	
	<TR><TD><br></TD></TR>

	<tr>
		<td>	
				<FONT class="plaintextlarge">* Replace the -AT- with an @ symbol<BR>
					(this is to fool automatic SPAM list generators)</FONT>
			
		</td>
	</tr>

</TABLE>

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

