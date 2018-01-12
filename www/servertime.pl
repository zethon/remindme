# short script called by the RemindMe PHP webpage front end it receives
# a server-formatted time string, the user's time zone, and a boolean 
# indicating if the user uses day lights savings time. it will print out
# server time string

#!/usr/bin/perl
use strict;
use DateTime;
use Date::Parse;
use Time::localtime;

my $servertz = '-0500';

#print "[[(".$ARGV[0].")(".$ARGV[1].")(".$ARGV[2].")]]";
print ParseTime($ARGV[0],$ARGV[1],$ARGV[2] eq "1" ? 1 : 0);

sub ParseTime()
{
	my ($rawdate,$usertz,$userdls) = @_;
	my ($sec,$min,$hour,$day,$month,$year,$dow,$doy, $dst) = localtime(time());
	my $epochtime = str2time($rawdate,$usertz);

	if (defined($epochtime))
	{
		my $retdt = DateTime->from_epoch(epoch => $epochtime, time_zone  => $usertz);
		
		if (($userdls && !$dst) || ($dst && !$userdls))
		{
			# when the server went from DST to EST this was commented out
			# to fix a nasty bug, will it have to be put back in when we
			# go back to DST?
			#$epochtime -= 3600;
		}
		
		my $temp = DateTime->from_epoch(epoch => $epochtime, time_zone  => $servertz);
		return $temp->ymd." ".$temp->hms;
	}
	
}
