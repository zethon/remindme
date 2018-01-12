#!usr/perl/bin
use Getopt::Std;
use XML::Simple;
use Data::Dumper;

use constant "GL_FOO" => 123456;

sub parseCmdLn()
{
	my %opts = undef;
	getopts('f:b:d',\%opts);
	
	die "FATAL ERROR: no bot specified.\n" if (!defined($opts{'b'}));
	my ($cfgfile) = defined($opts{'f'}) ? $opts{'f'} : 'bot-config.xml';
	
	return ($opts{'b'},$cfgfile,defined($opts{'d'}));
}

# to be implemented
sub IsRunning()
{
	my ($botname) = @_;
	return 0;
}

sub LoadBot()
{
	my ($xmlfile,$bot) = @_;
	my ($xml);
	
	eval { $xml = XMLin("$xmlfile",forcearray => ['bot'],forcecontent => 1,keeproot=>1); };
	if ($@)
	{
		print ("XML ERROR in ($xmlfile)\n");
		print ($@);
		die;	
	}
	
	die ("$0 (",__LINE__,"): Fatal Error, bot [$bot] does not exist\n") if (!defined($xml->{bots}->{bot}->{$bot}));
	return $xml->{bots}->{bot}->{$bot};
}

1;