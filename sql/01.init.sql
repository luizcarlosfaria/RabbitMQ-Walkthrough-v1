USE [Walkthrough]
GO 

/* Para impedir possíveis problemas de perda de dados, analise este script detalhadamente antes de executá-lo fora do contexto do designer de banco de dados.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Messages
	(
	MessageId int NOT NULL IDENTITY (1, 1),	
	Stored datetimeoffset(7) NOT NULL, --Data de gravação no SQL Server
	Processed datetimeoffset(7) NULL, --Última data de processamento

	[TimeSpent]  AS (datediff(millisecond,[Stored],[Processed])) PERSISTED,

	[Num] int NOT NULL

	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Messages ADD CONSTRAINT
	DF_Messages_Stored DEFAULT GETDATE() FOR Stored
GO
ALTER TABLE dbo.Messages ADD CONSTRAINT
	PK_Messages PRIMARY KEY CLUSTERED 
	(
	MessageId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Messages SET (LOCK_ESCALATION = TABLE)
GO


CREATE TABLE [dbo].[Metrics](
	[MetricId] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetimeoffset] NOT NULL,
	[WorkerCount] [int] NOT NULL,
	[WorkLoadSize] [int] NOT NULL,
	[ConsumerCount] [int] NOT NULL,
	[ConsumerThroughput] [int] NOT NULL,
	[QueueSize] [int] NOT NULL,
	[PublishRate] [float] NOT NULL,
	[ConsumeRate] [float] NOT NULL,
 CONSTRAINT [PK_Metrics] PRIMARY KEY CLUSTERED 
(
	[MetricId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

COMMIT