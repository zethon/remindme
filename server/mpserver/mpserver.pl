#!/usr/bin/perl
use strict;
use Getopt::Std;
use URI::Escape;
use HTTP::Daemon;
use HTTP::Status;
use HTTP::Response;
use MessageParser;
use Win32::Semaphore;

use constant XML_CONFIG => 'mpserver.xml';
use constant BOT_TIME_ZONE => '-0500';
use constant BOT_DSL => 1;	# needs to be updated every DSL switch?
use constant SEMAPHORE_NAME => 'xx_remindme_mpserver_xx';

my $semaphore = Win32::Semaphore->open(SEMAPHORE_NAME);
die "mpserver is already running...\n" if ($semaphore);

$semaphore = Win32::Semaphore->new(0, 1,SEMAPHORE_NAME);
$semaphore = Win32::Semaphore->open(SEMAPHORE_NAME);

my %opts = undef;
getopts('p:t:',\%opts);

my $port = defined($opts{'p'}) ? $opts{'p'} : &randomNumber(1024,0);
my $path = defined($opts{'t'}) ? $opts{'t'} : &randomNumber(1024,0);

$port = &randomNumber(1024,0) while ($port == 123);

# print the config file
open (FILE,">".XML_CONFIG);
print FILE "<mpserver_config>\n";
print FILE "\t<port>$port</port>\n";
print FILE "\t<path>$path</path>\n";
print FILE "</mpserver_config>\n";
close FILE;	

my $d = new HTTP::Daemon
    LocalAddr => 'localhost',
    LocalPort => $port;
    
print "mpserver URL:<", $d->url,"$path>\n";
while (my $c = $d->accept) {
  my $r = $c->get_request;
  if ($r) 
  {
      if ($r->method eq 'GET' && $r->url->path eq "/".$path) 
      {
      	&DispatchQuery($c,$r->url->query);
      } 
      elsif ($r->method eq 'GET' && $r->url->path eq "/ping") 
      {
		my $res = HTTP::Response->new(200, "OK");
		$res->content("OK");
		$c->send_response($res);
      }
      else 
      {
          $c->send_error(RC_FORBIDDEN)
      }
  }
  $c = undef;  # close connection
}

sub DispatchQuery()
{
	my ($con,$query) = @_;	
	my $sendstr;
	my %data;
	
	
	foreach my $param (split(/\&/,$query))
	{
		my ($idx,$val) = split(/=/,$param);
		$data{$idx} = $val;
	}

		my $action = $data{'action'};
		#print ("[[[[[[$action]]]]]]]]]]");

	
	if (!defined($data{'msg'}) || !defined($data{'tz'}) || !defined($data{'action'}) || !defined($data{'dls'}))
	{
		# should log this...
		print ("Something undefined, hack attempt?\n");
		$con->send_error(RC_FORBIDDEN);
		return;
	}
	else
	{
		
		my $parser = new MessageParser(BOTTIMEZONE =>BOT_TIME_ZONE, BOTDSL => BOT_DSL);
		
		my $rc = 0;
	
		if ($data{'action'} eq 'creation_parse')
		{
			$rc = $parser->CreationParse(uri_unescape($data{'msg'}),$data{'tz'},$data{'dls'});
		}
		elsif ($data{'action'} eq 'repeat_parse')
		{
			$rc = $parser->RepeatParse(uri_unescape($data{'msg'}),$data{'tz'},$data{'dls'});
		}
		elsif ($data{'action'} eq 'getservertime')
		{
			$rc = $parser->GetServerTime(uri_unescape($data{'msg'}),$data{'tz'},$data{'dls'});
		}

		if ($rc == 0)
		{
			$sendstr = '<mp_server_response><parser_error>'.$parser->{LASTERROR}.'</parser_error><action_received>'.$action.'</action_received><raw_time>'.$parser->{RAWTIME}.'</raw_time><raw_message>'.$parser->{RAWMESSAGE}.'</raw_message></mp_server_response>';
		}
		else
		{
			$sendstr = '<mp_server_response><message><to_user>'.lc($parser->{TOUSER}).'</to_user><text><![CDATA['.$parser->{RAWMSGTXT}.']]></text><user_time_string>'.$parser->{USERTIMESTRING}.'</user_time_string><server_time_string>'.$parser->{SERVERTIMESTRING}.'</server_time_string><raw_time><![CDATA['.$parser->{RAWTIME}.']]></raw_time></message></mp_server_response>';
		}
	}

	my $res = HTTP::Response->new(200, "OK");
	$res->content($sendstr);
	$con->send_response($res);

}

sub randomNumber
{
	my $rand;
	my ($upper,$lower) = @_;
	my $range = ($upper - $lower);
	
	my $count = 1;
    my @rand = ();

	while ( $count <= 2 ) 
	{
        while ( grep /^${rand}$/, @rand ) 
        {
            $rand = int(rand($range)+1);
        }

        if ($count == 2)
        {
        	my $val = $rand;
        	return $val;
        }

		push @rand,$rand;
        $count++;
	}

}

