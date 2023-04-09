# MsSql sp_executesql Parser 

C# console app. This project parse clipboard sql string:
* get profiler string begins with "exec sp_executesql" from clipboard
* parse it to set parameter values
* format it to get readable
* set result back to clipboard

## Build Project

You can use \Bin\MsSqlLogParse.exe or build it manually

Open solution file MsSqlLogParse.sln in Microsoft Visual 2010 (or higher)
Build solution or project

## Get started

Usage:
* Run MsSql Server Profiler, add new trace session
* In the profiler select a row and copy sql string from the bottom window to clipboard (Ctrl+C)
  This sql string must begins with "exec sp_executesql"
* Run \Bin\MsSqlLogParse.exe
* Paste parsed sql string from clipboard (Ctrl + V) in the MsSql Management Studio Sql Window (or text editor) 

## Examples

Input string:
``` 
exec sp_executesql N'SELECT A01.ID,A01.employeeID,A02.tabNum AS C1,A03.name AS C2,A01.dayCount,(SELECT CASE WHEN COALESCE(SUM(per.dayCountPlan),0) > (COALESCE(SUM(ev.dayCount),0) + COALESCE(SUM(vc.dayComp),0)) THEN COALESCE(SUM(per.dayCountPlan),0) - (COALESCE(SUM(ev.dayCount),0) + COALESCE(SUM(vc.dayComp),0)) ELSE 0 END FROM hr_empVacationPeriod per LEFT JOIN (select ev.empVacationPeriodID, SUM(ev.dayCount) as dayCount from hr_employeeVacation ev where ev.mi_deleteDate = dbo.f_maxdate() group by ev.empVacationPeriodID) ev ON ev.empVacationPeriodID = per.ID LEFT JOIN (select vc.empVacationPeriodID, SUM(vc.dayComp) as dayComp from hr_empVacationComp vc group by vc.empVacationPeriodID) vc ON vc.empVacationPeriodID = per.ID WHERE per.empVacationPlanID = A01.ID and per.mi_deleteDate = dbo.f_maxdate()) AS C3,(0) AS C4,A01.dateFrom,(case datepart(year, A01.dateTo) when 9999 then null when 2099 then null else A01.dateTo end) AS C5,A01.reason,A01.employeeNumberID,A01.dictVacationKindID,A03.code AS C6,A01.orderID,A01.mi_modifyDate  FROM hr_empVacationPlan A01  INNER JOIN hr_employeeNumber A02 ON A02.ID=A01.employeeNumberID  INNER JOIN hr_dictVacationKind A03 ON A03.ID=A01.dictVacationKindID  WHERE A01.employeeID=@P1 AND A01.employeeNumberID=@P2 AND A01.mi_deleteDate>=@P3 ORDER BY 8 DESC',N'@P1 bigint,@P2 bigint,@P3 datetime2(0)',3000000134152,3000001607686,'9999-12-31 00:00:00'
```

Output string:
```
SELECT A01.ID,A01.employeeID,A02.tabNum AS C1,A03.name AS C2,A01.dayCount,
	(SELECT CASE WHEN COALESCE(SUM(per.dayCountPlan),0) > (COALESCE(SUM(ev.dayCount),0) + COALESCE(SUM(vc.dayComp),0)) THEN COALESCE(SUM(per.dayCountPlan),
			0) - (COALESCE(SUM(ev.dayCount),0) + COALESCE(SUM(vc.dayComp),0)) ELSE 0 END 
	FROM hr_empVacationPeriod per 
		LEFT JOIN 
			(select ev.empVacationPeriodID, SUM(ev.dayCount) as dayCount 
			from hr_employeeVacation ev 
			where ev.mi_deleteDate = dbo.f_maxdate() 
			group by ev.empVacationPeriodID) ev ON ev.empVacationPeriodID = per.ID 
		LEFT JOIN 
			(select vc.empVacationPeriodID, SUM(vc.dayComp) as dayComp 
			from hr_empVacationComp vc 
			group by vc.empVacationPeriodID) vc ON vc.empVacationPeriodID = per.ID 
	WHERE per.empVacationPlanID = A01.ID 
		and per.mi_deleteDate = dbo.f_maxdate()) AS C3,(0) AS C4,A01.dateFrom,(case datepart(year, A01.dateTo) when 9999 then null when 2099 then null else A01.dateTo end) AS C5,
	A01.reason,A01.employeeNumberID,A01.dictVacationKindID,A03.code AS C6,A01.orderID,A01.mi_modifyDate 
FROM hr_empVacationPlan A01 
	INNER JOIN hr_employeeNumber A02 ON A02.ID=A01.employeeNumberID 
	INNER JOIN hr_dictVacationKind A03 ON A03.ID=A01.dictVacationKindID 
WHERE A01.employeeID=3000000134152 
	AND A01.employeeNumberID=3000001607686 
	AND A01.mi_deleteDate>='9999-12-31' 
ORDER BY 8 DESC
```
## Releases
2023-03-21
Universalized for processing different versions of syntax 'exec sp_executesql'
* with named parameters as '@p1=1, @p2=1'
* with parameters placed in wrong order or with missed parameters (e.g. @p1, @p2, @p4)