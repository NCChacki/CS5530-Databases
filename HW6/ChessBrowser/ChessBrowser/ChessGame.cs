using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBrowser
{
  class ChessGame
  {
    // The event name
    public string Event {  get; set; }
    // The event site
    public string Site { get; set; }
    // The round in which the game took place
    public string Round { get; set; }
    // The name of the white player
    public string White { get; set; }
    // The name of the black player
    public string Black { get; set; }
    // The white player's Elo rating at the time of the game
    public uint WhiteElo { get; set; }
    // The black player's Elo rating at the time of the game
    public uint BlackElo { get; set; }
    // The result of the game
    public string Result { get; set; }
    // The event date
    public string EventDate { get; set; }
    // The text representing the moves
    public string Moves { get; set; }
  }
}
