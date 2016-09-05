# FantaFootball
I am playing around with F# by doing something half useful: Connecting to the [football-data api](http://football-data.org/), getting the data and writing it into a database (for starters).
Additionally to that, I want to connect to whoscored.com and grab the match report data in order to be able to get the player performance infos. 
The whole thing should be done in F#. Possible use? Maybe a fantasy league kinda thing... but hey, who does not love football data?!
 
Please note: I did not commit the FootbalData Apikey to the repo. This means that the code will not compile, unless you have your own api-key. 

## ToDo:

This is just a personal pet project and I have two small kids at home, so its not sure how regularly I can work on this. Therefore keeping some to do lists to be able to restart faster. 

### Coding:
- [ ] The DataAccess stored procedure executor looks a bit ugly - basically a copy from the C# code. I should do a refactoring :-).
- [ ] Add the database files to the git repo, and rethink the data model.
- [x] Add database write functionality.
- [x] Add exception handling for api call.
- [ ] Create a cache of the football-data stuff, so that a web call is not needed anymore.
- [ ] Map whoscored teams / players to football-data (common ground is the shirt number at the moment for the players)

