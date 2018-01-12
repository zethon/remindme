#!/usr/bin/perl
use strict;

package ReminderSystem;

use constant REFRESH_TIMER		=> (60*45); # once an hour until the webpage is up
use constant REMINDER_TIME_ZONE => '-0500';

use constant REMINDER_DB_HOST 	=> 'www.remindme.cc';
use constant REMINDER_DB_NAME 	=> 'reminder';
use constant REMINDER_DB_USER	=> 'zethon';
use constant REMINDER_DB_PW		=> '';


use botLib;
use threads;
use threads::shared;
use Thread::Bless;
use Net::MySQL;
use Data::Dumper;
use DateTime;
use Date::Parse;

my ($userman,$parser);
my ($mysql,$thread);
my @reminders : shared;

sub new {
	my $self = {};
	shift;
	die("$0 (",__LINE__,"): Options must be name=>value pairs (odd number supplied)") if (@_ % 2);

	my ($self) = {@_}; 
	
	$mysql = Net::MySQL->new(
  				hostname => REMINDER_DB_HOST,  
  				database => REMINDER_DB_NAME,
  				user     => REMINDER_DB_USER,
  				password => REMINDER_DB_PW
  				);

	$userman = $self->{USERMANAGER};
	$parser = $self->{PARSER};
	$self->{Callback} = {};  				
  				
	return(bless($self));
}

sub LoadReminders()
{
	my ($self) = @_;

	# list system reminders
	$mysql->query('SELECT * FROM system_reminders WHERE (bot = "'.$self->{BOTNAME}.'") and (delivered = \'0\')');
	#print('SELECT * FROM system_reminders WHERE (bot = "'.$self->{BOTNAME}.'") and (delivered = \'0\')');
	  
	my $record_set = $mysql->create_record_iterator;
	if (!defined($record_set))
	{
		print (&botLib::getTimeString."LOADREMINDERS >> RecordSet returned NULL!\n");
		return 0;
	}

	lock(@reminders);
	@reminders = ();
	
	while (my $record = $record_set->each) 
	{
		my %reminder : shared = {};
		$reminder{user} = $record->[1];
		
		# now storing user's date/time in the database, convert to our timezone
		# when loading and creating
		my ($userinf) = $userman->GetUserObjById($reminder{user});
		
		$parser->{FAILONPASSEDTIME} = 0;
		$parser->{RAWTIME} = $record->[2];
		my $rc2 = $parser->ParseTime($userinf->{timezone},$userinf->{dls});
		
		if ($rc2 == 0)
		{
			print ("FAILED PARSETIME() REMINDER ID [".$record->[5]."]\n");	
		}
		
		$reminder{datetime} = $parser->GetEpoch();
		#$reminder{datetime} = str2time($record->[2]." ".$userinf->{timezone});

		$reminder{msg} = $record->[3];
		$reminder{delivered} = 0;
		$reminder{id} = $record->[5];
		push(@reminders,\%reminder);
	}	
	
	print (&botLib::getTimeString."LOADREMINDERS >> Seems to have worked.\n");
	return 1;
}

sub DoOneLoop()
{
	my $self = shift;
	my ($now) = time();
	my ($count) = 0;
	
	foreach my $rem (@reminders)
	{
		my %reminder = %{$rem};
		if ($reminder{datetime} < $now && !$reminder{delivered})
		{
			my ($delivered) = 0;
			if ($self->if_callback_exists('on_reminder'))
			{
				$delivered = &{$self->{Callback}->{on_reminder}}($self,$reminder{id},$reminder{user},$reminder{datetime},$reminder{msg});
			}
			# safeguard against constant reminding if database problems occur
			# will mark the reminder in memory as delivered
			lock(@reminders);
			$reminders[$count]{delivered} = $delivered ? 1 : 0;
		}
		$count++;
	}
}

sub StartRefreshThread()
{
	my ($self) = shift;
	
	my $func = sub 
	{
		my $self = shift;
		while (1) 
		{ 
			sleep REFRESH_TIMER;
			$self->LoadReminders();
		}
	};	
	
	$thread = threads->create($func,$self);
}

sub GetReminders()
{
	my $self = shift;
	return @reminders;	
}

sub CreateReminder()
{
	my ($self,$userid,$datetime,$msg) = @_;
	my $userinf = $userman->GetUserObjById($userid);

	my %reminder : shared = {};
	$reminder{user} = $userid;
	$reminder{msg} = $msg;
	
	#$reminder{datetime} = str2time($datetime);
	my $thetime = str2time($datetime);
	if (!defined($thetime) && ($datetime !~ /.\d minute/i) && ($datetime !~ /.\d hour/i))
	{
		return (0,"Invalid Date/Time ($datetime). Please type -help for help or visit ".botLib::URL_INFOPAGE);
	}
	elsif (!defined($thetime))
	{
		my ($temp) = $datetime;
		my ($minnum) = $1 if ($temp =~ /(.\d) minute/i);

		$temp = $datetime;
		my ($hournum) = $1  if ($temp =~ /(.\d) hour/i);

		$reminder{datetime} = time();
		$reminder{datetime} += ($minnum *60) + ($hournum * (60*60));
	}
	else
	{
		$reminder{datetime} = $thetime;
	}	
	
	return (0,"Reminder date/time has already passed") if ($reminder{datetime} < time()); 
	#my $dt = DateTime->from_epoch( epoch => $reminder{datetime},time_zone  => REMINDER_TIME_ZONE);
	my $dt = DateTime->from_epoch( epoch => $reminder{datetime}, time_zone => $userinf->{timezone});
	my $remid = $reminder{id} = $self->pingReminderCount();
	
	$msg =~ s/\'/\\\'/g;
	#$mysql->query("INSERT INTO system_reminders (id,bot,user,datetime,msg) VALUES ('$remid','".$self->{BOTNAME}."','$userid','".$dt->ymd." ".$dt->hms."','$msg')");
	$mysql->query("INSERT INTO system_reminders (bot,user,datetime,msg) VALUES ('".$self->{BOTNAME}."','$userid','".$dt->ymd." ".$dt->hms."','$msg')");
	
	if ($mysql->is_error)
	{
		return (0,"SQL ERROR: ".$mysql->get_error_message);
	}

	lock(@reminders);
	push(@reminders,\%reminder);

	return (1,"Reminder scheduled for ".$dt->ymd." ".$dt->hms." (".$userinf->{timezone}.")");
}

sub CreateReminder2()
{
	my ($self,$userid,$datetime,$epoch,$msg) = @_;
	my $userinf = $userman->GetUserObjById($userid);

	my %reminder : shared = {};
	$reminder{delivered} = 0;
	$reminder{user} = $userid;
	$reminder{msg} = $msg;
	$reminder{datetime} = $epoch;
	
	my $remid = $reminder{id} = $self->pingReminderCount();
	
	$msg =~ s/\'/\\\'/g;
	#$mysql->query("INSERT INTO system_reminders (id,bot,user,datetime,msg) VALUES ('$remid','".$self->{BOTNAME}."','$userid','$datetime','$msg')");
	$mysql->query("INSERT INTO system_reminders (bot,user,datetime,msg) VALUES ('".$self->{BOTNAME}."','$userid','$datetime','$msg')");
	
	if ($mysql->is_error)
	{
		return (0,"SQL ERROR: ".$mysql->get_error_message);
	}

	lock(@reminders);
	push(@reminders,\%reminder);

	return (1,"Reminder scheduled for $datetime (".$userinf->{timezone}.")");
}

sub MarkReminderAsDelivered()
{
	my ($self,$remid) = @_;
	#print ("UPDATE system_reminders SET delivered = '1' WHERE (id = '$remid');\n");
	$mysql->query("UPDATE system_reminders SET delivered = '1' WHERE (id = '$remid');");	
}


sub set_handler {
  my ($self, %events) = @_;

  return unless (%events);

  foreach my $event (keys %events) {
    $self->{Callback}->{$event} = $events{$event};
  }
}

sub if_callback_exists {
  my ($self, $callback) = @_;

  return (defined $callback && defined $self->{Callback} &&
    defined $self->{Callback}->{$callback} &&
    ref $self->{Callback}->{$callback} eq 'CODE');
}


sub pingReminderCount()
{
	my ($self) = shift;
	$mysql->query("UPDATE system_reminder_count SET count=count+1;");
	$mysql->query("SELECT * FROM system_reminder_count;");
	
	my $record_set = $mysql->create_record_iterator;
	
	return 0 if (!defined($record_set));
	
	while (my $record = $record_set->each) 
	{
		return $record->[0];
	}	
	return 0;	
}

sub GetUsersTotalPendingReminders()
{
	my ($self,$user) = @_;
	$mysql->query("SELECT * FROM system_reminders WHERE (user = '$user') and (delivered = '0');");

	my $count = 0;
	my $record_set = $mysql->create_record_iterator;

	return 0 if (!defined($record_set));

	$count++ while (my $record = $record_set->each);
	return $count;
}

1;