# RemindMe

RemindMe was a service/website (www.remindme.cc) I launch in 2002 and eventually took down around 2007. 

The idea was that users could create/edit/receive reminders through instant messaging. In other words, someone could send an IM to a bot such as:

`remind me tomorrow at 7pm to take out the trash`

And then the following day the user would get a message from the bot saying `take out the trash`. 

Features included:

* Support for AOL Instant Messenger (RIP), YahooIM and MSN Messenger
* Repeating reminders (i.e. "`remind me every tuesday to take out the trash`")
* Sending reminders to friends (i.e. "`remind anthony next tuesday to call his mom`")

## Design

The website was written in PHP with a MySQL database and is in the `/www` folder. 

The backend was ran on a computer hosted in my house. Since the computer only needed connections to the IM services and the website's database, it was easy enough to use my broadband conneciton at home. 

The original server was written entirely in Perl (and can be viewed in the `/old_server` folder). However, this version was difficult to maintain since the multithreaded nature of the backend proved problematic. This could have been the result of my code (likely), my old/slow hardware (also likley), or Perl's multithreaded support at the time (maybe). 

I rewrote the backend in C# using a locally hosted Perl REST service (`/server`) to do some of the text parsing. This was mostly because of a library in Perl (whose name I can't recall right now) that did real language parsing of dates. In other words you could pass it: `tuesday 4pm` and the library would return you an epoch timestamp. At the time there was a project called Perl.NET that would compile Perl code to .NET, but it was relatively new and didn't work too well. As a result I implemented a simple REST service (`/server/mpserver`) which my C# code (`/server/RemServer`) could invoke to get a timestamp.

## The End

RemindMe saw a little bit of success with about 100 reminders per day being delivered at its peak. However it was problematic and delivered maybe 70% to 80% of its reminders. I eventually shut the site down and let the domain expire since I had a full time job and was a student.

The code is presented here for archival puporses.
