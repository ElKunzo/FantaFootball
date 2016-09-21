IF OBJECT_ID(N'dbo.usp_PlayerScoreData_Update','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_PlayerScoreData_Update AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_PlayerScoreData_Update TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_PlayerScoreData_Update
(
	@PlayerData dbo.PlayerScoreDataType READONLY
)
AS


BEGIN
	 SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
     SET NOCOUNT ON;

	MERGE 
		dbo.tab_PlayerScoreData AS TRG
	USING
		(SELECT Id, FixtureId, PlayerId, TotalPoints, MinutesPlayed, GoalsScored, Assists, CleanSheet, ShotsSaved, PenaltiesSaved, PenaltiesMissed, GoalsConceded, YellowCards, RedCard, OwnGoals FROM @PlayerData) AS SRC
	ON
		TRG.fId = SRC.Id
	WHEN MATCHED THEN 
		UPDATE SET  frFixtureId = SRC.FixtureId,
					frPlayerId = SRC.PlayerId,
					fTotalPoints = SRC.TotalPoints,
					fMinutesPlayed = SRC.MinutesPlayed,
					fGoalsScored = SRC.GoalsScored,
					fAssists = SRC.Assists,
					fCleanSheet = SRC.CleanSheet,
					fShotsSaved = SRC.ShotsSaved,
					fPenaltiesSaved = SRC.PenaltiesSaved,
					fPenaltiesMissed = SRC.PenaltiesMissed,
					fGoalsConceded = SRC.GoalsConceded,
					fYellowCards = SRC.YellowCards,
					fRedCard = SRC.RedCard,
					fOwnGoals = SRC.OwnGoals,
				    fLastUpdatedUtc = GETUTCDATE()
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (frFixtureId, frPlayerId, fTotalPoints, fMinutesPlayed, fGoalsScored, fAssists, fCleanSheet, fShotsSaved, fPenaltiesSaved, fPenaltiesMissed, fGoalsConceded, fYellowCards, fRedCard, fOwnGoals, fLastUpdatedUtc)
		VALUES (SRC.FixtureId, SRC.PlayerId, SRC.TotalPoints, SRC.MinutesPlayed, SRC.GoalsScored, SRC.Assists, SRC.CleanSheet, SRC.ShotsSaved, SRC.PenaltiesSaved, SRC.PenaltiesMissed, SRC.GoalsConceded, SRC.YellowCards, SRC.RedCard, SRC.OwnGoals, GETUTCDATE());

	IF @@ERROR <> 0
		RETURN -1;

	RETURN 0;
END
GO
