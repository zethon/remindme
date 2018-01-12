# RemindMe

RemindMe was a website (www.remindme.cc) I started around 2002 and eventually took down around 2007. The idea was that users would send an IM to bot in the form of:

`remind me tomorrow at 7pm to take out the trash`

And then the following day the user would get a message from the bot saying `take out the trash`. 

## Design

The website was written in PHP with a MySQL database. 

The backend was originally implemented entirely in Perl. This was difficult to maintain as the multithreaded nature of the backend proved problematic. This could have been the result of my code (likely), my old/slow hardware (also likley), or Perl's multithreaded support at the time (maybe). 

I rewrote the backend in C# using a locally hosted Perl REST service to do some of the text parsing. This was mostly because of a library in Perl (whose name I can't recall right now) that did real language parsing of dates. In other words you could pass it: `tuesday 4pm` and the library would return you an epoch timestamp. At the time there was a project called Perl.NET that would compile Perl code to .NET, but it was relatively new and didn't work too well. As a result I implemented a simple REST service which my C# code could invoke to get a timestamp.

## The End

RemindMe saw a little bit of success with about 100 reminders per day being delivered at its peak. However it was problematic and delivered maybe 70% to 80% of its reminders. I eventually shut the site down and let the domain expire since I had a full time job and was a student.

The code is presented here for archival puporses.
