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