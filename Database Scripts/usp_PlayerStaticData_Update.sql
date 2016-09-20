IF OBJECT_ID(N'dbo.usp_PlayerStaticData_Update','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_PlayerStaticData_Update AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_PlayerStaticData_Update TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_PlayerStaticData_Update
(
	@PlayerData dbo.PlayerStaticDataType READONLY
)
AS


BEGIN
	 SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
     SET NOCOUNT ON;

	MERGE 
		dbo.tab_PlayerStaticData AS TRG
	USING
		(SELECT Id, FootballDataTeamId, TeamId, JerseyNumber, Position, Name, FullName, DateOfBirth, Nationality, ContractUntil, MarketValue FROM @PlayerData) AS SRC
	ON
		TRG.fId = SRC.Id
	WHEN MATCHED THEN 
		UPDATE SET  fFootballDataTeamId = SRC.FootballDataTeamId, 
				    frTeamId = SRC.TeamId,
				    fJerseyNumber = SRC.JerseyNumber,
					frPosition = SRC.Position,
					fName = SRC.Name,
					fFullName = SRC.FullName,
					fDateOfBirth = SRC.DateOfBirth,
					fNationality = SRC.Nationality,
					fContractUntil = SRC.ContractUntil,
					fMarketValue = SRC.MarketValue,
				    fLastUpdatedUtc = GETUTCDATE()
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (fWhoScoredId, fFootballDataTeamId, frTeamId, fJerseyNumber, frPosition, fName, fFullName, fDateOfBirth, fNationality, fContractUntil, fMarketValue, fLastUpdatedUtc)
		VALUES (-1, SRC.FootballDataTeamId, SRC.TeamId, SRC.JerseyNumber, SRC.Position, SRC.Name, SRC.FullName, SRC.DateOfBirth, SRC.Nationality, SRC.ContractUntil, SRC.MarketValue, GETUTCDATE());

	IF @@ERROR <> 0
		RETURN -1;

	RETURN 0;
END
GO
