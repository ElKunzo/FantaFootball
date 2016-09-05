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