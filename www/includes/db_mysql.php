<?php
/*======================================================================*\
|| #################################################################### ||
|| # vBulletin 3.0.7 - CAT Team
|| # ---------------------------------------------------------------- # ||
|| # Copyright �2000�2005 Jelsoft Enterprises Ltd. All Rights Reserved. ||
|| # This file may not be redistributed in whole or significant part. # ||
|| # ---------------- VBULLETIN IS NOT FREE SOFTWARE ---------------- # ||
|| #                       Nullified By CAT Team                      # ||
|| #################################################################### ||
\*======================================================================*/

error_reporting(E_ALL & ~E_NOTICE);

define('DBARRAY_NUM', MYSQL_NUM);
define('DBARRAY_ASSOC', MYSQL_ASSOC);
define('DBARRAY_BOTH', MYSQL_BOTH);
define('DB_QUERIES', false);
define('DB_EXPLAIN', false);

class DB_Sql
{
	var $querytime = 0;
	var $query_count = 0;
	var $database = '';

	var $link_id = 0;

	var $errdesc = '';
	var $errno = 0;
	var $reporterror = 0;

	var $appname = '';
	var $appshortname = '';

	function connect($server, $user, $password, $usepconnect = 0)
	{
		// connect to db server


		// do query

		if (DB_QUERIES)
		{
			echo "Connecting to database\n";

			global $pagestarttime;
			$pageendtime = microtime();
			$starttime = explode(' ', $pagestarttime);
			$endtime = explode(' ', $pageendtime);

			$beforetime = $endtime[0] - $starttime[0] + $endtime[1] - $starttime[1];

			echo "Time before: $beforetime\n";
			if (function_exists('memory_get_usage'))
			{
				echo "Memory Before: " . number_format((memory_get_usage() / 1024)) . 'KB' . " \n";
			}
		}

		if (0 == $this->link_id)
		{
			if (empty($password))
			{
				if ($usepconnect == 1)
				{
					$this->link_id = @mysql_pconnect($server, $user);
				}
				else
				{
					$this->link_id = @mysql_connect($server, $user);
				}
			}
			else
			{
				if ($usepconnect == 1)
				{
					$this->link_id = @mysql_pconnect($server, $user, $password);
				}
				else
				{
					$this->link_id = @mysql_connect($server, $user, $password);
				}
			}
			if (!$this->link_id)
			{
				$this->halt('Link-ID == false, connect failed');
				return false;
			}

			$this->select_db($this->database);

			if (DB_QUERIES)
			{
				$pageendtime = microtime();
				$starttime = explode(' ', $pagestarttime);
				$endtime = explode(' ', $pageendtime);

				$aftertime = $endtime[0] - $starttime[0] + $endtime[1] - $starttime[1];
				$this->querytime += $aftertime - $beforetime;

				echo "Time after: $aftertime\n";
				echo "Time taken: " . ($aftertime - $beforetime) . "\n";
				if (function_exists('memory_get_usage'))
				{
					echo "Memory After: " . number_format((memory_get_usage() / 1024)) . 'KB' . " \n";
				}

				echo "\n<hr />\n\n";
			}

			return true;
		}
	}

	function affected_rows()
	{
		$this->rows = mysql_affected_rows($this->link_id);
		return $this->rows;
	}

	function geterrdesc()
	{
		$this->error = mysql_error($this->link_id);
		return $this->error;
	}

	function geterrno()
	{
		$this->errno = mysql_errno($this->link_id);
		return $this->errno;
	}

	function select_db($database = '')
	{
		// select database
		if (!empty($database))
		{
			$this->database = $database;
		}

		$connectcheck = @mysql_select_db($this->database, $this->link_id);

		if($connectcheck)
		{
			return true;
		}
		else
		{
			$this->halt('cannot use database ' . $this->database);
			return false;
		}

	}

	function query_unbuffered($query_string)
	{
		return $this->query($query_string, 'mysql_unbuffered_query');
	}

	function shutdown_query($query_string, $arraykey = 0)
	{
		global $shutdownqueries;

		if (NOSHUTDOWNFUNC AND !$arraykey)
		{
			return $this->query($query_string);
		}
		elseif ($arraykey)
		{
			$shutdownqueries["$arraykey"] = $query_string;
		}
		else
		{
			$shutdownqueries[] = $query_string;
		}
	}

	function query($query_string, $query_type = 'mysql_query')
	{
		// aclaure: added this to allow multiple db_msql objects
		$this->select_db($this->database);

		if (DB_QUERIES)
		{
			echo 'Query' . ($query_type == 'mysql_unbuffered_query' ? ' (UNBUFFERED)' : '') . ":\n<i>" . htmlspecialchars($query_string) . "</i>\n";

			global $pagestarttime;
			$pageendtime = microtime();
			$starttime = explode(' ', $pagestarttime);
			$endtime = explode(' ', $pageendtime);

			$beforetime = $endtime[0] - $starttime[0] + $endtime[1] - $starttime[1];

			echo "Time before: $beforetime\n";
			if (function_exists('memory_get_usage'))
			{
				echo "Memory Before: " . number_format((memory_get_usage() / 1024)) . 'KB' . " \n";
			}
		}

		// do query
		$query_id = $query_type($query_string, $this->link_id);
		$this->lastquery = $query_string;
		if (!$query_id)
		{
			$this->halt('Invalid SQL: ' . $query_string);
		}

		$this->query_count++;

		if (DB_QUERIES)
		{
			$pageendtime = microtime();
			$starttime = explode(' ', $pagestarttime);
			$endtime = explode(' ', $pageendtime);

			$aftertime = $endtime[0] - $starttime[0] + $endtime[1] - $starttime[1];
			$this->querytime += $aftertime - $beforetime;

			echo "Time after: $aftertime\n";
			echo "Time taken: " . ($aftertime - $beforetime) . "\n";
			if (function_exists('memory_get_usage'))
			{
				echo "Memory After: " . number_format((memory_get_usage() / 1024)) . 'KB' . " \n";
			}

			if (DB_EXPLAIN AND preg_match('#(^|\s)SELECT\s+#si', $query_string))
			{
				$explain_id = mysql_query("EXPLAIN " . $query_string, $this->link_id);
				echo "</pre>\n";
				echo '
				<table width="100%" border="1" cellpadding="2" cellspacing="1">
				<tr>
					<td><b>table</b></td>
					<td><b>type</b></td>
					<td><b>possible_keys</b></td>
					<td><b>key</b></td>
					<td><b>key_len</b></td>
					<td><b>ref</b></td>
					<td><b>rows</b></td>
					<td><b>Extra</b></td>
				</tr>
				';
				while($array = mysql_fetch_assoc($explain_id))
				{
					echo "
					<tr>
						<td>$array[table]&nbsp;</td>
						<td>$array[type]&nbsp;</td>
						<td>$array[possible_keys]&nbsp;</td>
						<td>$array[key]&nbsp;</td>
						<td>$array[key_len]&nbsp;</td>
						<td>$array[ref]&nbsp;</td>
						<td>$array[rows]&nbsp;</td>
						<td>$array[Extra]&nbsp;</td>
					</tr>
					";
				}
				echo "</table>\n<br /><hr />\n";
				echo "\n<pre>";
			}
			else
			{
				echo "\n<hr />\n\n";
			}
		}

		return $query_id;
	}

	function query_array($query_string, $type = DBARRAY_ASSOC)
	{
		$returnarray = array();
		$query_id = $this->query($query_string);
		while ($stub = $this->fetch_array($query_id, $type))
		{
			$temp = array_values($stub);
			array_push($returnarray,$temp[0]);
		}		
		$this->free_result($query_id);
		$this->lastquery = $query_string;
		return $returnarray;
	}

	function fetch_array($query_id, $type = DBARRAY_ASSOC)
	{
		// retrieve row
		return @mysql_fetch_array($query_id, $type);
	}

	function free_result($query_id)
	{
		// retrieve row
		return @mysql_free_result($query_id);
	}

	function query_first($query_string, $type = DBARRAY_ASSOC)
	{
		// does a query and returns first row
		$query_id = $this->query($query_string);
		$returnarray = $this->fetch_array($query_id, $type);
		$this->free_result($query_id);
		$this->lastquery = $query_string;
		return $returnarray;
	}

	function data_seek($pos, $query_id)
	{
		// goes to row $pos
		return @mysql_data_seek($query_id, $pos);
	}

	function num_rows($query_id)
	{
		// returns number of rows in query
		return mysql_num_rows($query_id);
	}

	function num_fields($query_id)
	{
		// returns number of fields in query
		return mysql_num_fields($query_id);
	}

	function field_name($query_id, $columnnum)
	{
		// returns the name of a field in a query
		return mysql_field_name($query_id, $columnnum);
	}

	function insert_id()
	{
		// returns last auto_increment field number assigned
		return mysql_insert_id($this->link_id);
	}

	function close()
	{
		// closes connection to the database

		return mysql_close($this->link_id);
	}

	function print_query($htmlize = true)
	{
		// prints out the last query executed in <pre> tags
		$querystring = $htmlize ? htmlspecialchars($this->lastquery) : $this->lastquery;
		echo "<pre>$querystring</pre>";
	}

	function escape_string($string)
	{
		// escapes characters in string depending on Characterset
		return mysql_escape_string($string);
	}

	function halt($msg)
	{
		if ($this->link_id)
		{
			$this->errdesc = mysql_error($this->link_id);
			$this->errno = mysql_errno($this->link_id);
		}
		// prints warning message when there is an error
		global $technicalemail, $bbuserinfo, $vboptions, $_SERVER;

		if ($this->reporterror == 1)
		{
			$sendmail_path = @ini_get('sendmail_path');
			if ($sendmail_path === '')
			{
				// no sendmail, so we're using SMTP to send mail
				$delimiter = "\r\n";
			}
			else
			{
				$delimiter = "\n";
			}

			$msg = preg_replace("#(\r\n|\r|\n)#s", $delimiter, $msg);
			$message  = 'Database error in ' . $this->appname . ' ' . $vboptions['templateversion'] . ":$delimiter$delimiter$msg$delimiter";
			$message .= 'mysql error: ' . $this->errdesc . "$delimiter$delimiter";
			$message .= 'mysql error number: ' . $this->errno . "$delimiter$delimiter";
			$message .= 'Date: ' . date('l dS of F Y h:i:s A') . $delimiter;
			$message .= "Script: http://$_SERVER[HTTP_HOST]" . SCRIPTPATH . $delimiter;
			$message .= 'Referer: ' . REFERRER . $delimiter;
			if ($bbuserinfo['username'])
			{
				$message .= 'Username: ' . $bbuserinfo['username'] . $delimiter;
			}
			$message .= 'IP Address: ' . IPADDRESS . $delimiter;

//			include_once('./includes/functions_log_error.php');
//			if (function_exists('log_vbulletin_error'))
//			{
//				log_vbulletin_error($message, 'database');
//			}

			if (!empty($technicalemail) and empty($vboptions['disableerroremail']))
			{
				@mail ($technicalemail, $this->appshortname . ' Database error!', $message, "From: $technicalemail");
			}

			echo "<html><head><title>$vboptions[bbtitle] Database Error</title>";
			echo "<style type=\"text/css\"><!--.error { font: 11px tahoma, verdana, arial, sans-serif; }--></style></head>\r\n";
			echo "<body></table></td></tr></table></form>\r\n";
			echo "<blockquote><p class=\"error\">&nbsp;</p><p class=\"error\"><b>There seems to have been a slight problem with the $vboptions[bbtitle] database.</b><br />\r\n";
			echo "Please try again by pressing the <a href=\"javascript:window.location=window.location;\">refresh</a> button in your browser.</p>";
			echo "<p class=\"error\">An E-Mail has been dispatched to our <a href=\"mailto:$technicalemail\">Technical Staff</a>, who you can also contact if the problem persists.</p>";
			echo "<p class=\"error\">We apologise for any inconvenience.</p>";

			if ($bbuserinfo['usergroupid'] == 6 or ($permissions['adminpermissions'] & CANCONTROLPANEL))
			{
				// display error message on screen
				echo "<form><textarea class=\"error\" rows=\"15\" cols=\"100\" wrap=\"off\">" . htmlspecialchars($message) . "</textarea></form></blockquote>";
			}
			else
			{
				// display hidden error message
				echo "</blockquote>\r\n\r\n<!--\r\n" . htmlspecialchars($message) . " -->";
			}

			echo "\r\n\r\n</body></html>";
			exit;
		}
	}
}

/*======================================================================*\
|| ####################################################################
|| # Nullified By CAT Team
|| # CVS: $RCSfile: db_mysql.php,v $ - $Revision: 1.1 $
|| ####################################################################
\*======================================================================*/
?>