using Microsoft.Maui.Controls;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
  Author: Daniel Kopta, Cameron Davis and Chase Canning
  Chess browser backend 
*/

namespace ChessBrowser
{
  internal class Queries
  {

    /// <summary>
    /// This function runs when the upload button is pressed.
    /// Given a filename, parses the PGN file, and uploads
    /// each chess game to the user's database.
    /// </summary>
    /// <param name="PGNfilename">The path to the PGN file</param>
    internal static async Task InsertGameData( string PGNfilename, MainPage mainPage )
    {
      // This will build a connection string to your user's database on atr,
      // assuimg you've typed a user and password in the GUI
      string connection = mainPage.GetConnectionString();

      // Load and parse the PGN file
      // We recommend creating separate libraries to represent chess data and load the file
      List<ChessGame> games = PGNReader.GetGames(PGNfilename);      

      // Tell the GUI's progress bar how many total work steps there are
      // For example, one iteration of your main upload loop could be one work step
      mainPage.SetNumWorkItems(games.Count);


      using ( MySqlConnection conn = new MySqlConnection( connection ) )
      {
        try
        {
          // Open a connection
          conn.Open();

          // Generate appropriate insert commands with prepared statements

          MySqlCommand eventsCmd = conn.CreateCommand();
          eventsCmd.CommandText = "INSERT IGNORE INTO Events (Name, Site, Date) VALUES (@Name, @Site, @Date);";
          eventsCmd.Parameters.AddWithValue("@Name", "");
          eventsCmd.Parameters.AddWithValue("@Site", "");
          eventsCmd.Parameters.AddWithValue("@Date", "");


          MySqlCommand playersCmd = conn.CreateCommand();
          playersCmd.CommandText = "INSERT INTO Players (Name, Elo) VALUES (@WhiteName, @WhiteElo) ON DUPLICATE KEY UPDATE Elo = IF(Elo < @WhiteElo, @WhiteElo, Elo);" +
                                   "INSERT INTO Players (Name, Elo) VALUES (@BlackName, @BlackElo) ON DUPLICATE KEY UPDATE Elo = IF(Elo < @BlackElo, @BlackElo, Elo);";
          playersCmd.Parameters.AddWithValue("@WhiteName", "");
          playersCmd.Parameters.AddWithValue("@WhiteElo", "");
          playersCmd.Parameters.AddWithValue("@BlackName", "");
          playersCmd.Parameters.AddWithValue("@BlackElo", "");

          MySqlCommand gamesCmd = conn.CreateCommand();
          gamesCmd.CommandText = "INSERT IGNORE INTO Games VALUES(@Round, @Result, @Moves, " +
                                 "(SELECT pID FROM Players WHERE Name = @BlackName), " +
                                 "(SELECT pID FROM Players WHERE Name = @WhiteName), " +
                                 "(SELECT eID FROM Events WHERE Name = @EventName));";
          gamesCmd.Parameters.AddWithValue("@Round", "");
          gamesCmd.Parameters.AddWithValue("@Result", "");
          gamesCmd.Parameters.AddWithValue("@Moves", "");
          gamesCmd.Parameters.AddWithValue("@BlackName", "");
          gamesCmd.Parameters.AddWithValue("@WhiteName", "");
          gamesCmd.Parameters.AddWithValue("@EventName", "");

          // Iterate through all of the games and insert them to the database
          for (int i = 0; i < games.Count; i++)
          {
            eventsCmd.Parameters["@Name"].Value = games[i].Event;
            eventsCmd.Parameters["@Site"].Value = games[i].Site;
            eventsCmd.Parameters["@Date"].Value = games[i].EventDate;

            eventsCmd.ExecuteNonQuery();

            playersCmd.Parameters["@WhiteName"].Value = games[i].White;
            playersCmd.Parameters["@WhiteElo"].Value = games[i].WhiteElo;
            playersCmd.Parameters["@BlackName"].Value = games[i].Black;
            playersCmd.Parameters["@BlackElo"].Value = games[i].BlackElo;

            playersCmd.ExecuteNonQuery();

            gamesCmd.Parameters["@Round"].Value = games[i].Round;
            gamesCmd.Parameters["@Result"].Value = games[i].Result;
            gamesCmd.Parameters["@Moves"].Value = games[i].Moves;
            gamesCmd.Parameters["@BlackName"].Value = games[i].Black;
            gamesCmd.Parameters["@WhiteName"].Value = games[i].White;
            gamesCmd.Parameters["@EventName"].Value = games[i].Event;
            
            gamesCmd.ExecuteNonQuery();

            // Tell the GUI that one work step has completed
            await mainPage.NotifyWorkItemCompleted();
          }         

        }
        catch ( Exception e )
        {
          System.Diagnostics.Debug.WriteLine( e.Message );
        }
      }

    }


    /// <summary>
    /// Queries the database for games that match all the given filters.
    /// The filters are taken from the various controls in the GUI.
    /// </summary>
    /// <param name="white">The white player, or null if none</param>
    /// <param name="black">The black player, or null if none</param>
    /// <param name="opening">The first move, e.g. "1.e4", or null if none</param>
    /// <param name="winner">The winner as "W", "B", "D", or null if none</param>
    /// <param name="useDate">True if the filter includes a date range, False otherwise</param>
    /// <param name="start">The start of the date range</param>
    /// <param name="end">The end of the date range</param>
    /// <param name="showMoves">True if the returned data should include the PGN moves</param>
    /// <returns>A string separated by newlines containing the filtered games</returns>
    internal static string PerformQuery( string white, string black, string opening,
      string winner, bool useDate, DateTime start, DateTime end, bool showMoves,
      MainPage mainPage )
    {
      // This will build a connection string to your user's database on atr,
      // assuimg you've typed a user and password in the GUI
      string connection = mainPage.GetConnectionString();

      // Build up this string containing the results from your query
      string parsedResult = "";

      // Use this to count the number of rows returned by your query
      // (see below return statement)
      int numRows = 0;

      using ( MySqlConnection conn = new MySqlConnection( connection ) )
      {
        try
        {
          // Open a connection
          conn.Open();

          // TODO:
          //       Generate and execute an SQL command,
          //       then parse the results into an appropriate string and return it.

          MySqlCommand searchCommand = conn.CreateCommand();
          searchCommand.CommandText = "SELECT g.Result, e.Name as eName, e.Date, e.Site, wp.Name as wpName, wp.Elo as wpElo, bp.Name as bpName, bp.Elo as bpElo ";
          if (showMoves)
          {
            searchCommand.CommandText += ",g.Moves ";
          }
          searchCommand.CommandText += "FROM Games g JOIN Events e JOIN Players wp ON g.WhitePlayer = wp.pID " +
                                       "JOIN Players bp ON g.BlackPlayer = bp.pID WHERE g.eID = e.eID ";
          if (white != null)
          {
            searchCommand.CommandText += "AND wp.Name = @WPName ";
            searchCommand.Parameters.AddWithValue("@WPName", white);
          }
          if (black != null)
          {
            searchCommand.CommandText += "AND bp.Name = @BPName ";
            searchCommand.Parameters.AddWithValue("@BPName", black);
          }
          if (opening != null)
          {
            searchCommand.CommandText += "AND g.Moves LIKE \"@OpeningMove%\" ";
            searchCommand.Parameters.AddWithValue("@OpeningMove", opening);
          }
          if (winner != null)
          {
            searchCommand.CommandText += "AND g.Result LIKE \"@Winner\" ";
            searchCommand.Parameters.AddWithValue("@Winner", winner);
          }
          if (useDate)
          {
            searchCommand.CommandText += "AND e.Date BETWEEN @StartDate AND @EndDate ";
            searchCommand.Parameters.AddWithValue("@StartDate", start);
            searchCommand.Parameters.AddWithValue("@EndDate", end);
          }
          searchCommand.CommandText += ";";


          using ( MySqlDataReader reader = searchCommand.ExecuteReader())
          {
            while ( reader.Read() )
            {
              numRows++;
              parsedResult += "\nEvent: " + reader["eName"];
              parsedResult += "\nSite: " + reader["Site"];
              parsedResult += "\nDate: " + reader["Date"];
              parsedResult += "\nWhite: " + reader["wpName"] + " (" + reader["wpElo"] + ")";
              parsedResult += "\nBlack: " + reader["bpName"] + " (" + reader["bpElo"] + ")";
              parsedResult += "\nResult: " + reader["Result"];
              if (showMoves)
              {
                parsedResult += "\nMoves: " + reader["Moves"];
              }
              parsedResult += "\n";
            }
          }


        }
        catch ( Exception e )
        {
          System.Diagnostics.Debug.WriteLine( e.Message );
        }
      }

      return numRows + " results\n" + parsedResult;
    }

  }
}
