IF OBJECT_ID(N'dbo.usp_PlayerStaticData_InsertWhoScored','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_PlayerStaticData_InsertWhoScored AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_PlayerStaticData_InsertWhoScored TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_PlayerStaticData_InsertWhoScored
(
	@PlayerData dbo.PlayerStaticDataType READONLY
)
AS


BEGIN
	 SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
     SET NOCOUNT ON;

	INSERT INTO dbo.tab_PlayerStaticData (fWhoScoredId, fFootballDataTeamId, frTeamId, fJerseyNumber, frPosition, fName, fFullName, fDateOfBirth, fLastUpdatedUtc)
	SELECT WhoScoredId, FootballDataTeamId, TeamId, JerseyNumber, Position, Name, FullName, DateOfBirth, GETUTCDATE() FROM @PlayerData;

	IF @@ERROR <> 0
		RETURN -1;

	RETURN 0;
END
GO
