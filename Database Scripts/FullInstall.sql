IF NOT EXISTS(SELECT 1 FROM sys.sysusers WHERE name = N'FantaFootballRole' AND issqlrole = 1)
	CREATE ROLE FantaFootballRole;
GO

IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'FixtureDataType')
	CREATE TYPE dbo.FixtureDataType AS TABLE(
	Id INT NOT NULL,
	WhoScoredId INT,
	FootballDataId INT,
	StatusId INT NOT NULL,
	KickOffUtc DATETIME,
	MatchDay INT,
	HomeTeamId INT NOT NULL,
	AwayTeamId INT NOT NULL,
	HomeScore INT,
	AwayScore INT
)
GO

GRANT EXECUTE ON TYPE::dbo.FixtureDataType TO FantaFootballRole;
GO



IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'PlayerScoreDataType')
	CREATE TYPE dbo.PlayerScoreDataType AS TABLE(
	Id INT NOT NULL,
	FixtureId INT NOT NULL,
	PlayerId INT NOT NULL,
	TotalPoints INT,
	MinutesPlayed INT,
	GoalsScored INT,
	Assists INT,
	CleanSheet BIT,
	ShotsSaved INT,
	PenaltiesSaved INT,
	PenaltiesMissed INT,
	GoalsConceded INT,
	YellowCards INT,
	RedCard INT,
	OwnGoals INT
)
GO

GRANT EXECUTE ON TYPE::dbo.PlayerScoreDataType TO FantaFootballRole;
GO



IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'PlayerStaticDataType')
	CREATE TYPE dbo.PlayerStaticDataType AS TABLE(
	Id INT NOT NULL,
	WhoScoredId INT NOT NULL,
	FootballDataTeamId INT,
	TeamId INT NOT NULL,
	JerseyNumber INT,
	Position INT NOT NULL,
	Name NVARCHAR(500),
	FullName NVARCHAR(500),
	DateOfBirth DATETIME,
	Nationality NVARCHAR(50),
	ContractUntil DATETIME,
    MarketValue INT
)
GO

GRANT EXECUTE ON TYPE::dbo.PlayerStaticDataType TO FantaFootballRole;
GO



IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'TeamDataType')
	CREATE TYPE dbo.TeamDataType AS TABLE(
	Id INT NOT NULL,
	FootballDataId INT NOT NULL,
	WhoScoredId INT NOT NULL,
	Name NVARCHAR(500),
	FullName NVARCHAR(500),
    Code NVARCHAR(5),
    SquadMarketValue INT,
    CrestUrl NVARCHAR(500)
)
GO

GRANT EXECUTE ON TYPE::dbo.TeamDataType TO FantaFootballRole;
GO



IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = N'WhoScoredIdType')
	CREATE TYPE dbo.WhoScoredIdType AS TABLE(
	Id INT NOT NULL,
	WhoScoredId INT NOT NULL
)
GO

GRANT EXECUTE ON TYPE::dbo.WhoScoredIdType TO FantaFootballRole;
GO



CREATE TABLE tab_Positions(
	fId INT NOT NULL IDENTITY(1,1),
	fPositionName NVARCHAR(50),
	PRIMARY KEY (fId)
);

INSERT INTO dbo.tab_Positions VALUES ('Goalkeeper');
INSERT INTO dbo.tab_Positions VALUES ('Defender');
INSERT INTO dbo.tab_Positions VALUES ('Midfielder');
INSERT INTO dbo.tab_Positions VALUES ('Forward');
INSERT INTO dbo.tab_Positions VALUES ('Unknown');



CREATE TABLE tab_FixtureStatus(
	fId INT NOT NULL IDENTITY(1,1),
	fStatus NVARCHAR(20),
	PRIMARY KEY (fId)
);

INSERT INTO dbo.tab_FixtureStatus VALUES ('Scheduled');
INSERT INTO dbo.tab_FixtureStatus VALUES ('Timed');
INSERT INTO dbo.tab_FixtureStatus VALUES ('InPlay');
INSERT INTO dbo.tab_FixtureStatus VALUES ('Finished');
INSERT INTO dbo.tab_FixtureStatus VALUES ('Postponed');
INSERT INTO dbo.tab_FixtureStatus VALUES ('Canceled');
INSERT INTO dbo.tab_FixtureStatus VALUES ('Unknown');



CREATE TABLE tab_TeamData(
	fId INT NOT NULL IDENTITY(1,1),
	fFootballDataId INT NOT NULL,
	fWhoScoredId INT NOT NULL,
	fName NVARCHAR(500),
	fFullName NVARCHAR(500),
	fCode NVARCHAR(5),
    fSquadMarketValue INT,
    fCrestUrl NVARCHAR(500),
	fLastUpdatedUtc DATETIME,
	PRIMARY KEY (fId)
);



CREATE TABLE tab_FixtureData(
	fId INT NOT NULL IDENTITY(1,1),
	fWhoScoredId INT,
	fFootballDataId INT,
	frStatusId INT NOT NULL,
	fKickOffUtc DATETIME,
	fMatchDay INT,
	frHomeTeamId INT NOT NULL,
	frAwayTeamId INT NOT NULL,
	fHomeScore INT,
	fAwayScore INT,
	fLastUpdatedUtc DATETIME,
	PRIMARY KEY (fId),
	FOREIGN KEY (frStatusId) REFERENCES tab_FixtureStatus(fId),
	FOREIGN KEY (frHomeTeamId) REFERENCES tab_TeamData(fId),
	FOREIGN KEY (frAwayTeamId) REFERENCES tab_TeamData(fId),
);




CREATE TABLE tab_PlayerStaticData(
	fId INT NOT NULL IDENTITY(1,1),
	fWhoScoredId INT,
	fFootballDataTeamId INT,
	frTeamId INT NOT NULL,
	fJerseyNumber INT,
	frPosition INT NOT NULL,
	fName NVARCHAR(500),
	fFullName NVARCHAR(500),
	fDateOfBirth DATETIME,
	fNationality NVARCHAR(50),
	fContractUntil DATETIME,
	fMarketValue INT,
	fLastUpdatedUtc DATETIME,
	PRIMARY KEY (fId),
	FOREIGN KEY (frTeamId) REFERENCES tab_TeamData(fId),
	FOREIGN KEY (frPosition) REFERENCES tab_Positions(fId)
);



CREATE TABLE tab_PlayerScoreData(
	fId INT NOT NULL IDENTITY(1,1),
	frFixtureId INT NOT NULL,
	frPlayerId INT NOT NULL,
	fTotalPoints INT,
	fMinutesPlayed INT,
	fGoalsScored INT,
	fAssists INT,
	fCleanSheet BIT,
	fShotsSaved INT,
	fPenaltiesSaved INT,
	fPenaltiesMissed INT,
	fGoalsConceded INT,
	fYellowCards INT,
	fRedCard INT,
	fOwnGoals INT,
	fLastUpdatedUtc DATETIME,
	PRIMARY KEY (fId),
	FOREIGN KEY (frFixtureId) REFERENCES tab_FixtureData(fId),
	FOREIGN KEY (frPlayerId) REFERENCES tab_PlayerStaticData(fId)
);



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



IF OBJECT_ID(N'dbo.usp_FixtureData_UpdateWhoScoredId','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_FixtureData_UpdateWhoScoredId AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_FixtureData_UpdateWhoScoredId TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_FixtureData_UpdateWhoScoredId
(
	@WhoScoredIdData dbo.WhoScoredIdType READONLY
)
AS


BEGIN
	 SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
     SET NOCOUNT ON;

	MERGE 
		dbo.tab_FixtureData AS TRG
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




IF OBJECT_ID(N'dbo.usp_PlayerStaticData_UpdateWhoScoredId','P') IS NULL
	EXEC('CREATE PROCEDURE dbo.usp_PlayerStaticData_UpdateWhoScoredId AS SELECT NULL');
GO

GRANT EXECUTE ON dbo.usp_PlayerStaticData_UpdateWhoScoredId TO FantaFootballRole;
GO

ALTER PROCEDURE dbo.usp_PlayerStaticData_UpdateWhoScoredId
(
	@WhoScoredIdData dbo.WhoScoredIdType READONLY
)
AS


BEGIN
	 SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
     SET NOCOUNT ON;

	MERGE 
		dbo.tab_PlayerStaticData AS TRG
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



