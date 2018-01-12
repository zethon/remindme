#!/usr/bin/perl
use strict;

package MessageParser;

use DateTime;
use Date::Parse;
use Time::localtime;

use constant MP_ERROR_TIMEPASSED => 1;
use constant MP_ERROR_BADFORM => 2;

use constant PREPOSITIONS => ['on','in','at','next'];

sub new 
{
	my $self = {};
	shift;
	die("$0 (",__LINE__,"): Options must be name=>value pairs (odd number supplied)") if (@_ % 2);

	my ($self) = {@_}; 

	return(bless($self));
}


sub Parse()
{
	my ($self,$rawMessage,$usertz,$isdls) = @_;

	$self->{RAWMESSAGE} = $rawMessage;
	$self->{USERTIMEZONE} = $self->{BOTTIMEZONE} if (!defined($self->{USERTIMEZONE}));
	$self->{USERTIMEZONE} = $usertz if ($usertz ne '');
	$self->{USERDOESDLS} = defined($isdls) ? $isdls : 1;
		
	my ($rawTime,$rawMsgTxt);
	if ($rawMessage =~ /^remind\W+me\W+(.*?)\W*(?:to|that)\W+(.*)/i && $1 ne '')
	{
		$self->{RAWTIME} = $rawTime = $1;
		$self->{RAWMSGTXT} = $rawMsgTxt = $2;
	}
	elsif ($rawMessage =~ /(.*?)\W+remind\W+me\W+(?:to|that)\W+(.*)/i)
	{
		$self->{RAWTIME} = $rawTime = $1;
		$self->{RAWMSGTXT} = $rawMsgTxt = $2;
	}
	elsif ($rawMessage =~ /^remind\W+me\W+(?:to|that)\W+(.*)/i)
	{
		my $_preps = PREPOSITIONS;
		my @preps = @{$_preps};
		
		foreach my $prep (@preps)
		{
			if ($rawMessage =~ /^remind\W+me\W+to\W+(.*)\W+($prep.*)/i)
			{
				$self->{RAWTIME} = $rawTime = $2;
				$self->{RAWMSGTXT} = $rawMsgTxt = $1;
			}
		}
	}

	$self->{FAILONPASSEDTIME} = 1;
	return $self->ParseTime();
}

sub ParseTime()
{
	my ($self,$usertz,$userdls) = @_;
	$self->{USERTIMEZONE} = $usertz if (length($usertz)>0);
	$self->{USERDOESDLS} = $userdls if (defined($userdls));
	my $rawdate = $self->{RAWTIME};
		
	my ($sec,$min,$hour,$day,$month,$year,$dow,$doy, $dst) = localtime(time());

	my $temp_dt = DateTime->from_epoch(epoch => time(), time_zone  => $self->{BOTTIMEZONE});
	
	if ($rawdate =~ /(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten) minute/i || 
		$rawdate =~ /(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten) hour/i ||
		$rawdate =~ /(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten) day/i)
		#$rawdate =~ /(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten) week/i ||
		#$rawdate =~ /(?:\d+|one|two|three|four|five|six|seven|eight|nine|ten) month/i)
	{
		my $tepoch = time();
		if (($self->{USERDOESDLS} && !$dst) || ($dst && !$self->{USERDOESDLS}))
		{
			# when the server went from DST to EST this was commented out
			# to fix a nasty bug, will it have to be put back in when we
			# go back to DST?
			#$tepoch += 3600;
		}	
		
		my $tdate = DateTime->from_epoch(epoch => $tepoch, time_zone  => $self->{USERTIMEZONE});
		
		$rawdate =~ s/\W*one\W+/ 1 /ig;	$rawdate =~ s/\W*two\W+/ 2 /ig; $rawdate =~ s/\W*three\W+/ 3 /ig;
		$rawdate =~ s/\W*four\W+/ 4 /ig; $rawdate =~ s/\W*five\W+/ 5 /ig; $rawdate =~ s/\W*six\W+/ 6 /ig;
		$rawdate =~ s/\W*seven\W+/ 7 /ig; $rawdate =~ s/\W*eight\W+/ 8 /ig; $rawdate =~ s/\W*nine\W+/ 9 /ig;
		$rawdate =~ s/\W*ten\W+/ 10 /ig;
		
		
		my ($temp) = $rawdate;
		my ($minnum) = $1 if ($temp =~ /(\d+) minute/i);
	
		$temp = $rawdate;
		my ($hournum) = $1  if ($temp =~ /(\d+) hour/i);

		$temp = $rawdate;
		my ($daynum) = $1  if ($temp =~ /(\d+) day/i);
				
		$tdate->add(minutes=>$minnum) if ($minnum > 0);
		$tdate->add(hours=>$hournum) if ($hournum > 0);
		$tdate->add(days=>$daynum) if ($daynum > 0);
		$rawdate = $tdate->ymd." ".$tdate->hms;
	}
	elsif ($rawdate =~ /tomorrow/i)
	{
		my $daymod = $self->GetDayModifier();
		
		if ($daymod == 0) {	$temp_dt->add(days => 1 );	}
		elsif ($daymod == 1) {	$temp_dt->add(days => 2 );	}
		
		my $ret =  $temp_dt->ymd;
		$rawdate =~ s/tomorrow/$ret/g;
	}
	elsif ($rawdate =~ /(sunday|monday|tuesday|wednesday|thursday|friday|saturday)/i)
	{
		my $theday = $1;
		my $curday;
		$curday = 1 if ($1 eq 'monday');
		$curday = 2 if ($1 eq 'tuesday');
		$curday = 3 if ($1 eq 'wednesday');
		$curday = 4 if ($1 eq 'thursday');
		$curday = 5 if ($1 eq 'friday');
		$curday = 6 if ($1 eq 'saturday');
		$curday = 7 if ($1 eq 'sunday');			
		
		$curday += 7 if ($curday <= $temp_dt->day_of_week);
		
		$temp_dt->add(days => $curday - $temp_dt->day_of_week);
		my $ret =  $temp_dt->ymd;
		$rawdate =~ s/$theday/$ret/g;
	}	
	
	$rawdate =~ s/^\s+//;
	$rawdate =~ s/\s+$//;
	$rawdate =~ s/on//gi;
	$rawdate =~ s/at//gi;
	$rawdate =~ s/noon/pm/gi;
	$rawdate =~ s/midnight/am/gi;
	
	$rawdate = $self->InsertDate($rawdate) if (!$self->HasDate($rawdate));
	$rawdate .= " ".$temp_dt->year if ($rawdate !~ /200\d{1}\W/);
		
	$self->{RAWPARSEDDATE} = $rawdate;
	my $epochtime = str2time($rawdate,$self->{USERTIMEZONE});

	if (defined($epochtime))
	{
		my $retdt = DateTime->from_epoch(epoch => $epochtime, time_zone  => $self->{USERTIMEZONE});
		
		if (($self->{USERDOESDLS} && !$dst) || ($dst && !$self->{USERDOESDLS}))
		{
			# when the server went from DST to EST this was commented out
			# to fix a nasty bug, will it have to be put back in when we
			# go back to DST?
			#$epochtime -= 3600;
		}
		
		$self->{USERTIMESTRING} = $retdt->ymd." ".$retdt->hms;
		$self->{MSGEPOCH} = $epochtime;
		
		my $temp = DateTime->from_epoch(epoch => $epochtime, time_zone  => $self->{BOTTIMEZONE});
		$self->{SERVERTIMESTRING} = $temp->ymd." ".$temp->hms;
	}
	else
	{
		$self->{LASTERROR} =  MP_ERROR_BADFORM;
		return 0;
	}	


	if ($self->{MSGEPOCH} < time() && $self->{FAILONPASSEDTIME})
	{
			$self->{LASTERROR} = MP_ERROR_TIMEPASSED;
			return 0;
	}
	
	return 1;
}

# 0 - user is on the same day as us
# 1 - user is a day ahead
# -1 - user is a day behind
sub GetDayModifier()
{
	my ($self) = @_;
	
	my $curtime = DateTime->from_epoch(epoch => time(), time_zone  => $self->{BOTTIMEZONE});
	my $diff = $self->{BOTTIMEZONE} - $self->{USERTIMEZONE};
	my $today = $curtime->day;
	
	return 0 if ($diff == 0);
	
	my $absdiff = abs($diff);
	$absdiff = "0".$absdiff if ($absdiff < 1000);	
	
	# user is ahead of us
	if ($diff < 0)
	{
		my $hours = substr($absdiff,0,2);
		my $mins = substr($absdiff,2,2);
		
		$curtime->add(hours=>$hours,minutes=>$mins);
		
		return 1 if ($curtime->day != $today);
	}
	# the user is behind us
	elsif ($diff > 0)
	{
		my $hours = substr($absdiff,0,2);
		my $mins = substr($absdiff,2,2);
		
		$curtime->subtract(hours=>$hours,minutes=>$mins);
		
		return -1 if ($curtime->day != $today);		
	}
	
	return 0;
}

sub HasDate()
{
	my ($self,$datestr) = @_;
	
	return $datestr =~ /jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec|\\|\/|-/i;	
}

sub InsertDate()
{
	my ($self,$datestr) = @_;
	
	my $curtime = DateTime->from_epoch(epoch => time(), time_zone  => $self->{BOTTIMEZONE});
	my $daymod = $self->GetDayModifier();
	
	if ($daymod == 0)
	{
		$datestr = $curtime->ymd." $datestr";
	}
	elsif ($daymod == 1)
	{
		$curtime->add(days=>1);
		$datestr = $curtime->ymd." $datestr";
	}
	elsif ($daymod == -1)
	{
		$curtime->subtract(day=>1);
		$datestr = $curtime->ymd." $datestr"
	}

	return $datestr;	
}

sub GetUserTimeString()
{
	my ($self) = @_;
	return $self->{USERTIMESTRING};
}

sub GetServerTimeString()
{
	my ($self) = @_;
}

sub GetEpoch()
{
	my ($self) = @_;
	return $self->{MSGEPOCH};
}

1;