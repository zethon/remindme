exit 0 if ($ARGV[0] !~ /release/i);

open (FILE,'AssemblyInfo.cs');
my @lines = <FILE>;
close (FILE);

open (OUTFILE,">AssemblyInfo.cs");
foreach my $line (@lines)
{
	if ($line =~ /\[assembly:\s+AssemblyVersion\(\"\d+\.\d+\.\d+\.(\d+)\"\)\]/i)
	{
		my $newver = $1 + &randomNumber(175,&randomNumber(5));
		$line =~ s/$1/$newver/;
	}	
	
	print OUTFILE $line;
}
close OUTFILE;

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