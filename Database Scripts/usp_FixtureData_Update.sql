IF OBJECT_ID(N'dbo.usp_FixtureData_Update','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_FixtureData_Update AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_FixtureData_Update TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_FixtureData_Update
(
	@FixtureData dbo.FixtureDataType READONLY
)
AS


BEGIN
	 SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
     SET NOCOUNT ON;

	MERGE 
		dbo.tab_FixtureData AS TRG
	USING
		(SELECT Id, FootballDataId, StatusId, KickOffUtc, MatchDay, HomeTeamId, AwayTeamId, HomeScore, AwayScore FROM @FixtureData) AS SRC
	ON
		TRG.fId = SRC.Id
	WHEN MATCHED THEN 
		UPDATE SET	fFootballDataId = SRC.FootballDataId,
					frStatusId = SRC.StatusId,
					fKickOffUtc = SRC.KickOffUtc,
					fMatchDay = SRC.MatchDay,
					frHomeTeamId = SRC.HomeTeamId,
					frAwayTeamId = SRC.AwayTeamId,
					fHomeScore = SRC.HomeScore,
					fAwayScore = SRC.AwayScore,
				    fLastUpdatedUtc = GETUTCDATE()
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (fWhoScoredId, fFootballDataId, frStatusId, fKickOffUtc, fMatchDay, frHomeTeamId, frAwayTeamId, fHomeScore, fAwayScore, fLastUpdatedUtc)
		VALUES (-1, SRC.FootballDataId, SRC.StatusId, SRC.KickOffUtc, SRC.MatchDay, SRC.HomeTeamId, SRC.AwayTeamId, SRC.HomeScore, SRC.AwayScore, GETUTCDATE());

	IF @@ERROR <> 0
		RETURN -1;

	RETURN 0;
END
GO
