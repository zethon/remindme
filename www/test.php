<?php

function get_timezone_offset($remote_tz, $origin_tz = null) {
    if($origin_tz === null) {
        if(!is_string($origin_tz = date_default_timezone_get())) {
            return false; // A UTC timestamp was returned -- bail out!
        }
    }
    $origin_dtz = new DateTimeZone($origin_tz);
    $remote_dtz = new DateTimeZone($remote_tz);
    $origin_dt = new DateTime("now", $origin_dtz);
    $remote_dt = new DateTime("now", $remote_dtz);
    $offset = $origin_dtz->getOffset($origin_dt) - $remote_dtz->getOffset($remote_dt);
    return $offset;
}

function format_offset($offset)
{
	list($bigval,$decval) = split('[.]',$offset);
	
	if ($bigval[0] == '-' && strlen($bigval) == 2)
	{
		$bigval = $bigval[0].'0'.$bigval[1];
	}
	
	if ($bigval[0] != '-')
	{
		$bigval = '+'.sprintf("%02d",$bigval);
	}
	
	$decval = sprintf("%02d",$decval * 6);
	$retval = $bigval.$decval;
	
	if (strlen($retval) == 6)
	{
		$retval = substr($retval,0,5);
	}
		
	return $retval;
}


$tzlist = timezone_identifiers_list();
$gmttz = new DateTimeZone("GMT");

print("<pre>");


foreach ($tzlist as $tz)
{
	$offset = format_offset(get_timezone_offset($tz,"GMT") / 3600);
	print ("[$tz]($offset)\r\n");
}

print("</pre>");

$dateinfo = date_parse('2009-07-20 16:00:00 America/New_York');
$unixtime = mktime($dateinfo['hour'],$dateinfo['minute'],$dateinfo['second'],$dateinfo['month'],$dateinfo['day'],$dateinfo['year'],-1);
print "<h1>[$unixtime]</h1>";
date_default_timezone_set('GMT');
$datestr = date("m-d-Y H:i:s", $unixtime);
print "<h1>[$datestr]</h1>";


?>