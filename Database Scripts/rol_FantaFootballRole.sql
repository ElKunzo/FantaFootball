IF NOT EXISTS(SELECT 1 FROM sys.sysusers WHERE name = N'FantaFootballRole' AND issqlrole = 1)
	CREATE ROLE FantaFootballRole;
GO