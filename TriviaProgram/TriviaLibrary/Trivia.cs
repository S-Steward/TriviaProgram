using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime;

using System.ServiceModel;
using System.Data;

namespace TriviaLibrary
{
    public interface iCallback
    {
        //[OperationContract(IsOneWay = true)]
        //void UpdateGui(CallbackInfo info);
        [OperationContract(IsOneWay = true)]
        void OnUserRegister(string userName);
        [OperationContract(IsOneWay = true)]
        void OnQuestionLoad(int questionIndex, Question_Key questionItem);
        [OperationContract(IsOneWay = true)]
        void OnQuestionEnd();
        [OperationContract(IsOneWay = true)]
        void OnScoreUpdate(Dictionary<string, Int64> score);
        [OperationContract(IsOneWay = true)]
        void OnGameStart(int questionCount);
        [OperationContract(IsOneWay = true)]
        void OnGameEnd();
        [OperationContract(IsOneWay = true)]
        void OnQuestionUpdate(Int64 qTimeRemains);
        [OperationContract(IsOneWay = true)]
        void OnUserUpdate(List<string> userList);

    }

    [ServiceContract(CallbackContract = typeof(iCallback))]
    public interface iTrivia
    {
        [OperationContract]
        Guid RegisterForCallbacks();
        [OperationContract(IsOneWay = true)]
        void UnregisterForCallbacks(Guid key);
        [OperationContract(IsOneWay = true)]
        void RegisterUser(string userName);
        [OperationContract(IsOneWay = true)]
        void GameStart();
        [OperationContract(IsOneWay = true)]
        void GameEnd();
        [OperationContract(IsOneWay = true)]
        void checkAnswers();
        [OperationContract(IsOneWay = true)]
        void questionComplete();
        [OperationContract(IsOneWay = true)]
        void CorrectAnswer(string userName, string answerValue);

    }

    struct answerKey
    {
        public string userName;
        public Answer_Key a_Key;
        public Int64 answerTimeRemains;
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Trivia : iTrivia
    {
        #region Const Variables
        private const string CONFIG_LOADDELAY = "LoadDelay";
        private const string CONFIG_QTIME = "QTime";
        private const string USER_REGISTERED = "{0} has entered the game";
        private const string USER_EXISTS = "User already in game.";

        private const int QUESTION_TIMER_INTERVAL = 500;
        private const int QUESTION_TIMER_NOTIFY_INTERVAL = 1000;
        #endregion

        #region Variables
        private iCallback _currentCall;

        Timer _qTimer;
        static Int64 _currentQTimeRemains;
        int _questionIndex;
        private int _qTime = 3000;

        List<string> userList = new List<string>();

        #region Dictionary
        static Dictionary<string, iCallback> _callback = new Dictionary<string, iCallback>();
        static Dictionary<int, Question_Key> _questions = new Dictionary<int, Question_Key>();
        static Dictionary<string, answerKey> _answers = new Dictionary<string, answerKey>();
        static Dictionary<string, Int64> _scoreboard = new Dictionary<string, Int64>();
        private Dictionary<Guid, iCallback> _clientCallbacks = new Dictionary<Guid, iCallback>();

        #endregion

        #endregion

        public Trivia()
        {
            Console.WriteLine("Getting Questions and Answers");
            _questionIndex = 0;
            //GameStart();
        }

        public void RegisterUser(string userName)
        {
            _currentCall = OperationContext.Current.GetCallbackChannel<iCallback>();

            if (!_callback.ContainsKey(userName))
            {
                _callback.Add(userName, _currentCall);
                _scoreboard.Add(userName, 0);
                _currentCall.OnUserRegister(userName);
                try
                {
                    userList.Add(userName);
                    UserList();
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                Console.WriteLine("User " + userName + " has joined the game!");

            }
            else
            {
                throw new Exception(USER_EXISTS);
            }
            
        }

        public void UserList()
        {
            try
            {
                foreach(var id in _clientCallbacks)
                {
                    id.Value.OnUserUpdate(userList);
                }
            }
            catch (Exception ex)
            {
                string x = ex.Message;
                throw ex;
            }
        }

        public void GameStart()
        {
            loadFile();

            _qTimer = new Timer();
            _qTimer.Tick += new EventHandler(qTimer_Tick);
            _qTimer.Interval = QUESTION_TIMER_INTERVAL;

            foreach (Guid user in _clientCallbacks.Keys)
            {
                _currentCall = _clientCallbacks[user];
                _currentCall.OnScoreUpdate(_scoreboard);
                _currentCall.OnGameStart(_questions.Count);
            }

            loadQuestion();
        }

        private void loadFile()
        {
            if (_questions.Count <= 0)
            {
                try
                {
                    DataSet questionSet = new DataSet();
                    questionSet.ReadXml(@"TriviaFiles\Trivia.xml");

                    int index = 0;

                    foreach (DataRow row in questionSet.Tables[0].Rows)
                    {
                        Question_Key key = new Question_Key();
                        key.QuestionText = row["QuestionString"].ToString();
                        key.AnswerA = row["AnswerA"].ToString();
                        key.AnswerB = row["AnswerB"].ToString();
                        key.AnswerC = row["AnswerC"].ToString();
                        key.AnswerD = row["AnswerD"].ToString();
                        key.Correct = (Answer_Key)Enum.Parse(typeof(Answer_Key), row["Correct"].ToString());

                        _questions.Add(index, key);
                        index++;
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void loadQuestion()
        {
            loadQuestion(0);
        }

        private void loadQuestion(int delayTime)
        {
            System.Threading.Thread.Sleep(delayTime);

            foreach (Guid key in _clientCallbacks.Keys)
            {
                _currentCall = _clientCallbacks[key];
                _currentCall.OnQuestionLoad(_questionIndex,
                    (Question_Key)_questions[_questionIndex]);
            }

            _currentQTimeRemains = _qTime;

            _qTimer.Enabled = true;

        }

        public void questionComplete()
        {
            foreach (Guid key in _clientCallbacks.Keys)
            {
                _currentCall = _clientCallbacks[key];
                _currentCall.OnQuestionEnd();
            }

            checkAnswers();

            _questionIndex += 1;

            if (_questionIndex < _questions.Count)
            {
                loadQuestion(2000);
            }
            else
            {
                GameEnd();
            }

        }

        public void CorrectAnswer(string userName, string value)
        {
            
            answerKey ans_key = new answerKey();
            ans_key.answerTimeRemains = _currentQTimeRemains;
            ans_key.a_Key = (Answer_Key)Enum.Parse(typeof(Answer_Key), value);
            ans_key.userName = userName;

            if (_answers.ContainsKey(userName))
            {
                _answers[userName] = ans_key;
            }
            else
            {
                _answers.Add(userName, ans_key);
            }
        }

        public void checkAnswers()
        {
            Question_Key qKey = (Question_Key)_questions[_questionIndex];
            
            foreach (string userNameKey in _answers.Keys)
            {
                answerKey ans_key = (answerKey)_answers[userNameKey];

                if (qKey.Correct == ans_key.a_Key)
                {
                    _scoreboard[userNameKey] += 1;
                }
            }

            foreach (Guid key in _clientCallbacks.Keys)
            {
                _currentCall = _clientCallbacks[key];
                _currentCall.OnScoreUpdate(_scoreboard);
            }
        }

        private void qTimer_Tick(object sender, EventArgs e)
        {
            _currentQTimeRemains -= QUESTION_TIMER_INTERVAL;

            if (_currentQTimeRemains > 0)
            {
                if ((_currentQTimeRemains % QUESTION_TIMER_NOTIFY_INTERVAL) == 0)
                {
                    foreach (Guid key in _clientCallbacks.Keys)
                    {
                        _currentCall = _clientCallbacks[key];
                        _currentCall.OnQuestionUpdate(_currentQTimeRemains);
                    }
                }
            }
            else
            {
                _qTimer.Enabled = false;
                questionComplete();
            }
        }

        public Guid RegisterForCallbacks()
        {
            iCallback cb = OperationContext.Current.GetCallbackChannel<iCallback>();

            Guid key = Guid.NewGuid();
            _clientCallbacks.Add(key, cb);

            return key;
        }

        public void UnregisterForCallbacks(Guid key)
        {
            if (_clientCallbacks.Keys.Contains<Guid>(key))
                _clientCallbacks.Remove(key);
        }

        public void GameEnd()
        {
            System.Threading.Thread.Sleep(1500);

            foreach (Guid key in _clientCallbacks.Keys)
            {
                _currentCall = _clientCallbacks[key];
                _currentCall.OnGameEnd();
            }

            _qTimer.Dispose();

            _questions.Clear();
            _answers.Clear();
            _scoreboard.Clear();

            _clientCallbacks.Clear();
        }

    }
}
