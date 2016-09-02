namespace ElKunzo.FantaFootball.DataTransferObjects

open System

module Internal = 

    [<StructuredFormatDisplay("Id: {Id} - Name: {FullName}")>]
    type TeamStaticData = {
            Id : int;
            ExternalId : int;
            Name : string;
            FullName : string;
        }