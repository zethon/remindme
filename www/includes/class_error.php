<?php
//-----------------------------------------------------------------------------
// $Workfile: class_user.php $ $Revision: 1.1 $ $Author: addy $ 
// $Date: 2009/07/17 22:24:29 $
//-----------------------------------------------------------------------------
error_reporting(E_ALL & ~E_NOTICE);

define('RESULT_SUCCESS',0);
define('RESULT_GETUSER_ERROR',1);

class Result
{
	public $success = false;
	public $errrortype = 0;
	public $errorrmessage = '';	
	public $data = null;	
}

?>