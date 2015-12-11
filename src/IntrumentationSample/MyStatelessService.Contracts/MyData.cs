using System.Runtime.Serialization;

namespace MyStatelessService.Contracts
{
    [DataContract]
    public class MyData
    {
        [DataMember]
        public string A { get; set; }

        [DataMember]
        public int B { get; set; }
    }
}