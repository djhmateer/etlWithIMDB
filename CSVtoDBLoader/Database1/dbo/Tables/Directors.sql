CREATE TABLE [dbo].[Directors] (
    [directorid] INT            NOT NULL,
    [name]       NVARCHAR (255) NOT NULL,
    [rate]       NVARCHAR (255) NOT NULL,
    [gross]      NVARCHAR (255) NOT NULL,
    [num]        NVARCHAR (255) NOT NULL,
    CONSTRAINT [PK_Directors] PRIMARY KEY CLUSTERED ([directorid] ASC)
);

