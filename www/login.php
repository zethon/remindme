<?php
//-----------------------------------------------------------------------------
// $Workfile: login.php $ $Revision: 1.6 $ $Author: addy $ 
// $Date: 2009/07/11 22:10:19 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL);
require_once('./global.php');

// grab current time
$time=time(); 

$gUserName = strtolower($_POST['username']);

// handle the logout event
if ($_REQUEST['logout'] == 'true') 
{    
	//LogEvent("LOGGED OUT",$gUserName);
	session_destroy();
	setcookie ("username", $gUserName, $time-3200); 
	setcookie ("password", md5($_POST[password]), $time-3200); 
	header("Location: index.php");
	exit;
}

// handle validation event
if ($gUserName && $_POST['password']) 
{    
	if (User::ValidateByUsername($gUserName,$_POST['password'],true))
	{ 	
		if ($_POST['keep'] == 'on')
			$exTime = time() + 60*60*24*365;
		else
			$exTime = time() + 4000;
			
		setcookie ("username", $gUserName, $exTime); 
		setcookie ("password", md5($_POST[password]), $exTime); 

		session_register('userid');
		session_register('password');
		
		$_SESSION['userid'] = $userinfo['userid'];
		$_SESSION['password'] = $userinfo['password'];				
  		
		if ($_POST['viewreminder'] != "")
		{
			header("Location: viewreminder.php?remid=".$_POST['viewreminder']);
			exit;
		}
		else
		{
			header("Location: index.php");
			exit;
		}
 	} 
 	else 
 	{ 
 		$login_error= true; 
 	}
}

if (User::ValidateUser($_SESSION['userid'],$_SESSION['password']))
{ 
	header("Location: index.php");
	exit;
} 

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



<form name=login action=login.php method=post>
<input type=hidden name=viewreminder value="<? echo $_GET['viewreminder']?>">
<table align=left style="font-family:arial; font-size:12; border:1 solid #000000;">
<?
	if ($login_error == true)
	{
		echo "<tr><td class=\"errorinfo\" colspan=2>Invalid Login</font></td></tr>";
	}
?>

  <tr><td align=right>Username: </td><td><input type=text name=username size=15></td></tr>
  <tr><td align=right>Password: </td><td><input type=password name=password size=15></td></tr>
  <tr><td aliign=left colspan=2><input type=checkbox name=keep>&nbsp;Log me in on each visit</td></tr>
  <tr><td colspan=2>&nbsp;</td></tr>
  <tr>
	<td colspan=2 align=right>
		<a href="newpw.php" class=link>forgot pass</a><font class="plaintext">&nbsp;&nbsp;<b>::</b>&nbsp;&nbsp;</font><input type="submit" value="Login"/></td></tr>
</table>
</form>


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


<script type="text/javascript">
//at_attach("login_form_parent", "login_form_child", "click", "y", "pointer");
</script>


</body>
</html>


