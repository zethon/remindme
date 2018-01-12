#!/usr/bin/perl
use strict;


package UserManager;

use constant USER_REFRESH_TIMER	=> (60*45); # one an hour (until webpage is up)

use botLib;
use threads;
use threads::shared;
use Data::Dumper;
use XML::Simple;
use Net::MySQL;

my @users : shared;
my ($mysql,$thread);
my (@serviceNames) = botLib::SERVICES;

sub new {
	my $self = {};
	shift;
	die("$0 (",__LINE__,"): Options must be name=>value pairs (odd number supplied)") if (@_ % 2);

	my ($self) = {@_}; 

	$mysql = Net::MySQL->new(
  				hostname => botLib::REMINDER_DB_HOST,  
  				database => botLib::REMINDER_DB_NAME,
  				user     => botLib::REMINDER_DB_USER,
  				password => botLib::REMINDER_DB_PW
  				);
  					
	return(bless($self));
}

sub LoadUsers()
{
	my ($self) = shift;
	
	$mysql->query('SELECT * FROM system_users WHERE (bot = "'.$self->{BOTNAME}.'")');
	if ($mysql->is_error)
	{
		print (&botLib::getTimeString."LOADUSERS >> SQL ERROR (".$mysql->get_error_message.")\n");
		return (0,"SQL ERROR: ".$mysql->get_error_message);
	}
	
	my $record_set = $mysql->create_record_iterator;
	if (!defined($record_set))
	{
		print (&botLib::getTimeString."LOADUSERS >> RecordSet returned NULL!\n");
		return (0,"LOADUSERS: recordset returned NULL!\n");
	}	
	
	lock(@users);
	@users = ();
	while (my $record = $record_set->each) 
	{
		my %user : shared = {};
		$user{id} = $record->[0];
		$user{class} = $record->[1];
		$user{email} = $record->[2];
		$user{timezone} = $record->[8];
		$user{planid} = $record->[12];
		$user{contacts} = $self->LoadUserContacts($user{id});
		$user{dls} = $record->[13];
		#print Dumper($user{contacts});
				
#		$user{'contact-order'} = $record->[3];
#		#services (this feels bad because of concrete table order)
#		$user{aim} = ($record->[4] ne '') ? botLib::normalize($record->[4]) : undef;
#		$user{msn} = ($record->[5] ne '') ? lc($record->[5]) : undef;
#		$user{icq} = ($record->[6] ne '') ? $record->[6] : undef;
		

		
		push(@users,\%user);
		#print ("-------------------------------------------------------\n");
	}
	
	print (&botLib::getTimeString."LOADUSERS >> Seems to have worked.\n");
	return (1,'');
}

sub LoadUserContacts()
{
	my ($self,$userid) = @_;
	$mysql->query("SELECT * FROM system_contacts WHERE (userid = '$userid')");
	
	my $record_set = $mysql->create_record_iterator;
	if (!defined($record_set))
	{
		print (&botLib::getTimeString."LOADUSERCONTACTS >> RecordSet returned NULL!!!\n");
		return (0);
	}		
	
	my %rethash : shared = {};
	my $count = 0;
	while (my $record = $record_set->each) 
	{
		$rethash{$count."_service"} = $record->[2];
		$rethash{$count."_name"} = botLib::normalize($record->[3]);
		$rethash{$count."_order"} = $record->[4];
		$rethash{$count."_verified"} = $record->[5];
		
		$count++;
	}	
	
	return \%rethash;
}

sub GetUsers()
{
	my $self = shift;
	return @users;	
}

sub StartRefreshThread()
{
	my ($self) = shift;
	
	my $func = sub 
	{
		my $self = shift;
		while (1) 
		{ 
			sleep USER_REFRESH_TIMER;
			$self->LoadUsers();
		}
	};	
	
	$thread = threads->create($func,$self);
}

sub GetServiceNames()
{
	my ($self,$service) = @_;
	my (@retval);
	
	foreach my $loopvar (@users)
	{
		my %user = %{$loopvar};
		my %contacts = %{$user{contacts}};
		
		for (my $i = 0; $i < 3; $i++)
		{
			push(@retval,$contacts{$i."_name"})
				if ($contacts{$i."_service"} eq $serviceNames[$service] &&
						$contacts{$i."_name"} ne "");
		}
	}	
	return @retval;
}

sub GetUserByService()
{
	my ($self,$serviceid,$name) = @_;
	my $servicename = $serviceNames[$serviceid];
		
	foreach my $loopvar (@users)
	{
		my (%user) = %{$loopvar};
		my %contacts = %{$user{contacts}};

		for (my $i = 0; $i < 3; $i++)
		{		
			return $self->GetFriendlyUserObj(%user)
				if (($contacts{$i."_service"} eq $servicename) && ($contacts{$i."_name"} eq $name));
		}
	}
	return undef;
}

# not used
#sub GetScreenNameByUserId()
#{
#	my ($self,$serviceid,$userid) = @_;
#	my $servicename = $serviceNames[$serviceid];
#
#	foreach my $loopvar (@users)
#	{
#		my %user = %{$loopvar};
#
#		if (defined($user{$servicename}) && ($user{id} eq $userid))
#		{
#			return $user{$servicename};
#		}			
#	}
#	
#	return undef;		
#}

sub GetUserObjById()
{
	my ($self,$userid) = @_;
	
	foreach my $loopvar (@users)
	{
		my %user = %{$loopvar};
		return $self->GetFriendlyUserObj(%user)	if ($user{id} eq $userid);
	}
	
	return undef;			
}

sub GetFriendlyUserObj()
{
	my ($self,%userhash) = @_;
	my %conhash = %{$userhash{contacts}};
	my $retval;

	$retval->{id} = $userhash{id};
	$retval->{class} = $userhash{class};
	$retval->{email} = $userhash{email};
	$retval->{timezone} = $userhash{timezone};
	$retval->{planid} = $userhash{planid};
	$retval->{dls} = $userhash{dls};
	
	for (my $i = 0; $i < 3; $i++)
	{	
		my $contact = {};
		$contact->{service} = $conhash{$i."_service"};
		$contact->{name} = $conhash{$i."_name"};
		$contact->{order} = $conhash{$i."_order"};
		$contact->{verified} = $conhash{$i."_verified"};
		#${@{$retval->{contacts}}}[$conhash{$i."_order"}] = $contact;
		push(@{$retval->{contacts}},$contact);
		
	}
	
	return $retval;
}

sub GetNumAllowedReminders()
{
	my ($self,$planid) = @_;
	return 0 if (!defined($planid));
	
	$mysql->query("SELECT * FROM system_subscribe_plans WHERE (id = '$planid')");	
	if ($mysql->has_selected_record) 
	{
        my $record_set = $mysql->create_record_iterator;
    	while (my $record = $record_set->each)
    	{
    		return $record->[3];
    	}
    }	
}

1;