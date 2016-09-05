IF NOT EXISTS(SELECT 1 FROM sys.sysusers WHERE name = N'FantaFootballRole' AND issqlrole = 1)
	CREATE ROLE FantaFootballRole;
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
		(SELECT Id, WhoScoredId, FootballDataTeamId, TeamId, JerseyNumber, Position, Name, FullName, DateOfBirth, Nationality, ContractUntil, MarketValue FROM @PlayerData) AS SRC
	ON
		TRG.fId = SRC.Id
	WHEN MATCHED THEN 
		UPDATE SET  fWhoScoredId = SRC.WhoScoredId,
					fFootballDataTeamId = SRC.FootballDataTeamId, 
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
		VALUES (SRC.WhoScoredId, SRC.FootballDataTeamId, SRC.TeamId, SRC.JerseyNumber, SRC.Position, SRC.Name, SRC.FullName, SRC.DateOfBirth, SRC.Nationality, SRC.ContractUntil, SRC.MarketValue, GETUTCDATE());

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
				   fWhoScoredId = SRC.WhoScoredId, 
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


