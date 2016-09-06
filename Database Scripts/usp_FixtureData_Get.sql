IF OBJECT_ID(N'dbo.usp_FixtureData_Get','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_FixtureData_Get AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_FixtureData_Get TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_FixtureData_Get
AS

	set transaction isolation level read uncommitted
	set nocount on
	
	select 
		fId,
		fWhoScoredId,
		fFootballDataId,
		frStatusId,
		fKickOffUtc,
		fMatchDay,
		frHomeTeamId,
		frAwayTeamId,
		fHomeScore,
		fAwayScore
	from
		dbo.tab_FixtureData

	if @@ERROR <> 0
		return -1;

	return 0;

GO