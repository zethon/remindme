<?
require_once( '/home/remindme/bugs/www/core.php' );

function create_mantis_user($userid,$password,$email)
{
	return user_create($userid,$password,$email,25,false,true);
}

?>