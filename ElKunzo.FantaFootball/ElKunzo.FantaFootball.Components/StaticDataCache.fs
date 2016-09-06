namespace ElKunzo.FantaFootball.Components

open System
open System.Collections.Generic
open ElKunzo.FantaFootball.DataAccess.DatabaseDataAccess
open ElKunzo.FantaFootball.DataTransferObjects.Internal

module StaticDataCache = 

    [<AbstractClass>]
    type BaseCacheWithRefreshTimer<'a>(_spName, _mappingFunction, _refreshInterval) =
        let RefreshInterval = _refreshInterval
        let SpName = _spName
        let MappingFunction = _mappingFunction
        let mutable (Data:IReadOnlyList<'a>) = executeReadOnlyStoredProcedureAsync SpName MappingFunction Array.empty |> Async.RunSynchronously
        let mutable TimeStampUtc = DateTime.UtcNow
        member internal this.PublicData = 
            Data
        member this.IsOutdated () = 
            let difference = DateTime.UtcNow.Subtract(TimeStampUtc)
            difference > RefreshInterval
        member this.Update () = 
            Data <- executeReadOnlyStoredProcedureAsync SpName MappingFunction Array.empty |> Async.RunSynchronously
            TimeStampUtc <- DateTime.UtcNow
        abstract member TryGetItem : int -> 'a option

    type TeamDataCache (spName, mappingFunction, refreshInterval) =
        inherit BaseCacheWithRefreshTimer<TeamStaticData>(spName, mappingFunction, refreshInterval)
        override this.TryGetItem (id) = 
            if this.IsOutdated() then this.Update()
            this.PublicData |> Seq.tryFind (fun t -> t.Id = id)

    type PlayerStaticDataCache (spName, mappingFunction, refreshInterval) =
        inherit BaseCacheWithRefreshTimer<PlayerStaticData>(spName, mappingFunction, refreshInterval)
        override this.TryGetItem (id) = 
            if this.IsOutdated() then this.Update()
            this.PublicData |> Seq.tryFind (fun p -> p.Id = id)