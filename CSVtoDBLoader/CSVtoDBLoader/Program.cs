using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CSVtoDBLoader.Services;
using Dapper;
using Serilog;
using Xunit;

namespace CSVtoDBLoader
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            // Serilog to Console and File
            Util.SetupLogger();

            // Dapper connection
            using var db = Util.GetOpenConnection();

            bool writeToDb = true;
            if (writeToDb)
            {
                // Clear down db first in correct order
                db.Execute("DELETE FROM MoviesToDirectors");
                db.Execute("DELETE FROM Directors");
                db.Execute("DELETE FROM RunningTimes");
                db.Execute("DELETE FROM MoviesToActors");
                db.Execute("DELETE FROM Ratings");
                db.Execute("DELETE FROM Movies");

                db.Execute("DELETE FROM Actors");
            }

            // 1.Extract Actors
            var actors = LoadActorsFromCsv();
            Log.Information($"Total Actors imported from csv is {actors.Count}"); // 98,690

            // Transform
            foreach (var actor in actors)
            {
                // Check for leading or trailing whitespace
                if (ListStringsContainLeadingOrTrailingWhitespace(new List<string>
                    {actor.actorid.ToString(), actor.name, actor.sex}))
                {
                    Log.Warning($"whitespace in {actor.actorid}");
                }

                // Check for interesting/bad/unusual characters that could affect results
                // eg LF?.. all queries are paramerterised so no problems with ' chars
                // okay due to db handing unicode UTF-8 and using nvarchar to hold strings

                // Custom validation
                if (!IsSexACapitalMOrF(actor.sex))
                    Log.Warning("non M or F in Actor sex column");

                var sql = @"
                INSERT Actors
                VALUES (@actorid, @name, @sex)";

                if (writeToDb) db.Execute(sql, actor);
            }

            // 2.Movies (assume no transform)
            List<Movie> movies = LoadMoviesFromCsv();
            Log.Information($"Total movies imported from csv is {movies.Count}"); // 3,832

            foreach (var movie in movies)
            {
                if (ListStringsContainLeadingOrTrailingWhitespace(new List<string>
                    {movie.movieid, movie.title, movie.year}))
                    Log.Warning($"whitespace in {movie.movieid}");

                var sql = @"
                INSERT INTO Movies
                VALUES (@movieid, @title, @year)";

                if (writeToDb) db.Execute(sql, movie);
            }

            // 3.Directors
            List<Director> directors = LoadDirectorsFromCsv();
            Log.Information($"Total Directors imported from csv is {directors.Count}"); // 2,201

            foreach (var director in directors)
            {
                if (ListStringsContainLeadingOrTrailingWhitespace(new List<string>
                    {director.directorid, director.name, director.rate, director.gross, director.num}))
                    Log.Warning($"whitespace in {director.directorid}");

                // note I don't need the INTO nor the (directorid, name etc..)
                var sql = @"
                INSERT INTO Directors
                (directorid
                ,name
                ,rate
                ,gross
                ,num)
                VALUES (@directorid
                ,@name
                ,@rate
                ,@gross
                ,@num)
                ";
                if (writeToDb) db.Execute(sql, director);
            }

            // 4.MoviesToDirectors 
            List<MovieToDirector> moviesToDirectors = LoadMoviesToDirectorsFromCsv();
            Log.Information($"Total moviesToDirectors imported from csv is {moviesToDirectors.Count}"); // 4,141

            foreach (var mtd in moviesToDirectors)
            {
                // normal to have whitespace in genre
                if (ListStringsContainLeadingOrTrailingWhitespace(new List<string> { mtd.movieid, mtd.directorid }))
                    Log.Warning($"whitespace in {mtd.movieid}, {mtd.directorid}, {mtd.genre}");

                var sql = @"
                INSERT INTO MoviesToDirectors
                (movieid
                ,directorid
                ,genre)
                VALUES (@movieid
                ,@directorid
                ,@genre)
                ";
                if (writeToDb) db.Execute(sql, mtd);
            }

            // 5. RunningTimes
            List<RunningTime> runningTimes = LoadRunningTimesFromCsv();
            Log.Information($"Total runningTimes imported from csv is {runningTimes.Count}"); // 5,086 
            foreach (var rt in runningTimes)
            {
                // normal to have whitespace in addition
                if (ListStringsContainLeadingOrTrailingWhitespace(new List<string> { rt.movieid, rt.time, rt.time1 }))
                    Log.Warning($"whitespace in {rt.movieid}");

                var sql = @"
                INSERT INTO RunningTimes
                (movieid
                ,time
                ,addition
                ,time1)
                VALUES (@movieid
                ,@time
                ,@addition
                ,@time1)
                ";
                if (writeToDb) db.Execute(sql, rt);
            }

            // 6. MoviesToActors
            List<MovieToActor> moviesToActors = LoadMoviesToActorsFromCsv();
            Log.Information($"Total MoviesToActors imported from csv is {moviesToActors.Count}"); // 138,349 
            foreach (var mta in moviesToActors)
            {
                // normal to have whitespace in as_character
                if (ListStringsContainLeadingOrTrailingWhitespace(new List<string>
                    {mta.movieid, mta.actorid, mta.leading}))
                    Log.Warning($"whitespace in {mta.movieid}");

                var sql = @"
                INSERT INTO MoviesToActors
                (movieid
                ,actorid
                ,as_character
                ,leading)
                VALUES (@movieid
                ,@actorid
                ,@as_character
                ,@leading)
                ";
                if (writeToDb) db.Execute(sql, mta);
            }

            // 7. Ratings
            List<Rating> ratings = LoadRatingsFromCsv();
            Log.Information($"Total ratings imported from csv is {ratings.Count}");
            foreach (var r in ratings)
            {
                // There are 2 empty distribution rows
                if (ListStringsContainLeadingOrTrailingWhitespace(new List<string> { r.movieid, r.rank.ToString(), r.votes.ToString(), r.distribution }))
                    Log.Warning("Ratings whitespace somewhere in {@R} ", r);

                var sql = @"
                INSERT INTO Ratings
                (movieid
                ,rank
                ,votes
                ,distribution)
                VALUES (@movieid
                ,@rank
                ,@votes
                ,@distribution)
                ";
                if (writeToDb) db.Execute(sql, r);
            }

            Log.Information($@"Finished in {sw.Elapsed.TotalSeconds}s \n");
        }

        private static List<Rating> LoadRatingsFromCsv()
        {
            using var reader = new StreamReader("..\\..\\..\\..\\..\\data\\ratings.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ";";
            return csv.GetRecords<Rating>().ToList();
        }

        public class Rating
        {
            public string movieid { get; set; }
            public decimal rank { get; set; }
            public int votes { get; set; }
            public string distribution { get; set; }
        }

        private static List<MovieToActor> LoadMoviesToActorsFromCsv()
        {
            using var reader = new StreamReader("..\\..\\..\\..\\..\\data\\moviestoactors.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ";";
            return csv.GetRecords<MovieToActor>().ToList();
        }

        public class MovieToActor
        {
            public string movieid { get; set; }
            public string actorid { get; set; }
            public string as_character { get; set; }
            public string leading { get; set; }
        }

        private static List<RunningTime> LoadRunningTimesFromCsv()
        {
            using var reader = new StreamReader("..\\..\\..\\..\\..\\data\\runningtimes.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ";";
            return csv.GetRecords<RunningTime>().ToList();
        }

        public class RunningTime
        {
            public string movieid { get; set; }
            public string time { get; set; }
            public string addition { get; set; }
            public string time1 { get; set; }
        }


        private static List<Director> LoadDirectorsFromCsv()
        {
            using var reader = new StreamReader("..\\..\\..\\..\\..\\data\\directors.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ";";
            return csv.GetRecords<Director>().ToList();
        }

        public class Director
        {
            public string directorid { get; set; }
            public string name { get; set; }
            public string rate { get; set; }
            public string gross { get; set; }
            public string num { get; set; }
        }

        private static List<MovieToDirector> LoadMoviesToDirectorsFromCsv()
        {
            using (var reader = new StreamReader("..\\..\\..\\..\\..\\data\\moviestodirectors.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.Delimiter = ";";
                return csv.GetRecords<MovieToDirector>().ToList();
            }
        }

        public class MovieToDirector
        {
            public string movieid { get; set; }
            public string directorid { get; set; }
            public string genre { get; set; }

            public override string ToString() =>
                $"moveid:{movieid}, directorid:{directorid}, genre{genre}";
        }


        private static List<Movie> LoadMoviesFromCsv()
        {
            using (var reader = new StreamReader("..\\..\\..\\..\\..\\data\\movies.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.Delimiter = ";";
                return csv.GetRecords<Movie>().ToList();
            }
        }

        public class Movie
        {
            public string movieid { get; set; }
            public string title { get; set; }
            public string year { get; set; }
        }

        [Fact]
        public void Thing()
        {
            var actor = new Actor { actorid = 1, name = "Dave", sex = "M" };
            var list = new List<string> { actor.actorid.ToString(), actor.name, actor.sex };
            Assert.False(ListStringsContainLeadingOrTrailingWhitespace(list));

            actor = new Actor { actorid = 1, name = "Dave ", sex = "M" };
            list = new List<string> { actor.actorid.ToString(), actor.name, actor.sex };
            Assert.True(ListStringsContainLeadingOrTrailingWhitespace(list));
        }

        public static bool ListStringsContainLeadingOrTrailingWhitespace(List<string> list)
        {
            foreach (var thing in list)
            {
                if (ContainsLeadingOrTrailingWhitespace(thing))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsLeadingOrTrailingWhitespace(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return true;

            var trimmed = s.Trim();
            if (trimmed.Length < s.Length) return true;

            return false;
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("              ", true)]
        [InlineData("asdf", false)]
        [InlineData(" asdf", true)]
        [InlineData("asdf ", true)]
        [InlineData(" asdf  ", true)]
        [InlineData("asdf asdf", false)]
        public static void ContainsLeadingOrTrailingWhitespace_Tests(string s, bool expected)
        {
            var result = ContainsLeadingOrTrailingWhitespace(s);
            Assert.Equal(expected, result);
        }


        public static bool IsSexACapitalMOrF(string s) => s == "M" || s == "F";

        [Theory]
        [InlineData("M", true)]
        [InlineData("F", true)]
        [InlineData("m", false)]
        [InlineData("f", false)]
        [InlineData("M ", false)]
        [InlineData(" M", false)]
        [InlineData("Male", false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public static void IsSexACapitalMOrF_Tests(string s, bool expected)
        {
            var result = IsSexACapitalMOrF(s);
            Assert.Equal(expected, result);
        }

        private static List<Actor> LoadActorsFromCsv()
        {
            using var reader = new StreamReader("..\\..\\..\\..\\..\\data\\actors.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.Delimiter = ";";
            return csv.GetRecords<Actor>().ToList();
        }

        public class Actor
        {
            public int actorid { get; set; }
            public string name { get; set; }
            public string sex { get; set; }
        }
    }
}

