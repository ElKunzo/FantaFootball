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