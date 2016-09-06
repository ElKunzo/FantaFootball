# FantaFootball
I am playing around with F# by doing something half useful: Connecting to the [football-data api](http://football-data.org/), getting the data and writing it into a database (for starters).
Additionally to that, I want to connect to whoscored.com and grab the match report data in order to be able to get the player performance infos. 
The whole thing should be done in F#. Possible use? Maybe a fantasy league kinda thing... but hey, who does not love football data?!
 
Please note: I did not commit the FootbalData Apikey to the repo. This means that the code will not compile, unless you have your own api-key. 

This is just a personal pet project and I have two small kids at home, so its not sure how regularly I can work on this. 
Therefore keeping some to do list to be able to restart faster. 

## Progress and ToDo:

[x] Api key implemented.
[x] Added the database scripts for Team Static Data and Player Static Data. Both include some basic read (update) write stored procedures. 
[x] Added a cache for players and teams. Given that players and teams should not change so much over time, a TTL of one day should be enough.
[ ] Refactor Mapper. Maybe move the sql stuff into data access?!
[ ] Add some sort of "Cron Job" to query the football data api regularly (e.g. once per day) for new players / teams. Pay attention not to add duplicates 
to the DB (i.e. merge newly downloaded data with existing data).
[ ] Add who scored player Ids to the player Data.
[ ] Add fixtures (code and db), including who scored Id for the fixture.
[ ] Create some sort of web service / windows service
[ ] Add a ASP MVC application as frontend (start reading about asp ;-))




