# Netflix-App
Author: Joshua Herman 
Date: 

A graphical user interface application implementing N-tier desing in C# to execute SQL queries on a Netflix database. 

The application has 3 tiers: Presentation Tier, Business Tier, and Data Access Tier

Presentation Tier: Sets up the GUI and calls the Business Tier to obtain objects for data. 

Business Tier: SQL functions, Object Relational Mapping, and sorting data into tables. Extends Data Access Tier to retrieve the data from queries. 

Data Access Tier: Connects to the database and executes SQL queries written in the Business Tier.

Supports features for adding new reviews, viewing data on all movies and users, as well as all reviews each user made, and the reviews received by each movie. Also supports features for analysis on the data, for example, calculation top N movies by average rating. 
