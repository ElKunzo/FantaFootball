namespace ElKunzo.FantaFootball

open System

open ElKunzo.FantaFootball.Internal

module CacheRepository = 

    let Team = TeamStaticData.Cache("usp_TeamData_Get", TeamStaticData.mapFromSqlType, TimeSpan.FromMinutes(60.0))



    let PlayerStatic = PlayerStaticData.Cache("usp_PlayerStaticData_Get", PlayerStaticData.mapFromSqlType, TimeSpan.FromMinutes(60.0))



    let Fixture = FixtureData.Cache("usp_FixtureData_Get", FixtureData.mapFromSqlType, TimeSpan.FromMinutes(60.0))
