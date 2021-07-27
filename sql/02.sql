/*
delete from [Walkthrough].[dbo].[Metrics]
delete [Walkthrough].[dbo].[Messages]
*/
SELECT TOP (15) * FROM [Walkthrough].[dbo].[Metrics]  order by [MetricId] desc


-- Mensagens Não Processadas
SELECT * FROM [Walkthrough].[dbo].[Messages] where [Processed] is null order by [MessageId] desc

-- Mensagens processadas mais de uma vez
SELECT * FROM [Walkthrough].[dbo].[Messages] where [Num] > 1 order by [MessageId] desc

-- Mensagens processadas
SELECT * FROM [Walkthrough].[dbo].[Messages] where [Processed] is not null order by [MessageId] desc



SELECT AVG(TimeSpent), 'Média 1 min' as [Label] FROM [Walkthrough].[dbo].[Messages] where [Processed] > DATEADD(minute, -1, getutcdate()) UNION
SELECT AVG(TimeSpent), 'Média 2 min' as [Label] FROM [Walkthrough].[dbo].[Messages] where [Processed] > DATEADD(minute, -2, getutcdate()) UNION 
SELECT AVG(TimeSpent), 'Média 3 min' as [Label] FROM [Walkthrough].[dbo].[Messages] where [Processed] > DATEADD(minute, -3, getutcdate()) UNION 
SELECT AVG(TimeSpent), 'Média 4 min' as [Label] FROM [Walkthrough].[dbo].[Messages] where [Processed] > DATEADD(minute, -4, getutcdate()) UNION 
SELECT AVG(TimeSpent), 'Média 5 min' as [Label] FROM [Walkthrough].[dbo].[Messages] where [Processed] > DATEADD(minute, -5, getutcdate()) 


