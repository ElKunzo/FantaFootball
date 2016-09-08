namespace ElKunzo.FantaFootball

[<AutoOpen>]
module DomainTypes = 

    type OperationResult<'TSuccess,'TFailure> = 
        | Success of 'TSuccess
        | Failure of 'TFailure

    let isSuccess (result:OperationResult<'a,'b>) = 
        match result with
        | Success x -> true
        | Failure x -> false