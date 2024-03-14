using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBrowser
{
  static class PGNReader
  {
    public static List<ChessGame> GetGames(string filename)
    {
      List<ChessGame> games = new List<ChessGame>();

      if (File.Exists(filename))
      {
        string[] text = File.ReadAllLines(filename);
        int blankLines = 0;
        string allMoves = "";
        ChessGame currentGame = new ChessGame();

        foreach (string line in text)
        {
          if (line.StartsWith('['))
          {
            string data = line.Split('"', '"')[1];
            if (line.Contains("EventDate"))
            {
              if(data.Contains("?"))
              {
                currentGame.EventDate = "0000-00-00";
              } 
              else
              {
                currentGame.EventDate = data;
              }
            }
            else if (line.Contains("Event"))
            {
              currentGame.Event = data;
            }
            else if (line.Contains("Site"))
            {
              currentGame.Site = data;
            }
            else if (line.Contains("Round"))
            {
              currentGame.Round = data;
            }
            else if (line.Contains("WhiteElo"))
            {
              currentGame.WhiteElo = uint.Parse(data);
            }
            else if (line.Contains("BlackElo"))
            {
              currentGame.BlackElo = uint.Parse(data);
            }
            else if (line.Contains("White"))
            {
              currentGame.White = data;
            }
            else if (line.Contains("Black"))
            {
              currentGame.Black = data;
            }
            else if (line.Contains("Result"))
            {
              if (data.Equals("1-0"))
              {
                currentGame.Result = "W";
              } 
              else if (data.Equals("0-1"))
              {
                currentGame.Result = "B";
              }
              else
              {
                currentGame.Result = "D";
              }
            }
          } 
          else if(line.Equals("")){
            blankLines++;
            if(blankLines == 2)
            {
              currentGame.Moves = allMoves;
              games.Add(currentGame);

              currentGame = new ChessGame();
              blankLines = 0;
              allMoves = "";
            }
          }
          else
          {
            allMoves += line;
          }
        }
      } 
      else
      {
        Debug.WriteLine(filename + " could not be found.");
      }

      return games;
    }
  }
}
