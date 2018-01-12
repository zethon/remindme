# Searches through the reminder database and looks for people with improperly 
# configured MSN 

#!/usr/bin/perl
use strict;
use Net::MySQL;
use Data::Dumper;

use constant REMINDER_DB_HOST 	=> 'www.remindme.cc';
use constant REMINDER_DB_NAME 	=> 'reminder';
use constant REMINDER_DB_USER	=> 'zethon';
use constant REMINDER_DB_PW		=> '';

my $mysql = Net::MySQL->new(
			hostname => REMINDER_DB_HOST,  
			database => REMINDER_DB_NAME,
			user     => REMINDER_DB_USER,
			password => REMINDER_DB_PW
			);
			
$mysql->query('SELECT * FROM system_contacts WHERE (service = "msn") and (login != "0")');	
my $record_set = $mysql->create_record_iterator;

if (!defined($record_set))
{
	print ("RecordSet returned NULL!\n");
	die;
}		

my @violators;

while (my $record = $record_set->each) 
{
	my $userid = $record->[1];
	my $login = $record->[3];
	
	$mysql->query('SELECT email FROM system_users WHERE (id = "'.$userid.'")');
	my $emailset = $mysql->create_record_iterator;
	
	if (!defined($emailset))
	{
		print ("($userid) has no emailset\n");
		next;		
	}
	
	my $emailrec = $emailset->each;
	my $email = $emailrec->[0];
	
	if ($login !~ /\@/)
	{
		print ("-------------------\nInvalid Email\nid:($userid) email:($email) login($login)\n--------------\n");
		push (@violators,$email);
	}
}

my $emailto;

foreach (@violators)
{
	$emailto .= "$_,";	
}

chop($emailto);
print ("TO EMAIL: ($emailto)\n\n");

