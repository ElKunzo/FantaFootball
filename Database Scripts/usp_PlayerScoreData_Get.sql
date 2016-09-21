IF OBJECT_ID(N'dbo.usp_PlayerScoreData_Get','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_PlayerScoreData_Get AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_PlayerScoreData_Get TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_PlayerScoreData_Get
AS

	set transaction isolation level read uncommitted
	set nocount on
	
	select 
		fId,
		frFixtureId,
		frPlayerId,
		fTotalPoints,
		fMinutesPlayed,
		fGoalsScored,
		fAssists,
		fCleanSheet,
		fShotsSaved,
		fPenaltiesSaved,
		fPenaltiesMissed,
		fGoalsConceded,
		fYellowCards,
		fRedCard,
		fOwnGoals
	from
		dbo.tab_PlayerScoreData

	if @@ERROR <> 0
		return -1;

	return 0;

GO