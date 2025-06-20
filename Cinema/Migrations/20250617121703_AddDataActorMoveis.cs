using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema.Migrations
{
    /// <inheritdoc />
    public partial class AddDataActorMoveis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    insert into ActorMovies (ActorId, MovieId) values (25, 10);
    insert into ActorMovies (ActorId, MovieId) values (29, 26);
    insert into ActorMovies (ActorId, MovieId) values (41, 21);
    insert into ActorMovies (ActorId, MovieId) values (23, 30);
    insert into ActorMovies (ActorId, MovieId) values (26, 11);
    insert into ActorMovies (ActorId, MovieId) values (2, 13);
    insert into ActorMovies (ActorId, MovieId) values (28, 15);
    insert into ActorMovies (ActorId, MovieId) values (43, 13);
    insert into ActorMovies (ActorId, MovieId) values (40, 11);
    insert into ActorMovies (ActorId, MovieId) values (49, 29);
    insert into ActorMovies (ActorId, MovieId) values (11, 24);
    insert into ActorMovies (ActorId, MovieId) values (13, 1);
    insert into ActorMovies (ActorId, MovieId) values (44, 19);
    insert into ActorMovies (ActorId, MovieId) values (10, 9);
    insert into ActorMovies (ActorId, MovieId) values (42, 14);
    insert into ActorMovies (ActorId, MovieId) values (5, 16);
    insert into ActorMovies (ActorId, MovieId) values (44, 7);
    insert into ActorMovies (ActorId, MovieId) values (46, 11);
    insert into ActorMovies (ActorId, MovieId) values (16, 12);
    insert into ActorMovies (ActorId, MovieId) values (34, 7);
    insert into ActorMovies (ActorId, MovieId) values (1, 22);
    insert into ActorMovies (ActorId, MovieId) values (9, 2);
    insert into ActorMovies (ActorId, MovieId) values (42, 8);
    insert into ActorMovies (ActorId, MovieId) values (38, 19);
    insert into ActorMovies (ActorId, MovieId) values (27, 21);
    insert into ActorMovies (ActorId, MovieId) values (14, 15);
    insert into ActorMovies (ActorId, MovieId) values (22, 25);
    insert into ActorMovies (ActorId, MovieId) values (5, 6);
    insert into ActorMovies (ActorId, MovieId) values (24, 9);
    insert into ActorMovies (ActorId, MovieId) values (22, 28);
    insert into ActorMovies (ActorId, MovieId) values (29, 13);
    insert into ActorMovies (ActorId, MovieId) values (2, 21);
    insert into ActorMovies (ActorId, MovieId) values (22, 22);
    insert into ActorMovies (ActorId, MovieId) values (18, 29);
    insert into ActorMovies (ActorId, MovieId) values (6, 2);
    insert into ActorMovies (ActorId, MovieId) values (42, 28);
    insert into ActorMovies (ActorId, MovieId) values (47, 9);
    insert into ActorMovies (ActorId, MovieId) values (6, 9);
    insert into ActorMovies (ActorId, MovieId) values (9, 23);
    insert into ActorMovies (ActorId, MovieId) values (47, 29);
    insert into ActorMovies (ActorId, MovieId) values (40, 23);
    insert into ActorMovies (ActorId, MovieId) values (39, 11);
    insert into ActorMovies (ActorId, MovieId) values (34, 15);
    insert into ActorMovies (ActorId, MovieId) values (37, 4);
    insert into ActorMovies (ActorId, MovieId) values (22, 21);
    insert into ActorMovies (ActorId, MovieId) values (30, 1); 
    insert into ActorMovies (ActorId, MovieId) values (49, 24);
    insert into ActorMovies (ActorId, MovieId) values (10, 18);
    insert into ActorMovies (ActorId, MovieId) values (18, 19);
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
