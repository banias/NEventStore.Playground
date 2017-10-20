using System;
using CommonDomain.Core;
using Newtonsoft.Json;

namespace Domain
{
    [Serializable]
    public struct InvoiceNumber
    {
        [JsonProperty]
        public string Number { get; private set; }
        public InvoiceNumber(string value)
        {
            Number = value;
        }
    }

    [Serializable]
    public struct Currency
    {
        [JsonProperty]
        public string Code { get; private set; }

        public Currency(string code)
        {
            Code = code;
        }
    }

    [Serializable]
    public struct Amount
    {
        public decimal Value { get; }
        [JsonProperty]
        public Currency Currency { get; private set; }
        public Amount(decimal value,  Currency code)
        {
            Value = value;
            Currency = code;
        }

    }


    public class Connnection: AggregateBase
    {
        
    }
}
