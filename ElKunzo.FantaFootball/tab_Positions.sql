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