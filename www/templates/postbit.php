<?php
//-----------------------------------------------------------------------------
// $Workfile: functiona_misc.php $ $Revision: 1.3 $ $Author: addy $ 
// $Date: 2009/07/17 22:24:29 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL);

?>

	<table style="padding:3px; margin:5px; border-style:solid; border-width:1px; border-color:black; width:500px;">
		<tr>
			<td style="font-size: 12pt; font-weight:bold;">	
				<? print $postinfo['text']; ?>
			</td>
		</tr>
		<tr>
			<td>
				<small style="font-size:7pt;">
<?
	$username = $postinfo['text']; 
	if ($postinfo['anonymous'] == '1')
		$username = 'Anonymous';
		
	print ("$username @ $postinfo[dateline]");	
?>				
			</small>
			</td>
		</tr>
	</table>				
