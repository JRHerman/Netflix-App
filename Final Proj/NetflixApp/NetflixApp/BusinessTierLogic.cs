//
// Written by Joshua Herman
// CS 341
// U. of Illinois, Chicago
// Final project Spring 2018
//
// BusinessTier:  business logic, acting as interface between UI and data store.
// 
//

using System;
using System.Collections.Generic;
using System.Data;


namespace BusinessTier
{

  //
  // Business:
  //
  public class Business
  {
    //
    // Fields:
    //
    private string _DBFile;
    private DataAccessTier.Data dataTier;


    //
    // Constructor:
    //
    public Business(string DatabaseFilename)
    {
      _DBFile = DatabaseFilename;

      dataTier = new DataAccessTier.Data(DatabaseFilename);
    }


    //
    // TestConnection:
    //
    // Returns true if we can establish a connection to the database, false if not.
    //
    public bool TestConnection()
    {
      return dataTier.TestConnection();
    }

    // GetAllNamedMovies():
    // 
    // Returns a list of all the movies the Movie table, sorted 
    // by MovieName
    //

    public IReadOnlyList<Movie> GetAllNamedMovies()
    {
         List<Movie> movies = new List<Movie>(); //list of Movie
         string sql = String.Format("SELECT * FROM Movies ORDER BY MovieName ASC;", new object[0]); //get movies sort by MovieName ascending
         DataTable dt = this.dataTier.ExecuteNonScalarQuery(sql).Tables["TABLE"];
         foreach(DataRow row in dt.Rows)
         {
             var movieID = Convert.ToInt32(row["MovieID"]);
             var movieName = row["MovieName"].ToString();
           
             Movie movie = new Movie (movieID, movieName); //Movie object
             movies.Add(movie); //add object to list of Movie
         }
         return movies; //return it

    }
    //
    // GetNamedUser:
    //
    // Retrieves User object based on USER NAME; returns null if user is not
    // found.
    //
    // NOTE: there are "named" users from the Users table, and anonymous users
    // that only exist in the Reviews table.  This function only looks up "named"
    // users from the Users table.
    //
    public User GetNamedUser(string UserName)
    {
        string replace = UserName.Replace ("'", "''"); //replace single ' with '' 
        string sql = String.Format("SELECT * FROM Users WHERE UserName = '{0}'", replace); //get named users from Users table
        
        DataTable dt= this.dataTier.ExecuteNonScalarQuery(sql).Tables["TABLE"];

        if (dt.Rows.Count > 0)
        {
            DataRow rowOfData = dt.Rows[0];
            var userID = Convert.ToInt32(rowOfData["UserID"]);
            var occupation = rowOfData["Occupation"].ToString();
            var user = new User (userID, UserName, occupation); //User object
            return user; //return object
        }
            
      
      return null; //else we get here because the rows of dt were less than 0 then the table is null
    }


    //
    // GetAllNamedUsers:
    //
    // Returns a list of all the users in the Users table ("named" users), sorted 
    // by user name.
    //
    // NOTE: the database also contains lots of "anonymous" users, which this 
    // function does not return.
    //
    public IReadOnlyList<User> GetAllNamedUsers()
    {
      List<User> users = new List<User>(); //list of User

      string sql = String.Format("SELECT * FROM Users ORDER BY UserName ASC;", new object[0]); //query for all users in user table sorted by user name
      DataTable data = this.dataTier.ExecuteNonScalarQuery(sql).Tables["TABLE"];
      
      foreach(DataRow row in data.Rows)
      {
          var userID = Convert.ToInt32(row["UserID"]);
          var userName = row["UserName"].ToString();
          var occupation = row["Occupation"].ToString();
          User userData = new User (userID, userName, occupation); //User object
          users.Add(userData); //add object to list
      }

      return users; //return list
    }


    //
    // GetMovie:
    //
    // Retrieves Movie object based on MOVIE ID; returns null if movie is not
    // found.
    //
    public Movie GetMovie(int MovieID)
    {
        string sql = string.Format("SELECT MovieName FROM Movies WHERE MovieID = {0}", MovieID); //query for getting movie name using movieID to get it
      
        object obj = this.dataTier.ExecuteScalarQuery(sql);
        if (obj == DBNull.Value || obj == null) //check if null
        {
           return null;
        }
        var movieName = obj.ToString();
        var movie = new Movie (MovieID, movieName); //Movie object
        return movie; //return object
    }


    //
    // GetMovie:
    //
    // Retrieves Movie object based on MOVIE NAME; returns null if movie is not
    // found.
    //
    public Movie GetMovie(string MovieName)
    {
        string replace = MovieName.Replace("'", "''"); //replace ' with ''

        string sql = string.Format("SELECT MovieID FROM Movies Where MovieName = '{0}'", replace); //get MovieID from movie name in query

        object obj = this.dataTier.ExecuteScalarQuery(sql);

        if (obj == DBNull.Value || obj == null) //check if null
        {
             return null;
        }

        var movieID = Convert.ToInt32(obj);
        var movie = new Movie (movieID, MovieName); //make Movie object

        return movie; //return object

    }

    //
    // GetUser:
    //
    // Retrieves User object based on USER ID; returns null if user is not
    // found.
    //
    // NOTE: if the user exists in the Users table, then a meaningful name and
    // occupation are returned in the User object. If the user does not exist
    // in the Users table, then the user id has to be looked up in the Reviews
    // table to see if he/she has submitted 1 or more reviews as an "anonymous"
    // user. If the id is found in the Reviews table, then the user is an
    // "anonymous" user, so a User object with name = "<UserID>" and no occupation
    // ("") is returned. In other words, name = the user’s id surrounded by < >.
    //
    public User GetUser(int UserID)
    {
        string sql = String.Format("SELECT * FROM Users WHERE UserID = {0};", UserID); //get all data from Users table by userID
        DataTable dt = this.dataTier.ExecuteNonScalarQuery(sql).Tables["TABLE"];

        if (dt.Rows.Count > 0) //check if rows in table
        {
            DataRow row = dt.Rows[0];
            var userName = row["UserName"].ToString();
            var occupation = row["Occupation"].ToString();
            var user = new User(UserID,  userName, occupation); //make user object   
        }

        string sql2 = String.Format("SELECT Count(ReviewID) FROM Reviews WHERE UserID = {0};", UserID); //count number of reviews per user

        if (Convert.ToInt32(this.dataTier.ExecuteScalarQuery(sql2)) > 0) 
        {
             var user = new User (UserID, $"<{UserID}>", ""); //make user object
             return user; //return it
        }

        return null; //if here table empty
        }
    //
    // AddReview:
    //
    // Adds review based on MOVIE ID, returning a Review object containing
    // the review, review's id, etc.  If the add failed, null is returned.
    //
    public Review AddReview(int MovieID, int UserID, int Rating)
    {
      string sql = String.Format(@" 
       INSERT INTO Reviews(MovieID, UserID, Rating) 
       Values ({0}, {1}, {2})
       SELECT ReviewID 
       FROM Reviews
       WHERE ReviewID = SCOPE_IDENTITY();", 
       MovieID, UserID, Rating); //add review based on movieid, userid, and rating.

       object obj = this.dataTier.ExecuteScalarQuery(sql);

       if (obj == null) //check if null
       {
           return null;
       }
     
       var reviewID = Convert.ToInt32(obj);
       var review = new Review (reviewID, MovieID, UserID, Rating); //make object of review

       return review; //return it
    }


    //
    // GetMovieDetail:
    //
    // Given a MOVIE ID, returns detailed information about this movie --- all
    // the reviews, the total number of reviews, average rating, etc.  If the 
    // movie cannot be found, null is returned.
    //
    public MovieDetail GetMovieDetail(int MovieID)
    {
        Movie movie = this.GetMovie(MovieID); //call GetMovie to get Movie object given ID
        
        double average = 0.0;
        if (movie == null) //check if object is null
        {
            return null; 
        }        
      
        string sql = String.Format("SELECT COUNT(*) FROM Reviews WHERE MovieID = {0};", MovieID); //count number of reviews for the MovieID
        
        int reviewCount = Convert.ToInt32(this.dataTier.ExecuteScalarQuery(sql));
       
        if (reviewCount > 0) // if count is more than 0
        {
            string sql2 = String.Format(@"
            SELECT ROUND(AVG(CAST(Rating AS Float)), 4)
            FROM Reviews 
            WHERE MovieID = {0};",
            MovieID); //get the average rating

            average = Convert.ToDouble(this.dataTier.ExecuteScalarQuery(sql2));
        }

        List <Review> movieReviews = new List<Review>(); //list of Review

        if (reviewCount > 0) //if count is more than 0
        {
            string sql3 = String.Format(@"
            SELECT UserID, Rating, ReviewID
            FROM Reviews 
            WHERE MovieID = {0}
            ORDER BY Rating DESC, UserID ASC;",
            MovieID); //table of userID, rating, reviewID. rating descending userID ascending

           DataTable dt = this.dataTier.ExecuteNonScalarQuery(sql3).Tables["TABLE"];
           
           foreach(DataRow row in dt.Rows)
           {

                var reviewID = Convert.ToInt32(row["ReviewID"]);
                var userID = Convert.ToInt32(row["UserID"]);
                var rating = Convert.ToInt32(row["Rating"]);
                Review review = new Review (reviewID, MovieID, userID, rating); //Review object
                movieReviews.Add(review); //add review to list
           }
        }

        var detailOfMovie = new MovieDetail (movie, average, reviewCount, movieReviews); //MovieDetail object
        
        return detailOfMovie; //return object
         
    }


    //
    // GetUserDetail:
    //
    // Given a USER ID, returns detailed information about this user --- all
    // the reviews submitted by this user, the total number of reviews, average 
    // rating given, etc.  If the user cannot be found, null is returned.
    //
    public UserDetail GetUserDetail(int UserID)
    {
         User user = this.GetUser(UserID); //get user given ID

         double average = 0.0;

         if (user == null) //check if user is null
         {
            return null;
         }
       
         string sql = String.Format("SELECT COUNT(*) FROM Reviews WHERE USERID = {0};", UserID); //count number of review of userID

         int reviewCount = Convert.ToInt32(this.dataTier.ExecuteScalarQuery(sql));

         if (reviewCount > 0) //if count is more than 0 
         {
            string sql2 = String.Format(@"
            SELECT ROUND(AVG(CAST(Rating AS FLOAT)), 4)
            FROM Reviews
            WHERE UserID = {0};",
            UserID); //get average reviews
           
            average = Convert.ToDouble(this.dataTier.ExecuteScalarQuery(sql2));
         }

         List <Review> userReview = new List<Review>(); //list of Review

         if (reviewCount > 0) //if count is more than 0 
         {
             string sql3 = string.Format(@"
             SELECT J.Rating, J.ReviewID, J.MovieID
             FROM Movies
             INNER JOIN (
                SELECT Rating, ReviewID, MovieID
                FROM Reviews
                WHERE UserID = {0}) AS J
             ON J.MovieID = Movies.MovieID    
             ORDER BY Movies.MovieName ASC, J.Rating ASC;",
             UserID); // join tables get rating review id movie id join by movieID and order by movie name desc rating asc
            
            DataTable dt = this.dataTier.ExecuteNonScalarQuery(sql3).Tables["TABLE"];
            foreach(DataRow row in dt.Rows)
            {
                var reviewID = Convert.ToInt32(row["ReviewID"]);
                var movieID = Convert.ToInt32(row["MovieID"]);
                var rating = Convert.ToInt32(row["Rating"]);
                Review review = new Review(reviewID, movieID, UserID, rating); //Review object
                userReview.Add(review); //add to list
            } 
         }
     var detail = new UserDetail(user, average, reviewCount, userReview); //UserDetail object
     return detail; //return object
    }


    //
    // GetTopMoviesByAvgRating:
    //
    // Returns the top N movies in descending order by average rating.  If two
    // movies have the same rating, the movies are presented in ascending order
    // by name.  If N < 1, an EMPTY LIST is returned.
    //
    public IReadOnlyList<Movie> GetTopMoviesByAvgRating(int N)
    {
      List<Movie> movies = new List<Movie>(); //list of Movie

      string sql = String.Format(@"
          SELECT TOP {0} Movies.MovieID, Movies.MovieName
          FROM Movies
          INNER JOIN (
          SELECT MovieID, ROUND(AVG(CAST(Rating AS FLOAT)), 4) as AvgRating
          FROM Reviews
          GROUP BY MOVIEID) AS J 
          ON J.MovieID = Movies.MovieID
          ORDER BY J.AvgRating DESC, Movies.MovieName ASC;",
          N); //get top N movies get movieID and name join tables on MovieID sort by avg rating desc and moviename asc
       DataTable dt = this.dataTier.ExecuteNonScalarQuery(sql).Tables["TABLE"];
       foreach (DataRow row in dt.Rows)
       {
           var movieID = Convert.ToInt32(row["MovieID"]);
           var movieName = row["MovieName"].ToString();
           Movie movie = new Movie (movieID, movieName); //make Movie obj
           movies.Add(movie); //add object to list
       }

       return movies; //return list
    }


  }//class
}//namespace
