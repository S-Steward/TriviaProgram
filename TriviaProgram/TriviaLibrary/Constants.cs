using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TriviaLibrary
{
    public enum Answer_Key
    {
        None = 0,
        A = 1,
        B = 2, 
        C = 3,
        D = 4
    }

    [DataContract]
    public struct Question_Key
    {
        [DataMember]
        public string QuestionText;

        [DataMember]
        public string AnswerA;

        [DataMember]
        public string AnswerB;

        [DataMember]
        public string AnswerC;

        [DataMember]
        public string AnswerD;

        [DataMember]
        public Answer_Key Correct;

    }
}
