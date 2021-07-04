### **TODOs / FIXES / IDEAS**

**Upcoming Matches:**

###### Discord Bot:
- Pick games with only x amount of starts and up
- Use cronjob in bot (that will run 24/7), and just call GetUpcommingMatches function and store them in list
- How it should work
    1. Collect all upcoming matches (from return) 
    2. Place them in a list (?) and sort them by time (ascending) 
    3. Each 30 min(!?) iterate through list, check and compare date of each match
  and pick one with 30 min later (30 min until start)
  4. Create Discord channel with the name of each team i.e X-vs-Z

https://github.com/atifaziz/NCrontab