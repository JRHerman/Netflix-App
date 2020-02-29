using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace NetflixApp
{
  public partial class Form1 : Form
  {
    //
    // Class members:
    //
    private string m_connectionInfo;

    private void SetConnectionInfo(string filename)
    {
      string version;

      version = "MSSQLLocalDB";  // for VS 2015:

      m_connectionInfo = String.Format(@"Data Source=(LocalDB)\{0};AttachDbFilename=|DataDirectory|\{1};Integrated Security=True;",
        version,
        filename);
    }


    //
    // Constructor:
    //
    public Form1()
    {
      InitializeComponent();
    }


    //
    // Form1_Load:  called just before the form is displayed to the user, this event
    // is triggered before all other events.
    //
    private void Form1_Load(object sender, EventArgs e)
    {
      SetConnectionInfo(this.txtDatabase.Text);

      //
      // NOTE: we ping SQL Server now to start it running, just in case it's slow to 
      // start (e.g. when running on a virtual machine):
      //
      try
      {
        BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

        biztier.TestConnection();
      }
      catch
      {
        // ignore any exceptions since it may timeout the first time:
      }
    }


    private void tbarRating_Scroll(object sender, EventArgs e)
    {
      lblRating.Text = tbarRating.Value.ToString();
    }

    //
    // Add Review:
    //
    private void cmdInsertReview_Click(object sender, EventArgs e)
    {
      //
      // Get the movie name from the list of movies:
      //
      if (this.listBox1.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a movie...");
        return;
      }

      string MovieName = this.listBox1.Text;

      //
      // And the user name from the list of users:
      //
      if (this.listBox2.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a user...");
        return;
      }

      string UserName = this.listBox2.Text;

      //
      // NOTE: since a movie and a user is selected, the movie and user IDs are 
      // available from the associated text boxes:
      //
      int movieid = Convert.ToInt32(this.txtMovieID.Text);
      int userid = Convert.ToInt32(this.txtUserID.Text);

      //
      // Insert movie review:
      //
      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

      var review = biztier.AddReview(movieid, userid, System.Int32.Parse(lblRating.Text));

      //
      // display results:
      //
      if (review != null) // success!
      {
        string msg = string.Format("Success, review added (review id {0}).", review.ReviewID);
        MessageBox.Show(msg);
      }
      else
      {
        MessageBox.Show("**Failure, insert was not added (?) **");
      }
    }


    //
    // All Movies:
    //
    private void cmdAllMovies_Click(object sender, EventArgs e)
    {
      // clear listbox and textboxes
      listBox1.Items.Clear();
      this.txtMovieID.Clear();
      this.txtAvgRating.Clear();

      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text); //biztier object

      var users = biztier.GetAllNamedMovies(); //call get all named movies

      if (users.Count == 0) //check if count of users is 0 if so empty DB
      {
        MessageBox.Show("**Error: no users, is database empty?!");
      }
      else
      {
        //
        // we have movies data, display:
        //
        foreach (var user in users) 
        {
          listBox1.Items.Add(user.MovieName);
        }
      }
    }
   


    //
    // When the user selects a movie, display movie id and average rating...
    //
    private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      string name;

      name = this.listBox1.Text;  // selected movie:

      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

      var movie = biztier.GetMovie(name);

      if (movie == null)  // should never happen but...
      {
        MessageBox.Show("**ERROR: Movie not found?!");
        this.txtMovieID.Text = "";
        this.txtAvgRating.Text = "";
        return;
      }

      this.txtMovieID.Text = movie.MovieID.ToString();

      var details = biztier.GetMovieDetail(movie.MovieID);

      if (details == null)  // not found, should never happen, but...
      {
        this.txtAvgRating.Text = "0";
      }
      else
      {
        this.txtAvgRating.Text = details.AvgRating.ToString();
      }
    }


    //
    // Reviews for selected movie:
    //
    private void cmdMovieReviews_Click(object sender, EventArgs e)
    {
      string name;

      if (this.listBox1.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a movie...");
        return;
      }

      name = this.listBox1.Text;

      //
      // NOTE: since a movie is selected, the movie id is in the associated textbox:
      //

      int movieid = Convert.ToInt32(this.txtMovieID.Text);

      //
      // Get all the reviews for this movie:
      //
      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

      var details = biztier.GetMovieDetail(movieid);

      // 
      // Display the results in a subform:
      //
      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Reviews for \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      if (details.NumReviews == 0)
      {
        frm.listBox1.Items.Add("No reviews...");
      }
      else
      {
        foreach (var review in details.Reviews)
        {
          string msg = string.Format("{0}: {1}", review.UserID, review.Rating);

          frm.listBox1.Items.Add(msg);
        }
      }

      frm.ShowDialog();
    }


    //
    // Summary of reviews (by each rating) for selected movie:
    //
    private void cmdReviewsSummary_Click(object sender, EventArgs e)
    {
      string name;

      if (this.listBox1.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a movie...");
        return;
      }

      name = this.listBox1.Text;

      //
      // NOTE: since a movie is selected, the movie id is in the associated textbox:
      //

      int movieid = Convert.ToInt32(this.txtMovieID.Text);

      //
      // Let's get all the reviews, grouped by rating and count each group:
      //
      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

      var details = biztier.GetMovieDetail(movieid);

      var query = from r in details.Reviews
                  group r by r.Rating into grp
                  orderby grp.Key descending
                  select new { Rating = grp.Key, Count = grp.Count() };

      //
      // display results:
      //
      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Review Summary for \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      int N = query.Count();

      if (N == 0)
      {
        frm.listBox1.Items.Add("No reviews...");
      }
      else
      {
        int total = 0;

        //
        // we have ratings data, display:
        //
        foreach (var tuple in query)
        {
          string msg = string.Format("{0}: {1}", tuple.Rating, tuple.Count);

          frm.listBox1.Items.Add(msg);

          total = total + tuple.Count;
        }

        frm.listBox1.Items.Add("");
        frm.listBox1.Items.Add("Total: " + total.ToString());
      }

      frm.ShowDialog();
    }


    //
    // All Users:
    //
    private void cmdAllUsers_Click(object sender, EventArgs e)
    {
      listBox2.Items.Clear();
      this.txtUserID.Clear();
      this.txtOccupation.Clear();

      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

      var users = biztier.GetAllNamedUsers();

      if (users.Count == 0)
      {
        MessageBox.Show("**Error: no users, is database empty?!");
      }
      else
      {
        //
        // we have ratings data, display:
        //
        foreach (var user in users)
        {
          listBox2.Items.Add(user.UserName);
        }
      }
    }


    //
    // User has selected a user in the list:
    //
    private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
    {
      string name;

      name = this.listBox2.Text;  // selected user:

      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

      var user = biztier.GetNamedUser(name);

      if (user == null)  // not found, should never happen but...
      {
        MessageBox.Show("**Error: user not found?!");

        this.txtUserID.Text = "";
        this.txtOccupation.Text = "";
      }
      else
      {
        this.txtUserID.Text = user.UserID.ToString();
        this.txtOccupation.Text = user.Occupation;
      }
    }


    //
    // Reviews for selected user:
    //
    private void cmdUserReviews_Click(object sender, EventArgs e)
    {
      string name;

      if (this.listBox2.SelectedIndex < 0)
      {
        MessageBox.Show("Please select a user...");
        return;
      }

      name = this.listBox2.Text;

      //
      // NOTE: since a user is selected, the user id is in the associated textbox:
      //

      int userid = Convert.ToInt32(this.txtUserID.Text);

      //
      // Get all the reviews by this user:
      //
      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

      var details = biztier.GetUserDetail(userid);

      // 
      // Display the results in a subform:
      //
      SubForm frm = new SubForm();

      frm.lblHeader.Text = string.Format("Reviews by \"{0}\"", name);

      frm.listBox1.Items.Add(name);
      frm.listBox1.Items.Add("");

      if (details.NumReviews == 0)
      {
        frm.listBox1.Items.Add("No reviews...");
      }
      else
      {
        foreach (var review in details.Reviews)
        {
          string moviename = biztier.GetMovie(review.MovieID).MovieName;

          string msg = string.Format("{0} -> {1}", moviename, review.Rating);

          frm.listBox1.Items.Add(msg);
        }
      }

      frm.ShowDialog();
    }


    //
    // File >> Test Connection:
    //
    private void testConnectionToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

        if (biztier.TestConnection())
          MessageBox.Show("**Successful connection.");
        else
          MessageBox.Show("**Error, unable to establish connection -- is database installed?");
      }
      catch
      {
        MessageBox.Show("**Error: unable to establish connection -- is database installed?");
      }
    }


    //
    // File >> Exit:
    //
    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }


    //
    // File >> Top Movies by Avg Rating:
    //
    private void topMoviesByRatingToolStripMenuItem_Click(object sender, EventArgs e)
    {
      int N;

      if (System.Int32.TryParse(this.txtTopN.Text, out N) == false)  // failed, invalid input...
      {
        MessageBox.Show("**Error, please enter a valid integer for top-N computation...");
        return;
      }
      if (N < 1)
      {
        MessageBox.Show("**Error, please enter a valid integer for top-N computation...");
        return;
      }

      BusinessTier.Business biztier = new BusinessTier.Business(this.txtDatabase.Text);

      var movies = biztier.GetTopMoviesByAvgRating(N);

      if (movies == null)  // shouldn't happen, but...
      {
        MessageBox.Show("**Error: no movies, is database empty?!");
      }
      else
      {
        //
        // we have ratings data, display in our subform:
        //
        SubForm frm = new SubForm();

        frm.lblHeader.Text = "Top Movies by Average Rating";

        foreach (var movie in movies)
        {
          var detail = biztier.GetMovieDetail(movie.MovieID);

          string msg = string.Format("{0}: {1}", movie.MovieName, detail.AvgRating);

          frm.listBox1.Items.Add(msg);
        }

        frm.ShowDialog();
      }
    }


    //
    // whenever the user changes the database filename, update our
    // internal connection string info:
    //
    private void txtDatabase_TextChanged(object sender, EventArgs e)
    {
      SetConnectionInfo(this.txtDatabase.Text);
    }

        private void lblRating_Click(object sender, EventArgs e)
        {

        }
    }//class
}//namespace
