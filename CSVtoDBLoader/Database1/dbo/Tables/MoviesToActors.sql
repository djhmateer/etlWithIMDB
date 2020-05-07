CREATE TABLE [dbo].[MoviesToActors] (
    [movieid]      INT             NOT NULL,
    [actorid]      INT             NOT NULL,
    [as_character] NVARCHAR (1024) NULL,
    [leading]      NVARCHAR (255)  NULL,
    CONSTRAINT [FK_MoviesToActors_Actors] FOREIGN KEY ([actorid]) REFERENCES [dbo].[Actors] ([actorid]),
    CONSTRAINT [FK_MoviesToActors_Movies] FOREIGN KEY ([movieid]) REFERENCES [dbo].[Movies] ([movieid])
);

