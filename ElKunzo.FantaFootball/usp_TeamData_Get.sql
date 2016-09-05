IF OBJECT_ID(N'dbo.usp_TeamData_Get','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_TeamData_Get AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_TeamData_Get TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_TeamData_Get
AS

	set transaction isolation level read uncommitted
	set nocount on
	
	select 
		fId,
		fFootballDataId,
		fWhoScoredId,
		fName,
		fFullName,
		fCode,
		fSquadMarketValue,
		fCrestUrl
	from
		dbo.tab_TeamData

	if @@ERROR <> 0
		return -1;

	return 0;

GO