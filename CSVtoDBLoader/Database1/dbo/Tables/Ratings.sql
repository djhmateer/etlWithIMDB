CREATE TABLE [dbo].[Ratings] (
    [movieid]      INT            NOT NULL,
    [rank]         NVARCHAR (255) NOT NULL,
    [votes]        INT            NOT NULL,
    [distribution] NVARCHAR (255) NOT NULL,
    CONSTRAINT [FK_Ratings_Movies] FOREIGN KEY ([movieid]) REFERENCES [dbo].[Movies] ([movieid])
);

