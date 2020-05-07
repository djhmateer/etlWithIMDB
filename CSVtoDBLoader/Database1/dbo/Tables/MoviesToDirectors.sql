CREATE TABLE [dbo].[MoviesToDirectors] (
    [movieid]    INT           NOT NULL,
    [directorid] INT           NOT NULL,
    [genre]      NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_MoviesToDirectors] PRIMARY KEY CLUSTERED ([movieid] ASC, [directorid] ASC),
    CONSTRAINT [FK_MoviesToDirectors_Directors] FOREIGN KEY ([directorid]) REFERENCES [dbo].[Directors] ([directorid]),
    CONSTRAINT [FK_MoviesToDirectors_Movies1] FOREIGN KEY ([movieid]) REFERENCES [dbo].[Movies] ([movieid])
);

