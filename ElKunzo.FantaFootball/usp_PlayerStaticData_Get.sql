IF OBJECT_ID(N'dbo.usp_PlayerStaticData_Get','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_PlayerStaticData_Get AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_PlayerStaticData_Get TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_PlayerStaticData_Get
AS

	set transaction isolation level read uncommitted
	set nocount on
	
	select 
		fId,
		fWhoScoredId,
		fFootballDataTeamId,
		frTeamId,
		fJerseyNumber,
		frPosition,
		fName,
		fFullName,
		fDateOfBirth,
		fNationality,
		fContractUntil,
		fMarketValue
	from
		dbo.tab_PlayerStaticData

	if @@ERROR <> 0
		return -1;

	return 0;

GO