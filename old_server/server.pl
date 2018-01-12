#!usr/perl/bin
use strict;
use Net::AIM;
use Net::MSN;
use Net::vICQ;
use MIME::Lite;
use Term::ReadKey;
use Data::Dumper;

# custom packages
use botLib;
use UserManager;
use ReminderSystem;
use MessageParser;

require 'botinit.pl';

# load configuration for bot
# load user list
# load reminders
# create connection objects
# start main loop

# globals
my (@serviceNames) = botLib::SERVICES;

my ($g_botname,$g_botfile,$g_debug) = &parseCmdLn();
die "$g_botname is already running...\n" if (IsRunning($g_botname));
my ($g_bot) = &LoadBot($g_botfile,$g_botname);

# create message parser
my $parser = new MessageParser(BOTTIMEZONE => botLib::BOT_TIME_ZONE);

# load user manager
my ($g_userman) = new UserManager(BOTNAME=>$g_botname);
my ($rc,$error) = $g_userman->LoadUsers();
die "$error" if (!$rc);
$g_userman->StartRefreshThread();

# load reminder system
my ($g_reminders) = new ReminderSystem(BOTNAME=>$g_botname,USERMANAGER=>$g_userman, PARSER=>$parser);
$g_reminders->set_handler('on_reminder',\&on_reminder);


# connections
my ($aim,$msn,$icq,$yahoo) = undef; # connection objects
my ($g_aimbuds,$g_msnbuds,$g_icqbuds) = undef; # online buddy arrays
my (%g_blocked) = undef; # hash for non users who already im'd the bot

my ($aiminfo,$msninfo,$icqinfo,$yahooinfo) = undef;

# load bot connection info
foreach (@{$g_bot->{connections}->{connection}})
{
	#print ("[$_->{type}]\n");
	my ($password) = $_->{password}->{content};
	
	if ($_->{type} eq 'aim')
	{
		$aiminfo->{name} = $_->{'screen-name'}->{content};
		$aiminfo->{password} = $password;
		
		$aim = new Net::AIM;
		$aim->newconn(Screenname=>$aiminfo->{name},Password=>$aiminfo->{password}) or $aim = undef;
		my $conn = $aim->getconn();
		$conn->set_handler('im_in',\&on_aim_im);
		$conn->set_handler('config',\&on_aim_signon);
		$conn->set_handler('update_buddy',\&on_aim_update_buddy);
		$conn->set_handler('eviled',\&on_aim_eviled);
	}
	elsif ($_->{type} eq 'msn')
	{
		$msninfo->{name} = $_->{'passport-name'}->{content};
		$msninfo->{password} = $password;

		$msn = new Net::MSN;
		$msn->set_event(
  				on_connect => \&on_msn_connect,
  				on_message => \&on_msn_message,
  				on_buddyupdate => \&on_msn_update_buddy,
		);		
		
	}
	elsif ($_->{type} eq 'icq')
	{
		
		$icqinfo->{name} = $_->{'uin'}->{content};
		$icqinfo->{password} = $password;
		$icq = Net::vICQ->new($icqinfo->{name}, $icqinfo->{password}, 1);
		$icq->{_Hide_IP} = 1;
		
		$icq->Add_Hook("Srv_Mes_Received", \&on_icq_msg);
		$icq->Add_Hook("Srv_GSC_Ready", \&on_icq_connect);
		$icq->Add_Hook("Srv_BLM_Contact_Online", \&on_icq_update_buddy);
		$icq->Add_Hook("Srv_BLM_Contact_Offline", \&on_icq_update_buddy);		
	} # icq configuration
	
}

# connect and start main loop
# aim's connect() is implied with do_one_loop
$g_reminders->LoadReminders();
$g_reminders->StartRefreshThread();

$icq->Connect();
$msn->connect($msninfo->{name}, $msninfo->{password});

my $key;
ReadMode 1;	
while (1)
{
	$aim->do_one_loop();
	$msn->check_event();	
	$icq->Execute_Once();	
	$g_reminders->DoOneLoop();
	if (defined ($key = ReadKey(-1)))
	{
		listUsers() if ($key eq 'l');
		listReminders() if ($key eq 'r');
		print ("CURRENT EPOCH: [".time()."]\n") if ($key eq 'e');
		last if ($key eq 'q');
	}	
}
ReadMode 0;
exit 155;
# end of main functionality

sub listUsers()
{
	my (@users) = $g_userman->GetUsers();
		
	print ("\nUSERS\n");
	foreach my $loopvar (@users)
	{
		my %user = %{$loopvar};
		print ("$user{id}\n");	
	}
	print "\n";
}

sub listReminders()
{
	my @reminders = $g_reminders->GetReminders();
	
	print ("\nREMINDERS\n");
	foreach my $remref (@reminders)
	{
		my %reminder = %{$remref};
		print ($reminder{id}.":".$reminder{datetime}.":".$reminder{user}.":".$reminder{msg}.":".$reminder{delivered}."\n");
	}
	print "\n";
}

### im help function 
sub help {
	return "\nRemindMe Bot Help (Updated June 12th, 2004)\n\n".
		   "formats: remind me [date/time] to [message]\n".
		   "         [date/time] remind me to [message]\n".
		   "         remind me to [message] [date/time]\n\n".
		   "Users MUST specify some time notation. If no ".
		   "date is specified, today's date is used. ".
		   "RemindMe understands many date formats. Colons ".
		   "are required when specifying a time in HH:MM ".
		   "format. Use AM or PM or neither to notate ".
		   "military time.\n\nYour default timezone and daylight ".
		   "savings time settings are configured through ".
		   botLib::URL_INFOPAGE." Please do not include a time zone in ".
		   "your reminder.\n\n".
		   "Examples:\n".
		   "remind me tomorrow at 3pm to find a new job\n".
		   "in 10 minutes remind me to check the oven\n".
		   "remind me in 2 hours and 15 minutes that the Sopranos is on\n".
		   "remind me to go to the dentist at 2pm\n".
		   "remind me on 02/04/2004 at 3:15pm that it's mom's b-day\n".
		   "on 04 feb 04 at 14:05 remind me to go work\n\n".
		   "For more info visit: ".botLib::URL_INFOPAGE;
}
		   
### connection handlers
sub on_msn_connect {
	print (&botLib::getTimeString."MSN Connected....\n");
	&on_uni_connect(botLib::SERVICE_MSN);
}

sub on_aim_signon()
{
	print (&botLib::getTimeString."AIM Connected...\n");
	$aim->set_info("Hello, I am a RemindMe bot. For more information please see ".botLib::URL_INFOPAGE);
	&on_uni_connect(botLib::SERVICE_AIM);
}


sub on_icq_connect()
{
	print (&botLib::getTimeString."ICQ Connected...\n");
	&on_uni_connect(botLib::SERVICE_ICQ);
}

## incoming im handlers
sub on_msn_message 
{
	my ($sb, $chandle, $friendly, $message) = @_;
	on_uni_message(botLib::SERVICE_MSN,$chandle,$message);
}


sub on_aim_im 
{
	my ($self, $evt, $from, $to) = @_;
	my $args = $evt->args();
	my ($nick, $auto, $msg) = @$args;
	$msg =~ s/<[^>]*>//g;
	&on_uni_message(botLib::SERVICE_AIM,$aim->normalize($from),$msg) unless (lc($auto) eq 't');
}

sub on_icq_msg {
    my($Object, $details) = @_;
    
    return unless (lc($details->{'MessageType'}) eq 'normal' || 
    				lc($details->{'MessageType'}) eq 'text_message');
    my ($sender) = $details->{'Sender'};
    my ($msg) = $details->{'text'};

 	$msg =~ s/^[ \t]+//;
 	$msg =~ s/[ \t]+$//;
 	$msg =~ s/\x00//g;

    on_uni_message(botLib::SERVICE_ICQ,$sender,$msg);
}

sub on_aim_eviled()
{
	my ($self, $evt, $from, $to) = @_;
	#my ($level, $culprit) = $evt->args;
	my ($userinf) = undef;

	if ($from eq '')
	{
		$from = 'An anonymous user';
	}
	else
	{
		$from = $self->normalize($from);
		$userinf = $g_userman->GetUserByService(botLib::SERVICE_AIM,$from);
	}
	
	# warned by a user
	if (defined($userinf))
	{
		on_uni_message(botLib::SERVICE_AIM,$from,'Please do not warn this bot. The system administrator has been notified.');
	}
	# warned by an unknown
	else
	{
		#should we hit them back twice??
		if ($from ne '') 
		{
			$self->evil($from);
			$self->evil($from);
			$g_blocked{$from} = 255;
		}		
	}
	
	print (&botLib::getTimeString."EVILED (aim) << $from \n");	
}

sub on_aim_update_buddy
{
   my ($self, $evt, $from, $to) = @_;
   my ($nick) = $from;

   my ($bud, $online, $evil, $signon_time, $idle_amount, $user_class) = @{$evt->args()};
   &on_uni_update_buddy(botLib::SERVICE_AIM,$aim->normalize($bud),$online eq 'T');
}

sub on_msn_update_buddy
{
	my ($sb,$username,$fname,$status) = @_;
	&on_uni_update_buddy(botLib::SERVICE_MSN,$username,$status eq 'NLN');
}

sub on_icq_update_buddy
{
	my ($object,$details) = @_;
	
	my $uin = $details->{'Sender'};
	my $status = lc($details->{'Status'});
	
	&on_uni_update_buddy(botLib::SERVICE_ICQ,$uin,$status eq 'online');
}

# universal functions for all services
sub on_uni_connect
{
	my ($serviceid) = @_;
	
	# add icq users to buddy list
	# PROBLEM: -loadusers will leave removed users on the 'buddy list'
	# SOLUTION: find a way to reset the buddy list generated at ICQ's login
	if ($serviceid == botLib::SERVICE_ICQ)
	{
		my @icqlist = $g_userman->GetServiceNames(botLib::SERVICE_ICQ);
		my %contacts;
		
		foreach (@icqlist)
		{
			$contacts{$_}{name} = $_;
			$contacts{$_}{aliases} = [];
		}
	
		my ($details);
		my @uins = ();
		foreach (keys %contacts)
		{
			push(@uins,$_);
		}
		$details->{ContactList} = \@uins;
		$icq->Send_Command("Cmd_CTL_UploadList", $details);
	}
	else
	{
		# add msn and aim users to buddy list
		foreach my $user ($g_userman->GetServiceNames($serviceid))
		{
			uni_add_buddy($serviceid,$user);
		}
	}	
}

sub on_uni_update_buddy
{
	my ($serviceid,$buddy,$online) = @_;
	my ($oldvalue);

	if ($serviceid == botLib::SERVICE_AIM)
	{
		$oldvalue = $g_aimbuds->{$buddy};
		$g_aimbuds->{$buddy} = $online;
	}
	elsif ($serviceid == botLib::SERVICE_MSN)
	{
		$oldvalue = $g_msnbuds->{$buddy};
		$g_msnbuds->{$buddy} = $online;
	}	
	elsif ($serviceid == botLib::SERVICE_ICQ)
	{
		$oldvalue = $g_icqbuds->{$buddy};
		$g_icqbuds->{$buddy} = $online;
	}
	# aim sends a lot of updat_buddy messages
	#print (&botLib::getTimeString."UPBUD (".$serviceNames[$serviceid].") << $buddy ($online)\n");		
	
	if ($oldvalue != $online)
	{
		if ($online)
		{
			print (&botLib::getTimeString."UPBUD (".$serviceNames[$serviceid].") << $buddy ($online) Signed on.\n");		
		}
		else
		{
			print (&botLib::getTimeString."UPBUD (".$serviceNames[$serviceid].") << $buddy ($online) Signed off.\n");		
		}
		
	}
}

# incoming im 
sub on_uni_message
{
	my ($serviceid,$user,$message) = @_;
	
	print (&botLib::getTimeString."INMSG (".$serviceNames[$serviceid].") << $user : $message\n");
	
	my ($userinf) = $g_userman->GetUserByService($serviceid,$user);
	
	# IM from a non-user
	if (!defined($userinf))
	{
		# random people will only get two responses from the bot (until a restart)
		$g_blocked{$user}++;
		if ($g_blocked{$user} <= 10)
		{
			&uni_send_im($serviceid,$user,'Hello, I am a RemindMe bot. Please go to '.botLib::URL_INFOPAGE." for more information.\n\nIf you have already registered, please be patient and IM me again in 15-20 minutes. This message will change once I have received your user information.");
		}

		return;
	}
	
	if (lc($message) eq '-help' || lc($message) eq 'help')
	{
		&uni_send_im($serviceid,$user,&help);
	}
	elsif (lc($message) eq '-loadusers' && ($userinf->{class} eq 'admin'))
	{
		my ($rc,$msg) = $g_userman->LoadUsers();	
		if (!$rc)
		{
			&uni_send_im($serviceid,$user,$msg);
		}
		else 
		{
			&on_uni_connect(botLib::SERVICE_AIM);
			&on_uni_connect(botLib::SERVICE_MSN);
			&on_uni_connect(botLib::SERVICE_ICQ);
			&uni_send_im($serviceid,$user,'Users successfully loaded.');
		}
			
	}
	elsif (lc($message) eq '-restart' && ($userinf->{class} eq 'admin'))
	{
		exit 200;			
	}	
	elsif (lc($message) eq '-shutdown' && ($userinf->{class} eq 'admin'))
	{
		exit 155;			
	}
	elsif (lc($message) eq '-loadreminders' && ($userinf->{class} eq 'admin'))
	{
		$g_reminders->LoadReminders();
		&uni_send_im($serviceid,$user,'Reminder refresh executed. Sucessfully?');
	}	
	elsif ($message =~ /remind me/i)
	{
		my $rc = $parser->Parse($message,$userinf->{'timezone'},$userinf->{'dls'});

		if ($rc == 0)
		{
	 		if ($parser->GetEpoch() < time())
 			{
 				&uni_send_im($serviceid,$user,"Reminder time has already passed. If typing a type (ie. 5:00) make sure to denote AM or PM.");	
 			}
			else
			{
 				&uni_send_im($serviceid,$user,"Invalid time specification");	
 			}
 			
 			return;
		}
		

		my $pending_rems = $g_reminders->GetUsersTotalPendingReminders($userinf->{id});
		my $allowed_rems = $g_userman->GetNumAllowedReminders($userinf->{planid});
		
		if ($pending_rems < $allowed_rems)
		{
			#my ($rc,$retmsg) = $g_reminders->CreateReminder($userinf->{id},$rawdate,$msg);
			my ($rc,$retmsg) = $g_reminders->CreateReminder2($userinf->{id},$parser->GetUserTimeString(),$parser->GetEpoch(),$parser->{RAWMSGTXT});
			&uni_send_im($serviceid,$user,$retmsg);
		}
		else
		{
			&uni_send_im($serviceid,$user,"You have exceeded your number of allowed pending reminders. Please visit ".botLib::URL_INFOPAGE." for information on upgrading your account.");
		}
	}
	elsif ($message =~ /^thank/i || $message =~ /^thanx/i || $message =~ /^thx/i || lc($message) eq 'ty')
	{
		&uni_send_im($serviceid,$user,"You're welcome.");
	}
	else
	{
		&uni_send_im($serviceid,$user,"Hello $userinf->{id}. Please type -help for help.");
	}
}

sub uni_send_im
{
	my ($service,$username,$msg) = @_;
	print (&botLib::getTimeString."OUTMSG (".$serviceNames[$service].") >> $username : $msg\n");	
	
	if ($service == botLib::SERVICE_AIM)
	{
		$aim->send_im($username,$msg);
	} 
	elsif ($service == botLib::SERVICE_MSN)
	{
		$msn->sendmsg($username,$msg);
	}
	elsif ($service == botLib::SERVICE_ICQ)
	{
  		my $details;
  		$details->{uin} = $username;
  		$details->{text} = $msg;
  		$details->{MessageType} = "text";
    	$icq->Send_Command("Cmd_Send_Message", $details);  
	}
}

sub uni_add_buddy
{
	my ($service,$buddy) = @_;
	if ($service == botLib::SERVICE_AIM)
	{
		$aim->add_buddy(1,'Buddies',$buddy);
	} 
	elsif ($service == botLib::SERVICE_MSN)
	{
		$msn->buddyaddfl($buddy, $buddy);
	  	$msn->buddyaddal($buddy, $buddy);
	}
	elsif ($service == botLib::SERVICE_ICQ)
	{
  		my $details;
  		$details->{uin} = $buddy;

		$icq->Send_Command("Cmd_Add_List", $details);		
	}

	print (&botLib::getTimeString."ADDBUD (".$serviceNames[$service].") >> $buddy\n");	
}

sub uni_is_buddy_online
{
	my ($serviceid,$buddy) = @_;

	return 0 unless (defined($buddy));
	if ($serviceid == botLib::SERVICE_AIM)
	{
		return 0 unless (defined($g_aimbuds->{$buddy}));
		return $g_aimbuds->{$buddy};
	}	
	elsif ($serviceid == botLib::SERVICE_MSN)
	{
		return 0 unless (defined($g_msnbuds->{$buddy}));
		return $g_msnbuds->{$buddy};
	}		
	elsif ($serviceid == botLib::SERVICE_ICQ)
	{
		return 0 unless (defined($g_icqbuds->{$buddy}));
		return $g_icqbuds->{$buddy};
	}		
	return 0;
}

sub on_reminder()
{
	my ($object,$remid,$userid,$datetime,$msg) = @_;
	my ($user) = $g_userman->GetUserObjById($userid);
	return if (!defined($user));
	
	foreach my $contact (@{$user->{contacts}})
	{
		my ($servnum) = botLib::SERVICE_INDX->{$contact->{service}};
		
		if (&uni_is_buddy_online($servnum,$contact->{name}))
		{
			&uni_send_im($servnum,$contact->{name},"Reminder: $msg");
			$g_reminders->MarkReminderAsDelivered($remid);
			
			if ($user->{archive_reminders} || $user->{class} eq 'admin')
			{
				&botLib::SendTemplateEmail($user->{email},'RemindMe Reminder',
						'templates\reminder-email-default.html',REMINDER=>$msg);
			}
			
			return 1;
		}		
	}
	
	my ($now) = time();
	
	# send email and mark the reminder as delivered
	if (($now - $datetime) > 7200)
	{
		print (&botLib::getTimeString."SENDMAIL (".$user->{email}.") >> Reminder : $msg\n");

		&botLib::SendEmail($user->{email},'RemindMe Reminder Alert',
				"We did not see you online, so we are sending your reminder to you through email.\n".
				"\nReminder: $msg\n");
				
		$g_reminders->MarkReminderAsDelivered($remid);
		return 1;
	}
	
	return 0;
}

