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
	Created datetime2(7) NOT NULL,
	Processed datetime2(7) NOT NULL,
	Stored datetime2(7) NOT NULL,

	[TimeSpentInQueue]  AS (datediff(millisecond,[Created],[Processed])) PERSISTED,
	[TimeSpentProcessing]  AS (datediff(millisecond,[Processed],[Stored])) PERSISTED,
	[TimeSpent]  AS (datediff(millisecond,[Created],[Stored])) PERSISTED,

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
COMMIT






