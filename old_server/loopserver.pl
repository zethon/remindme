#!/usr/bin/perl
use strict;

use constant MAX_CRASHES => 100;

my ($command) = 'perl server.pl '.join(' ',@ARGV);
my ($crashes) = 0;

open (FILE,">>crashlog.txt");
print FILE &getTimeString."LOOPSERVER STARTED ($command)\n";
close (FILE);

while (1)
{
	my($retval) =  (system($command)) >> 8;
	if ($retval == 155)
	{
		open (FILE,">>crashlog.txt");
		print FILE &getTimeString."Server shut down gracefully.\n";
		close (FILE);
		last;
	}
	if ($retval == 200)
	{
		open (FILE,">>crashlog.txt");
		print FILE &getTimeString."Server restarted.\n";
		close (FILE);
	}
	else
	{
		$crashes++;
		open (FILE,">>crashlog.txt");
		print FILE &getTimeString."crash number $crashes\n";
		close (FILE);
		
		if ($crashes == MAX_CRASHES)
		{
			open (FILE,">>crashlog.txt");
			print FILE &getTimeString."MAX NUMBER OF CRASHES HAS OCCURED, SHUTTING DOWN SERVER\n";
			close (FILE);
			die;
		}
	}
}
# end of main functionality

sub getTimeString()
{
	# ugly time functions
	my ($Sec,$Min,$Hour,$Day,$Month,$Year,$Week_Day) = (localtime()); 
	if ($Min  < 10) { $Min = "0".$Min; }
	if ($Hour < 10) { $Hour = "0".$Hour; }
	if ($Day < 10) { $Day = "0".$Day; }		
	$Month++;
	$Year += 1900;

	return "($Year/$Month/$Day $Hour$Min): ";
}
