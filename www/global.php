<?php
//-----------------------------------------------------------------------------
// $Workfile: report.php $ $Revision: 1.3 $ $Author: addy $ 
// $Date: 2009/07/17 22:24:29 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL);
session_start();

define('DIR', (($getcwd = getcwd()) ? $getcwd : '.'));

require_once(DIR . '/includes/config.php');
require_once(DIR . '/includes/db_mysql.php');
require_once(DIR . '/includes/functions_time.php');
require_once(DIR . '/includes/class_error.php');
require_once(DIR . '/includes/class_user.php');
require_once(DIR . '/includes/class_reminder.php');

$db = new DB_Sql;
$db->database = $config['Database']['dbname'];
if (!$db->connect($config['Database']['servername'], $config['Database']['username'], $config['Database']['password'],0))
{
	echo "<b>Cannot connect to database</b>";
	exit;
}

$userinfo = User::ValidateUser($_SESSION['userid'],$_SESSION['password']);