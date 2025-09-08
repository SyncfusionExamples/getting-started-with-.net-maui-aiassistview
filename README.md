# getting-started-with-.net-maui-aiassistview
This demo explains about how to use .NET MAUI AI Assist View(SfAIAssistView) in .NET MAUI apps.

## Sample

```xaml

   <ContentPage.BindingContext>
        <local:GettingStartedViewModel x:Name="viewModel"/>
    </ContentPage.BindingContext>

    <aiAssist:SfAIAssistView x:Name="sfAIAssistView"
                             ShowHeader="true"
                             HeaderTemplate="{StaticResource headerTemplate}"
                             AssistItems="{Binding AssistItems}" 
                             ItemCopyCommand="{Binding CopyCommand}"
                             RequestCommand="{Binding AssistViewRequestCommand}"
                             ItemRetryCommand="{Binding RetryCommand}"
                             StopRespondingCommand="{Binding StopRespondingCommand}">
    </aiAssist:SfAIAssistView>

View Model:

        public GettingStartedViewModel()
        {
            azureAIService = new AzureAIService();
            this.AssistViewRequestCommand = new Command<object>(ExecuteRequestCommand);

            ...
        }
    
        public ICommand AssistViewRequestCommand { get; set; }

        private async void ExecuteRequestCommand(object obj)
        {
            var request = (obj as Syncfusion.Maui.AIAssistView.RequestEventArgs).RequestItem;
            await this.GetResult(request).ConfigureAwait(true);
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

```
## Requirements to run the demo

To run the demo, refer to [System Requirements for .NET MAUI](https://help.syncfusion.com/maui/system-requirements)

## Troubleshooting:
### Path too long exception

If you are facing path too long exception when building this example project, close Visual Studio and rename the repository to short and build the project.

## License

Syncfusion® has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion® liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or seeing the samples, you acknowledge and agree Syncfusion®'s samples will not allow you seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion®'s samples.
