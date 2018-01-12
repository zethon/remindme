#!/usr/bin/perl
use strict;
use MessageParser;

my $botTimeZone = '-0500';

ParseMessage('remind me in 10 minutes to eat a llama','-0600',1);

sub ParseMessage()
{
	my ($message,$usertz,$userdls) = @_;
	
	my $parser = new MessageParser(BOTTIMEZONE=>'-0500');
	my $rc = $parser->Parse($message,$usertz,$userdls); 
	
	if ($rc == 0)
	{
		# return error XML
		print ("<reminder>\n");
		print ("\t<error_code>$rc</error_code>\n");
		print ("\t<error_string>".$parser->{LASTERROR}."</error_string>\n");
		print ("</reminder>\n");		
	}
	else
	{
		print ("<reminder>\n");
		print ("\t<message>".$parser->{RAWMSGTXT}."</message>\n");
		print ("\t<usertime>".$parser->GetUserTimeString()."</usertime>\n");
		print ("\t<epochtime>".$parser->GetEpoch()."</epochtime>\n");
		print ("</reminder>\n");		
	}	
}


sub ParseTime()
{
	my ($timestring,$usertz,$userdls) = @_;
	
	my $parser = new MessageParser(BOTTIMEZONE=>'-0500');
	$parser->{FAILONPASSEDTIME} = 0;
	$parser->{RAWTIME} = $timestring;

	my $rc = $parser->ParseTime($usertz,$userdls);	
	
	if ($rc == 0)
	{
		# return error XML
		print ("<reminder>\n");
		print ("\t<error_code>$rc</error_code>\n");
		print ("\t<error_string>".$parser->{LASTERROR}."</error_string>\n");
		print ("</reminder>\n");		
	}
	else
	{
		print ("<reminder>\n");
		print ("\t<epochtime>".$parser->GetEpoch()."</epochtime>\n");
		print ("</reminder>\n");		
	}	

	
}
