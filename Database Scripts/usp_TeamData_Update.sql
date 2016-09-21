IF OBJECT_ID(N'dbo.usp_TeamData_Update','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_TeamData_Update AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_TeamData_Update TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_TeamData_Update
(
	@TeamData dbo.TeamDataType READONLY
)
AS


BEGIN
	 SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
     SET NOCOUNT ON;

	MERGE 
		dbo.tab_TeamData AS TRG
	USING
		(SELECT Id, FootballDataId, WhoScoredId, Name, FullName, Code, SquadMarketValue, CrestUrl FROM @TeamData) AS SRC
	ON
		TRG.fId = SRC.Id
	WHEN MATCHED THEN 
		UPDATE SET fFootballDataId = SRC.FootballDataId, 
				   fName = SRC.Name, 
				   fFullName = SRC.FullName, 
				   fCode = SRC.Code,
				   fSquadMarketValue = SRC.SquadMarketValue,
				   fCrestUrl = SRC.CrestUrl,
				   fLastUpdatedUtc = GETUTCDATE()
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (fFootballDataId, fWhoScoredId, fName, fFullName, fCode, fSquadMarketValue, fCrestUrl, fLastUpdatedUtc)
		VALUES (SRC.FootballDataId, SRC.WhoScoredId, SRC.Name, SRC.FullName, SRC.Code, SRC.SquadMarketValue, SRC.CrestUrl, GETUTCDATE());

	IF @@ERROR <> 0
		RETURN -1;

	RETURN 0;
END
GO
