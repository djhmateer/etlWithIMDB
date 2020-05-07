CREATE TABLE [dbo].[RunningTimes] (
    [movieid]  INT            NOT NULL,
    [time]     NVARCHAR (255) NOT NULL,
    [addition] NVARCHAR (255) NULL,
    [time1]    INT            NOT NULL,
    CONSTRAINT [FK_RunningTimes_Movies] FOREIGN KEY ([movieid]) REFERENCES [dbo].[Movies] ([movieid])
);

