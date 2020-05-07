CREATE TABLE [dbo].[Actors] (
    [actorid] INT            NOT NULL,
    [name]    NVARCHAR (255) NOT NULL,
    [sex]     NVARCHAR (10)  NOT NULL,
    CONSTRAINT [PK_Actors] PRIMARY KEY CLUSTERED ([actorid] ASC)
);

