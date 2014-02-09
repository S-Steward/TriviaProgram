using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;
using TriviaLibrary;
using System.ServiceModel.Description;
 



namespace GameplayWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CallbackBehavior(ConcurrencyMode=ConcurrencyMode.Single,
    UseSynchronizationContext = false)]
    public partial class MainWindow : Window , iCallback
    {
        
        private iTrivia trivia;
        
        private Guid myCallbackKey;
       
        public MainWindow()
        {
            InitializeComponent();
            
            //Player connects to server
            try
            {
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

                EndpointAddress endpointAddress = new EndpointAddress(String.Format("net.tcp://{0}:{1}/TriviaLibrary/Trivia", "172.20.248.18", "10000"));

                DuplexChannelFactory<iTrivia> channel = new DuplexChannelFactory<iTrivia>(this, binding, endpointAddress);
                trivia = channel.CreateChannel();
                myCallbackKey = trivia.RegisterForCallbacks();
                
                //After connection set up a new game for user
                SetupNewGame();
                
                
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }

        //private int qCount { get; set; }

        //private const string FORMAT_QUESTION_COUNT = "{0} of {1}";
        private string EntryName { get; set; }

        private SendOrPostCallback postCallback { get; set; }

        private Question_Key CurrentQuestion { get; set; }

        private Answer_Key CurrentAnswer { get; set; }

        //private int QuestionCount { get; set; }

       // private const string GAME_START = "Game Started, {0} questions.";

        private Dictionary<string, Int64> Scoreboard { get; set; }

        
       

        
        //Set game functions to invisible while players register

        private void SetupNewGame()
        {
            // Clear listboxes

            lstScoreboard.Items.Clear();

            //Show Register Window
            registerBar.Visibility = Visibility.Visible;
            playerRegister.Visibility = Visibility.Visible;
            registerButton.Visibility = Visibility.Visible;

            //Hide buttons
            A.Visibility = Visibility.Collapsed;
            B.Visibility = Visibility.Collapsed;
            C.Visibility = Visibility.Collapsed;
            D.Visibility = Visibility.Collapsed;
            endGame.Visibility = Visibility.Collapsed;
           
            
            
        }

        //After users have clicked register, initialize game
        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            string userName;
            try
            {
                if (playerRegister.Text.Trim() != string.Empty)
                {
                    trivia.RegisterUser(playerRegister.Text.Trim());
                    userName = playerRegister.Text;
                    UserRegisterComplete(playerRegister.Text);

                }
                else
                {
                    // blank entry
                    questionLabel.Content = "Please enter a User Name";
                    playerRegister.Text = string.Empty;
                    playerRegister.Focus();
                }
            }
            catch (Exception ex)
            {
                postCallback = delegate
                {
                    MessageBox.Show(string.Format(ex.Message));
                };

                 
            }
            
            
            

        }

        //Updates player list
        public void OnUserUpdate(List<string> userName)
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (userName.Count >= 1)
                        playerOne.Content = userName[0];
                    else if (userName.Count >= 2)
                        playerTwo.Content = userName[1];
                    else if (userName.Count >= 3)
                        playerThree.Content = userName[2];
                    else if (userName.Count >= 4)
                        playerFour.Content = userName[3];

                }));
                

            }
            catch (Exception ex)
            {
                string x = ex.Message;
                throw ex;
            }
        }

        public void OnUserRegister(string userName)
        {
            postCallback = delegate
            {
                trivia.RegisterUser(userName);
            };
        }

        //Registration is complete, initialize game functions
        private void UserRegisterComplete(string userName)
        {
            EntryName = userName;
            //int questionCounter = 10;

            registerBar.Visibility = Visibility.Collapsed;
            playerRegister.Visibility = Visibility.Collapsed;
            registerButton.Visibility = Visibility.Collapsed;
            
            A.Visibility = Visibility.Visible;
            B.Visibility = Visibility.Visible;
            C.Visibility = Visibility.Visible;
            D.Visibility = Visibility.Visible;

            trivia.GameStart();
        }

        

        //Seems to be implemented
        public void OnGameStart(int questionCounter)
        {
           
            //QuestionCount = questionCounter;
            
            postCallback = delegate
            {
                BeginGame(questionCounter);
            };
            
           // MarshalToUIThread(postCallback);
        }

        public void OnGameEnd()
        {
             EndGame();
        }

        private void SetAnswerButton(Button button, bool isEnabled, string buttonText)
        {
            button.Content = buttonText;
            button.IsEnabled = isEnabled;
        }

        private void ResetAnswerButtons(bool resetValues)
        {
            if (resetValues)
            {
                SetAnswerButton(A, false, "");
                SetAnswerButton(B, false, "");
                SetAnswerButton(C, false, "");
                SetAnswerButton(D, false, "");
            }
        }

        //When game has ended, find user with max points and declare winner
        private void EndGame()
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                // Find winner
                   
                string winnerUser = GetWinner();

                if (winnerUser == EntryName)
                {
                    //Show a set window to winner
                    GameplayWindow.YouWin winner = new YouWin();
                    winner.Show();

                }
                else
                {
                    //Show a set window to winner
                    GameplayWindow.YouLose loser = new YouLose();
                    loser.Show();
                }

                // Retrieve winner
                questionLabel.Content = string.Format(winnerUser + " is the winner of this game");

                ResetAnswerButtons(true);

                // Show end buttons, hide answers
                A.Visibility = Visibility.Collapsed;
                B.Visibility = Visibility.Collapsed;
                C.Visibility = Visibility.Collapsed;
                D.Visibility = Visibility.Collapsed;
                endGame.Visibility = Visibility.Visible;
                }));
            }
            catch (Exception ex)
            {
                string x = ex.Message;
                throw ex;
            }
        }       

        
        private string GetWinner()
        {
            string winnerKey = string.Empty;


            long key = 0;
            string tempKey = string.Empty;
            
            //Take into account user score and assertain winner
            foreach (string userKey in Scoreboard.Keys)
            {
                if(Scoreboard[userKey] >= key)
                {
                    tempKey = userKey;
                    key = Scoreboard[userKey];
                }
            }

            winnerKey = tempKey;
            
            return winnerKey;
        }

        public void OnQuestionLoad(int questionIndex, Question_Key questionItem)
        {
            
            try
            {
                // Dim question text
                //questionLabel.Opacity = 1;
                 this.Dispatcher.Invoke((Action)(() =>
                {
                // Clear stored answer and save question object
                CurrentAnswer = Answer_Key.None;
                CurrentQuestion = questionItem;
                
                // Load question text
                questionLabel.Content = CurrentQuestion.QuestionText;
                //QuestionCount = 10;
                questionCounter.Content = "Question: ";
                // private const string GAME_START = "Game Started, {0} questions.";

                // Prep answer buttons
                SetAnswerButton(A, true, CurrentQuestion.AnswerA);
                SetAnswerButton(B, true, CurrentQuestion.AnswerB);
                SetAnswerButton(C, true, CurrentQuestion.AnswerC);
                SetAnswerButton(D, true, CurrentQuestion.AnswerD);
                }));
            }
            catch( Exception ex)
            {
                string x = ex.Message;
                throw ex;
            }
        }

        
        

        private void BeginGame(int questionCounter)
        {
            questionLabel.Content = "Beginning game with friend";
            //string.Format(GAME_START, questionCounter);
        }

        public void OnQuestionEnd()
        {
            this.Dispatcher.Invoke((Action)(() =>
                {
                    //answerLabel.Content = CurrentQuestion.Correct == CurrentAnswer ? DISPLAY_CORRECT : DISPLAY_INCORRECT;


                    // Reset buttons
                    SetAnswerButton(A, false, "");
                    SetAnswerButton(B, false, "");

                    SetAnswerButton(C, false, "");
                    SetAnswerButton(D, false, "");
                }));
        }

        public void OnQuestionUpdate(Int64 qTimeRemain)
        {
            postCallback = delegate
            {
                OnQuestionUpdate(qTimeRemain);                
            };

            //answerLabel.Content = qTimeRemain;
        
        }

        public void OnScoreUpdate(Dictionary<string, Int64> score )
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    Scoreboard = score;
                    {
                        lstScoreboard.Items.Clear();


                        foreach (string userName in score.Keys)
                        {
                            lstScoreboard.Items.Add(string.Format("{0} : {1}", userName, score[userName]));
                        }
                    }
                }));
            }

            catch(Exception ex)
            {
                string x = ex.Message;
                throw ex;
            }
        }

        private void answer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button clickedButton = (Button)sender;
                
                switch (clickedButton.Tag.ToString().Trim().ToUpper())
                {
                    case "A":
                        CurrentAnswer = Answer_Key.A;
                        break;

                    case "B":
                        CurrentAnswer = Answer_Key.B;
                        break;

                    case "C":
                        CurrentAnswer = Answer_Key.C;
                        break;

                    case "D":
                        CurrentAnswer = Answer_Key.D;
                        break;

                    default:
                        CurrentAnswer = Answer_Key.None;
                        break;
                }

                // Set all buttons back to original color
                ResetAnswerButtons(false);

                // Send answer
                trivia.CorrectAnswer(EntryName, CurrentAnswer.ToString());

                trivia.questionComplete();

                
                
            }
            catch (Exception ex)
            {
                string x = ex.Message;
                throw ex;
            }
           
        }

        private void endGame_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

       

        
    }
}
