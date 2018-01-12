#!/usr/bin/perl
use strict;
use Net::SMTP;  

package botLib;
	use constant BOT_TIME_ZONE => '-0500';
	
	use constant SERVICES => ('aim','msn','icq');
	use constant SERVICE_INDX => {
									'aim' => 0,
									'msn' => 1,
									'icq' => 2,
								 };
	
	use constant SERVICE_AIM => 0;
	use constant SERVICE_MSN => 1;
	use constant SERVICE_ICQ => 2;
	
	use constant URL_INFOPAGE => 'http://www.remindme.cc';
	
	use constant REMINDER_DB_HOST 	=> 'www.remindme.cc';
	use constant REMINDER_DB_NAME 	=> 'reminder';
	use constant REMINDER_DB_USER	=> 'zethon';
	use constant REMINDER_DB_PW		=> '';
	

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

sub normalize {
   my $data = shift;

   $data =~ s/ //g;
   $data =~ tr/A-Z/a-z/;
   return $data;
}

sub SendEmail()
{
	my ($MailTo,$subject,$message) = @_;
	my $MailFrom = 'remindme@remindme.cc'; 
	my $ServerName = 'remindme.cc';
	
	my $smtp = Net::SMTP->new($ServerName, Debug => 0); 
	return 0 unless $smtp;
	
	$smtp->mail( $MailFrom );  
	$smtp->to( $MailTo ); 

	$smtp->data();
	$smtp->datasend("To:  $MailTo\n"); 	
	$smtp->datasend("From:  $MailFrom\n");  
	$smtp->datasend("Subject: $subject\n");  
	$smtp->datasend("\n"); 
	$smtp->datasend("$message\n\n"); 
	$smtp->dataend(); 
	$smtp->quit();  
}


sub SendTemplateEmail()
{
	my ($MailTo,$subject,$file,$replhash) = @_;
	my %replace = %{$replhash};
	
	open (FILE,$file) or return;
	my @lines = <FILE>;
	close (FILE);
	
	foreach my $line (@lines)
	{
		foreach my $key (keys(%replace))
		{
			my $tkey = '%'.$key.'%';
			$line =~ s/$tkey/$replace{$key}/gi;
		}	
	}
	
	
	&SendEmail($MailTo,$subject,${@lines});
}
1;