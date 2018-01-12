<?php
//-----------------------------------------------------------------------------
// $Workfile: service.php $ $Revision: 1.13 $ $Author: addy $ 
// $Date: 2008/12/06 04:31:33 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL);
require_once('./global.php');

$query = "SELECT * FROM system_users";
$userlist = $db->query($query);
$usernamehash = array();

// fix the system contacts table
while ($userinfo = $db->fetch_array($userlist))
{
	$q2 = "UPDATE system_contacts SET username = userid, userid = $userinfo[userid] WHERE userid = '$userinfo[username]'";
	
	//print "($q2)<br/>";
	
	$foo = $userinfo['username'];
	$usernamehash[$foo] = $userinfo['userid'];
	
	//$db->query($q2);
}


print ("<hr>");

$remlist = $db->query('SELECT * FROM system_reminders');
while ($reminfo = $db->fetch_array($remlist))
{
	$username = $reminfo['user'];
	$creatorname = $reminfo['creator'];
	
	$userid = $usernamehash[$username];
	$creatorid = $usernamehash[$creatorname];
	
	$q = "UPDATE system_reminders SET user=$userid,creator=$creatorid WHERE (id = $reminfo[id]);";
	//print "[$q]($username)($creatorname)<br/>";
	//$db->query($q);
	
}

$remlist = $db->query('SELECT * FROM system_allow_lists');
while ($reminfo = $db->fetch_array($remlist))
{
	$ownername = $reminfo['owner'];
	$buddyname = $reminfo['buddy'];
	
	$ownerid = $usernamehash[$ownername];
	$buddyid = $usernamehash[$buddyname];
	
	$q = "UPDATE system_allow_lists SET owner=$ownerid,buddy=$buddyid WHERE (owner = '$ownername' AND buddy = '$buddyname');";
	//print "[$q]($ownername)($buddyname)<br/>";
	//$db->query($q);
}

// fix the contacts
$remhash = array();
$servicelist = $db->query('SELECT * FROM service');
while ($service = $db->fetch_array($servicelist))
{
	$servicename = strtolower($service['name']);
	$remhash[$servicename] = $service['serviceid'];
}

$contacttable = $db->query('SELECT * FROM system_contacts');
while ($contact = $db->fetch_array($contacttable))
{
	$servicename = $contact['service'];
	$serviceid = $remhash[$servicename];
	
	//print("UPDATE system_contacts SET serviceid=$serviceid WHERE (id = ".$contact['id'].") <br/>");
	$db->query("UPDATE system_contacts SET serviceid=$serviceid WHERE (id = ".$contact['id'].")");
}

?>