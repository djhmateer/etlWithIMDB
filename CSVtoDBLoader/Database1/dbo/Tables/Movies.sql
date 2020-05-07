CREATE TABLE [dbo].[Movies] (
    [movieid] INT            NOT NULL,
    [title]   NVARCHAR (255) NOT NULL,
    [year]    INT            NOT NULL,
    CONSTRAINT [PK_Movies] PRIMARY KEY CLUSTERED ([movieid] ASC)
);

