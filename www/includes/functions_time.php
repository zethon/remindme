<?php
//-----------------------------------------------------------------------------
// $Workfile: functions_time.php $ $Revision: 1.2 $ $Author: addy $ 
// $Date: 2009/07/18 19:53:34 $
//-----------------------------------------------------------------------------

function get_timezone_offset($remote_tz, $origin_tz = null) 
{
    if($origin_tz === null) 
    {
        if(!is_string($origin_tz = date_default_timezone_get())) 
        {
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

function get_server_timestamp($timeinfo,$timezone)
{
	$currentTZ = date_default_timezone_get();
	date_default_timezone_set($timezone); 
	$dateinfo = date_parse($timeinfo);
	$unixtime = mktime($dateinfo['hour'],$dateinfo['minute'],$dateinfo['second'],$dateinfo['month'],$dateinfo['day'],$dateinfo['year'],-1);
	date_default_timezone_set('GMT');
	$retval = date("Y-m-d H:i:s", $unixtime);
	date_default_timezone_set($currentTZ); 

	return $retval;
}

?>