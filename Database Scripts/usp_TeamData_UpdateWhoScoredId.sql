IF OBJECT_ID(N'dbo.usp_TeamData_UpdateWhoScoredId','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_TeamData_UpdateWhoScoredId AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_TeamData_UpdateWhoScoredId TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_TeamData_UpdateWhoScoredId
(
	@WhoScoredIdData dbo.WhoScoredIdType READONLY
)
AS


BEGIN
	 SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
     SET NOCOUNT ON;

	MERGE 
		dbo.tab_TeamData AS TRG
	USING
		(SELECT Id, WhoScoredId FROM @WhoScoredIdData) AS SRC
	ON
		TRG.fId = SRC.Id
	WHEN MATCHED THEN 
		UPDATE SET  fWhoScoredId = SRC.WhoScoredId,
				    fLastUpdatedUtc = GETUTCDATE();

	IF @@ERROR <> 0
		RETURN -1;

	RETURN 0;
END
GO
