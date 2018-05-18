SELECT 
[UnionAll1].[C1] AS [C1]
FROM  (SELECT 
 [GroupBy1].[A1] AS [C1]
 FROM ( SELECT 
  COUNT(1) AS [A1]
  FROM [dbo].[Assignments] AS [Extent1]
 )  AS [GroupBy1]
UNION ALL
 SELECT 
 [GroupBy2].[A1] AS [C1]
 FROM ( SELECT 
  COUNT(1) AS [A1]
  FROM [dbo].[Assignments] AS [Extent2]
 )  AS [GroupBy2]) AS [UnionAll1]
