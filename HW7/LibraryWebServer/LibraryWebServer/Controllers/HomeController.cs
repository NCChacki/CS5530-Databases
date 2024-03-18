using LibraryWebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "TestProject1" )]
namespace LibraryWebServer.Controllers
{
  public class HomeController : Controller
  {

    // WARNING:
    // This very simple web server is designed to be as tiny and simple as possible
    // This is NOT the way to save user data.
    // This will only allow one user of the web server at a time (aside from major security concerns).
    private static string user = "";
    private static int card = -1;

    private readonly ILogger<HomeController> _logger;


    /// <summary>
    /// Given a Patron name and CardNum, verify that they exist and match in the database.
    /// If the login is successful, sets the global variables "user" and "card"
    /// </summary>
    /// <param name="name">The Patron's name</param>
    /// <param name="cardnum">The Patron's card number</param>
    /// <returns>A JSON object with a single field: "success" with a boolean value:
    /// true if the login is accepted, false otherwise.
    /// </returns>
    [HttpPost]
    public IActionResult CheckLogin( string name, int cardnum )
    {
      // Determine if login is successful or not.
      bool loginSuccessful = false;

      using (Team88LibraryContext db = new Team88LibraryContext())
      {
        var query = from p in db.Patrons
                    where p.Name == name
                    select p.CardNum;
        foreach(int val in query)
        {
          if (val == cardnum)
          {
            loginSuccessful = true;
            break;
          }
        }
        
      }

    if (!loginSuccessful)
    {
      return Json(new { success = false });
    }
    else
    {
      user = name;
      card = cardnum;
      return Json(new { success = true });
    }
    }


    /// <summary>
    /// Logs a user out. This is implemented for you.
    /// </summary>
    /// <returns>Success</returns>
    [HttpPost]
    public ActionResult LogOut()
    {
      user = "";
      card = -1;
      return Json( new { success = true } );
    }

    /// <summary>
    /// Returns a JSON array representing all known books.
    /// Each book should contain the following fields:
    /// {"isbn" (string), "title" (string), "author" (string), "serial" (uint?), "name" (string)}
    /// Every object in the list should have isbn, title, and author.
    /// Books that are not in the Library's inventory (such as Dune) should have a null serial.
    /// The "name" field is the name of the Patron who currently has the book checked out (if any)
    /// Books that are not checked out should have an empty string "" for name.
    /// </summary>
    /// <returns>The JSON representation of the books</returns>
    [HttpPost]
    public ActionResult AllTitles()
    {
      using (Team88LibraryContext db = new Team88LibraryContext())
      {
        // SELECT ... FROM Titles JOIN Inventory JOIN CheckedOut JOIN Patrons ...
        var query = from t in db.Titles
                    join i in db.Inventory
                    on t.Isbn equals i.Isbn into temp
                    from j in temp.DefaultIfEmpty()
                    join c in db.CheckedOut
                    on j.Serial equals c.Serial into temp2
                    from j2 in temp2.DefaultIfEmpty()
                    join p in db.Patrons
                    on j2.CardNum equals p.CardNum into temp3
                    from j3 in temp3.DefaultIfEmpty()
                    select new {
                      isbn = t.Isbn,
                      title = t.Title,
                      author = t.Author,
                      serial = j == null ? null : (uint?) j.Serial,
                      name = j3 == null ? "" : j3.Name
                    };

        return Json( query.ToArray() );
      }
    }

    /// <summary>
    /// Returns a JSON array representing all books checked out by the logged in user 
    /// The logged in user is tracked by the global variable "card".
    /// Every object in the array should contain the following fields:
    /// {"title" (string), "author" (string), "serial" (uint) (note this is not a nullable uint) }
    /// Every object in the list should have a valid (non-null) value for each field.
    /// </summary>
    /// <returns>The JSON representation of the books</returns>
    [HttpPost]
    public ActionResult ListMyBooks()
    {
      using (Team88LibraryContext db = new Team88LibraryContext())
      {
        // SELECT ... FROM Titles JOIN Inventory JOIN CheckedOut JOIN Patrons as p WHERE p.CardNum = card
        var query = from t in db.Titles
                    join i in db.Inventory
                    on t.Isbn equals i.Isbn into temp
                    from j in temp.DefaultIfEmpty()
                    join c in db.CheckedOut
                    on j.Serial equals c.Serial into temp2
                    from j2 in temp2.DefaultIfEmpty()
                    join p in db.Patrons
                    on j2.CardNum equals p.CardNum into temp3
                    from j3 in temp3.DefaultIfEmpty()
                    where j3.CardNum == card
                    select new
                    {
                      title = t.Title,
                      author = t.Author,
                      serial = j.Serial
                    };

        return Json(query.ToArray()); 
      }
    }


    /// <summary>
    /// Updates the database to represent that
    /// the given book is checked out by the logged in user (global variable "card").
    /// In other words, insert a row into the CheckedOut table.
    /// You can assume that the book is not currently checked out by anyone.
    /// </summary>
    /// <param name="serial">The serial number of the book to check out</param>
    /// <returns>success</returns>
    [HttpPost]
    public ActionResult CheckOutBook( int serial )
    {
      // You may have to cast serial to a (uint)

      using (Team88LibraryContext db = new Team88LibraryContext())
      {
        CheckedOut c = new CheckedOut();
        c.CardNum = (uint) card;
        c.Serial  = (uint) serial;

        db.CheckedOut.Add(c);

        try
        {
          db.SaveChanges();
          return Json(new { success = true });
        }
        catch
        {
          return Json(new { success = false });
        }
      }
    }

    /// <summary>
    /// Returns a book currently checked out by the logged in user (global variable "card").
    /// In other words, removes a row from the CheckedOut table.
    /// You can assume the book is checked out by the user.
    /// </summary>
    /// <param name="serial">The serial number of the book to return</param>
    /// <returns>Success</returns>
    [HttpPost]
    public ActionResult ReturnBook( int serial )
    {
      // You may have to cast serial to a (uint)

      using (Team88LibraryContext db = new Team88LibraryContext())
      {
        // SELECT * FROM CheckedOut WHERE Serial = serial
        var query = from c in db.CheckedOut
                    where c.Serial == serial
                    select c;

        db.CheckedOut.RemoveRange(query);

        try
        {
          db.SaveChanges();
          return Json(new { success = true });
        }
        catch
        {
          return Json(new { success = false });
        }
      }
    }


    /*******************************************/
    /****** Do not modify below this line ******/
    /*******************************************/


    public IActionResult Index()
    {
        if ( user == "" && card == -1 )
            return View( "Login" );

        return View();
    }


    /// <summary>
    /// Return the Login page.
    /// </summary>
    /// <returns></returns>
    public IActionResult Login()
    {
        user = "";
        card = -1;

        ViewData["Message"] = "Please login.";

        return View();
    }

    /// <summary>
    /// Return the MyBooks page.
    /// </summary>
    /// <returns></returns>
    public IActionResult MyBooks()
    {
        if ( user == "" && card == -1 )
            return View( "Login" );

        return View();
    }

    public HomeController( ILogger<HomeController> logger )
    {
        _logger = logger;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
    public IActionResult Error()
    {
        return View( new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier } );
    }
  }
}