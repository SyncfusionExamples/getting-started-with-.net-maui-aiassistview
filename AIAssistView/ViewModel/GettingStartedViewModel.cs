using Syncfusion.Maui.AIAssistView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace GettingStarted
{
    public class GettingStartedViewModel : INotifyPropertyChanged
    {
        #region Field
        private ObservableCollection<IAssistItem> messages;
        private ObservableCollection<GettingStartedModel> headerInfoCollection;
        private List<List<string>> suggestionlist = new List<List<string>>();
        private AzureAIService azureAIService;
        private Thickness headerPadding;
        private bool cancelResponse;
        #endregion

        #region Constructor
        public GettingStartedViewModel()
        {
            azureAIService = new AzureAIService();
            this.GetHeaderInfo();
            this.GenerateSuggestions();
            this.messages = new ObservableCollection<IAssistItem>();
            this.CopyCommand = new Command<object>(ExecuteCopyCommand);
            this.RetryCommand = new Command<object>(ExecuteRetryCommand);
            this.AssistViewRequestCommand = new Command<object>(ExecuteRequestCommand);
            this.HeaderItemTappedCommand = new Command(HeaderItemTapCommand);
            this.StopRespondingCommand = new Command(ExecuteStopResponding);
        }
        #endregion

        private ObservableCollection<string> HeaderMessages { get; set; } = new ObservableCollection<string>
        {
            "Ownership",
            "Brainstorming",
            "Listening",
             "Resilience",
        };

        private ObservableCollection<string> ImagesCollection { get; set; } = new ObservableCollection<string>
        {
             "ownership.png",
            "brainstorming.png",
            "listening.png",
            "resilience.png",
        };

        public ICommand CopyCommand { get; set; }
        public ICommand RetryCommand { get; set; }
        public ICommand AssistViewRequestCommand { get; set; }
        public ICommand HeaderItemTappedCommand { get; set; }
        public ICommand StopRespondingCommand { get; set; }

        public ObservableCollection<GettingStartedModel> HeaderInfoCollection
        {
            get
            {
                return headerInfoCollection;
            }
            set
            {
                this.headerInfoCollection = value;
            }
        }

        public ObservableCollection<IAssistItem> AssistItems
        {
            get
            {
                return this.messages;
            }

            set
            {
                this.messages = value;
                RaisePropertyChanged("AssistItems");
            }
        }


        public Thickness HeaderPadding
        {
            get { return headerPadding; }
            set { headerPadding = value; RaisePropertyChanged("HeaderPadding"); }
        }

        public bool CancelResponse
        {
            get
            {
                return cancelResponse;
            }
            set
            {
                cancelResponse = value;
                RaisePropertyChanged(nameof(CancelResponse));
            }
        }

        private async void HeaderItemTapCommand(object obj)
        {
            AssistItem request = new AssistItem() { Text = (obj as Label).Text, IsRequested = true };
            this!.AssistItems.Add(request);
            await this.GetResponseWithSuggestion(request).ConfigureAwait(true);
        }

        private async void ExecuteRequestCommand(object obj)
        {
            var request = (obj as Syncfusion.Maui.AIAssistView.RequestEventArgs).RequestItem;
            await this.GetResult(request).ConfigureAwait(true);
        }

        private void ExecuteCopyCommand(object obj)
        {
            string text = (obj as AssistItem).Text;
            text = Regex.Replace(text, "<.*?>|&nbsp;", string.Empty);
            Clipboard.SetTextAsync(text);
        }

        private async void ExecuteRetryCommand(object obj)
        {
            var request = (obj as AssistItem).RequestItem;
            IAssistItem item = (obj as AssistItem).RequestItem as IAssistItem;
            if (item != null)
            {
                request = item;
            }
            await this.GetResult(request).ConfigureAwait(true);
        }

        private void ExecuteStopResponding()
        {
            this.CancelResponse = true;
            AssistItem responseItem = new AssistItem() { Text = "You canceled the response" };
            responseItem.ShowAssistItemFooter = false;
            this.AssistItems.Add(responseItem);
        }

        private void GetHeaderInfo()
        {
            var headerInfo = new ObservableCollection<GettingStartedModel>();
            for (int i = 0; i < 4; i++)
            {
                var gallery = new GettingStartedModel()
                {
                    Image = this.ImagesCollection[i],
                    HeaderMessage = this.HeaderMessages[i],
                };
                headerInfo.Add(gallery);
            }
            this.headerInfoCollection = headerInfo;
        }

        private async Task GetResult(object inputQuery)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            AssistItem request = (AssistItem)inputQuery;
            if (request != null)
            {
                var userAIPrompt = this.GetUserAIPrompt(request.Text);
                var response = await azureAIService!.GetResultsFromAI(request.Text, userAIPrompt).ConfigureAwait(true);
                response = response.Replace("\n", "<br>");
                if (!CancelResponse)
                {
                    AssistItem responseItem = new AssistItem() { Text = response };
                    responseItem.RequestItem = inputQuery;
                    this.AssistItems.Add(responseItem);
                }
            }

            this.CancelResponse = false;
        }

        private string GetUserAIPrompt(string userPrompt)
        {
            string userQuery = $"Given User query: {userPrompt}." +
                      $"\nSome conditions need to follow:" +
                      $"\nGive heading of the topic and simplified answer in 4 points with numbered format" +
                      $"\nGive as string alone" +
                      $"\nRemove ** and remove quotes if it is there in the string.";
            return userQuery;
        }

        private async Task GetResponseWithSuggestion(object inputQuery)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            AssistItem request = (AssistItem)inputQuery;
            if (request != null)
            {
                var userAIPrompt = this.GetUserAIPrompt(request.Text);
                var response = await azureAIService!.GetResultsFromAI(request.Text, userAIPrompt).ConfigureAwait(true);
                response = response.Replace("\n", "<br>");
                await Task.Delay(1000).ConfigureAwait(true);
                var suggestion = this.GetSuggestion(request.Text);
                await Task.Delay(1000).ConfigureAwait(true);
                if (!CancelResponse)
                {
                    AssistItem responseItem = new AssistItem() { Text = response, Suggestion = suggestion };
                    responseItem.RequestItem = inputQuery;
                    this.AssistItems.Add(responseItem);
                }
            }

            this.CancelResponse = false;
        }

        private void GenerateSuggestions()
        {
            List<string> firstHeaderSuggestion = new List<string> { "Initiation", "Responsibility", "Accountability" };
            List<string> secondHeaderSuggestion = new List<string> { "Different Perspective", "More Ideas" };
            List<string> thirdHeaderSuggestion = new List<string> { "Active Listening", "Passive Listening" };
            suggestionlist.Add(firstHeaderSuggestion);
            suggestionlist.Add(secondHeaderSuggestion);
            suggestionlist.Add(thirdHeaderSuggestion);
        }

        private AssistItemSuggestion GetSuggestion(string prompt)
        {
            var promptSuggestions = new AssistItemSuggestion();

            for (int i = 0; i < HeaderMessages.Count() - 1; i++)
            {
                if (HeaderMessages[i].Contains(prompt))
                {
                    var suggestions = new ObservableCollection<ISuggestion>();
                    foreach (var items in suggestionlist[i])
                    {
                        suggestions.Add(new AssistSuggestion() { Text = items });
                    }
                    promptSuggestions.Items = suggestions;
                    promptSuggestions.Orientation = SuggestionsOrientation.Horizontal;
                    return promptSuggestions;
                }
            }
            return promptSuggestions;
        }

        #region PropertyChanged

        /// <summary>
        /// Property changed handler.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when property is changed.
        /// </summary>
        /// <param name="propName">changed property name</param>
        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}
